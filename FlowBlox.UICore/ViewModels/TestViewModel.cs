using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Events;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels.TestView;
using FlowBlox.UICore.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class TestViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly SynchronizationContext? _uiContext;
        private readonly IFlowBloxMessageBoxService _messageBoxService;
        private readonly IDialogService _dialogService;

        private readonly List<TestCaseEntryViewModel> _selectedEntries = new();
        private FlowBloxRegistry? _registry;
        private bool _isReadOnly;
        private string _summaryText = string.Empty;

        public ObservableCollection<TestCaseEntryViewModel> TestCases { get; } = new();

        public RelayCommand RefreshCommand { get; }
        public RelayCommand CreateCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand RunCommand { get; }
        public RelayCommand OpenProtocolCommand { get; }
        public RelayCommand ResetStatusCommand { get; }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            private set
            {
                if (_isReadOnly == value)
                    return;

                _isReadOnly = value;
                OnPropertyChanged();
                InvalidateCommands();
            }
        }

        public string SummaryText
        {
            get => _summaryText;
            private set
            {
                if (_summaryText == value)
                    return;

                _summaryText = value;
                OnPropertyChanged();
            }
        }

        public bool HasTestCases => TestCases.Count > 0;

        public TestViewModel()
        {
            _uiContext = SynchronizationContext.Current;
            _messageBoxService = FlowBloxServiceLocator.Instance.GetService<IFlowBloxMessageBoxService>();
            _dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();

            RefreshCommand = new RelayCommand(Refresh);
            CreateCommand = new RelayCommand(CreateTestCase, CanCreateTestCase);
            EditCommand = new RelayCommand(EditTestCase, CanEditTestCase);
            DeleteCommand = new RelayCommand(DeleteTestCases, CanDeleteTestCases);
            RunCommand = new RelayCommand(RunSelectedOrAllTestsAsync, CanRunTests);
            OpenProtocolCommand = new RelayCommand(OpenProtocol, CanOpenProtocol);
            ResetStatusCommand = new RelayCommand(ResetStatus, CanResetStatus);

            FlowBloxProjectManager.Instance.ProjectChanged += ProjectManager_ProjectChanged;
            RebindAndRefresh();
        }

        public void OnAfterUIRegistryInitialized() => RebindAndRefresh();

        public void SetRuntimeActive(bool isRuntimeActive) => IsReadOnly = isRuntimeActive;

        public void UpdateSelection(IEnumerable<TestCaseEntryViewModel> selectedEntries)
        {
            _selectedEntries.Clear();
            foreach (var entry in selectedEntries ?? Enumerable.Empty<TestCaseEntryViewModel>())
            {
                if (entry != null)
                    _selectedEntries.Add(entry);
            }

            foreach (var row in TestCases)
            {
                row.IsSelected = _selectedEntries.Contains(row);
            }

            InvalidateCommands();
        }

        private void ProjectManager_ProjectChanged(object? sender, ProjectChangedEventArgs e)
            => RebindAndRefresh();

        private void RebindAndRefresh()
        {
            Unsubscribe();

            _registry = FlowBloxRegistryProvider.GetRegistry();
            if (_registry != null)
            {
                _registry.OnManagedObjectAdded += Registry_OnManagedObjectAdded;
                _registry.OnManagedObjectRemoved += Registry_OnManagedObjectRemoved;
            }

            Refresh();
        }

        private void Registry_OnManagedObjectAdded(ManagedObjectAddedEventArgs eventArgs)
        {
            if (eventArgs?.AddedObject is not FlowBloxTestDefinition)
                return;

            PostToUi(Refresh);
        }

        private void Registry_OnManagedObjectRemoved(ManagedObjectRemovedEventArgs eventArgs)
        {
            if (eventArgs?.RemovedObject is not FlowBloxTestDefinition)
                return;

            PostToUi(Refresh);
        }

        private void Refresh()
        {
            var previouslySelectedDefinitions = _selectedEntries
                .Select(x => x.TestDefinition)
                .Where(x => x != null)
                .ToHashSet();

            var currentState = TestCases.ToDictionary(
                x => x.TestDefinition,
                x => new TestCaseSnapshot(x.Status, x.ProtocolPath));

            TestCases.Clear();
            _selectedEntries.Clear();

            if (_registry != null)
            {
                var allTests = _registry.GetManagedObjects<FlowBloxTestDefinition>()
                    .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                foreach (var testDefinition in allTests)
                {
                    var row = new TestCaseEntryViewModel(testDefinition)
                    {
                        Name = testDefinition.Name ?? string.Empty,
                        RequiredForExecution = testDefinition.RequiredForExecution,
                        RequiredFor = BuildRequiredForText(testDefinition),
                        DefinedAt = BuildDefinedAtText(testDefinition)
                    };

                    if (currentState.TryGetValue(testDefinition, out var snapshot))
                    {
                        row.Status = snapshot.Status;
                        row.ProtocolPath = snapshot.ProtocolPath;
                    }

                    if (previouslySelectedDefinitions.Contains(testDefinition))
                    {
                        row.IsSelected = true;
                        _selectedEntries.Add(row);
                    }

                    TestCases.Add(row);
                }
            }

            UpdateSummary();
            OnPropertyChanged(nameof(HasTestCases));
            InvalidateCommands();
        }

        private string BuildRequiredForText(FlowBloxTestDefinition testDefinition)
        {
            if (_registry == null || testDefinition == null || !testDefinition.RequiredForExecution)
                return Resources.TestView.Text_None;

            var requiredFlowBlocks = _registry.GetFlowBlocks()
                .Where(x => x.TestDefinitions.Contains(testDefinition))
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            if (requiredFlowBlocks.Count == 0)
                return Resources.TestView.Text_None;

            return string.Join(", ", requiredFlowBlocks);
        }

        private static string BuildDefinedAtText(FlowBloxTestDefinition testDefinition)
        {
            if (testDefinition?.Entries == null)
                return Resources.TestView.Text_None;

            var flowBlockNames = testDefinition.Entries
                .Where(x => x.FlowBlock != null)
                .Select(x => x.FlowBlock.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            if (flowBlockNames.Count == 0)
                return Resources.TestView.Text_None;

            return string.Join(", ", flowBlockNames);
        }

        private void CreateTestCase()
        {
            if (_registry == null)
                return;

            var newTestDefinition = new FlowBloxTestDefinition();
            _registry.PostProcessManagedObjectCreated(newTestDefinition);

            var view = new TestDefinitionView(newTestDefinition, currentFlowBlock: null);
            var result = _dialogService.ShowWPFDialog(view, isModal: true);
            if (result == true)
            {
                _registry.RegisterManagedObject(newTestDefinition);
            }
        }

        private bool CanCreateTestCase() => !IsReadOnly;

        private void EditTestCase()
        {
            if (_selectedEntries.Count != 1)
                return;

            var selectedTest = _selectedEntries[0].TestDefinition;
            if (selectedTest == null)
                return;

            var view = new TestDefinitionView(selectedTest, currentFlowBlock: null);
            _dialogService.ShowWPFDialog(view, isModal: true);
            Refresh();
        }

        private bool CanEditTestCase()
            => _selectedEntries.Count == 1 && _selectedEntries[0].TestDefinition != null;

        private void DeleteTestCases()
        {
            if (_registry == null)
                return;

            var targets = ResolveSelectionOrAll();
            if (targets.Count == 0)
                return;

            var confirmResult = _messageBoxService.ShowMessageBox(
                string.Format(Resources.TestView.Message_DeleteConfirm_Description, targets.Count),
                Resources.TestView.Message_DeleteConfirm_Title,
                FlowBloxMessageBoxTypes.Question);

            if (confirmResult != FlowBloxMessageBoxDialogResult.Yes)
                return;

            foreach (var target in targets)
            {
                DeleteProtocolFile(target);
                _registry.Unregister(target.TestDefinition);
            }
        }

        private bool CanDeleteTestCases()
            => !IsReadOnly && ResolveSelectionOrAll().Count > 0;

        private async void RunSelectedOrAllTestsAsync()
        {
            if (IsReadOnly)
                return;

            var targets = ResolveSelectionOrAll();
            if (targets.Count == 0)
                return;

            foreach (var target in targets)
            {
                target.Status = TestCaseStatus.Running;
                DeleteProtocolFile(target);
            }

            UpdateSummary();
            InvalidateCommands();

            foreach (var target in targets)
            {
                var executionResult = await ExecuteTestAsync(target.TestDefinition);
                target.Status = executionResult.Success ? TestCaseStatus.Succeeded : TestCaseStatus.Failed;
                target.ProtocolPath = executionResult.ProtocolPath;
                UpdateSummary();
            }

            InvalidateCommands();
        }

        private bool CanRunTests()
            => !IsReadOnly && TestCases.Count > 0;

        private async Task<TestExecutionResult> ExecuteTestAsync(FlowBloxTestDefinition testDefinition)
        {
            if (testDefinition == null)
                return new TestExecutionResult(false, null);

            var logs = new List<(DateTime Timestamp, FlowBloxLogLevel Level, string Message)>();
            var testExecutor = new FlowBloxTestExecutor();
            FlowBloxTestResult? testResult = null;
            BaseRuntime? runtime = null;

            try
            {
                testExecutor.Initialize(testDefinition, currentFlowBlock: null);
                runtime = testExecutor.GetRuntime();
                runtime.LogMessageCreated += Runtime_LogMessageCreated;
                testResult = await testExecutor.ExecuteTestAsync();
            }
            catch (Exception ex)
            {
                logs.Add((DateTime.Now, FlowBloxLogLevel.Error, ex.ToString()));
            }
            finally
            {
                if (runtime != null)
                    runtime.LogMessageCreated -= Runtime_LogMessageCreated;

                try
                {
                    testExecutor.Shutdown();
                }
                catch
                {
                    // ignore cleanup errors to keep status reporting robust
                }
            }

            var protocolPath = WriteProtocolToTempFile(testDefinition, logs);
            return new TestExecutionResult(testResult?.Success == true, protocolPath);

            void Runtime_LogMessageCreated(BaseRuntime runtime, string message, FlowBloxLogLevel logLevel)
            {
                logs.Add((DateTime.Now, logLevel, message));
            }
        }

        private static string? WriteProtocolToTempFile(
            FlowBloxTestDefinition testDefinition,
            IEnumerable<(DateTime Timestamp, FlowBloxLogLevel Level, string Message)> logs)
        {
            var snapshot = logs?.ToList() ?? new List<(DateTime Timestamp, FlowBloxLogLevel Level, string Message)>();
            if (snapshot.Count == 0)
                return null;

            var directory = Path.Combine(Path.GetTempPath(), "FlowBlox", "tests");
            Directory.CreateDirectory(directory);

            var safeTestName = IOUtil.GetValidFileName(testDefinition?.Name ?? "test");
            var path = Path.Combine(directory, $"{safeTestName}_{DateTime.Now:yyyyMMdd_HHmmss}.log.txt");

            var builder = new StringBuilder();
            foreach (var entry in snapshot)
            {
                builder.Append('[')
                    .Append(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Append("] [")
                    .Append(entry.Level)
                    .Append("] ")
                    .AppendLine(entry.Message ?? string.Empty);
            }

            File.WriteAllText(path, builder.ToString());
            return path;
        }

        private void OpenProtocol()
        {
            if (_selectedEntries.Count != 1)
                return;

            var path = _selectedEntries[0].ProtocolPath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            FlowBloxEditingHelper.OpenUsingEditor(path);
        }

        private bool CanOpenProtocol()
        {
            if (_selectedEntries.Count != 1)
                return false;

            var path = _selectedEntries[0].ProtocolPath;
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        }

        private void ResetStatus()
        {
            var targets = ResolveSelectionOrAll();
            if (targets.Count == 0)
                return;

            foreach (var target in targets)
            {
                DeleteProtocolFile(target);
                target.Status = TestCaseStatus.None;
            }

            UpdateSummary();
            InvalidateCommands();
        }

        private bool CanResetStatus()
            => ResolveSelectionOrAll().Any(x => x.Status != TestCaseStatus.None || !string.IsNullOrWhiteSpace(x.ProtocolPath));

        private void DeleteProtocolFile(TestCaseEntryViewModel entry)
        {
            if (entry == null)
                return;

            var path = entry.ProtocolPath;
            entry.ProtocolPath = null;
            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // ignore cleanup errors
            }
        }

        private List<TestCaseEntryViewModel> ResolveSelectionOrAll()
        {
            if (_selectedEntries.Count > 0)
                return _selectedEntries.ToList();

            return TestCases.ToList();
        }

        private void UpdateSummary()
        {
            var successful = TestCases.Count(x => x.Status == TestCaseStatus.Succeeded);
            var failed = TestCases.Count(x => x.Status == TestCaseStatus.Failed);
            var total = TestCases.Count;

            SummaryText = string.Format(Resources.TestView.StatusSummary_Format, successful, failed, total);
        }

        private void InvalidateCommands()
        {
            CreateCommand.Invalidate();
            EditCommand.Invalidate();
            DeleteCommand.Invalidate();
            RunCommand.Invalidate();
            OpenProtocolCommand.Invalidate();
            ResetStatusCommand.Invalidate();
        }

        private void PostToUi(Action action)
        {
            if (action == null)
                return;

            if (_uiContext != null && _uiContext != SynchronizationContext.Current)
            {
                _uiContext.Post(_ => action(), null);
                return;
            }

            action();
        }

        private void Unsubscribe()
        {
            if (_registry != null)
            {
                _registry.OnManagedObjectAdded -= Registry_OnManagedObjectAdded;
                _registry.OnManagedObjectRemoved -= Registry_OnManagedObjectRemoved;
            }
        }

        public void Dispose()
        {
            FlowBloxProjectManager.Instance.ProjectChanged -= ProjectManager_ProjectChanged;
            Unsubscribe();
            _selectedEntries.Clear();
            TestCases.Clear();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private readonly record struct TestCaseSnapshot(TestCaseStatus Status, string? ProtocolPath);
        private readonly record struct TestExecutionResult(bool Success, string? ProtocolPath);
    }
}
