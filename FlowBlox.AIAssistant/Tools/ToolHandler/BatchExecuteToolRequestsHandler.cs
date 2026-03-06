using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class BatchExecuteToolRequestsHandler : ToolHandlerBase
    {
        private readonly Func<ToolRequest, CancellationToken, Task<ToolResponse>> _executeTool;

        public BatchExecuteToolRequestsHandler(Func<ToolRequest, CancellationToken, Task<ToolResponse>> executeTool)
        {
            _executeTool = executeTool;
        }

        public override string Name => "BatchExecuteToolRequests";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Executes multiple tool requests.",
            new JObject
            {
                ["continueOnError"] = "bool?",
                ["requests"] = "[{toolName,arguments}]"
            });

        public override async Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            if (args["requests"] is not JArray requests || !requests.Any())
            {
                return ToolHandlerUtilities.Fail("requests is required and must contain at least one entry.");
            }

            var continueOnError = args.Value<bool?>("continueOnError") ?? false;
            var allOk = true;
            var batchResults = new JArray();

            foreach (var request in requests.OfType<JObject>())
            {
                var toolName = request.Value<string>("toolName") ?? string.Empty;
                var toolArgs = request["arguments"] as JObject ?? new JObject();

                if (string.IsNullOrWhiteSpace(toolName)
                    || string.Equals(toolName, Name, StringComparison.OrdinalIgnoreCase))
                {
                    allOk = false;
                    batchResults.Add(new JObject
                    {
                        ["toolName"] = toolName,
                        ["ok"] = false,
                        ["error"] = string.IsNullOrWhiteSpace(toolName)
                            ? "toolName is required."
                            : "Nested batch not supported."
                    });

                    if (!continueOnError)
                    {
                        break;
                    }

                    continue;
                }

                var response = await _executeTool(
                        new ToolRequest
                        {
                            ToolName = toolName,
                            Arguments = toolArgs
                        },
                        ct)
                    .ConfigureAwait(false);

                allOk &= response.Ok;
                batchResults.Add(new JObject
                {
                    ["toolName"] = toolName,
                    ["ok"] = response.Ok,
                    ["error"] = response.Error,
                    ["result"] = response.Result
                });

                if (!response.Ok && !continueOnError)
                {
                    break;
                }
            }

            var payload = new JObject
            {
                ["allOk"] = allOk,
                ["batchResults"] = batchResults
            };

            if (allOk)
            {
                return ToolHandlerUtilities.Ok(payload);
            }

            return ToolHandlerUtilities.Fail("One or more batch requests failed.", payload);
        }
    }
}
