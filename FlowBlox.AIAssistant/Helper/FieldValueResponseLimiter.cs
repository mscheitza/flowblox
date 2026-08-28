using FlowBlox.AIAssistant.Builder;
using FlowBlox.AIAssistant.Constants;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using FlowBlox.Core.Util;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Helper
{
    internal sealed class FieldValueResponseLimiter
    {
        private readonly AssistantTokenBudget _tokenBudget;
        private readonly int _maxTokens;
        private int _usedTokens;
        private int _limitedValues;
        private int _omittedValues;

        private FieldValueResponseLimiter(AssistantTokenBudget tokenBudget, int maxTokens)
        {
            _tokenBudget = tokenBudget ?? throw new ArgumentNullException(nameof(tokenBudget));
            _maxTokens = Math.Clamp(maxTokens, 1, 20000);
        }

        public int MaxTokens => _maxTokens;
        public int UsedTokens => _usedTokens;
        public int RemainingTokens => Math.Max(0, _maxTokens - _usedTokens);

        public static FieldValueResponseLimiter FromConfiguration()
        {
            var configuration = LoadConfiguration();
            var approximateCharactersPerToken = Math.Clamp(
                configuration.ApproximateCharactersPerToken,
                AssistantConfigurationLimits.MinApproximateCharactersPerToken,
                AssistantConfigurationLimits.MaxApproximateCharactersPerToken);

            return new FieldValueResponseLimiter(
                new AssistantTokenBudget
                {
                    ApproximateCharactersPerToken = approximateCharactersPerToken
                },
                configuration.MaxFieldValuesTokensPerResponse);
        }

        public LimitedFieldValue Limit(string? value, int startIndex = 0, string? searchValues = null)
        {
            value ??= string.Empty;
            var searchResult = ResolveSearchStartIndex(value, searchValues);
            var normalizedStartIndex = searchResult.Index ?? Math.Clamp(startIndex, 0, value.Length);
            var availableValue = value.Substring(normalizedStartIndex);
            var estimatedTotalTokens = _tokenBudget.EstimateTokens(value);

            if (RemainingTokens <= 0)
            {
                _omittedValues++;
                return new LimitedFieldValue(
                    "Limit exceeded due to max field-value tokens per response.",
                    normalizedStartIndex,
                    value.Length,
                    0,
                    estimatedTotalTokens,
                    0,
                    true,
                    true,
                    searchResult.MatchedValue,
                    searchResult.HasSearchValues);
            }

            var estimatedAvailableTokens = _tokenBudget.EstimateTokens(availableValue);
            if (estimatedAvailableTokens <= RemainingTokens)
            {
                _usedTokens += estimatedAvailableTokens;
                return new LimitedFieldValue(
                    availableValue,
                    normalizedStartIndex,
                    value.Length,
                    availableValue.Length,
                    estimatedTotalTokens,
                    estimatedAvailableTokens,
                    false,
                    false,
                    searchResult.MatchedValue,
                    searchResult.HasSearchValues);
            }

            _limitedValues++;
            var tokensForValue = RemainingTokens;
            var suffix = $"... (truncated due to max field-value tokens per response limit of {_maxTokens} tokens)";
            var maxCharacters = Math.Max(1, tokensForValue * _tokenBudget.ApproximateCharactersPerToken);
            var allowedCharacters = Math.Max(0, maxCharacters - suffix.Length);
            var visibleValue = allowedCharacters <= 0
                ? suffix
                : availableValue.Substring(0, Math.Min(allowedCharacters, availableValue.Length)) + suffix;
            var returnedTokens = _tokenBudget.EstimateTokens(visibleValue);
            _usedTokens = _maxTokens;

            return new LimitedFieldValue(
                visibleValue,
                normalizedStartIndex,
                value.Length,
                Math.Min(allowedCharacters, availableValue.Length),
                estimatedTotalTokens,
                returnedTokens,
                true,
                false,
                searchResult.MatchedValue,
                searchResult.HasSearchValues);
        }

        public JObject CreateMetadata()
        {
            return new JObject
            {
                ["maxTokens"] = _maxTokens,
                ["usedTokens"] = _usedTokens,
                ["remainingTokens"] = RemainingTokens,
                ["limitedValues"] = _limitedValues,
                ["omittedValues"] = _omittedValues
            };
        }

        private static AssistantConfiguration LoadConfiguration()
        {
            var rawConfig = FlowBloxOptions.GetOptionInstance()
                .GetOption("AI.AssistantConfiguration")?
                .Value ?? string.Empty;
            var parseResult = AssistantConfigurationJson.Parse(rawConfig);
            return parseResult.Configuration ?? new AssistantConfiguration();
        }

        private static SearchStartIndexResult ResolveSearchStartIndex(string value, string? searchValues)
        {
            var candidates = ParseSearchValues(searchValues);
            if (candidates.Count == 0)
                return SearchStartIndexResult.Empty;

            int? bestIndex = null;
            string? matchedValue = null;
            foreach (var candidate in candidates)
            {
                var index = value.IndexOf(candidate, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    continue;

                if (!bestIndex.HasValue || index < bestIndex.Value)
                {
                    bestIndex = index;
                    matchedValue = candidate;
                }
            }

            return new SearchStartIndexResult(bestIndex, matchedValue, true);
        }

        private static IReadOnlyList<string> ParseSearchValues(string? searchValues)
        {
            if (string.IsNullOrWhiteSpace(searchValues))
                return Array.Empty<string>();

            return searchValues
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private sealed record SearchStartIndexResult(int? Index, string? MatchedValue, bool HasSearchValues)
        {
            public static SearchStartIndexResult Empty { get; } = new(null, null, false);
        }
    }

    internal sealed record LimitedFieldValue(
        string Value,
        int StartIndex,
        int TotalLength,
        int CurrentLength,
        int EstimatedTotalTokens,
        int EstimatedReturnedTokens,
        bool Truncated,
        bool LimitExceeded,
        string? MatchedSearchValue,
        bool HasSearchValues)
    {
        public JObject ToMetadata()
        {
            var metadata = new JObject
            {
                ["startIndex"] = StartIndex,
                ["totalLength"] = TotalLength,
                ["currentLength"] = CurrentLength,
                ["estimatedTotalTokens"] = EstimatedTotalTokens,
                ["estimatedReturnedTokens"] = EstimatedReturnedTokens,
                ["truncated"] = Truncated,
                ["limitExceeded"] = LimitExceeded
            };

            if (HasSearchValues)
            {
                metadata["searchMatched"] = !string.IsNullOrEmpty(MatchedSearchValue);
                metadata["matchedSearchValue"] = string.IsNullOrEmpty(MatchedSearchValue)
                    ? JValue.CreateNull()
                    : MatchedSearchValue;
            }

            return metadata;
        }
    }
}