using MahApps.Metro.IconPacks;
using System.Globalization;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.ProjectPanel
{
    public sealed class FlowBlockRowIconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "field" => PackIconMaterialKind.DatabaseOutline,
                "modifier" => PackIconMaterialKind.FunctionVariant,
                "condition" => PackIconMaterialKind.SourceBranch,
                "required" => PackIconMaterialKind.AlertCircleOutline,
                "activation" => PackIconMaterialKind.FilterOutline,
                _ => PackIconMaterialKind.CogOutline
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
