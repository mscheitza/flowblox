using System.Globalization;

namespace FlowBlox.Core.Util.Parsing
{
    public sealed class FloatParsingStrategy : ValueParsingStrategy<float>
    {
        private readonly NumberFormatInfo _format;

        public FloatParsingStrategy(NumberFormatInfo format)
        {
            _format = NumberFormatInfo.ReadOnly((NumberFormatInfo)format.Clone());
        }

        public override float Parse(string value)
        {
            if (float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, _format, out var result))
                return result;
            throw new FormatException($"Failed to convert value '{value}' to float using decimal separator '{_format.NumberDecimalSeparator}' and group separator '{_format.NumberGroupSeparator}'.");
        }
    }
}
