using Microsoft.ML.Tokenizers;

namespace FlowBlox.Core.Models.FlowBlocks.AI.OnnxQA
{
    internal static class OnnxQASpanSelector
    {
        public static OnnxQASpanCandidate? SelectBestSpan(
            ReadOnlySpan<float> startLogits,
            ReadOnlySpan<float> endLogits,
            OnnxQAEncodedWindow window,
            IReadOnlyList<EncodedToken> contextTokens,
            int maximumAnswerLength)
        {
            OnnxQASpanCandidate? best = null;
            var contextEndIndex = window.ContextStartIndex + window.ContextTokenCount;

            for (var startIndex = window.ContextStartIndex; startIndex < contextEndIndex; startIndex++)
            {
                var maximumEndIndex = Math.Min(contextEndIndex - 1, startIndex + maximumAnswerLength - 1);
                for (var endIndex = startIndex; endIndex <= maximumEndIndex; endIndex++)
                {
                    var firstToken = contextTokens[window.ContextTokenOffset + startIndex - window.ContextStartIndex];
                    var lastToken = contextTokens[window.ContextTokenOffset + endIndex - window.ContextStartIndex];
                    var characterStart = firstToken.Offset.Start.Value;
                    var characterEnd = lastToken.Offset.End.Value;
                    if (characterStart < 0 || characterEnd <= characterStart)
                        continue;

                    var score = startLogits[startIndex] + endLogits[endIndex];
                    if (best == null || score > best.Score)
                        best = new OnnxQASpanCandidate(characterStart, characterEnd, score);
                }
            }

            return best;
        }
    }

    internal sealed record OnnxQASpanCandidate(int CharacterStart, int CharacterEnd, float Score);
}
