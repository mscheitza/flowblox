using System.Globalization;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters
{
    public class TruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const int maxLength = 500;
            string stringValue = value as string;
            if (stringValue != null && stringValue.Length > maxLength)
                return stringValue.Substring(0, maxLength) + "...";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
