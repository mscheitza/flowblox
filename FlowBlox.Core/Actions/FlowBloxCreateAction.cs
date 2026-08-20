using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Actions
{
    public class FlowBloxCreateAction : FlowBloxBaseAction
    {
        public BaseFlowBlock FlowBlock { get; set; }

        public override void Undo()
        {
            FlowBloxRegistryProvider.GetRegistry().RemoveFlowBlock(FlowBlock);
            base.Undo();
        }

        public override void Invoke()
        {
            FlowBloxRegistryProvider.GetRegistry().RegisterFlowBlock(FlowBlock);
            base.Invoke();
        }
    }
}
