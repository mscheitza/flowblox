using System.Globalization;
using System.Windows.Data;
using FlowBlox.Core.Enums;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class UserInputEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowBloxTestConfigurationSelectionMode selectionMode)
            {
                if (selectionMode == FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue ||
                    selectionMode == FlowBloxTestConfigurationSelectionMode.UserInput)
                {
                    return true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
