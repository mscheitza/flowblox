namespace FlowBlox.Core.Models.FlowBlocks.AIRemote.Base
{
    internal sealed class OpenAICompatibleUsageReporter : UsageReporterBase
    {
        private readonly string _providerDisplayName;

        public OpenAICompatibleUsageReporter(string providerDisplayName)
        {
            _providerDisplayName = string.IsNullOrWhiteSpace(providerDisplayName)
                ? "OpenAI-compatible provider"
                : providerDisplayName;
        }

        protected override string ProviderDisplayName => _providerDisplayName;

        protected override AIProviderUsage ExtractUsage(object? response)
        {
            var usage = FindUsageObject(response);
            return new AIProviderUsage(
                GetIntProperty(usage, "InputTokenCount"),
                GetIntProperty(usage, "OutputTokenCount"));
        }

        protected override IEnumerable<UsageLogValue> GetLogValues(object? response)
        {
            var usage = FindUsageObject(response);
            if (usage == null)
                yield break;

            var inputDetails = GetPropertyValue(usage, "InputTokenDetails");
            var outputDetails = GetPropertyValue(usage, "OutputTokenDetails");

            yield return LogValue("input_tokens", GetIntProperty(usage, "InputTokenCount"));
            yield return LogValue("output_tokens", GetIntProperty(usage, "OutputTokenCount"));
            yield return LogValue("total_tokens", GetIntProperty(usage, "TotalTokenCount"));
            yield return LogValue("cached_input_tokens", GetIntProperty(inputDetails, "CachedTokenCount"));
            yield return LogValue("reasoning_output_tokens", GetIntProperty(outputDetails, "ReasoningTokenCount"));
        }

        private static object? FindUsageObject(object? response)
        {
            if (response == null)
                return null;

            var innerContent = GetPropertyValue(response, "InnerContent");
            var innerUsage = GetPropertyValue(innerContent, "Usage");
            if (innerUsage != null)
                return innerUsage;

            var metadata = GetPropertyValue(response, "Metadata") as IReadOnlyDictionary<string, object?>;
            if (TryGetMetadataValue(metadata, "Usage", out var metadataUsage) ||
                TryGetMetadataValue(metadata, "usage", out metadataUsage))
                return metadataUsage;

            return null;
        }

        private static bool TryGetMetadataValue(
            IReadOnlyDictionary<string, object?>? metadata,
            string key,
            out object? value)
        {
            value = null;
            if (metadata == null)
                return false;

            if (metadata.TryGetValue(key, out value))
                return true;

            foreach (var kvp in metadata)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = kvp.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
