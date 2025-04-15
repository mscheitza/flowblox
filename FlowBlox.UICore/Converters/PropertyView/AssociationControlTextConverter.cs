using FlowBlox.Core;
using FlowBlox.Core.Util.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters.PropertyView
{
    class AssociationControlTextConverter : IValueConverter
    {
        public string EmptyText => FlowBloxResourceUtil.GetLocalizedString("AssociationTextBox_Empty", typeof(FlowBloxTexts));

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