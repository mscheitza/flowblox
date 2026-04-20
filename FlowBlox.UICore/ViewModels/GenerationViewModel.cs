using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.Commands;
using System.Windows.Input;
using FlowBlox.Core.Models.Testing;
using MahApps.Metro.Controls;
using System.Windows;

namespace FlowBlox.UICore.ViewModels
{
    public class GenerationViewModel : INotifyPropertyChanged
    {
        private FlowBloxTestDefinition _testDefinition;
        private BaseFlowBlock _currentFlowBlock;
        private ObservableCollection<RuntimeLog> _runtimeLogs;
        private Window _ownerWindow;

        public GenerationViewModel()
        {
            OpenInEditorCommand = new RelayCommand(OpenInEditor);
            RuntimeLogs = new ObservableCollection<RuntimeLog>();
            BindingOperations.EnableCollectionSynchronization(RuntimeLogs, new object());
        }

        public ICommand OpenInEditorCommand { get; }

        public Window OwnerWindow
        {
            get => _ownerWindow;
            set => _ownerWindow = value;
        }

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

        public async Task Generate()
        {
            RuntimeLogs.Clear();
            FlowBlockGenerationStrategyExecutor generationStrategyExecutor = new FlowBlockGenerationStrategyExecutor(CurrentFlowBlock);
            generationStrategyExecutor.LogCreated += GenerationStrategyExecutor_LogCreated;

            try
            {
                await generationStrategyExecutor.ExecuteGenerationAsync();
            }
            catch (Exception ex)
            {
                await MessageBoxHelper.ShowMessageBoxAsync(
                    _ownerWindow as MetroWindow,
                    MessageBoxType.Error,
                    ex.Message);
            }
            finally
            {
                generationStrategyExecutor.LogCreated -= GenerationStrategyExecutor_LogCreated;
            }
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
