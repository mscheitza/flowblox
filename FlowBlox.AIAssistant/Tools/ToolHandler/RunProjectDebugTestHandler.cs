using System.Diagnostics;
using System.Text.Json;
using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Runtime.Debugging;
using FlowBlox.Core.Runner.Contracts;
using FlowBlox.Core.Util;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class RunProjectDebugTestHandler : ToolHandlerBase
    {
        public override string Name => "RunProjectDebugTest";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Runs the current project in RunnerHost debug mode and returns runtime protocol output. " +
            "FieldValueChanges and GeneratedResults must be read via GetLastDebugArtefact. " +
            "If a target is set, runtime always stops at target: default is stopping before target execution. Set includeTargetExecution=true to execute target once and then stop.",
            new JObject
            {
                ["targetFlowBlockName"] = "string?",
                ["maxRuntimeSeconds"] = "int? (default: 30)",
                ["includeTargetExecution"] = "bool? (default: false, only relevant when target is set)",
                ["maxCapturedFieldValueChanges"] = "int? (default: 100)",
                ["maxFieldValueLength"] = "int? (default: 2000)",
                ["maxProtocolEntries"] = "int? (default: 250)",
                ["usageHint"] = "Use this call for protocol output only. Read FieldValueChanges/GeneratedResults via GetLastDebugArtefact. Prefer one optimistic debug run first (usually includeTargetExecution=false). Tune maxProtocolEntries as needed."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            try
            {
                var project = ToolHandlerUtilities.GetProject();
                project.RefreshOrderedTopLevelCollectionsForSerialization();

                var maxRuntimeSeconds = Math.Max(1, args.Value<int?>("maxRuntimeSeconds") ?? 30);
                var includeTargetExecution = args.Value<bool?>("includeTargetExecution") ?? false;
                var targetFlowBlockName = (args.Value<string>("targetFlowBlockName") ?? string.Empty).Trim();
                var maxCapturedChanges = Math.Max(0, args.Value<int?>("maxCapturedFieldValueChanges") ?? 100);
                var maxFieldValueLength = Math.Max(1, args.Value<int?>("maxFieldValueLength") ?? 2000);
                var maxProtocolEntries = Math.Max(
                    1,
                    args.Value<int?>("maxProtocolEntries")
                    ?? args.Value<int?>("maxProtocolPreviewEntries")
                    ?? 250);

                if (!string.IsNullOrWhiteSpace(targetFlowBlockName))
                {
                    var exists = project.FlowBlocks.Any(x =>
                        string.Equals(x.Name, targetFlowBlockName, StringComparison.OrdinalIgnoreCase));
                    if (!exists)
                        return Task.FromResult(ToolHandlerUtilities.Fail($"Target flow block '{targetFlowBlockName}' does not exist in current project."));
                }

                var workDir = Path.Combine(Path.GetTempPath(), "FlowBloxAiAssistantDebug", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(workDir);

                var projectFile = Path.Combine(workDir, "project.fbprj");
                var requestFile = Path.Combine(workDir, "request.json");
                var responseFile = Path.Combine(workDir, "response.json");
                var debuggingResultFile = Path.Combine(workDir, "debugging-result.json");

                project.Save(projectFile);

                var runnerRequest = new RunnerRequest
                {
                    ProjectFile = projectFile,
                    AutoRestart = false,
                    AbortOnError = false,
                    AbortOnWarning = false,
                    ExternalDebuggingInformation = new RunnerExternalDebuggingInformation
                    {
                        MaxRuntimeSeconds = maxRuntimeSeconds,
                        TargetFlowBlockName = string.IsNullOrWhiteSpace(targetFlowBlockName) ? null : targetFlowBlockName,
                        IncludeTargetExecution = includeTargetExecution,
                        MaxCapturedFieldValueChanges = maxCapturedChanges,
                        MaxFieldValueLength = maxFieldValueLength,
                        DebuggingResultFilePath = debuggingResultFile
                    }
                };

                WriteJson(requestFile, runnerRequest);

                var cmd = RunnerHostResolver.Resolve();
                var exitCode = StartRunnerHost(cmd, requestFile, responseFile);

                if (!File.Exists(responseFile))
                    return Task.FromResult(ToolHandlerUtilities.Fail($"RunnerHost did not create response file: {responseFile}"));

                var responseJson = File.ReadAllText(responseFile);
                var response = JsonSerializer.Deserialize<RunnerResponse>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response == null)
                    return Task.FromResult(ToolHandlerUtilities.Fail("RunnerResponse could not be deserialized."));

                JObject debuggingResult;
                if (File.Exists(debuggingResultFile))
                {
                    var debuggingJson = File.ReadAllText(debuggingResultFile);
                    debuggingResult = string.IsNullOrWhiteSpace(debuggingJson)
                        ? new JObject()
                        : JObject.Parse(debuggingJson);
                }
                else
                {
                    debuggingResult = new JObject();
                }

                var runId = Guid.NewGuid().ToString("N");
                AiAssistantDebugRunState.Set(new AiAssistantDebugRunSnapshot
                {
                    RunId = runId,
                    CreatedUtc = DateTime.UtcNow,
                    DebuggingResultFilePath = debuggingResultFile,
                    DebuggingResult = debuggingResult
                });

                var protocol = GetArrayIgnoreCase(debuggingResult, "Protocol", "protocol");
                var warnings = GetArrayIgnoreCase(debuggingResult, "Warnings", "warnings");
                var errors = GetArrayIgnoreCase(debuggingResult, "Errors", "errors");
                var protocolLimited = BuildProtocol(protocol, maxProtocolEntries);

                var payload = new JObject
                {
                    ["runId"] = runId,
                    ["runnerExitCode"] = exitCode,
                    ["success"] = response.Success,
                    ["responseExitCode"] = response.ExitCode,
                    ["errorMessage"] = response.ErrorMessage ?? string.Empty,
                    ["exception"] = response.Exception ?? string.Empty,
                    ["cancellationKind"] = response.CancellationKind?.ToString() ?? string.Empty,
                    ["cancellationReason"] = response.CancellationReason ?? string.Empty,
                    ["includeTargetExecution"] = includeTargetExecution,
                    ["debuggingResultFilePath"] = response.DebuggingResultFilePath ?? debuggingResultFile,
                    ["protocolEntryCount"] = protocolLimited.Count,
                    ["protocolEntryCountTotal"] = protocol.Count,
                    ["maxProtocolEntries"] = maxProtocolEntries,
                    ["warningCount"] = warnings.Count,
                    ["errorCount"] = errors.Count,
                    ["protocol"] = protocolLimited,
                    ["outputSummary"] = BuildOutputSummary(response)
                };

                if (response.Success || response.CancellationKind == RuntimeCancellationKind.DebuggingTargetReached)
                    return Task.FromResult(ToolHandlerUtilities.Ok(payload));

                return Task.FromResult(ToolHandlerUtilities.Fail("Debug test run failed.", payload));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail(ex.Message));
            }
        }

        private static JArray BuildProtocol(JArray protocol, int maxEntries)
        {
            var result = new JArray();
            foreach (var entry in protocol.Take(maxEntries))
            {
                result.Add(entry);
            }

            return result;
        }

        private static JArray GetArrayIgnoreCase(JObject root, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (root[key] is JArray direct)
                    return direct;
            }

            var property = root.Properties()
                .FirstOrDefault(x => keys.Any(k => string.Equals(k, x.Name, StringComparison.OrdinalIgnoreCase)));

            return property?.Value as JArray ?? new JArray();
        }

        private static JObject BuildOutputSummary(RunnerResponse response)
        {
            var outputs = new JObject();
            foreach (var kvp in response.Outputs)
            {
                outputs[kvp.Key] = kvp.Value?.Count ?? 0;
            }

            return outputs;
        }

        private static int StartRunnerHost(RunnerHostResolver.RunnerHostCommand runnerHostCommand, string requestFile, string responseFile)
        {
            var psi = new ProcessStartInfo
            {
                FileName = runnerHostCommand.FileName,
                Arguments = runnerHostCommand.BuildArguments(requestFile, responseFile),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null)
                throw new InvalidOperationException("RunnerHost process could not be started.");

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
