using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FlowBlox.UICore.ViewModels;
using System.Collections.ObjectModel;
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
