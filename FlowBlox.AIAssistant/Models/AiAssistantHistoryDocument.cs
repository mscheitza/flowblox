namespace FlowBlox.AIAssistant.Models
{
    public sealed class AiAssistantHistoryDocument
    {
        public Guid HistoryGuid { get; set; } = Guid.NewGuid();
        public Guid ProjectGuid { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
        public int LastRound { get; set; }
        public string LastProjectJsonHash { get; set; } = string.Empty;
        public string ConversationSummary { get; set; } = string.Empty;
        public int SummarizedMessageCount { get; set; }
        public List<AssistantTranscriptLine> Transcripts { get; set; } = new();
    }
}
