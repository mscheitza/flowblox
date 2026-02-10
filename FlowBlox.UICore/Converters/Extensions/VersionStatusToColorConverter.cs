using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FlowBlox.UICore.Converters.Extensions
{
    public class VersionStatusToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return Brushes.DimGray;

            bool isDirty = values[0] is bool dirty && dirty;
            bool isReleased = values[1] is bool released && released;

            if (isDirty)
            {
                return Brushes.DarkViolet;
            }
            else if (isReleased)
            {
                return Brushes.DarkBlue;
            }
            else
            {
                return Brushes.DimGray;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
