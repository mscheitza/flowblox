using FlowBlox.Core.Models.Testing;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Commands;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using System.ComponentModel;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Runtime;
using System;
using System.Windows.Data;
using System.Collections.Generic;
using FlowBlox.UICore.Converters.TestDefinition;
using System.Linq;
using System.Windows.Controls;
using FlowBlox.UICore.Converters.Insight;
using System.Windows;
using FlowBlox.UICore.Converters;
using FlowBlox.UICore.Resources;
using FlowBlox.UICore.Views;
using FlowBlox.UICore.Utilities;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Models;

namespace FlowBlox.UICore.ViewModels
{
    public class TestDefinitionViewModel : INotifyPropertyChanged
    {
        private bool _isDirty;
        private FlowBloxTestDefinition _testDefinition;
        private BaseFlowBlock _currentFlowBlock;
        private FlowBloxTestExecutor _testExecutor;
        private List<BaseFlowBlock> _testDefinitionUsages;
        private Window _ownerWindow;

        public TestDefinitionViewModel()
        {
            ExecuteTestCommand = new RelayCommand(ExecuteTest);
            EditConditionsCommand = new RelayCommand(EditConditions);
            OpenInEditorCommand = new RelayCommand(OpenInEditor);
            _testDefinition = new FlowBloxTestDefinition();
            SubscribeToPropertyChangeEvents(_testDefinition);
            _testExecutor = new FlowBloxTestExecutor();
            RuntimeLogs = new ObservableCollection<RuntimeLog>();
            BindingOperations.EnableCollectionSynchronization(RuntimeLogs, new object());
            _testDefinitionUsages = new List<BaseFlowBlock>();
        }

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
                        if (e.PropertyName == nameof(FlowBloxTestConfiguration.SelectionMode))
                        {
                            if (config.SelectionMode == FlowBloxTestConfigurationSelectionMode.UserInput_ExistingValue ||
                                config.SelectionMode == FlowBloxTestConfigurationSelectionMode.First ||
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
                    _testDefinition = value;
                    SubscribeToPropertyChangeEvents(_testDefinition);
                    OnPropertyChanged(nameof(TestDefinition));
                    OnPropertyChanged(nameof(CanExecute));
                }
            }
        }

        public BaseFlowBlock CurrentFlowBlock
        {
            get => _currentFlowBlock;
            set
            {
                _currentFlowBlock = value;
                OnPropertyChanged(nameof(CurrentFlowBlock));
            }
        }

        public List<BaseFlowBlock> TestDefinitionUsages
        {
            get => _testDefinitionUsages;
            set
            {
                _testDefinitionUsages = value;
                OnPropertyChanged(nameof(TestDefinitionUsages));
            }
        }

        public ObservableCollection<FlowBloxTestConfigurationSelectionMode> SelectionModes { get; set; } = new ObservableCollection<FlowBloxTestConfigurationSelectionMode>
        {
            FlowBloxTestConfigurationSelectionMode.UserInput,
            FlowBloxTestConfigurationSelectionMode.UserInput_ExistingValue,
            FlowBloxTestConfigurationSelectionMode.First,
            FlowBloxTestConfigurationSelectionMode.Index,
            FlowBloxTestConfigurationSelectionMode.Last
        };

        public ObservableCollection<RuntimeLog> RuntimeLogs { get; set; }

        public ICommand ExecuteTestCommand { get; }

        public ICommand EditConditionsCommand { get; }

        public ICommand OpenInEditorCommand { get; }

        public List<string> TestResultsColumnNames { get; private set; }
        public ObservableCollection<FlowBlockOutDataset> TestResults { get; private set; }
        public ObservableCollection<DataGridColumn> TestResultsColumns { get; private set; }

        private void ExecuteTest()
        {
            RuntimeLogs.Clear();

            _testExecutor.Initialize(_testDefinition, _currentFlowBlock);
            _testExecutor.GetRuntime().LogMessageCreated += TestDefinitionViewModel_LogMessageCreated;
            _testExecutor.ExecuteTest();
            _testExecutor.Shutdown();

            if (_currentFlowBlock is BaseResultFlowBlock resultFlowBlock && resultFlowBlock.GridElementResult.ResultCount > 0)
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
            var flowBloxTestConfiguration = target as FlowBloxTestConfiguration;
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
