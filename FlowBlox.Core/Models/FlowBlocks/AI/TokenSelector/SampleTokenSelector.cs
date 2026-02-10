using Microsoft.ML.OnnxRuntime.Tensors;

namespace FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector
{
    public class SampleTokenSelector : INextTokenSelector
    {
        private readonly Random _random = new Random();

        public int SelectNextToken(Tensor<float> logits, int lastIndex)
        {
            int vocabSize = logits.Dimensions[2];
            float[] scores = new float[vocabSize];

            float max = float.NegativeInfinity;
            for (int i = 0; i < vocabSize; i++)
                if (logits[0, lastIndex, i] > max)
                    max = logits[0, lastIndex, i];

            float sum = 0;
            for (int i = 0; i < vocabSize; i++)
            {
                scores[i] = MathF.Exp(logits[0, lastIndex, i] - max);
                sum += scores[i];
            }

            float sample = (float)_random.NextDouble() * sum;
            float cumulative = 0;
            for (int i = 0; i < vocabSize; i++)
            {
                cumulative += scores[i];
                if (sample < cumulative)
                    return i;
            }

            return vocabSize - 1;
        }
    }
}
