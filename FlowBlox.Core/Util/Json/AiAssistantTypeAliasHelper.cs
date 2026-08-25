using System;
using System.Linq;

namespace FlowBlox.Core.Util.Json
{
    public static class AiAssistantTypeAliasHelper
    {
        private static readonly (string Alias, string Prefix)[] Aliases =
        [
            ("FMB", "FlowBlox.Core.Models.Base"),
            ("FMC", "FlowBlox.Core.Models.Components"),
            ("FMFB", "FlowBlox.Core.Models.FlowBlocks.Base"),
            ("FMG", "FlowBlox.Core.Models.Generators"),
            ("FM", "FlowBlox.Core.Models"),
            ("FC", "FlowBlox.Core")
        ];

        public static string AliasSummary =>
            string.Join(", ", Aliases.Select(x => $"{x.Alias}={x.Prefix}"));

        public static string CompressTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return typeName;

            var result = typeName;
            foreach (var alias in Aliases.OrderByDescending(x => x.Prefix.Length))
            {
                result = ReplaceQualifiedPrefix(result, alias.Prefix, alias.Alias);
            }

            return result;
        }

        private static string ReplaceQualifiedPrefix(string value, string from, string to)
        {
            if (string.Equals(value, from, StringComparison.Ordinal))
                return to;

            if (value.StartsWith(from + ".", StringComparison.Ordinal))
                return to + value.Substring(from.Length);

            return value.Replace("," + from + ".", "," + to + ".", StringComparison.Ordinal)
                .Replace("[" + from + ".", "[" + to + ".", StringComparison.Ordinal)
                .Replace(":" + from + ".", ":" + to + ".", StringComparison.Ordinal)
                .Replace(" " + from + ".", " " + to + ".", StringComparison.Ordinal);
        }
    }
}
