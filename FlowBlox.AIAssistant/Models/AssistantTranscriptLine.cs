namespace FlowBlox.AIAssistant.Models
{
    public enum AssistantTranscriptKind
    {
        User,
        Assistant,
        Error,
        Status
    }

    public class AssistantTranscriptLine
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Text { get; set; } = string.Empty;
        public AssistantTranscriptKind Kind { get; set; } = AssistantTranscriptKind.Status;
    }
}
