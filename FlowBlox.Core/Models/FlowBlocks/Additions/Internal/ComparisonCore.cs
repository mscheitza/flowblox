using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.Fields;
using System.Text.RegularExpressions;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    internal sealed class ComparisonCore
    {
        public bool Check(ComparisonOperator op, string rawRightValue, object leftValue)
        {
            if (leftValue is int || leftValue is int?)
                return Check(op, rawRightValue, (int?)leftValue);

            if (leftValue is DateTime || leftValue is DateTime?)
                return Check(op, rawRightValue, (DateTime?)leftValue);

            return Check(op, rawRightValue, leftValue?.ToString());
        }

        public bool Check(ComparisonOperator op, string rawRightValue, string left)
        {
            string right = GetRuntimeValue(rawRightValue);

            switch (op)
            {
                case ComparisonOperator.HasValue: return !string.IsNullOrEmpty(left);
                case ComparisonOperator.HasNoValue: return string.IsNullOrEmpty(left);
                case ComparisonOperator.Equals: return left == right;
                case ComparisonOperator.NotEquals: return left != right;
                case ComparisonOperator.GreaterThan: return string.Compare(left, right) > 0;
                case ComparisonOperator.LowerThan: return string.Compare(left, right) < 0;
                case ComparisonOperator.GreaterThanOrEquals: return string.Compare(left, right) >= 0;
                case ComparisonOperator.LowerThanOrEquals: return string.Compare(left, right) <= 0;
            }

            if (right == null)
                return false;

            switch (op)
            {
                case ComparisonOperator.Contains:
                    return left != null && left.Contains(right);
                case ComparisonOperator.NotContains:
                    return left == null || !left.Contains(right);
            }

            if (left == null)
                return false;

            switch (op)
            {
                case ComparisonOperator.RegexIsTrue:
                    return Regex.IsMatch(left, right);
                case ComparisonOperator.RegexIsFalse:
                    return !Regex.IsMatch(left, right);
                default:
                    throw new NotSupportedException("This operator is not supported.");
            }
        }

        public bool Check(ComparisonOperator op, string rawRightValue, int? left)
        {
            if (!int.TryParse(GetRuntimeValue(rawRightValue), out int right))
                throw new InvalidOperationException("Invalid comparison value for Integer operation.");

            switch (op)
            {
                case ComparisonOperator.HasValue: return left.HasValue;
                case ComparisonOperator.HasNoValue: return !left.HasValue;
                case ComparisonOperator.Equals: return left == right;
                case ComparisonOperator.NotEquals: return left != right;
                case ComparisonOperator.GreaterThan: return left > right;
                case ComparisonOperator.LowerThan: return left < right;
                case ComparisonOperator.GreaterThanOrEquals: return left >= right;
                case ComparisonOperator.LowerThanOrEquals: return left <= right;
                default:
                    throw new NotSupportedException("This operator is not supported for Integer.");
            }
        }

        public bool Check(ComparisonOperator op, string rawRightValue, DateTime? left)
        {
            if (!DateTime.TryParse(GetRuntimeValue(rawRightValue), out DateTime right))
                throw new InvalidOperationException("Invalid comparison value for DateTime operation.");

            switch (op)
            {
                case ComparisonOperator.HasValue: return left.HasValue;
                case ComparisonOperator.HasNoValue: return !left.HasValue;
                case ComparisonOperator.Equals: return left == right;
                case ComparisonOperator.NotEquals: return left != right;
                case ComparisonOperator.GreaterThan: return left > right;
                case ComparisonOperator.LowerThan: return left < right;
                case ComparisonOperator.GreaterThanOrEquals: return left >= right;
                case ComparisonOperator.LowerThanOrEquals: return left <= right;
                default:
                    throw new NotSupportedException("This operator is not supported for DateTime.");
            }
        }

        private static string GetRuntimeValue(string value)
        {
            string runtimeValue = value;
            runtimeValue = FlowBloxFieldHelper.ReplaceFieldsInString(runtimeValue);
            return runtimeValue;
        }
    }
}
