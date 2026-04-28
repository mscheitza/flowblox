namespace FlowBlox.AIAssistant.Models
{
    public sealed class FlowBlocksConnectionsChangedEventArgs : EventArgs
    {
        public IReadOnlyList<FlowBlockConnectionChange> Changes { get; init; } = Array.Empty<FlowBlockConnectionChange>();

        public bool HasChanges => Changes.Count > 0;
    }
}
