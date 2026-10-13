using AssociationConverterResources = FlowBlox.UICore.Resources.ClassResources.AssociationConverter;
using System.Globalization;
using System.Windows.Data;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.UICore.PropertyView.Converters
{
    public class AssociationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? value.ToString() : FlowBloxResourceUtil.GetLocalizedString("Create", typeof(AssociationConverterResources));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("AssociationConverter does not support ConvertBack.");
        }
    }
}
