using Anthropic.SDK.Messaging;
using FlowBlox.Core.Models.FlowBlocks.AIRemote.Base;

namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Providers
{
    internal sealed class AnthropicUsageReporter : UsageReporterBase
    {
        protected override string ProviderDisplayName => "Anthropic";

        protected override AIProviderUsage ExtractUsage(object? response)
        {
            var usage = (response as MessageResponse)?.Usage;
            return new AIProviderUsage(
                usage?.InputTokens,
                usage?.OutputTokens);
        }

        protected override IEnumerable<UsageLogValue> GetLogValues(object? response)
        {
            var usage = (response as MessageResponse)?.Usage;
            if (usage == null)
                yield break;

            yield return LogValue("input_tokens", usage.InputTokens);
            yield return LogValue("output_tokens", usage.OutputTokens);
            yield return LogValue("cache_creation_input_tokens", usage.CacheCreationInputTokens);
            yield return LogValue("cache_read_input_tokens", usage.CacheReadInputTokens);
        }
    }
}
