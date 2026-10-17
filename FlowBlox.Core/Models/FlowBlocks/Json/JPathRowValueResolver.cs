using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    internal static class JPathRowValueResolver
    {
        public static bool TryResolve(
            JToken rootToken,
            string valuePath,
            string rowPath,
            string fallbackValue,
            out IReadOnlyList<string> values)
        {
            var rowTokens = rootToken.SelectTokens(rowPath, errorWhenNoMatch: false).ToList();
            if (rowTokens.Count == 0)
            {
                values = [];
                return false;
            }

            var resultTokens = rootToken.SelectTokens(valuePath, errorWhenNoMatch: false).ToList();
            var matchesByRow = new Dictionary<JToken, List<JToken>>(ReferenceEqualityComparer.Instance);
            foreach (var rowToken in rowTokens)
                matchesByRow.TryAdd(rowToken, []);

            foreach (var resultToken in resultTokens)
            {
                for (var current = resultToken; current != null; current = current.Parent)
                {
                    if (!matchesByRow.TryGetValue(current, out var rowMatches))
                        continue;

                    rowMatches.Add(resultToken);
                    break;
                }
            }

            values = rowTokens
                .Select(rowToken => SerializeRowValue(matchesByRow[rowToken], fallbackValue))
                .ToList();
            return true;
        }

        private static string SerializeRowValue(IReadOnlyList<JToken> rowMatches, string fallbackValue)
        {
            if (rowMatches.Count == 0)
                return fallbackValue;

            return rowMatches.Count == 1
                ? JPathTokenSerializer.Serialize(rowMatches[0])
                : JPathTokenSerializer.Serialize(new JArray(rowMatches.Select(match => match.DeepClone())));
        }
    }

    internal static class JPathTokenSerializer
    {
        public static string Serialize(JToken token)
        {
            if (token is JValue value)
                return value.Value?.ToString() ?? string.Empty;

            return Newtonsoft.Json.JsonConvert.SerializeObject(token, Newtonsoft.Json.Formatting.None);
        }
    }
}
