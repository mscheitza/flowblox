using FlowBlox.AIAssistant.Constants;

namespace FlowBlox.AIAssistant.Builder
{
    internal sealed class AssistantTokenBudget
    {
        public int MaxContextTokens { get; set; }
        public int ReservedResponseTokens { get; set; }
        public int ApproximateCharactersPerToken { get; set; }

        public int EstimateTokens(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            var charsPerToken = Math.Max(AssistantConfigurationLimits.MinApproximateCharactersPerToken, ApproximateCharactersPerToken);
            return Math.Max(1, (int)Math.Ceiling(text.Length / (double)charsPerToken));
        }
    }
}