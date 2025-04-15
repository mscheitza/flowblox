using FlowBlox.Core.Models.FlowBlocks.Additions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowBlox.UICore.ViewModels
{
    public class InsightViewModel
    {
        public ObservableCollection<FlowBlockOutDataset> FlowBlockOutDatasets { get; set; }
        public FlowBlockOutDataset CurrentDataset { get; private set; }
        public List<string> ColumnNames { get; private set; }
        public int CurrentDatasetIndex => FlowBlockOutDatasets.IndexOf(CurrentDataset);

        public InsightViewModel(List<FlowBlockOutDataset> flowBlockOutDatasets, FlowBlockOutDataset currentDataset)
        {
            FlowBlockOutDatasets = new ObservableCollection<FlowBlockOutDataset>(flowBlockOutDatasets);
            CurrentDataset = currentDataset;
            InitializeColumnNames();
        }

        public InsightViewModel()
        {
        }

        private void InitializeColumnNames()
        {
            if (FlowBlockOutDatasets.Any())
            {
                ColumnNames = FlowBlockOutDatasets.First().FieldValueMappings.Select(fvm => fvm.Field.Name).ToList();
            }
        }
    }
}
