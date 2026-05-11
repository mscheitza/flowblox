using System.Text.Json.Serialization;
using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeInputProcessingSummary
    {
        public RuntimeInputProcessingSummary(
            BaseFlowBlock flowBlock,
            BaseFlowBlock iterationContext,
            int inputDatasetCount,
            IReadOnlyDictionary<string, Enum> inputBehaviours)
        {
            FlowBlock = flowBlock;
            IterationContext = iterationContext;
            InputDatasetCount = Math.Max(0, inputDatasetCount);
            InputBehaviours = inputBehaviours ?? new Dictionary<string, Enum>();
        }

        [JsonIgnore]
        public BaseFlowBlock FlowBlock { get; }

        [JsonIgnore]
        public BaseFlowBlock IterationContext { get; }

        public int InputDatasetCount { get; }

        public IReadOnlyDictionary<string, Enum> InputBehaviours { get; }
    }
}
