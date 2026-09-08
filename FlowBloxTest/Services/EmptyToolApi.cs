using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;

namespace FlowBloxTest.Services
{
    internal sealed class EmptyToolApi : IFlowBloxAIToolApi
    {
        public Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct)
        {
            return Task.FromResult(new ToolResponse { Ok = true });
        }

        public IReadOnlyList<ToolDefinition> GetToolDefinitions()
        {
            return Array.Empty<ToolDefinition>();
        }
    }
}