namespace FlowBlox.AIAssistant.Models
{
    public sealed class FlowBlocksLayoutChangedEventArgs : EventArgs
    {
        public int UpdatedFlowBlocks { get; init; }
        public int TotalFlowBlocks { get; init; }
        public int ComponentsProcessed { get; init; }
    }
}
