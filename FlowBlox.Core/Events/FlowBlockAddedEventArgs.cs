using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Events
{
    public class FlowBlockAddedEventArgs : EventArgs
    {
        public BaseFlowBlock AddedFlowBlock { get; }

        public FlowBlockAddedEventArgs(BaseFlowBlock addedFlowBlock)
        {
            AddedFlowBlock = addedFlowBlock;
        }
    }
}
