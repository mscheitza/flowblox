using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.AIAssistant.Models
{
    public class FlowBlocksChangedEventArgs : EventArgs
    {
        public IReadOnlyList<BaseFlowBlock> AddedFlowBlocks { get; init; } = Array.Empty<BaseFlowBlock>();
        public IReadOnlyList<string> RemovedFlowBlockNames { get; init; } = Array.Empty<string>();

        public bool HasChanges => AddedFlowBlocks.Count > 0 || RemovedFlowBlockNames.Count > 0;
    }
}
