using FlowBlox.Core.Attributes;
using FlowBlox.Core.Attributes.FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow.ExecuteProject;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using static FlowBlox.Core.Util.RunnerHostResolver;

namespace FlowBlox.Core.Models.FlowBlocks
{
    // TODO: Auch bei den Extensions sollten wir die Endpoint Uri speichern! Einmal verifizieren!

    [FlowBlockUIGroup("ExecuteProjectFlowBlock_Groups_Project", 0)]
    [FlowBlockUIGroup("ExecuteProjectFlowBlock_Groups_Parameters", 1)]
    [Display(Name = "ExecuteProjectFlowBlock_DisplayName", Description = "ExecuteProjectFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class ExecuteProjectFlowBlock : BaseSingleResultFlowBlock
    {
        [ConditionallyRequired()]
        [ActivationCondition(ActivationMethod = nameof(IsProjectFileVisible))]
        [Display(
            Name = "ExecuteProjectFlowBlock_ProjectFile",
            Description = "ExecuteProjectFlowBlock_ProjectFile_Description",
            ResourceType = typeof(FlowBloxTexts),
            GroupName = "ExecuteProjectFlowBlock_Groups_Project",
            Order = 0)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFileSelection | UIOptions.EnableFieldSelection)]
        [FlowBlockUIFileSelection("FlowBlox project (*.fbprj)|*.fbprj|All files (*.*)|*.*")]
        public string ProjectFile { get; set; }

        private bool IsProjectFileVisible() => string.IsNullOrWhiteSpace(ProjectSpaceGuid);

        [ConditionallyRequired()]
        [ActivationCondition(ActivationMethod = nameof(IsProjectSpaceGuidVisible))]
        [Display(
            Name = "ExecuteProjectFlowBlock_ProjectSpaceGuid",
            Description = "ExecuteProjectFlowBlock_ProjectSpaceGuid_Description",
            ResourceType = typeof(FlowBloxTexts),
            GroupName = "ExecuteProjectFlowBlock_Groups_Project",
            Order = 1)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string ProjectSpaceGuid { get; set; }

        private bool IsProjectSpaceGuidVisible() => string.IsNullOrWhiteSpace(ProjectFile);

        [ActivationCondition(ActivationMethod = nameof(IsProjectSpaceGuidVisible))]
        [Display(
            Name = "ExecuteProjectFlowBlock_ProjectSpaceVersion",
            Description = "ExecuteProjectFlowBlock_ProjectSpaceVersion_Description",
            ResourceType = typeof(FlowBloxTexts),
            GroupName = "ExecuteProjectFlowBlock_Groups_Project",
            Order = 2)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public int? ProjectSpaceVersion { get; set; }

        [Display(
            Name = "ExecuteProjectFlowBlock_AbortOnError",
            Description = "ExecuteProjectFlowBlock_AbortOnError_Description",
            ResourceType = typeof(FlowBloxTexts),
            GroupName = "ExecuteProjectFlowBlock_Groups_Project",
            Order = 3)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public bool AbortOnError { get; set; } = true;

        [Display(
            Name = "ExecuteProjectFlowBlock_AbortOnWarning",
            Description = "ExecuteProjectFlowBlock_AbortOnWarning_Description",
            ResourceType = typeof(FlowBloxTexts),
            GroupName = "ExecuteProjectFlowBlock_Groups_Project",
            Order = 4)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public bool AbortOnWarning { get; set; } = false;

        [Display(
            Name = "ExecuteProjectFlowBlock_ParameterMappings",
            Description = "ExecuteProjectFlowBlock_ParameterMappings_Description",
            ResourceType = typeof(FlowBloxTexts),
            GroupName = "ExecuteProjectFlowBlock_Groups_Parameters",
            Order = 0)]
        [FlowBlockUI(Factory = UIFactory.GridView, DisplayLabel = false)]
        [FlowBlockDataGrid(IsMovable = true)]
        public ObservableCollection<ExecuteProjectParameterMappingEntry> ParameterMappings { get; set; } = new();

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.play_box_outline, 16, SKColors.MediumPurple);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.play_box_outline, 32, SKColors.MediumPurple);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;
        public override FlowBlockCategory GetCategory() => FlowBlockCategory.ControlFlow;

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                if (string.IsNullOrWhiteSpace(ProjectSpaceGuid) && string.IsNullOrWhiteSpace(ProjectFile))
                    throw new ValidationException("Either the project file or a Project Space GUID must be specified.");

                // If a version is provided, a ProjectSpaceGuid must also be set.
                if (ProjectSpaceVersion.HasValue && string.IsNullOrWhiteSpace(ProjectSpaceGuid))
                    throw new ValidationException("ProjectSpaceGuid must be set when ProjectSpaceVersion is provided.");

                var cmd = RunnerHostResolver.Resolve();

                var workDir = CreateWorkingDirectory();
                var requestFile = Path.Combine(workDir, "request.json");
                var responseFile = Path.Combine(workDir, "response.json");

                var request = BuildRunnerRequest();

                WriteJson(requestFile, request);

                _ = StartRunnerHost(cmd, requestFile, responseFile);

                if (!File.Exists(responseFile))
                    throw new InvalidOperationException($"RunnerHost did not create a response file: {responseFile}");

                var responseJson = File.ReadAllText(responseFile);
                if (string.IsNullOrWhiteSpace(responseJson))
                    throw new InvalidOperationException("Runner response file is empty.");

                var response = JsonSerializer.Deserialize<Runner.Contracts.RunnerResponse>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response == null)
                    throw new InvalidOperationException("Runner response could not be deserialized.");

                if (!response.Success)
                {
                    runtime.Report($"Child project execution failed (ExitCode={response.ExitCode}).", FlowBloxLogLevel.Error);
                    if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                        runtime.Report(response.ErrorMessage, FlowBloxLogLevel.Error);

                    runtime.Aborted = true;
                    return;
                }

                GenerateResult(runtime, responseJson);
            });
        }

