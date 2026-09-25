using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class MultilineTextVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isMultiline = value is string text && (text.Contains('\r') || text.Contains('\n'));
            var showSingleLine = string.Equals(parameter?.ToString(), "SingleLine", StringComparison.Ordinal);
            return showSingleLine != isMultiline ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
