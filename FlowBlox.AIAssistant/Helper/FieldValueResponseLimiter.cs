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

        public LimitedFieldValue Limit(string? value)
        {
            value ??= string.Empty;
            var estimatedTotalTokens = _tokenBudget.EstimateTokens(value);

            if (RemainingTokens <= 0)
            {
                _omittedValues++;
                return new LimitedFieldValue(
                    "Limit exceeded due to max field-value tokens per response.",
                    0,
                    value.Length,
                    0,
                    estimatedTotalTokens,
                    0,
                    true,
                    true);
            }

            if (estimatedTotalTokens <= RemainingTokens)
            {
                _usedTokens += estimatedTotalTokens;
                return new LimitedFieldValue(
                    value,
                    0,
                    value.Length,
                    value.Length,
                    estimatedTotalTokens,
                    estimatedTotalTokens,
                    false,
                    false);
            }

            _limitedValues++;
            var suffix = $"... (truncated due to max field-value tokens per response limit of {_maxTokens} tokens)";
            var maxCharacters = Math.Max(1, RemainingTokens * _tokenBudget.ApproximateCharactersPerToken);
            var allowedCharacters = Math.Max(0, maxCharacters - suffix.Length);
            var visibleValue = allowedCharacters <= 0
                ? suffix
                : value.Substring(0, Math.Min(allowedCharacters, value.Length)) + suffix;
            var returnedTokens = _tokenBudget.EstimateTokens(visibleValue);
            _usedTokens = _maxTokens;

            return new LimitedFieldValue(
                visibleValue,
                0,
                value.Length,
                Math.Min(allowedCharacters, value.Length),
                estimatedTotalTokens,
                returnedTokens,
                true,
                false);
        }

        public InspectedFieldValue Inspect(string? value, FieldValueInspectionOptions options)
        {
            value ??= string.Empty;
            options ??= new FieldValueInspectionOptions();

            var searchResult = ResolveSearchIndex(value, options.SearchValues);
            var searchIndex = searchResult.Index
                ?? (options.SearchIndex.HasValue ? Math.Clamp(options.SearchIndex.Value, 0, value.Length) : 0);
            var maxCharacters = Math.Max(1, _maxTokens * _tokenBudget.ApproximateCharactersPerToken);
            var range = ResolveInspectionRange(value.Length, searchIndex, maxCharacters, options.SearchMode);
            var currentValue = value.Substring(range.StartIndex, range.Length);
            var estimatedReturnedTokens = _tokenBudget.EstimateTokens(currentValue);
            _usedTokens = Math.Min(_maxTokens, estimatedReturnedTokens);
            if (range.StartIndex != 0 || range.EndIndex < value.Length)
                _limitedValues = 1;

            return new InspectedFieldValue(
                currentValue,
                range.StartIndex,
                range.EndIndex,
                searchIndex,
                value.Length,
                currentValue.Length,
                _tokenBudget.EstimateTokens(value),
                estimatedReturnedTokens,
                range.StartIndex == 0 && range.EndIndex >= value.Length,
                searchResult.MatchedValue,
                searchResult.HasSearchValues,
                NormalizeSearchMode(options.SearchMode));
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

        private static SearchIndexResult ResolveSearchIndex(string value, string? searchValues)
        {
            var candidates = ParseSearchValues(searchValues);
            if (candidates.Count == 0)
                return SearchIndexResult.Empty;

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

            return new SearchIndexResult(bestIndex, matchedValue, true);
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

        private static InspectionRange ResolveInspectionRange(
            int totalLength,
            int searchIndex,
            int maxCharacters,
            string? searchMode)
        {
            if (string.Equals(searchMode, "LookAround", StringComparison.OrdinalIgnoreCase))
            {
                var half = Math.Max(1, maxCharacters / 2);
                var startIndex = Math.Max(0, searchIndex - half);
                var endIndex = Math.Min(totalLength, searchIndex + half);

                if (endIndex - startIndex < maxCharacters)
                {
                    var missing = maxCharacters - (endIndex - startIndex);
                    startIndex = Math.Max(0, startIndex - missing);
                    endIndex = Math.Min(totalLength, endIndex + missing);
                }

                return new InspectionRange(startIndex, endIndex);
            }

            var normalizedStartIndex = Math.Clamp(searchIndex, 0, totalLength);
            return new InspectionRange(
                normalizedStartIndex,
                Math.Min(totalLength, normalizedStartIndex + maxCharacters));
        }

        private static string NormalizeSearchMode(string? searchMode)
        {
            return string.Equals(searchMode, "LookAround", StringComparison.OrdinalIgnoreCase)
                ? "LookAround"
                : "StartAt";
        }

        private sealed record SearchIndexResult(int? Index, string? MatchedValue, bool HasSearchValues)
        {
            public static SearchIndexResult Empty { get; } = new(null, null, false);
        }

        private sealed record InspectionRange(int StartIndex, int EndIndex)
        {
            public int Length => Math.Max(0, EndIndex - StartIndex);
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
        bool LimitExceeded)
    {
        public bool IsComplete => !Truncated && !LimitExceeded;

        public JObject ToMetadata()
        {
            if (IsComplete)
                return new JObject();

            return new JObject
            {
                ["startIndex"] = StartIndex,
                ["totalLength"] = TotalLength,
                ["currentLength"] = CurrentLength,
                ["estimatedTotalTokens"] = EstimatedTotalTokens,
                ["estimatedReturnedTokens"] = EstimatedReturnedTokens,
                ["truncated"] = Truncated,
                ["limitExceeded"] = LimitExceeded
            };
        }
    }

    internal sealed class FieldValueInspectionOptions
    {
        public int? SearchIndex { get; set; }
        public string? SearchValues { get; set; }
        public string SearchMode { get; set; } = "StartAt";
    }

    internal sealed record InspectedFieldValue(
        string Value,
        int StartIndex,
        int EndIndex,
        int SearchIndex,
        int TotalLength,
        int CurrentLength,
        int EstimatedTotalTokens,
        int EstimatedReturnedTokens,
        bool IsComplete,
        string? MatchedSearchValue,
        bool HasSearchValues,
        string SearchMode)
    {
        public JObject ToMetadata()
        {
            var metadata = new JObject
            {
                ["searchMode"] = SearchMode,
                ["startIndex"] = StartIndex,
                ["endIndex"] = EndIndex,
                ["searchIndex"] = SearchIndex,
                ["totalLength"] = TotalLength,
                ["currentLength"] = CurrentLength,
                ["estimatedTotalTokens"] = EstimatedTotalTokens,
                ["estimatedReturnedTokens"] = EstimatedReturnedTokens
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