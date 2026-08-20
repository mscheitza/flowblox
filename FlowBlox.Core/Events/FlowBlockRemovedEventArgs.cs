using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Events
{
    public class FlowBlockRemovedEventArgs : EventArgs
    {
        public BaseFlowBlock RemovedFlowBlock { get; }

        public FlowBlockRemovedEventArgs(BaseFlowBlock removedFlowBlock)
        {
            RemovedFlowBlock = removedFlowBlock;
        }
    }
}
