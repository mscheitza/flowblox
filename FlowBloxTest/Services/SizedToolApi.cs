using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;
using Newtonsoft.Json.Linq;

namespace FlowBloxTest.Services
{
    internal sealed class SizedToolApi : IFlowBloxAIToolApi
    {
        public const int TypeInfoResponseLength = 5000;
        private const int CreateFlowBlockResponseLength = 1500;

        public List<ToolRequest> Requests { get; } = new();

        public Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct)
        {
            Requests.Add(request);

            var fillerLength = string.Equals(request.ToolName, "GetTypeKindsInfo", StringComparison.OrdinalIgnoreCase)
                ? TypeInfoResponseLength
                : CreateFlowBlockResponseLength;

            return Task.FromResult(new ToolResponse
            {
                Ok = true,
                Result = new JObject
                {
                    ["toolName"] = request.ToolName,
                    ["arguments"] = request.Arguments ?? new JObject(),
                    ["payload"] = new string('x', fillerLength)
                }
            });
        }

        public IReadOnlyList<ToolDefinition> GetToolDefinitions()
        {
            return
            [
                new ToolDefinition
                {
                    Name = "GetTypeKindsInfo",
                    Description = "Returns type metadata.",
                    ArgumentsSchema = new JObject
                    {
                        ["typeFullName"] = "string"
                    }
                },
                new ToolDefinition
                {
                    Name = "CreateFlowBlock",
                    Description = "Creates a flow block.",
                    ArgumentsSchema = new JObject
                    {
                        ["typeFullName"] = "string",
                        ["name"] = "string"
                    }
                }
            ];
        }
    }
}