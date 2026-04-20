using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.FlowBlocks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.Testing
{
    [Display(Name = "FlowBloxTestDefinition_DisplayName", Description = "FlowBloxTestDefinition_Description", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("FlowBloxTestDefinition_DisplayName_Plural", typeof(FlowBloxTexts))]
    public class FlowBloxTestDefinition : ManagedObject
    {
        private ObservableCollection<FlowBlockTestDataset> _entries;

        [Display(Name = "FlowBloxTestDefinition_Entries", Description = "FlowBloxTestDefinition_Entries_Description", ResourceType = typeof(FlowBloxTexts))]
        public ObservableCollection<FlowBlockTestDataset> Entries
        {
            get => _entries;
            set
            {
                if (!ReferenceEquals(_entries, value))
                {
                    if (_entries != null)
                        _entries.CollectionChanged -= TestDatasets_CollectionChanged;

                    _entries = value ?? new ObservableCollection<FlowBlockTestDataset>();
                    _entries.CollectionChanged += TestDatasets_CollectionChanged;

                    OnPropertyChanged();
                    RecalculateRequiredFlagsAcrossDefinition();
                }
            }
        }

        private void TestDatasets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RecalculateRequiredFlagsAcrossDefinition();
        }

        /// <summary>
        /// Aggregates all required fields and required associated-flow-block datasets for active entries.
        /// Sets/removes UI-required flags on datasets/configurations.
        /// </summary>
        public void RecalculateRequiredFlagsAcrossDefinition()
        {
            if (!IsLoaded)
                return;

            var executedDatasets = Entries
                .Where(ds => ds.Execute && ds.FlowBlock != null)
                .ToList();

            var requiredAssociatedFlowBlocks = executedDatasets
                .SelectMany(ds => ResolveAssociatedFlowBlocks(ds.FlowBlock))
                .ToHashSet();

            var requiredFQNames = executedDatasets
                .SelectMany(ds => ds.FlowBlock.GetRequiredFields())
                .Select(f => f.FullyQualifiedName)
                .Where(fqn => !string.IsNullOrWhiteSpace(fqn))
                .ToHashSet();

            foreach (var dataset in Entries)
            {
                dataset.UIRequiredForExecution = dataset.FlowBlock != null
                    && requiredAssociatedFlowBlocks.Contains(dataset.FlowBlock);
            }

            foreach (var cfg in Entries.SelectMany(ds => ds.FlowBloxTestConfigurations))
            {
                var fQFieldName = cfg.FieldElement?.FullyQualifiedName;
                var isRequired = !string.IsNullOrWhiteSpace(fQFieldName)
                    && requiredFQNames.Contains(fQFieldName);
                cfg.UIRequiredForExecution = isRequired;
            }
        }

        private static IEnumerable<BaseFlowBlock> ResolveAssociatedFlowBlocks(BaseFlowBlock flowBlock)
        {
            foreach (var property in AssociatedFlowBlockResolver.GetResolvableProperties(flowBlock))
            {
                var resolution = AssociatedFlowBlockResolver.Resolve(flowBlock, property);
                if (resolution.FlowBlock != null)
                    yield return resolution.FlowBlock;
            }
        }

        [Display(Name = "FlowBloxTestDefinition_RequiredForExecution", Description = "FlowBloxTestDefinition_RequiredForExecution_Description", ResourceType = typeof(FlowBloxTexts))]
        public bool RequiredForExecution { get; set; }

        public FlowBloxTestDefinition()
        {
            Entries = new ObservableCollection<FlowBlockTestDataset>();
        }

        public override List<string> GetDisplayableProperties()
            => [nameof(Name)];
    }
}
