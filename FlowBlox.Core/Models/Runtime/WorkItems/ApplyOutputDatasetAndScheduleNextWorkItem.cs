using FlowBlox.Core.Models.FlowBlocks.Base;
using global::FlowBlox.Core.Models.FlowBlocks.Additions;

namespace FlowBlox.Core.Models.Runtime.WorkItems
{
    internal sealed class ApplyOutputDatasetAndScheduleNextWorkItem : IRuntimeWorkItem
    {
        private readonly BaseResultFlowBlock _resultBlock;
        private readonly FlowBlockOutDataset _dataset;

        public ApplyOutputDatasetAndScheduleNextWorkItem(BaseResultFlowBlock resultBlock, FlowBlockOutDataset dataset)
        {
            _resultBlock = resultBlock;
            _dataset = dataset;
        }

        public void Run(BaseRuntime runtime)
        {
            _resultBlock.OutputDataset_CurrentlyProcessing = _dataset;

            foreach (var fieldValueMapping in _dataset.FieldValueMappings)
            {
                fieldValueMapping.Field.SetValue(runtime, fieldValueMapping.Value);
            }

            runtime.TaskRunner.ScheduleNext(_resultBlock);
        }
    }
}