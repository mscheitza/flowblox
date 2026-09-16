using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Models.Runtime.WorkItems
{
    internal sealed class CompleteOutputDatasetProcessingWorkItem : IRuntimeWorkItem
    {
        private readonly BaseResultFlowBlock _resultBlock;

        public CompleteOutputDatasetProcessingWorkItem(BaseResultFlowBlock resultBlock)
        {
            _resultBlock = resultBlock;
        }

        public void Run(BaseRuntime runtime)
        {
            _resultBlock.CompleteOutputDatasetProcessing();
        }
    }
}
