using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool reverse = parameter?
                .ToString()?
                .Equals("Reverse", StringComparison.OrdinalIgnoreCase) == true;

            if (value is int count)
            {
                bool isVisible = count > 0;

                if (reverse)
                    isVisible = !isVisible;

                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
