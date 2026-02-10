using FlowBlox.Core.Models.Components;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    /// <summary>
    /// Builds a single result row (Dictionary<FieldElement, string>) based on ResultFieldByEnumValue{TEnum}. TEnum must be an enum type.
    /// </summary>
    public sealed class ResultFieldByEnumValueResultBuilder<TEnum>
        where TEnum : struct, Enum
    {
        private readonly Dictionary<TEnum, string> _values = new Dictionary<TEnum, string>();

        /// <summary>
        /// Assigns a value for a given enum destination. Null is allowed.
        /// </summary>
        public ResultFieldByEnumValueResultBuilder<TEnum> For(TEnum destination, string value)
        {
            _values[destination] = value;
            return this;
        }

        /// <summary>
        /// Builds a single row (Dictionary<FieldElement, string>) based on the provided result fields.
        /// </summary>
        public Dictionary<FieldElement, string> Build(IEnumerable<ResultFieldByEnumValue<TEnum>> resultFields)
        {
            var row = new Dictionary<FieldElement, string>();
            if (resultFields == null) return row;

            foreach (var entry in resultFields)
            {
                if (entry?.ResultField == null || !entry.EnumValue.HasValue)
                    continue;

                _values.TryGetValue(entry.EnumValue.Value, out var value);
                row[entry.ResultField] = value;
            }

            return row;
        }

        /// <summary>
        /// Convenience method: returns an IEnumerable containing exactly one row.
        /// </summary>
        public IEnumerable<Dictionary<FieldElement, string>> BuildSingleRow(IEnumerable<ResultFieldByEnumValue<TEnum>> resultFields)
        {
            yield return Build(resultFields);
        }
    }
}