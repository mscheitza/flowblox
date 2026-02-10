using Microsoft.ML.OnnxRuntime.Tensors;

namespace FlowBlox.Core.Models.FlowBlocks.AI.PositionSelector
{
    public class ArgMaxPositionSelector : ISequencePositionSelector
    {
        public int SelectPosition(Tensor<float> logits)
        {
            float max = float.NegativeInfinity;
            int index = -1;

            for (int i = 0; i < logits.Length; i++)
            {
                if (logits[0, i] > max)
                {
                    max = logits[0, i];
                    index = i;
                }
            }

            return index;
        }
    }

}
