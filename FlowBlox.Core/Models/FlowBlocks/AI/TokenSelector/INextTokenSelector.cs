using Microsoft.ML.OnnxRuntime.Tensors;

namespace FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector
{
    public interface INextTokenSelector
    {
        int SelectNextToken(Tensor<float> logits, int lastIndex);
    }
}
