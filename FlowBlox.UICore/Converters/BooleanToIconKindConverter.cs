using MahApps.Metro.IconPacks;
using System.Globalization;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters
{
    public class BooleanToPackIconKindConverter : IValueConverter
    {
        public PackIconMaterialKind TrueIcon { get; set; } = PackIconMaterialKind.CheckCircleOutline;
        public PackIconMaterialKind FalseIcon { get; set; } = PackIconMaterialKind.CloseCircleOutline;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueIcon : FalseIcon;
            }

            return FalseIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
