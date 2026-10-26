using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Actions
{
    public class FlowBloxDeleteAction : FlowBloxBaseAction
    {
        public BaseFlowBlock FlowBlock { get; set; }
        public BaseFlowBlock HistoryFlowBlock { get; set; }

        public override void Undo()
        {
            FlowBloxRegistryProvider.GetRegistry().RegisterFlowBlock(HistoryFlowBlock ?? FlowBlock);
            base.Undo();
        }

        public override void Invoke()
        {
            base.Invoke();
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var target = registry.GetFlowBlocks().Contains(FlowBlock)
                ? FlowBlock
                : HistoryFlowBlock ?? FlowBlock;
            registry.RemoveFlowBlock(target);
        }
    }
}
