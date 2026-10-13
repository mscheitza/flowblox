using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static partial class ToolHandlerUtilities
    {
        public static IReadOnlyList<string> ParseContainsSearchTerms(JToken? token)
        {
            var values = token is JArray array
                ? array.Values<string>()
                : new[] { token?.Value<string>() };

            return values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .SelectMany(x => x!.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries))
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static bool ContainsAnySearchTerm(
            IEnumerable<string?> values,
            IReadOnlyCollection<string> terms) =>
            terms.Count == 0 || terms.Any(term => values.Any(value =>
                (value ?? string.Empty).IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0));
    }
}
