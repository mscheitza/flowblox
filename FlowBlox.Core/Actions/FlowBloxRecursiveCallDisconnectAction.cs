using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.ControlFlow;

namespace FlowBlox.Core.Actions
{
    public class FlowBloxRecursiveCallDisconnectAction : FlowBloxBaseAction
    {
        public RecursiveCallFlowBlock RecursiveCallFlowBlock { get; set; }

        public BaseFlowBlock TargetFlowBlock { get; set; }

        public override void Undo()
        {
            if (RecursiveCallFlowBlock != null)
                RecursiveCallFlowBlock.TargetFlowBlock = TargetFlowBlock;

            base.Undo();
        }

        public override void Invoke()
        {
            if (RecursiveCallFlowBlock != null)
                RecursiveCallFlowBlock.TargetFlowBlock = null;

            base.Invoke();
        }
    }
}
