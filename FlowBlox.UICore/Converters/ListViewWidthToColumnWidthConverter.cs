using System;
using System.Globalization;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters
{
    public sealed class ListViewWidthToColumnWidthConverter : IValueConverter
    {
        public double PaddingAndScrollbarCompensation { get; set; } = 35;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double totalWidth)
                return 0d;

            if (parameter == null || !double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ratio))
                ratio = 0.33;

            var usable = Math.Max(0, totalWidth - PaddingAndScrollbarCompensation);
            return usable * ratio;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