        private static string CreateWorkingDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), "FlowBloxRunner", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private FlowBlox.Core.Runner.Contracts.RunnerRequest BuildRunnerRequest()
        {
            var userFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var optionOverrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in ParameterMappings ?? Enumerable.Empty<ExecuteProjectParameterMappingEntry>())
            {
                if (entry == null)
                    continue;

                if (string.IsNullOrWhiteSpace(entry.TargetKey))
                    throw new ValidationException("TargetKey must not be empty in parameter mappings.");

                if (string.IsNullOrWhiteSpace(entry.TargetValue))
                    throw new ValidationException($"TargetValue must not be empty in parameter mappings for '{entry.TargetKey}'.");

                var value = FlowBloxFieldHelper.ReplaceFieldsInString(entry.TargetValue);

                if (entry.TargetType == ExecuteProjectTargetType.UserField)
                    userFields[entry.TargetKey] = value;
                else
                    optionOverrides[entry.TargetKey] = value;
            }

            return new FlowBlox.Core.Runner.Contracts.RunnerRequest
            {
                ProjectFile = FlowBloxFieldHelper.ReplaceFieldsInString(ProjectFile),

                // ProjectSpace support
                ProjectSpaceGuid = string.IsNullOrWhiteSpace(ProjectSpaceGuid)
                    ? null
                    : FlowBloxFieldHelper.ReplaceFieldsInString(ProjectSpaceGuid),

                ProjectSpaceVersion = ProjectSpaceVersion,

                AutoRestart = false, // hard requirement
                AbortOnError = AbortOnError,
                AbortOnWarning = AbortOnWarning,
                UserFields = userFields,
                OptionOverrides = optionOverrides
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
            return proc.ExitCode;
        }

        private static void WriteJson(string path, object obj)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}