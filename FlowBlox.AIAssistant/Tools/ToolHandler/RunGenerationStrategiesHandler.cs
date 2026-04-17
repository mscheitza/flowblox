using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.FlowBlocks.Base;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class RunGenerationStrategiesHandler : ToolHandlerBase
    {
        public override string Name => "RunGenerationStrategies";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Executes generation strategies for a specific flow block and returns protocol logs (status/message). " +
            "Use this to validate generator configuration without running full project debug execution.",
            new JObject
            {
                ["flowBlockName"] = "string",
                ["usageHint"] = "After generator configuration on a flow block, run this tool to generate values. Then verify generated target values via GetComponentSnapshot."
            });

        public override async Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            try
            {
                var flowBlockName = (args.Value<string>("flowBlockName") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(flowBlockName))
                {
                    return ToolHandlerUtilities.Fail("flowBlockName is required.");
                }

                var registry = ToolHandlerUtilities.GetRegistry();
                var flowBlock = ToolHandlerUtilities.ResolveFlowBlockByName(registry, flowBlockName);
                if (flowBlock == null)
                {
                    return ToolHandlerUtilities.Fail($"Flow block '{flowBlockName}' was not found.");
                }

                if (flowBlock.GenerationStrategies == null || flowBlock.GenerationStrategies.Count == 0)
                {
                    return ToolHandlerUtilities.Fail(
                        $"Flow block '{flowBlock.Name}' has no generation strategies configured.");
                }

                ct.ThrowIfCancellationRequested();

                var protocolEntries = new List<JObject>();
                var generationExecutor = new FlowBlockGenerationStrategyExecutor(flowBlock);

                void OnLogCreated(object? sender, LogCreatedEventArgs eventArgs)
                {
                    lock (protocolEntries)
                    {
                        protocolEntries.Add(new JObject
                        {
                            ["timestamp"] = DateTime.Now.ToString("O"),
                            ["status"] = eventArgs.LogLevel.ToString(),
                            ["message"] = eventArgs.Message ?? string.Empty
                        });
                    }
                }

                generationExecutor.LogCreated += OnLogCreated;
                bool success;
                try
                {
                    success = await generationExecutor.ExecuteGenerationAsync();
                }
                finally
                {
                    generationExecutor.LogCreated -= OnLogCreated;
                }

                ct.ThrowIfCancellationRequested();

                var protocol = new JArray(protocolEntries);
                var payload = new JObject
                {
                    ["flowBlockName"] = flowBlock.Name,
                    ["generationStrategyCount"] = flowBlock.GenerationStrategies.Count,
                    ["success"] = success,
                    ["protocolEntryCount"] = protocol.Count,
                    ["protocol"] = protocol
                };

                if (success)
                {
                    return ToolHandlerUtilities.Ok(payload);
                }

                return ToolHandlerUtilities.Fail("Generation strategies execution failed.", payload);
            }
            catch (OperationCanceledException)
            {
                return ToolHandlerUtilities.Fail("Generation strategies execution was cancelled.");
            }
            catch (Exception ex)
            {
                return ToolHandlerUtilities.Fail(ex.Message);
            }
        }
    }
}
