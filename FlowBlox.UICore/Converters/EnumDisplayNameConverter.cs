using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var field = value.GetType().GetField(value.ToString());
            var displayAttribute = field.GetCustomAttributes(typeof(DisplayAttribute), false)
                                        .FirstOrDefault() as DisplayAttribute;
            return displayAttribute != null ? displayAttribute.GetName() : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
