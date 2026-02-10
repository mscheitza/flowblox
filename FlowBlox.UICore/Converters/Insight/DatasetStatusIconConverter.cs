using System.Globalization;
using System.Windows.Data;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.IconPacks;
using FlowBlox.Core.Models.FlowBlocks.Additions;

namespace FlowBlox.UICore.Converters.Insight
{
    public class DatasetStatusIconMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || !(values[0] is FlowBlockOutDataset) || !(values[1] is InsightViewModel))
                return null;

            var dataset = values[0] as FlowBlockOutDataset;
            var viewModel = values[1] as InsightViewModel;
            int datasetIndex = viewModel.FlowBlockOutDatasets.IndexOf(dataset);
            int currentIndex = viewModel.CurrentDatasetIndex;

            if (datasetIndex < currentIndex)
                return PackIconMaterialKind.Check;
            if (datasetIndex == currentIndex)
                return PackIconMaterialKind.Cog;
            return PackIconMaterialKind.Clock;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
