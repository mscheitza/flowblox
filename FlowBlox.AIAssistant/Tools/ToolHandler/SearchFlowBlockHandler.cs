using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util;
using FlowBlox.Grid.Elements.Util;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class SearchFlowBlockHandler : ToolHandlerBase
    {
        public override string Name => "SearchFlowBlock";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Searches flow block kinds by name using case-insensitive contains matching (OR). Empty searchForNames lists all flow block kinds.",
            new JObject
            {
                ["searchForNames"] = "string? (comma/space-separated name parts; OR search, case-insensitive contains)",
                ["usageHint"] = "Use searchForNames as comma/space-separated name parts (OR search). Empty searchForNames returns all flow block kinds."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var searchForNames = ParseSearchTerms(args);
            var normalizedTerms = searchForNames
                .Select(x => (x ?? string.Empty).Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var candidates = ToolHandlerUtilities.GetProject()
                .CreateInstances<BaseFlowBlock>()
                .GroupBy(x => x.GetType().FullName ?? x.GetType().Name, StringComparer.Ordinal)
                .Select(x => x.First())
                .Select(flowBlock => new SearchCandidate
                {
                    Instance = flowBlock,
                    DisplayName = FlowBloxComponentHelper.GetDisplayName(flowBlock) ?? string.Empty,
                    FullName = flowBlock.GetType().FullName ?? flowBlock.GetType().Name,
                    TypeName = flowBlock.GetType().Name,
                    CategoryPath = ToolHandlerUtilities.PathOf(flowBlock.GetCategory())
                });

            var filtered = normalizedTerms.Count == 0
                ? candidates
                : candidates.Where(candidate => normalizedTerms.Any(term => Matches(candidate, term)));

            var flowBlockKinds = filtered
                .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.FullName, StringComparer.OrdinalIgnoreCase)
                .Select(x =>
                {
                    var info = ToolHandlerUtilities.ToTypeInfo(x.Instance);
                    info["categoryPath"] = new JArray(x.CategoryPath);
                    return info;
                });

            var payload = new JObject
            {
                ["searchForNames"] = new JArray(normalizedTerms),
                ["matchMode"] = "CaseInsensitiveContainsOR",
                ["flowBlockKinds"] = new JArray(flowBlockKinds)
            };

            return Task.FromResult(ToolHandlerUtilities.Ok(payload));
        }

        private static IEnumerable<string> ParseSearchTerms(JObject args)
        {
            var commaSeparated = args.Value<string>("searchForNames")
                                 ?? args.Value<string>("SearchForNames")
                                 ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(commaSeparated))
            {
                return commaSeparated
                    .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim());
            }

            var arrayToken = args["searchForNames"] ?? args["SearchForNames"];
            if (arrayToken is JArray arr)
            {
                return arr
                    .Values<string>()
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .SelectMany(x => x.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Trim());
            }

            return Array.Empty<string>();
        }

        private static bool Matches(SearchCandidate candidate, string term)
        {
            return ContainsIgnoreCase(candidate.DisplayName, term)
                   || ContainsIgnoreCase(candidate.FullName, term)
                   || ContainsIgnoreCase(candidate.TypeName, term);
        }

        private static bool ContainsIgnoreCase(string value, string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return true;

            return (value ?? string.Empty).IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class SearchCandidate
        {
            public BaseFlowBlock Instance { get; set; }
            public string DisplayName { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string TypeName { get; set; } = string.Empty;
            public string[] CategoryPath { get; set; } = Array.Empty<string>();
        }
    }
}
