using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    [Display(Name = "FlowBloxTestDefinition_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("FlowBloxTestDefinition_DisplayName_Plural", typeof(FlowBloxTexts))]
    public class FlowBloxTestDefinition : ManagedObject, INotifyPropertyChanged
    {
        private ObservableCollection<FlowBlockTestDataset> _entries;
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
        /// Aggregates all required fields from all active datasets/FlowBlocks
        /// and sets/removes the RequiredForExecution flag on all configurations.
        /// </summary>
        public void RecalculateRequiredFlagsAcrossDefinition()
        {
            if (!IsLoaded)
                return;

            var requiredFQNames = this.Entries
                .Where(ds => ds.Execute && ds.FlowBlock != null)
                .SelectMany(ds => ds.FlowBlock.GetRequiredFields())
                .Select(f => f.FullyQualifiedName)
                .Where(fqn => !string.IsNullOrWhiteSpace(fqn))
                .ToHashSet();

            foreach (var cfg in this.Entries.SelectMany(ds => ds.FlowBloxTestConfigurations))
            {
                var fQFieldName = cfg.FieldElement.FullyQualifiedName;
                var isRequired = requiredFQNames.Contains(fQFieldName);
                cfg.RequiredForExecution = isRequired;
            }
        }

        [Display(Name = "FlowBloxTestDefinition_RequiredForExecution", ResourceType = typeof(FlowBloxTexts))]
        public bool RequiredForExecution { get; set; }

        public FlowBloxTestDefinition()
        {
            Entries = new ObservableCollection<FlowBlockTestDataset>();
        }

        public override List<string> GetDisplayableProperties()
            => [nameof(Name)];
    }
}
