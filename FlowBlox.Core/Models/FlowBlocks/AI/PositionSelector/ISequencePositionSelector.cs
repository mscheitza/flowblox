using Microsoft.ML.OnnxRuntime.Tensors;

namespace FlowBlox.Core.Models.FlowBlocks.AI.PositionSelector
{
    public interface ISequencePositionSelector
    {
        int SelectPosition(Tensor<float> logits);
    }
}