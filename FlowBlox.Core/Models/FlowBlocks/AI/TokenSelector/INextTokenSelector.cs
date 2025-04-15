using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector
{
    public interface INextTokenSelector
    {
        int SelectNextToken(Tensor<float> logits, int lastIndex);
    }
}
