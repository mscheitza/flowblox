using FlowBlox.AIAssistant.Builder;

namespace FlowBlox.AIAssistant.Models
{
    public sealed class AiAssistantHistoryDocument
    {
        public Guid HistoryGuid { get; set; } = Guid.NewGuid();
        public Guid ProjectGuid { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
        public string LastProjectJsonHash { get; set; } = string.Empty;
        public string ConversationSummary { get; set; } = string.Empty;
        public int SummarizedMessageCount { get; set; }
        public int EstimatedUsedTokens { get; set; }
        public List<AssistantSessionMessage> SessionMessages { get; set; } = new();
        public List<AssistantTranscriptLine> Transcripts { get; set; } = new();
    }
}