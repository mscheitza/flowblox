using System.Globalization;

namespace FlowBlox.Core.Util.Parsing
{
    public sealed class DoubleParsingStrategy : ValueParsingStrategy<double>
    {
        private readonly NumberFormatInfo _format;

        public DoubleParsingStrategy(NumberFormatInfo format)
        {
            _format = NumberFormatInfo.ReadOnly((NumberFormatInfo)format.Clone());
        }

        public override double Parse(string value)
        {
            if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, _format, out var result))
                return result;
            throw new FormatException($"Failed to convert value '{value}' to double using decimal separator '{_format.NumberDecimalSeparator}' and group separator '{_format.NumberGroupSeparator}'.");
        }
    }
}
