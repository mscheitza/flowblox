namespace FlowBlox.AIAssistant.Models
{
    public class AssistantResult
    {
        public bool Success { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string AssistantText { get; set; } = string.Empty;
        public string RawModelOutput { get; set; } = string.Empty;
        public List<AssistantTranscriptLine> TranscriptLines { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
