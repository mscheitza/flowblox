using FlowBlox.Core.Attributes;
using FlowBlox.Core.Attributes.FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow.ExecuteProject;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using static FlowBlox.Core.Util.RunnerHostResolver;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [FlowBlockUIGroup("ExecuteProjectFlowBlock_Groups_Project", 0)]
    [FlowBlockUIGroup("ExecuteProjectFlowBlock_Groups_Parameters", 1)]
    [FlowBlockUIGroup("ExecuteProjectFlowBlock_Groups_Output", 2)]
    [Display(Name = "ExecuteProjectFlowBlock_DisplayName", Description = "ExecuteProjectFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class ExecuteProjectFlowBlock : BaseFlowBlock
    {
        [Required]
        [Display(
            Name = "ExecuteProjectFlowBlock_ProjectFile",
            ResourceType = typeof(FlowBloxTexts),
            GroupName = "ExecuteProjectFlowBlock_Groups_Project",
            Order = 0)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFileSelection)]
        [FlowBlockUIFileSelection("FlowBlox project (*.fbprj)|*.fbprj|All files (*.*)|*.*")]
        public string ProjectFile { get; set; }

        [Display(
            Name = "ExecuteProjectFlowBlock_RunnerHostPath",
            ResourceType = typeof(FlowBloxTexts),
            GroupName = "ExecuteProjectFlowBlock_Groups_Project",
            Order = 1)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFileSelection)]
        [FlowBlockUIFileSelection("RunnerHost (*.exe;*.dll)|*.exe;*.dll|All files (*.*)|*.*")]
        public string RunnerHostPath { get; set; }

        [Display(Name = "ExecuteProjectFlowBlock_AbortOnError", ResourceType = typeof(FlowBloxTexts), GroupName = "ExecuteProjectFlowBlock_Groups_Project", Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public bool AbortOnError { get; set; } = true;

        [Display(Name = "ExecuteProjectFlowBlock_AbortOnWarning", ResourceType = typeof(FlowBloxTexts), GroupName = "ExecuteProjectFlowBlock_Groups_Project", Order = 3)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public bool AbortOnWarning { get; set; } = false;

        [Display(Name = "ExecuteProjectFlowBlock_ParameterMappings", ResourceType = typeof(FlowBloxTexts), GroupName = "ExecuteProjectFlowBlock_Groups_Parameters", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.GridView, DisplayLabel = false)]
        [FlowBlockDataGrid(IsMovable = true)]
        public ObservableCollection<ExecuteProjectParameterMappingEntry> ParameterMappings { get; set; } = new ObservableCollection<ExecuteProjectParameterMappingEntry>();

        [Display(Name = "ExecuteProjectFlowBlock_OutputMappings", ResourceType = typeof(FlowBloxTexts), GroupName = "ExecuteProjectFlowBlock_Groups_Output", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.GridView, DisplayLabel = false)]
        [FlowBlockDataGrid(IsMovable = true)]
        public ObservableCollection<ExecuteProjectOutputMappingEntry> OutputMappings { get; set; } = new ObservableCollection<ExecuteProjectOutputMappingEntry>();

        [Display(Name = "ExecuteProjectFlowBlock_ImportChildOutputs", ResourceType = typeof(FlowBloxTexts), GroupName = "ExecuteProjectFlowBlock_Groups_Output", Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public bool ImportChildOutputs { get; set; } = true;

        [Display(Name = "ExecuteProjectFlowBlock_ImportedOutputPrefix", ResourceType = typeof(FlowBloxTexts), GroupName = "ExecuteProjectFlowBlock_Groups_Output", Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public string ImportedOutputPrefix { get; set; } = "Child";

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.play_box_outline, 16, SKColors.MediumPurple);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.play_box_outline, 32, SKColors.MediumPurple);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.ControlFlow;

        public override string NamePrefix => "Execute";

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                if (string.IsNullOrWhiteSpace(ProjectFile))
                    throw new ValidationException("ProjectFile must not be empty.");

                var cmd = RunnerHostResolver.Resolve();

                var workDir = CreateWorkingDirectory();
                var requestFile = Path.Combine(workDir, "request.json");
                var responseFile = Path.Combine(workDir, "response.json");

                var request = BuildRunnerRequest(runtime, workDir);

                WriteJson(requestFile, request);

                var exitCode = StartRunnerHost(cmd, requestFile, responseFile);

                if (!File.Exists(responseFile))
                    throw new InvalidOperationException($"RunnerHost did not create a response file: {responseFile}");

                var response = ReadJson<FlowBlox.Core.Runner.Contracts.RunnerResponse>(responseFile);

                // Bubble up errors if child execution failed.
                if (response == null)
                    throw new InvalidOperationException("Runner response could not be deserialized.");

                if (!response.Success)
                {
                    // Report in runtime and then abort (or throw) to stop the parent flow.
                    runtime.Report($"Child project execution failed (ExitCode={response.ExitCode}).", FlowBloxLogLevel.Error);
                    if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                        runtime.Report(response.ErrorMessage, FlowBloxLogLevel.Error);

                    // Ensure parent execution stops.
                    runtime.Aborted = true;

                    // Still allow next blocks? Usually not.
                    return;
                }

                // Option A: Import all child outputs into parent runtime.
                if (ImportChildOutputs && response.Outputs != null)
                {
                    ImportOutputsIntoParent(runtime, response);
                }

                // Option B: Create a parent output dataset from mapped input fields (optional but requested).
                if (OutputMappings != null && OutputMappings.Count > 0)
                {
                    AppendMappedOutputDataset(runtime);
                }

                ExecuteNextFlowBlocks(runtime);
            });
        }

        private static string CreateWorkingDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), "FlowBloxRunner", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private object BuildRunnerRequest(BaseRuntime runtime, string workDir)
        {
            var userFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var optionOverrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in ParameterMappings ?? Enumerable.Empty<ExecuteProjectParameterMappingEntry>())
            {
                if (entry == null)
                    continue;

                if (string.IsNullOrWhiteSpace(entry.TargetKey))
                    throw new ValidationException("TargetKey must not be empty in parameter mappings.");

                if (entry.SourceField == null)
                    throw new ValidationException($"SourceField must be set for parameter mapping '{entry.TargetKey}'.");

                var value = entry.SourceField.StringValue;

                if (entry.TargetType == ExecuteProjectTargetType.UserField)
                    userFields[entry.TargetKey] = value;
                else
                    optionOverrides[entry.TargetKey] = value;
            }

            // Use RunnerRequest from FlowBlox.Core.Runner.Contracts.
            return new Runner.Contracts.RunnerRequest
            {
                ProjectFile = this.ProjectFile,
                AutoRestart = false, // hard requirement
                NoDesignerMode = true,

                AbortOnError = this.AbortOnError,
                AbortOnWarning = this.AbortOnWarning,

                UserFields = userFields,
                OptionOverrides = optionOverrides,

                WorkingDirectory = workDir
            };
        }

        private static int StartRunnerHost(RunnerHostCommand runnerHostCommand, string requestFile, string responseFile)
        {
            var psi = new ProcessStartInfo
            {
                FileName = runnerHostCommand.FileName,
                Arguments = runnerHostCommand.BuildArguments(requestFile, responseFile),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            proc.WaitForExit();
            var exitCode = proc.ExitCode;
            return exitCode;
        }

        private static void WriteJson(string path, object obj)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private static T ReadJson<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private void ImportOutputsIntoParent(BaseRuntime runtime, FlowBlox.Core.Runner.Contracts.RunnerResponse response)
        {
            var prefix = string.IsNullOrWhiteSpace(ImportedOutputPrefix) ? "Child" : ImportedOutputPrefix;

            foreach (var output in response.Outputs)
            {
                var importedName = $"{prefix}.{this.Name}.{output.Key}";

                foreach (var ds in output.Value)
                {
                    runtime.AppendOutputDataset(importedName, new FlowBloxProjectOutputDataset
                    {
                        OutputName = importedName,
                        CreatedUtc = ds.CreatedUtc,
                        Values = ds.Values ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    });
                }
            }
        }

        private void AppendMappedOutputDataset(BaseRuntime runtime)
        {
            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in OutputMappings ?? Enumerable.Empty<ExecuteProjectOutputMappingEntry>())
            {
                if (entry == null)
                    continue;

                if (string.IsNullOrWhiteSpace(entry.OutputPropertyName))
                    throw new ValidationException("OutputPropertyName must not be empty in output mappings.");

                if (entry.SourceField == null)
                    throw new ValidationException($"SourceField must be set for output property '{entry.OutputPropertyName}'.");

                values[entry.OutputPropertyName] = entry.SourceField.Value;
            }

            var outputName = this.Name;
            runtime.AppendOutputDataset(outputName, new FlowBloxProjectOutputDataset
            {
                OutputName = outputName,
                CreatedUtc = DateTime.UtcNow,
                Values = values
            });
        }
    }
}