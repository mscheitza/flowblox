using Microsoft.ML.OnnxRuntime.Tensors;

namespace FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector
{
    public class ArgMaxTokenSelector : INextTokenSelector
    {
        public int SelectNextToken(Tensor<float> logits, int lastIndex)
        {
            int vocabSize = logits.Dimensions[2];
            float maxLogit = float.NegativeInfinity;
            int nextToken = -1;

            for (int i = 0; i < vocabSize; i++)
            {
                float logit = logits[0, lastIndex, i];
                if (logit > maxLogit)
                {
                    maxLogit = logit;
                    nextToken = i;
                }
            }

            return nextToken;
        }
    }

}
