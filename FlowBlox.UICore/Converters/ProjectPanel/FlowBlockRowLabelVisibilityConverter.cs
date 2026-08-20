using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.ProjectPanel
{
    public sealed class FlowBlockRowLabelVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals(value?.ToString(), "header", StringComparison.Ordinal)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
