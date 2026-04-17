using FlowBlox.Core.Enums;
using FlowBlox.Core;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Converters;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Views;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FlowBlox.UICore.ViewModels
{
    public class TestDefinitionViewModel : INotifyPropertyChanged
    {
        private bool _isDirty;
        private FlowBloxTestDefinition _testDefinition;
        private BaseFlowBlock _currentFlowBlock;
        private FlowBloxTestExecutor _testExecutor;
        private Window _ownerWindow;

        public TestDefinitionViewModel()
        {
            ExecuteTestCommand = new RelayCommand(ExecuteTestAsync);
            EditConditionsCommand = new RelayCommand(EditConditions);
            EditContentCommand = new RelayCommand(EditContent);
            OpenInEditorCommand = new RelayCommand(OpenInEditor);
            AddExpectationCommand = new RelayCommand(_ => AddExpectation(), _ => SelectedConfiguration != null);
            DeleteExpectationCommand = new RelayCommand(_ => DeleteExpectation(), _ => SelectedConfiguration?.ExpectationConditions != null && SelectedExpectation != null);
            _testDefinition = new FlowBloxTestDefinition();
            SubscribeToPropertyChangeEvents(_testDefinition);
            _testExecutor = new FlowBloxTestExecutor();
            Configurations = new ObservableCollection<FlowBloxFieldTestConfiguration>();
            SortedEntries = new ObservableCollection<FlowBlockTestDataset>();
            RuntimeLogs = new ObservableCollection<RuntimeLog>();
            BindingOperations.EnableCollectionSynchronization(RuntimeLogs, new object());
        }

        public string HeaderDescription =>
            FlowBloxResourceUtil.GetLocalizedString("FlowBloxTestDefinition_Description", typeof(FlowBloxTexts));

        private void SubscribeToPropertyChangeEvents(FlowBloxTestDefinition testDefinition)
        {
            testDefinition.PropertyChanged += _testDefinition_PropertyChanged;
            foreach (var entry in testDefinition.Entries)
            {
                entry.PropertyChanged += (s, e) => IsDirty = true;
                foreach (var config in entry.FlowBloxTestConfigurations)
                {
                    config.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(FlowBloxFieldTestConfiguration.SelectionMode))
                        {
                            if (config.SelectionMode == FlowBloxTestConfigurationSelectionMode.UserInput_ExpectedValue ||
                                config.SelectionMode == FlowBloxTestConfigurationSelectionMode.First ||
                                config.SelectionMode == FlowBloxTestConfigurationSelectionMode.Index ||
                                config.SelectionMode == FlowBloxTestConfigurationSelectionMode.Last)
                            {
                                entry.Execute = true;
                            }
                        }

                        IsDirty = true;
                    };
                }
            }
        }

        private void LoadCurrentCondfigurations(FlowBloxTestDefinition testDefinition, BaseFlowBlock currentFlowBlock)
        {
            Configurations.Clear();

            if (testDefinition == null || currentFlowBlock == null)
            {
                SelectedConfiguration = null;
                return;
            }

            var configs = testDefinition.Entries
                .Where(e => e.FlowBlock == currentFlowBlock)
                .SelectMany(e => e.FlowBloxTestConfigurations)
                .ToList();

            Configurations.AddRange(configs);

            SelectedConfiguration = Configurations.FirstOrDefault();
        }

        private void AddExpectation()
        {
            if (SelectedConfiguration == null)
                return;

            if (SelectedConfiguration.ExpectationConditions == null)
                SelectedConfiguration.ExpectationConditions = new ObservableCollection<ExpectationCondition>();

            var defaultOp = Enum.GetValues(typeof(ComparisonOperator)).Cast<ComparisonOperator>().First();

            var ec = new ExpectationCondition
            {
                ExpectationConditionTarget = ExpectationConditionTarget.FirstValue,
                Index = 0,
                Operator = defaultOp,
                Value = string.Empty
            };

            SelectedConfiguration.ExpectationConditions.Add(ec);
            SelectedExpectation = ec;
            IsDirty = true;
        }

        private void DeleteExpectation()
        {
            if (SelectedConfiguration?.ExpectationConditions == null || SelectedExpectation == null)
                return;

            SelectedConfiguration.ExpectationConditions.Remove(SelectedExpectation);
            SelectedExpectation = null;
            IsDirty = true;
        }

        public List<ExpectationConditionTarget> ExpectationTargets => Enum.GetValues(typeof(ExpectationConditionTarget))
            .Cast<ExpectationConditionTarget>()
            .ToList();

        public List<ComparisonOperator> ComparisonOperators => Enum.GetValues(typeof(ComparisonOperator))
            .Cast<ComparisonOperator>()
            .ToList();

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(nameof(IsDirty));
                    OnPropertyChanged(nameof(CanApply));
                }
            }
        }

        private void _testDefinition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsDirty = true;
            if (e.PropertyName == nameof(TestDefinition.Name))
            {
                OnPropertyChanged(nameof(CanApply));
            }
        }

        public FlowBloxTestDefinition TestDefinition
        {
            get => _testDefinition;
            set
            {
                if (_testDefinition != value)
                {
                    if (_testDefinition?.Entries != null)
                        _testDefinition.Entries.CollectionChanged -= Entries_CollectionChanged;

                    _testDefinition = value;
                    _testDefinition.RecalculateRequiredFlagsAcrossDefinition();
                    SubscribeToPropertyChangeEvents(_testDefinition);

                    if (_testDefinition?.Entries != null)
                        _testDefinition.Entries.CollectionChanged += Entries_CollectionChanged;

                    RefreshSortedEntries();
                    OnPropertyChanged(nameof(TestDefinition));
                    OnPropertyChanged(nameof(CanExecute));
                }
            }
        }

        private void Entries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshSortedEntries();
        }

        private void RefreshSortedEntries()
        {
            SortedEntries.Clear();
            if (_testDefinition?.Entries == null)
                return;

            foreach (var entry in _testDefinition.Entries.Where(x => x.FlowBlock == null))
                SortedEntries.Add(entry);

            foreach (var entry in _testDefinition.Entries.Where(x => x.FlowBlock != null))
                SortedEntries.Add(entry);

            OnPropertyChanged(nameof(SortedEntries));
        }

        public ObservableCollection<FlowBlockTestDataset> SortedEntries { get; }

        

        public BaseFlowBlock CurrentFlowBlock
        {
            get => _currentFlowBlock;
            set
            {
                _currentFlowBlock = value;
                LoadCurrentOrLatestFlowBlock();
                OnPropertyChanged(nameof(CurrentFlowBlock));
            }
        }

        private readonly FlowBloxTestDefinitionLatestFlowBlockResolver _latestResolver = new FlowBloxTestDefinitionLatestFlowBlockResolver();

        private void LoadCurrentOrLatestFlowBlock()
        {
            var targetFlowBlock = CurrentFlowBlock ?? _latestResolver.ResolveLatestFlowBlock(_testDefinition);
            LoadCurrentCondfigurations(_testDefinition, targetFlowBlock);
            LoadCapturedFlowBlocks(targetFlowBlock);
        }

        private void LoadCapturedFlowBlocks(BaseFlowBlock currentFlowBlock)
        {
            FlowBloxTestCapture flowBloxCapture = new FlowBloxTestCapture();
            FlowBloxRegistry registry = FlowBloxRegistryProvider.GetRegistry();
            var capturedFlowBlocks = flowBloxCapture.CreateCapture(registry.GetStartFlowBlock(), currentFlowBlock);
            flowBloxCapture.CreateCapture(registry.GetStartFlowBlock(), currentFlowBlock);
            CapturedFlowBlocks = flowBloxCapture.GetCapturedFlowBlocks();
        }

        private IEnumerable<BaseFlowBlock> _capturedFlowBlocks;
        public IEnumerable<BaseFlowBlock> CapturedFlowBlocks
        {
            get => _capturedFlowBlocks;
            set
            {
                _capturedFlowBlocks = value;
                OnPropertyChanged(nameof(CapturedFlowBlocks));
            }
        }

        public ObservableCollection<FlowBloxFieldTestConfiguration> Configurations { get; }

        private FlowBloxFieldTestConfiguration _selectedConfiguration;
        public FlowBloxFieldTestConfiguration SelectedConfiguration
        {
            get => _selectedConfiguration;
            set
            {
                if (_selectedConfiguration != value)
                {
                    _selectedConfiguration = value;
                    OnPropertyChanged(nameof(SelectedConfiguration));
                }
            }
        }

        private ExpectationCondition _selectedExpectation;
        public ExpectationCondition SelectedExpectation
        {
            get => _selectedExpectation;
            set
            {
                if (_selectedExpectation != value)
                {
                    _selectedExpectation = value;
                    OnPropertyChanged(nameof(SelectedExpectation));
                }
            }
        }

        public ObservableCollection<RuntimeLog> RuntimeLogs { get; set; }

        public RelayCommand ExecuteTestCommand { get; }
        public RelayCommand EditConditionsCommand { get; }
        public RelayCommand EditContentCommand { get; }
        public RelayCommand OpenInEditorCommand { get; }
        public RelayCommand AddExpectationCommand { get; }
        public RelayCommand DeleteExpectationCommand { get; }

        private int _selectedResultTabIndex;
        public int SelectedResultTabIndex
        {
            get => _selectedResultTabIndex;
            set
            {
                if (_selectedResultTabIndex != value)
                {
                    _selectedResultTabIndex = value;
                    OnPropertyChanged(nameof(SelectedResultTabIndex));
                }
            }
        }

        public List<string> TestResultsColumnNames { get; private set; }
        public ObservableCollection<FlowBlockOutDataset> TestResults { get; private set; }
        public ObservableCollection<DataGridColumn> TestResultsColumns { get; private set; }

        private async void ExecuteTestAsync()
        {
            RuntimeLogs.Clear();
            SelectedResultTabIndex = 1;

            var includedFlowBlocks = _testDefinition?.Entries?
                .Select(x => x.FlowBlock)
                .ExceptNull()
                .ToList();

            try
            {
                _testExecutor.Initialize(_testDefinition, _currentFlowBlock, includedFlowBlocks);
            }
            catch (Exception ex)
            {
                var messageTemplate = FlowBloxResourceUtil.GetLocalizedString(
                    "Message_RuntimeInitializationFailed",
                    typeof(Resources.TestDefinitionView));
                var message = string.Format(messageTemplate, ex.Message);

                await MessageBoxHelper.ShowMessageBoxAsync(_ownerWindow as MetroWindow, MessageBoxType.Error, message);
                return;
            }

            _testExecutor.GetRuntime().LogMessageCreated += TestDefinitionViewModel_LogMessageCreated;
            await _testExecutor.ExecuteTestAsync();

            _testExecutor.Shutdown();

            if (_currentFlowBlock is BaseResultFlowBlock resultFlowBlock && resultFlowBlock.GridElementResult?.ResultCount > 0)
            {
                TestResults = new ObservableCollection<FlowBlockOutDataset>(resultFlowBlock.GridElementResult.Results);
                TestResultsColumnNames = resultFlowBlock.GridElementResult.Results
                    .First()
                    .FieldValueMappings
                    .Select(fvm => fvm.Field.Name)
                    .ToList();

                OnPropertyChanged(nameof(TestResultsColumnNames));
                OnPropertyChanged(nameof(TestResults));
                UpdateTestResultsColumns();
            }
        }

        private void UpdateTestResultsColumns()
        {
            var columns = TestResultsColumnNames
                .Select(columnName => new DataGridTextColumn
                {
                    Header = columnName,
                    Binding = new Binding
                    {
                        Path = new PropertyPath("."),
                        Converter = new FlowBlockOutDatasetToValueConverter(),
                        ConverterParameter = columnName
                    },
                    IsReadOnly = true,
                    Width = 200
                })
                .ToList();

            TestResultsColumns = new ObservableCollection<DataGridColumn>(columns);
            OnPropertyChanged(nameof(TestResultsColumns));
        }

        public bool CanApply
        {
            get
            {
                if (!IsDirty)
                    return false;

                if (string.IsNullOrEmpty(_testDefinition.Name))
                    return false;
                
                return true;
            }
        }

        public bool CanExecute
        {
            get
            {
                return true;
            }
        }

        public Window OwnerWindow
        {
            get
            {
                return _ownerWindow;
            }
            set
            {
                _ownerWindow = value;
            }
        }

        private void TestDefinitionViewModel_LogMessageCreated(BaseRuntime runtime, string message, FlowBloxLogLevel logLevel)
        {
            var log = new RuntimeLog
            {
                Timestamp = DateTime.Now,
                Message = message,
                LogLevel = logLevel
            };
            RuntimeLogs.Add(log);
            OnPropertyChanged(nameof(RuntimeLogs));
        }

        private void EditConditions(object target)
        {
            var flowBloxTestConfiguration = target as FlowBloxFieldTestConfiguration;
            if (flowBloxTestConfiguration == null)
                return;

            var propertyView = new Views.PropertyWindow(new PropertyWindowArgs(flowBloxTestConfiguration, readOnly: false))
            {
                Owner = _ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (propertyView.ShowDialog() == true)
            { 
                flowBloxTestConfiguration.ExpectationConditions = flowBloxTestConfiguration.ExpectationConditions;
                OnPropertyChanged(nameof(TestDefinition));
            }
        }

        private void EditContent(object target)
        {
            var configuration = target as FlowBloxFieldTestConfiguration;
            if (configuration == null)
                return;

            var view = new EditContentView(configuration.UserInput)
            {
                Owner = _ownerWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            
            var result = view.ShowDialog();
            if (result == true)
            {
                configuration.UserInput = view.ContentText;
            }
        }

        private void OpenInEditor(object target)
        {
            if (target is FlowBlockOutDataset outDataset)
            {
                if (outDataset.FieldValueMappings.Count > 1)
                {
                    var multiValueSelectionDialog = new Views.MultiValueSelectionDialog("Field selection", "", new GenericSelectionHandler<FlowBlockOutDatasetFieldValueMapping>(outDataset.FieldValueMappings, t => t.Field.Name));
                    multiValueSelectionDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    multiValueSelectionDialog.Owner = _ownerWindow;
                    if (multiValueSelectionDialog.ShowDialog() == true)
                    {
                        var selectedFieldValueMapping = (FlowBlockOutDatasetFieldValueMapping)multiValueSelectionDialog.SelectedItem?.Value;
                        if (selectedFieldValueMapping != null)
                        {
                            FlowBloxEditingHelper.OpenUsingEditor(selectedFieldValueMapping.Value, selectedFieldValueMapping.Field.FullyQualifiedName);
                        }
                    }
                }
                else if (outDataset.FieldValueMappings.Count == 1)
                {
                    var selectedFieldValueMapping = outDataset.FieldValueMappings.First();
                    FlowBloxEditingHelper.OpenUsingEditor(selectedFieldValueMapping.Value, selectedFieldValueMapping.Field.FullyQualifiedName);
                }
            }
            else if (target is RuntimeLog runtimeLog)
            {
                FlowBloxEditingHelper.OpenUsingEditor(runtimeLog.Message, $"{runtimeLog.Timestamp}_{runtimeLog.LogLevel}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
