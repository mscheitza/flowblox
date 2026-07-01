using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.AIAssistant.Services
{
    public class AiExecutorResult
    {
        public bool Success { get; set; }
        public string OutputText { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public string RawOutput { get; set; } = string.Empty;
        public string ResponseId { get; set; } = string.Empty;
    }

    public interface IAiExecutor
    {
        Task<AiExecutorResult> ExecuteChatAsync(
            AIChatRequest request,
            AssistantConfiguration configuration,
            CancellationToken ct);
    }
}
