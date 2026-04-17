using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestCapture
    {
        private List<BaseFlowBlock> _capturedFlowBlocks;
        private FlowBloxRegistry _registry;

        public FlowBloxTestCapture()
        {
            _registry = FlowBloxRegistryProvider.GetRegistry();
        }

        public List<BaseFlowBlock> GetCapturedFlowBlocks() => _capturedFlowBlocks;

        public bool CreateCapture(BaseFlowBlock startFlowBlock, BaseFlowBlock targetFlowBlock)
        {
            if (startFlowBlock == null)
                throw new ArgumentNullException(nameof(startFlowBlock));

            if (targetFlowBlock == null)
                throw new ArgumentNullException(nameof(targetFlowBlock));

            _capturedFlowBlocks = new List<BaseFlowBlock>();
            var visited = new HashSet<BaseFlowBlock>();

            return CaptureFlowBlocks(startFlowBlock, targetFlowBlock, _capturedFlowBlocks, visited);
        }

        private bool CaptureFlowBlocks(
            BaseFlowBlock flowBlock,
            BaseFlowBlock targetFlowBlock,
            List<BaseFlowBlock> capturedFlowBlocks,
            HashSet<BaseFlowBlock> visited)
        {
            if (!visited.Add(flowBlock))
                return false;

            bool targetCaptured = false;

            foreach (var nextFlowBlock in flowBlock.GetNextFlowBlocks())
            {
                if (nextFlowBlock == targetFlowBlock)
                {
                    AddIfMissing(capturedFlowBlocks, nextFlowBlock);
                    targetCaptured = true;
                    continue;
                }

                if (nextFlowBlock.ReferencedFlowBlocks.Count == 1)
                {
                    AddIfMissing(capturedFlowBlocks, nextFlowBlock);

                    if (CaptureFlowBlocks(nextFlowBlock, targetFlowBlock, capturedFlowBlocks, visited))
                        targetCaptured = true;
                }
                else if (nextFlowBlock.ReferencedFlowBlocks.Count > 1 &&
                         nextFlowBlock.ReferencedFlowBlocks.All(x => visited.Contains(x)))
                {
                    AddIfMissing(capturedFlowBlocks, nextFlowBlock);

                    if (CaptureFlowBlocks(nextFlowBlock, targetFlowBlock, capturedFlowBlocks, visited))
                        targetCaptured = true;
                }
            }

            return targetCaptured;
        }

        private static void AddIfMissing(List<BaseFlowBlock> capturedFlowBlocks, BaseFlowBlock flowBlock)
        {
            if (!capturedFlowBlocks.Contains(flowBlock))
                capturedFlowBlocks.Add(flowBlock);
        }
    }
}