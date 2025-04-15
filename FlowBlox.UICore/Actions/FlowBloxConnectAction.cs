using FlowBlox.Core.Models.FlowBlocks.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.UICore.Actions
{
    public class FlowBloxConnectAction : FlowBloxBaseAction
    {
        public BaseFlowBlock From { get; set; }

        public BaseFlowBlock To { get; set; }

        public override void Undo()
        {
            var referencedFlowBlocks = To.ReferencedFlowBlocks;

            if (referencedFlowBlocks.Contains(From))
                referencedFlowBlocks.Remove(From);

            To.ReferencedFlowBlocks = referencedFlowBlocks;

            base.Undo();
        }

        public override void Invoke()
        {
            var referencedFlowBlocks = To.ReferencedFlowBlocks;

            if (!referencedFlowBlocks.Contains(From))
                referencedFlowBlocks.Add(From);

            To.ReferencedFlowBlocks = referencedFlowBlocks;

            base.Invoke();
        }
    }
}
