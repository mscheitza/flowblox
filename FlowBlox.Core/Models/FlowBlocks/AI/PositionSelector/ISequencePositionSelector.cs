using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AI.PositionSelector
{
    public interface ISequencePositionSelector
    {
        int SelectPosition(Tensor<float> logits);
    }
}