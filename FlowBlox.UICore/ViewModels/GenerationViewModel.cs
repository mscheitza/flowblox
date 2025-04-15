using System;
using FlowBlox.Core.Models.Testing;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.UICore.Models;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Utilities;
using System.Linq;
using System.Windows;
using FlowBlox.UICore.Commands;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class GenerationViewModel : INotifyPropertyChanged
    {
        private FlowBloxTestDefinition _testDefinition;
        private BaseFlowBlock _currentFlowBlock;
        private ObservableCollection<RuntimeLog> _runtimeLogs;

        public GenerationViewModel()
        {
            OpenInEditorCommand = new RelayCommand(OpenInEditor);
            RuntimeLogs = new ObservableCollection<RuntimeLog>();
            BindingOperations.EnableCollectionSynchronization(RuntimeLogs, new object());
        }

        public ICommand OpenInEditorCommand { get; }

        public FlowBloxTestDefinition TestDefinition
        {
            get => _testDefinition;
            set
            {
                if (_testDefinition != value)
                {
                    _testDefinition = value;
                    OnPropertyChanged(nameof(TestDefinition));
                }
            }
        }

        public BaseFlowBlock CurrentFlowBlock
        {
            get => _currentFlowBlock;
            set
            {
                if (_currentFlowBlock != value)
                {
                    _currentFlowBlock = value;
                    OnPropertyChanged(nameof(CurrentFlowBlock));
                }
            }
        }

        public ObservableCollection<RuntimeLog> RuntimeLogs
        {
            get => _runtimeLogs;
            set
            {
                if (_runtimeLogs != value)
                {
                    _runtimeLogs = value;
                    OnPropertyChanged(nameof(RuntimeLogs));
                }
            }
        }

        public void Generate()
        {
            RuntimeLogs.Clear();
            FlowBlockGenerationStrategyExecutor generationStrategyExecutor = new FlowBlockGenerationStrategyExecutor(CurrentFlowBlock, TestScope.All);
            generationStrategyExecutor.LogCreated += GenerationStrategyExecutor_LogCreated;
            generationStrategyExecutor.ExecuteGeneration();
        }

        private void OpenInEditor(object target)
        {
            if (target is RuntimeLog runtimeLog)
                FlowBloxEditingHelper.OpenUsingEditor(runtimeLog.Message, $"{runtimeLog.Timestamp}_{runtimeLog.LogLevel}");
        }

        private void GenerationStrategyExecutor_LogCreated(object sender, LogCreatedEventArgs e)
        {
            var log = new RuntimeLog
            {
                Timestamp = DateTime.Now,
                Message = e.Message,
                LogLevel = e.LogLevel
            };
            RuntimeLogs.Add(log);
            OnPropertyChanged(nameof(RuntimeLogs));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
