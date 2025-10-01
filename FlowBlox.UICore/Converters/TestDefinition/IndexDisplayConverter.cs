using FlowBlox.Core.Enums;
using System.Globalization;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class IndexDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) 
                return "-";

            if (values[0] is ExpectationConditionTarget ect &&
                ect == ExpectationConditionTarget.ValueAtIndex)
            {
                return values[1]?.ToString() ?? "-";
            }

            return "-";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing];
    }
}