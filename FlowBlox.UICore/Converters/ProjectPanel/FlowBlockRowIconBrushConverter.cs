using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FlowBlox.UICore.Converters.ProjectPanel
{
    public sealed class FlowBlockRowIconBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = value?.ToString() switch
            {
                "field" => "#6DE6BE",
                "modifier" => "#90EE90",
                "condition" => "#F08080",
                "required" => "#FFDAB9",
                "activation" => "#DAA520",
                _ => "#7DB7FF"
            };

            return (Brush)new BrushConverter().ConvertFromString(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
