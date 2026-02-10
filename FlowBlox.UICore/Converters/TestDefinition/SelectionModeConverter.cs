using System.Globalization;
using System.Windows.Data;
using FlowBlox.Core.Enums;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class SelectionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectionMode = (FlowBloxTestConfigurationSelectionMode)value;
            return selectionMode != FlowBloxTestConfigurationSelectionMode.UserInput;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}