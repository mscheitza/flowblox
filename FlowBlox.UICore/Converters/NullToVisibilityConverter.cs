using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible;
            if (value is string)
                isVisible = !string.IsNullOrEmpty((string)value);
            else
                isVisible = value != null;

            if (parameter != null && parameter.ToString() == "invert")
            {
                isVisible = !isVisible;
            }
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
