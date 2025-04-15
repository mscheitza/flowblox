using FlowBlox.Core.Models.FlowBlocks;
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
    public class FlowBloxInvokeAction : FlowBloxBaseAction
    {
        private BaseFlowBlock _recentTargetFlowBlock;

        public InvokerFlowBlock From { get; set; }

        public BaseFlowBlock To { get; set; }

        public override void Undo()
        {
            From.TargetFlowBlock = _recentTargetFlowBlock;
            base.Undo();
        }

        public override void Invoke()
        {
            _recentTargetFlowBlock = From.TargetFlowBlock;
            From.TargetFlowBlock = To;
            base.Invoke();
        }
    }
}
