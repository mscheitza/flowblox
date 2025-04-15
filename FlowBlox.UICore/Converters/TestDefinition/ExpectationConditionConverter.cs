using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Testing;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class ExpectationConditionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var conditions = value as List<ExpectationCondition>;
            return conditions != null ? string.Join(", ", conditions.Select(c => c.DisplayName)) : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
