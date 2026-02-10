using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace FlowBlox.UICore.Converters.Extensions
{
    public class VersionStatusToIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return PackIconMaterialKind.File;

            bool isDirty = values[0] is bool dirty && dirty;
            bool isReleased = values[1] is bool released && released;

            if (isDirty)
            {
                return PackIconMaterialKind.Pencil;
            }
            else if (isReleased)
            {
                return PackIconMaterialKind.Earth;
            }
            else
            {
                return PackIconMaterialKind.File;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
