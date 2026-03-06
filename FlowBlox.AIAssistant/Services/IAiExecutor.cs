using FlowBlox.AIAssistant.Models;

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
        Task<AiExecutorResult> ExecutePromptAsync(
            string systemPrompt,
            string userPrompt,
            AssistantConfiguration configuration,
            string previousResponseId,
            CancellationToken ct);
    }
}
