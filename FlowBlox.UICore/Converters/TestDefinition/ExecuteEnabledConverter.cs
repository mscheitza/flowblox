using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.TestDefinition
{
    public class ExecuteEnabledMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3 ||
                !(values[0] is BaseFlowBlock currentFlowBlock) ||
                !(values[1] is FlowBlockTestDataset entry) || 
                !(values[2] is List<BaseFlowBlock> testDefinitionUsages))
                return false;
            
            if (testDefinitionUsages.Except([currentFlowBlock]) .Contains(entry.FlowBlock))
                return false;

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}