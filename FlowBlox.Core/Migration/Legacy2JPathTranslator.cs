using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Migration
{
    /// <summary>
    /// Translates the former FlowBlox slash-delimited JSON path syntax to
    /// Newtonsoft.Json's JPath syntax.
    /// </summary>
    public static class Legacy2JPathTranslator
    {
        private static readonly Regex BinaryFilterRegex = new Regex(
            @"^@(?<property>.+?)(?<operator>>=|<=|!=|=|>|<)(?<value>.*)$",
            RegexOptions.Compiled);

        private static readonly Regex UnaryFilterRegex = new Regex(
            @"^@(?<property>.+?)\s+is\s+(?<not>not\s+)?(?<condition>null|empty)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsNewtonsoftJPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var normalizedPath = path.Trim();
            if (normalizedPath.StartsWith("$/", StringComparison.Ordinal) ||
                (!normalizedPath.StartsWith("$", StringComparison.Ordinal) &&
                 normalizedPath.Contains('/', StringComparison.Ordinal)))
                return false;

            try
            {
                _ = new JObject()
                    .SelectTokens(normalizedPath, errorWhenNoMatch: false)
                    .ToList();
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public static string Translate(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || IsNewtonsoftJPath(path))
                return path;

            var normalizedPath = path.Trim();
            var projectsRootArray = normalizedPath.StartsWith("$/", StringComparison.Ordinal);
            if (projectsRootArray)
                normalizedPath = normalizedPath[2..];

            var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder("$");

            if (projectsRootArray && segments.Length > 0)
                result.Append("[*]");

            foreach (var segment in segments)
            {
                if (TryTranslateFilter(segment, out var filter))
                {
                    result.Append(filter);
                }
                else if (int.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                {
                    result.Append('[').Append(index).Append(']');
                }
                else
                {
                    AppendProperty(result, segment);
                }
            }

            return result.ToString();
        }

        private static bool TryTranslateFilter(string segment, out string filter)
        {
            filter = null;

            var unaryMatch = UnaryFilterRegex.Match(segment);
            if (unaryMatch.Success)
            {
                var property = CreateCurrentPropertyAccessor(unaryMatch.Groups["property"].Value.Trim());
                var negate = unaryMatch.Groups["not"].Success;
                var condition = unaryMatch.Groups["condition"].Value.ToLowerInvariant();

                filter = condition == "null"
                    ? $"[?({property} {(negate ? "!=" : "==")} null)]"
                    : negate
                        ? $"[?({property} != null && {property} != '')]"
                        : $"[?({property} == null || {property} == '')]";
                return true;
            }

            var binaryMatch = BinaryFilterRegex.Match(segment);
            if (!binaryMatch.Success)
                return false;

            var comparisonOperator = binaryMatch.Groups["operator"].Value;
            if (comparisonOperator == "=")
                comparisonOperator = "==";

            filter = $"[?({CreateCurrentPropertyAccessor(binaryMatch.Groups["property"].Value.Trim())} " +
                $"{comparisonOperator} {CreateLiteral(binaryMatch.Groups["value"].Value.Trim())})]";
            return true;
        }

        private static string CreateCurrentPropertyAccessor(string propertyName)
        {
            return IsSimplePropertyName(propertyName)
                ? $"@.{propertyName}"
                : $"@['{EscapeSingleQuoted(propertyName)}']";
        }

        private static string CreateLiteral(string value)
        {
            if (value.Length >= 2 &&
                ((value[0] == '\'' && value[^1] == '\'') ||
                 (value[0] == '"' && value[^1] == '"')))
                value = value[1..^1];

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _) ||
                bool.TryParse(value, out _) ||
                string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
                return value.ToLowerInvariant();

            return $"'{EscapeSingleQuoted(value)}'";
        }

        private static void AppendProperty(StringBuilder path, string propertyName)
        {
            if (IsSimplePropertyName(propertyName))
                path.Append('.').Append(propertyName);
            else
                path.Append("['").Append(EscapeSingleQuoted(propertyName)).Append("']");
        }

        private static bool IsSimplePropertyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName) ||
                !(char.IsLetter(propertyName[0]) || propertyName[0] == '_' || propertyName[0] == '$'))
                return false;

            return propertyName.Skip(1).All(x => char.IsLetterOrDigit(x) || x == '_' || x == '$');
        }

        private static string EscapeSingleQuoted(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("'", "\\'", StringComparison.Ordinal);
        }
    }
}
