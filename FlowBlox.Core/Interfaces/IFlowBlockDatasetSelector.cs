using FlowBlox.Core.Models.FlowBlocks.Additions;

namespace FlowBlox.Core.Interfaces
{
    public interface IFlowBlockDatasetSelector
    {
        List<FlowBlockOutDataset> GetResults();
    }
}
