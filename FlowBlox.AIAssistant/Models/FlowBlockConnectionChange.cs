namespace FlowBlox.AIAssistant.Models
{
    public enum FlowBlockConnectionChangeMode
    {
        Connect,
        Disconnect
    }

    public sealed class FlowBlockConnectionChange
    {
        public string From { get; init; } = string.Empty;
        public string To { get; init; } = string.Empty;
        public string LinkMode { get; init; } = string.Empty;
        public FlowBlockConnectionChangeMode Mode { get; init; }
    }
}
