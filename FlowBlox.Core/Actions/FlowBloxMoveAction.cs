using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Drawing;

namespace FlowBlox.Core.Actions
{
    public class FlowBloxMoveAction : FlowBloxBaseAction
    {
        public BaseFlowBlock FlowBlock { get; set; }

        public Point From { get; set; }

        public Point To { get; set; }

        public override void Undo()
        {
            if (FlowBlock != null)
                FlowBlock.Location = From;

            base.Undo();
        }

        public override void Invoke()
        {
            if (FlowBlock != null)
                FlowBlock.Location = To;

            base.Invoke();
        }
    }
}
