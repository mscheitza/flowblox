using FlowBlox.Core.Models.FlowBlocks.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace FlowBlox.Core.Models.Testing
{
    internal class FlowBloxTestCapture
    {
        private List<BaseFlowBlock> _capturedFlowBlocks;

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

            foreach (var nextFlowBlock in flowBlock.GetNextFlowBlocks())
            {
                capturedFlowBlocks.Add(nextFlowBlock);

                if (nextFlowBlock == targetFlowBlock)
                    return true;

                return CaptureFlowBlocks(nextFlowBlock, targetFlowBlock, ref capturedFlowBlocks);
            }

            return false;
        }
    }
}
