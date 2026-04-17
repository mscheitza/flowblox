using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Testing;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class RunTestDefinitionHandler : ToolHandlerBase
    {
        public override string Name => "RunTestDefinition";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Executes a FlowBlox test definition and returns runtime protocol plus serialized test results of the target flow block. " +
            "Use only when test-definition execution is explicitly requested or needed for generator/test debugging.",
            new JObject
            {
                ["testDefinitionName"] = "string",
                ["targetFlowBlockName"] = "string? (optional; if omitted, latest linked flow block is used)",
                ["synchronizeBeforeRun"] = "bool? (default: true)",
                ["maxProtocolPreviewEntries"] = "int? (default: 250)",
                ["usageHint"] = "Prefer RunGenerationStrategies first for generator work. Use this tool when test-definition execution or detailed test-result inspection is required."
            });

        public override async Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            try
            {
                var testDefinitionName = (args.Value<string>("testDefinitionName") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(testDefinitionName))
                    return ToolHandlerUtilities.Fail("testDefinitionName is required.");

                var targetFlowBlockName = (args.Value<string>("targetFlowBlockName") ?? string.Empty).Trim();
                var synchronizeBeforeRun = args.Value<bool?>("synchronizeBeforeRun") ?? true;
                var maxProtocolPreviewEntries = Math.Max(1, args.Value<int?>("maxProtocolPreviewEntries") ?? 250);

                var registry = ToolHandlerUtilities.GetRegistry();
                var testDefinition = ToolHandlerUtilities.ResolveManagedObjectByName(registry, testDefinitionName) as FlowBloxTestDefinition;
                if (testDefinition == null)
                    return ToolHandlerUtilities.Fail($"Test definition '{testDefinitionName}' was not found.");

                BaseFlowBlock? targetFlowBlock = null;
                if (!string.IsNullOrWhiteSpace(targetFlowBlockName))
                {
                    targetFlowBlock = ToolHandlerUtilities.ResolveFlowBlockByName(registry, targetFlowBlockName);
                    if (targetFlowBlock == null)
                        return ToolHandlerUtilities.Fail($"Flow block '{targetFlowBlockName}' was not found.");
                }

                if (synchronizeBeforeRun)
                {
                    var synchronizer = new FlowBloxTestDefinitionSynchronizer();
                    synchronizer.Synchronize(testDefinition, targetFlowBlock);
                }

                var latestResolver = new FlowBloxTestDefinitionLatestFlowBlockResolver();
                var effectiveTargetFlowBlock = targetFlowBlock ?? latestResolver.ResolveLatestFlowBlock(testDefinition);
                if (effectiveTargetFlowBlock == null)
                    return ToolHandlerUtilities.Fail(
                        $"Test definition '{testDefinition.Name}' has no linked flow block. " +
                        "Provide targetFlowBlockName or link the test definition to a flow block first.");

                var includedFlowBlocks = testDefinition.Entries
                    .Select(x => x.FlowBlock)
                    .Where(x => x != null)
                    .Cast<BaseFlowBlock>()
                    .Distinct()
                    .ToList();

                var protocolEntries = new List<JObject>();
                var testExecutor = new FlowBloxTestExecutor();
                FlowBloxTestResult? testResult = null;
                bool initialized = false;
                BaseFlowBlock? resultSnapshotFlowBlock = null;

                void OnLogCreated(FlowBlox.Core.Models.Runtime.BaseRuntime runtime, string message, FlowBlox.Core.Enums.FlowBloxLogLevel logLevel)
                {
                    lock (protocolEntries)
                    {
                        protocolEntries.Add(new JObject
                        {
                            ["timestamp"] = DateTime.Now.ToString("O"),
                            ["status"] = logLevel.ToString(),
                            ["message"] = message ?? string.Empty
                        });
                    }
                }

                try
                {
                    ct.ThrowIfCancellationRequested();

                    testExecutor.Initialize(testDefinition, effectiveTargetFlowBlock, includedFlowBlocks);
                    initialized = true;
                    resultSnapshotFlowBlock = effectiveTargetFlowBlock;

                    var runtime = testExecutor.GetRuntime();
                    runtime.LogMessageCreated += OnLogCreated;
                    try
                    {
                        testResult = await testExecutor.ExecuteTestAsync();
                    }
                    finally
                    {
                        runtime.LogMessageCreated -= OnLogCreated;
                    }
                }
                finally
                {
                    if (initialized)
                    {
                        try
                        {
                            testExecutor.Shutdown();
                        }
                        catch
                        {
                        }
                    }
                }

                ct.ThrowIfCancellationRequested();

                var protocol = new JArray(protocolEntries);
                var filteredFieldValueAssignments = (testResult?.FieldValueAssignments ?? new Dictionary<string, string>())
                    .Where(x => !x.Key.StartsWith("$user::", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

                var payload = new JObject
                {
                    ["testDefinitionName"] = testDefinition.Name,
                    ["targetFlowBlockName"] = effectiveTargetFlowBlock.Name,
                    ["synchronizedBeforeRun"] = synchronizeBeforeRun,
                    ["success"] = testResult?.Success ?? false,
                    ["protocolEntryCount"] = protocol.Count,
                    ["maxProtocolPreviewEntries"] = maxProtocolPreviewEntries,
                    ["protocol"] = protocol,
                    ["protocolPreview"] = new JArray(protocol.Take(maxProtocolPreviewEntries)),
                    ["fieldValueAssignments"] = JObject.FromObject(filteredFieldValueAssignments),
                    ["testResults"] = BuildTestResults(resultSnapshotFlowBlock)
                };

                if (testResult?.Success == true)
                    return ToolHandlerUtilities.Ok(payload);

                return ToolHandlerUtilities.Fail("Test definition execution failed.", payload);
            }
            catch (OperationCanceledException)
            {
                return ToolHandlerUtilities.Fail("Test definition execution was cancelled.");
            }
            catch (Exception ex)
            {
                return ToolHandlerUtilities.Fail(ex.Message);
            }
        }

        private static JObject BuildTestResults(BaseFlowBlock? flowBlock)
        {
            if (flowBlock is not BaseResultFlowBlock resultFlowBlock)
            {
                return new JObject
                {
                    ["available"] = false,
                    ["reason"] = "Target flow block is not a BaseResultFlowBlock."
                };
            }

            var results = resultFlowBlock.GridElementResult?.Results ?? new List<FlowBlockOutDataset>();
            if (results.Count == 0)
            {
                return new JObject
                {
                    ["available"] = true,
                    ["resultCount"] = 0,
                    ["columns"] = new JArray(),
                    ["rows"] = new JArray()
                };
            }

            var columns = results
                .First()
                .FieldValueMappings
                .Select(x => x.Field?.Name ?? string.Empty)
                .ToList();

            var rows = new JArray();
            for (int i = 0; i < results.Count; i++)
            {
                var row = results[i];
                var values = new JArray(
                    row.FieldValueMappings.Select(mapping => new JObject
                    {
                        ["fieldName"] = mapping.Field?.Name ?? string.Empty,
                        ["fullyQualifiedFieldName"] = mapping.Field?.FullyQualifiedName ?? string.Empty,
                        ["value"] = mapping.Value ?? string.Empty
                    }));

                rows.Add(new JObject
                {
                    ["index"] = i,
                    ["values"] = values
                });
            }

            return new JObject
            {
                ["available"] = true,
                ["resultCount"] = results.Count,
                ["columns"] = new JArray(columns),
                ["rows"] = rows
            };
        }
    }
}
