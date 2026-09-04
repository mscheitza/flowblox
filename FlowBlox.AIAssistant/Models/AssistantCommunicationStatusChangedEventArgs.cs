namespace FlowBlox.AIAssistant.Models
{
    public sealed class AssistantCommunicationStatusChangedEventArgs : EventArgs
    {
        public AssistantCommunicationStatusChangedEventArgs(string text)
        {
            Text = text?.Trim() ?? string.Empty;
        }

        public string Text { get; }
        public bool IsVisible => !string.IsNullOrWhiteSpace(Text);

        public static AssistantCommunicationStatusChangedEventArgs Hidden { get; } = new(string.Empty);
    }
}