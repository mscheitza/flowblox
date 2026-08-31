using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    public interface IAIProvider
    {
        string ProviderType { get; }

        Task<AIResponse> ExecuteChatAsync(AIChatRequest request, CancellationToken ct);

        Task<AIResponse> ExecuteChatAsync(BaseRuntime runtime, AIChatRequest request, CancellationToken ct);

        Task<AIResponse> ExecuteAsync(AIRequest request, CancellationToken ct);

        Task<AIResponse> ExecuteAsync(BaseRuntime runtime, AIRequest request, CancellationToken ct);
    }
}