using System.Globalization;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters
{
    public class ExceptionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Exception exception)
            {
                return exception.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
