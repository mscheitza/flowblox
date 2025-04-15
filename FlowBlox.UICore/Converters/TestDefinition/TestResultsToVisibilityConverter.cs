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
using FlowBlox.Core.Models.FlowBlocks.Additions;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class TestResultsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<FlowBlockOutDataset> testResults && testResults.Count > 0)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
