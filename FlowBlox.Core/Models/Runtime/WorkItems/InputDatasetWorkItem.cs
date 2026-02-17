using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Models.Runtime.WorkItems
{
    internal sealed class InputDatasetWorkItem : IRuntimeWorkItem
    {
        private readonly BaseFlowBlock _block;
        private readonly FlowBlockOutDataset _dataset;
        private readonly Action<BaseRuntime, BaseFlowBlock, FlowBlockOutDataset> _applyDatasetAndExecute;

        public InputDatasetWorkItem(
            BaseFlowBlock block,
            FlowBlockOutDataset dataset,
            Action<BaseRuntime, BaseFlowBlock, FlowBlockOutDataset> applyDatasetAndExecute)
        {
            _block = block;
            _dataset = dataset;
            _applyDatasetAndExecute = applyDatasetAndExecute;
        }

        public void Run(BaseRuntime runtime)
        {
            _applyDatasetAndExecute(runtime, _block, _dataset);
        }
    }
}
