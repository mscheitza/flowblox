using FlowBlox.AIAssistant.Models;

namespace FlowBlox.AIAssistant.Tools
{
    public interface IFlowBloxAIToolApi
    {
        Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct);
    }
}
