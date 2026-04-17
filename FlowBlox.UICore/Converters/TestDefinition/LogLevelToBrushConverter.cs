using FlowBlox.Core.Enums;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class LogLevelToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush SuccessBrush = CreateFrozenBrush("#2F855A");
        private static readonly SolidColorBrush InfoBrush = CreateFrozenBrush("#2B6CB0");
        private static readonly SolidColorBrush WarningBrush = CreateFrozenBrush("#B7791F");
        private static readonly SolidColorBrush ErrorBrush = CreateFrozenBrush("#C53030");
        private static readonly SolidColorBrush DefaultBrush = CreateFrozenBrush("#6B7280");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not FlowBloxLogLevel logLevel)
                return DefaultBrush;

            return logLevel switch
            {
                FlowBloxLogLevel.Success => SuccessBrush,
                FlowBloxLogLevel.Info => InfoBrush,
                FlowBloxLogLevel.Warning => WarningBrush,
                FlowBloxLogLevel.Error => ErrorBrush,
                _ => DefaultBrush
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static SolidColorBrush CreateFrozenBrush(string colorCode)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode));
            brush.Freeze();
            return brush;
        }
    }
}
