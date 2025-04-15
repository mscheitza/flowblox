using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.UICore.Converters;
using FlowBlox.UICore.Converters.Insight;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für InsightWindow.xaml
    /// </summary>
    public partial class InsightWindow : MetroWindow
    {
        public InsightWindow(List<FlowBlockOutDataset> flowBlockOutDatasets, FlowBlockOutDataset currentDataset)
        {
            InitializeComponent();
            this.DataContext = new InsightViewModel(flowBlockOutDatasets, currentDataset);
            this.Loaded += InsightWindow_Loaded;
        }

        private void InsightWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as InsightViewModel;
            if (viewModel != null && viewModel.ColumnNames != null)
            {
                foreach (var columnName in viewModel.ColumnNames)
                {
                    dataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = columnName,
                        Binding = new Binding
                        {
                            Path = new PropertyPath("."),
                            Converter = new FlowBlockOutDatasetToValueConverter(),
                            ConverterParameter = columnName
                        },
                        Width = 200
                    });
                }
            }
        }
    }

}
