using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Tools;

namespace FlowBloxTest.Services
{
    internal sealed class CancelingToolApi : IFlowBloxAIToolApi
    {
        public Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct)
        {
            throw new OperationCanceledException();
        }

        public IReadOnlyList<ToolDefinition> GetToolDefinitions()
        {
            return Array.Empty<ToolDefinition>();
        }
    }
}