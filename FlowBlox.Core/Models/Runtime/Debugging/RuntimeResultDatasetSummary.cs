using System.Text.Json.Serialization;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeResultDatasetSummary
    {
        public RuntimeResultDatasetSummary(BaseResultFlowBlock flowBlock, int datasetCount)
        {
            FlowBlock = flowBlock;
            DatasetCount = datasetCount;
        }

        [JsonIgnore]
        public BaseResultFlowBlock FlowBlock { get; }
        public int DatasetCount { get; }
    }
}
