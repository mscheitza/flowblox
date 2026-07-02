namespace FlowBlox.AIAssistant.Builder
{
    public sealed class AssistantConversationMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string PairId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
