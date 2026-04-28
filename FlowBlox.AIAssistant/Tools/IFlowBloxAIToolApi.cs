using FlowBlox.AIAssistant.Models;

namespace FlowBlox.AIAssistant.Tools
{
    public interface IFlowBloxAIToolApi
    {
        event EventHandler<FlowBlocksConnectionsChangedEventArgs>? FlowBlocksConnectionsChanged;
        Task<ToolResponse> ExecuteAsync(ToolRequest request, CancellationToken ct);
        IReadOnlyList<ToolDefinition> GetToolDefinitions();
    }
}
