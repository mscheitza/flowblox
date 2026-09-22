using System.Globalization;
using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    public enum JsonPathComparisonOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEquals,
        LessThan,
        LessThanOrEquals,
        IsNull,
        IsNotNull,
        IsEmpty,
        IsNotEmpty
    }

    public sealed class JsonPathComparison
    {
        public string PropertyName { get; }
        public JsonPathComparisonOperator Operator { get; }
        public string CompareValue { get; }

        private JsonPathComparison(
            string propertyName,
            JsonPathComparisonOperator comparisonOperator,
            string compareValue)
        {
            PropertyName = propertyName;
            Operator = comparisonOperator;
            CompareValue = compareValue;
        }

        public static bool TryParse(string segment, out JsonPathComparison comparison)
        {
            comparison = null;

            if (string.IsNullOrWhiteSpace(segment) || !segment.StartsWith("@", StringComparison.Ordinal))
                return false;

            var expression = segment[1..].Trim();
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            if (TryParseUnary(expression, " is not empty", JsonPathComparisonOperator.IsNotEmpty, out comparison) ||
                TryParseUnary(expression, " is empty", JsonPathComparisonOperator.IsEmpty, out comparison) ||
                TryParseUnary(expression, " is not null", JsonPathComparisonOperator.IsNotNull, out comparison) ||
                TryParseUnary(expression, " is null", JsonPathComparisonOperator.IsNull, out comparison))
                return true;

            return TryParseBinary(expression, out comparison);
        }

        public bool IsMatch(JToken token)
        {
            var value = ResolveValue(token);

            return Operator switch
            {
                JsonPathComparisonOperator.Equals => Compare(value, CompareValue) == 0,
                JsonPathComparisonOperator.NotEquals => Compare(value, CompareValue) != 0,
                JsonPathComparisonOperator.GreaterThan => Compare(value, CompareValue) > 0,
                JsonPathComparisonOperator.GreaterThanOrEquals => Compare(value, CompareValue) >= 0,
                JsonPathComparisonOperator.LessThan => Compare(value, CompareValue) < 0,
                JsonPathComparisonOperator.LessThanOrEquals => Compare(value, CompareValue) <= 0,
                JsonPathComparisonOperator.IsNull => IsNull(value),
                JsonPathComparisonOperator.IsNotNull => !IsNull(value),
                JsonPathComparisonOperator.IsEmpty => IsEmpty(value),
                JsonPathComparisonOperator.IsNotEmpty => !IsEmpty(value),
                _ => false
            };
        }

        private static bool TryParseUnary(
            string expression,
            string suffix,
            JsonPathComparisonOperator comparisonOperator,
            out JsonPathComparison comparison)
        {
            comparison = null;

            if (!expression.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return false;

            var propertyName = expression[..^suffix.Length].Trim();
            if (string.IsNullOrWhiteSpace(propertyName))
                return false;

            comparison = new JsonPathComparison(propertyName, comparisonOperator, null);
            return true;
        }

        private static bool TryParseBinary(string expression, out JsonPathComparison comparison)
        {
            comparison = null;

            var operators = new[]
            {
                (Text: ">=", Operator: JsonPathComparisonOperator.GreaterThanOrEquals),
                (Text: "<=", Operator: JsonPathComparisonOperator.LessThanOrEquals),
                (Text: "!=", Operator: JsonPathComparisonOperator.NotEquals),
                (Text: "=", Operator: JsonPathComparisonOperator.Equals),
                (Text: ">", Operator: JsonPathComparisonOperator.GreaterThan),
                (Text: "<", Operator: JsonPathComparisonOperator.LessThan)
            };

            foreach (var op in operators)
            {
                var index = expression.IndexOf(op.Text, StringComparison.Ordinal);
                if (index <= 0)
                    continue;

                var propertyName = expression[..index].Trim();
                var compareValue = expression[(index + op.Text.Length)..].Trim();
                if (string.IsNullOrWhiteSpace(propertyName))
                    return false;

                comparison = new JsonPathComparison(propertyName, op.Operator, Unquote(compareValue));
                return true;
            }

            return false;
        }

        private JToken ResolveValue(JToken token)
        {
            if (token is not JObject obj)
                return null;

            return obj.TryGetValue(PropertyName, StringComparison.OrdinalIgnoreCase, out var value)
                ? value
                : null;
        }

        private static int Compare(JToken token, string compareValue)
        {
            if (TryGetDecimal(token, out var leftNumber) &&
                decimal.TryParse(compareValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightNumber))
                return leftNumber.CompareTo(rightNumber);

            var left = token is JValue value
                ? value.Value?.ToString() ?? string.Empty
                : token?.ToString(Newtonsoft.Json.Formatting.None) ?? string.Empty;

            return string.Compare(left, compareValue ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetDecimal(JToken token, out decimal value)
        {
            value = default;

            if (token is not JValue jValue || jValue.Value == null)
                return false;

            if (jValue.Value is decimal d)
            {
                value = d;
                return true;
            }

            return decimal.TryParse(
                Convert.ToString(jValue.Value, CultureInfo.InvariantCulture),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value);
        }

        private static bool IsNull(JToken token)
        {
            return token == null ||
                token.Type == JTokenType.Null ||
                token.Type == JTokenType.Undefined;
        }

        private static bool IsEmpty(JToken token)
        {
            if (IsNull(token))
                return true;

            return token switch
            {
                JValue value => string.IsNullOrEmpty(value.Value?.ToString()),
                JArray array => array.Count == 0,
                JObject obj => !obj.Properties().Any(),
                _ => false
            };
        }

        private static string Unquote(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if ((value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)) ||
                (value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal)))
                return value[1..^1];

            return value;
        }
    }
}
