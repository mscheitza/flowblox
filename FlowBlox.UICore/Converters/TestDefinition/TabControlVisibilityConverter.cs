using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class TabControlVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var testResults = values.ElementAt(0) as ObservableCollection<FlowBlockOutDataset>;
            var runtimeLogs = values.ElementAt(1) as ObservableCollection<RuntimeLog>;

            if (testResults?.Any() == true ||
                runtimeLogs?.Any() == true)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
