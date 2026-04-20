using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.ShellExecution;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class ExecuteInputFileCommandHandler : ToolHandlerBase
    {
        public override string Name => "ExecuteInputFileCommand";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Executes the script command configured on a managed input file (resolved in $Project::InputDirectory). " +
            "Use this to load required project inputs (for example a Python script that downloads ONNX models into the project input directory). " +
            "Script creation/update is always allowed; execution requires explicit user confirmation in the AI Assistant UI.",
            new JObject
            {
                ["key"] = "string (required, input file relative path under $Project::InputDirectory)",
                ["usageHint"] =
                    "Use this only for input-file commands configured via CreateOrUpdateInputFile. " +
                    "The following placeholders in command are resolved: $InputFile::Path, $InputFile::RelativePath"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            try
            {
                var project = ToolHandlerUtilities.GetProject();
                project.InputFiles ??= new List<FlowBloxInputFile>();

                var key = (args.Value<string>("key") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(key))
                    return Task.FromResult(ToolHandlerUtilities.Fail("key is required."));

                FlowBloxInputFileHelper.ValidateRelativePathOrThrow(key);
                var normalizedKey = FlowBloxInputFileHelper.NormalizeRelativePath(key);

                var inputFile = project.InputFiles.FirstOrDefault(x =>
                    string.Equals(
                        FlowBloxInputFileHelper.NormalizeRelativePath(x?.RelativePath ?? string.Empty),
                        normalizedKey,
                        StringComparison.OrdinalIgnoreCase));

                if (inputFile == null)
                    return Task.FromResult(ToolHandlerUtilities.Fail($"Input file '{normalizedKey}' was not found."));

                var rawCommand = inputFile.Command ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rawCommand))
                    return Task.FromResult(ToolHandlerUtilities.Fail($"Input file '{normalizedKey}' has no command configured."));

                // Keep input files synchronized before script execution (same behavior as runtime startup path).
                FlowBloxInputFileHelper.SynchronizeInputFiles(project);

                var resolvedCommand = FlowBloxInputFileHelper.ReplaceInputFilePlaceholders(rawCommand, project, inputFile);
                resolvedCommand = FlowBloxFieldHelper.ReplaceFieldsInString(resolvedCommand ?? string.Empty);
                if (string.IsNullOrWhiteSpace(resolvedCommand))
                    return Task.FromResult(ToolHandlerUtilities.Fail($"Resolved command is empty for input file '{normalizedKey}'."));

                var result = FlowBloxShellExecutor.Execute(new FlowBloxShellExecutionRequest
                {
                    Command = resolvedCommand,
                    WorkingDirectory = project.ProjectInputDirectory,
                    CancellationToken = ct
                });

                var payload = new JObject
                {
                    ["key"] = normalizedKey,
                    ["workingDirectory"] = project.ProjectInputDirectory ?? string.Empty,
                    ["command"] = resolvedCommand,
                    ["success"] = result.Success,
                    ["exitCode"] = result.ExitCode,
                    ["standardOutput"] = result.StandardOutput ?? string.Empty,
                    ["standardError"] = result.StandardError ?? string.Empty,
                    ["exceptionMessage"] = result.ExceptionMessage ?? string.Empty
                };

                if (!result.Success)
                {
                    var error = !string.IsNullOrWhiteSpace(result.ExceptionMessage)
                        ? result.ExceptionMessage
                        : $"Command failed for '{normalizedKey}' (ExitCode={result.ExitCode}).";

                    return Task.FromResult(ToolHandlerUtilities.Fail(error, payload));
                }

                return Task.FromResult(ToolHandlerUtilities.Ok(payload));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(ex.Message));
            }
        }
    }
}
