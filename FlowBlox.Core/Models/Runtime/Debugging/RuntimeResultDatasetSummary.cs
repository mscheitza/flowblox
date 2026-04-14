using System.Text.Json.Serialization;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeResultDatasetSummary
    {
        public RuntimeResultDatasetSummary(BaseResultFlowBlock flowBlock, IReadOnlyList<FlowBlockOutDataset> datasets)
        {
            FlowBlock = flowBlock;
            Datasets = datasets ?? Array.Empty<FlowBlockOutDataset>();
        }

        [JsonIgnore]
        public BaseResultFlowBlock FlowBlock { get; }

        [JsonIgnore]
        public IReadOnlyList<FlowBlockOutDataset> Datasets { get; }

        public int DatasetCount => Datasets?.Count ?? 0;
    }
}
