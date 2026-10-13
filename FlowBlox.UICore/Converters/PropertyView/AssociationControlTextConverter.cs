using AssociationControlTextConverterResources = FlowBlox.UICore.Resources.ClassResources.AssociationControlTextConverter;
using FlowBlox.Core;
using FlowBlox.Core.Util.Resources;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.PropertyView
{
    class AssociationControlTextConverter : IValueConverter
    {
        public string EmptyText => FlowBloxResourceUtil.GetLocalizedString("Empty", typeof(AssociationControlTextConverterResources));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrEmpty(value?.ToString()) ? EmptyText : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}