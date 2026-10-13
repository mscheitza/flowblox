using System.Globalization;

namespace FlowBlox.Core.Util.Parsing
{
    public sealed class DateTimeParsingStrategy : ValueParsingStrategy<DateTime>
    {
        private readonly string _format;

        public DateTimeParsingStrategy(string format)
        {
            _format = format;
        }

        public override DateTime Parse(string value)
        {
            if (DateTime.TryParseExact(value, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;
            throw new FormatException($"Failed to convert value '{value}' to DateTime using format '{_format}'.");
        }
    }
}
