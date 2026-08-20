using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Actions
{
    public class FlowBloxDeleteAction : FlowBloxBaseAction
    {
        public BaseFlowBlock FlowBlock { get; set; }

        public override void Undo()
        {
            FlowBloxRegistryProvider.GetRegistry().RegisterFlowBlock(FlowBlock);
            base.Undo();
        }

        public override void Invoke()
        {
            FlowBloxRegistryProvider.GetRegistry().RemoveFlowBlock(FlowBlock);
            base.Invoke();
        }
    }
}
