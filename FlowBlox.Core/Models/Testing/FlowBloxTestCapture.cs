using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using System.Linq;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestCapture
    {
        private List<BaseFlowBlock> _capturedFlowBlocks;
        private FlowBloxRegistry _registry;

        public FlowBloxTestCapture()
        {
            this._registry = FlowBloxRegistryProvider.GetRegistry();
        }

        public List<BaseFlowBlock> GetCapturedFlowBlocks() => _capturedFlowBlocks;

        public bool CreateCapture(BaseFlowBlock startFlowBlock, BaseFlowBlock currentFlowBlock)
        {
            _capturedFlowBlocks = new List<BaseFlowBlock>();
            return CaptureFlowBlocks(startFlowBlock, currentFlowBlock, ref _capturedFlowBlocks);
        }

        private bool CaptureFlowBlocks(BaseFlowBlock flowBlock, BaseFlowBlock targetFlowBlock, ref List<BaseFlowBlock> capturedFlowBlocks)
        {
            if (capturedFlowBlocks == null)
                capturedFlowBlocks = new List<BaseFlowBlock>();

            bool targetCaptured = false;
            var nextFlowBlocks = flowBlock.GetNextFlowBlocks();
            if (nextFlowBlocks.Any(nextFlowBlock => nextFlowBlock == targetFlowBlock))
            {
                capturedFlowBlocks.Add(targetFlowBlock);
                targetCaptured = true;
            }
            else
            {
                foreach (var nextFlowBlock in nextFlowBlocks)
                {
                    capturedFlowBlocks.Add(nextFlowBlock);

                    if (CaptureFlowBlocks(nextFlowBlock, targetFlowBlock, ref capturedFlowBlocks))
                        targetCaptured = true;
                }
            }
            return targetCaptured;
        }
    }
}