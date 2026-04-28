using FlowBlox.Core.Actions;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Actions
{
    public class FlowBloxDisconnectAction : FlowBloxBaseAction
    {
        public BaseFlowBlock From { get; set; }

        public BaseFlowBlock To { get; set; }

        public override void Undo()
        {
            var referencedFlowBlocks = To.ReferencedFlowBlocks;

            if (!referencedFlowBlocks.Contains(From))
                referencedFlowBlocks.Add(From);

            To.ReferencedFlowBlocks = referencedFlowBlocks;

            base.Undo();
        }

        public override void Invoke()
        {
            var referencedFlowBlocks = To.ReferencedFlowBlocks;

            if (referencedFlowBlocks.Contains(From))
                referencedFlowBlocks.Remove(From);

            To.ReferencedFlowBlocks = referencedFlowBlocks;

            base.Invoke();
        }
    }
}
