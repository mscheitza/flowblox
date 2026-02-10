using FlowBlox.Core.Models.FlowBlocks.Additions;
using System.Globalization;
using System.Windows.Data;

namespace FlowBlox.UICore.Converters
{
    public class FlowBlockOutDatasetToValueConverter : IValueConverter
    {
        private const int MaxFieldValueLength = 500;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowBlockOutDataset dataset && parameter is string fieldName)
            {
                var fieldMapping = dataset.FieldValueMappings.FirstOrDefault(fvm => fvm.Field.Name == fieldName);

                var withoutLinebreaks = fieldMapping?.Value?
                    .Replace("\r\n", " ")
                    .Replace("\n", " ");

                return withoutLinebreaks;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
