using MahApps.Metro.IconPacks;
using System.Globalization;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.FieldSelection
{
    public sealed class FieldIconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = value as string ?? "";

            return key switch
            {
                "user" => PackIconMaterialKind.Account,
                "connected" => PackIconMaterialKind.LinkVariant,
                "disconnected" => PackIconMaterialKind.LinkVariantOff,
                _ => PackIconMaterialKind.HelpCircleOutline
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}