using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    [Serializable]
    internal sealed class FlowBlockPassedResultBuffer
    {
        private readonly Dictionary<int, Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>>> _resultsByExecutionLayer = new();

        public void Add(BaseRuntime runtime, BaseResultFlowBlock sourceFlowBlock)
        {
            var results = GetResults(runtime);
            if (!results.ContainsKey(sourceFlowBlock))
                results.Add(sourceFlowBlock, new HashSet<FlowBlockOut>());

            var result = sourceFlowBlock.GridElementResult;
            if (!results[sourceFlowBlock].Contains(result))
                results[sourceFlowBlock].Add(result);
        }

        public Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>> GetResults(BaseRuntime runtime)
        {
            var executionLayer = runtime.ExecutionLayer;
            if (!_resultsByExecutionLayer.ContainsKey(executionLayer))
                _resultsByExecutionLayer.Add(executionLayer, new Dictionary<BaseFlowBlock, HashSet<FlowBlockOut>>());

            return _resultsByExecutionLayer[executionLayer];
        }

        public void Clear(BaseRuntime runtime)
        {
            GetResults(runtime).Clear();
        }
    }
}
