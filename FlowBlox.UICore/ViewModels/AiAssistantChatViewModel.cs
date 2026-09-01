using FlowBlox.AIAssistant.History;
using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Logging;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Utilities;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace FlowBlox.UICore.ViewModels
{
    public class AiAssistantChatViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AiAssistantService _service;
        private readonly IFlowBloxMessageBoxService _messageBoxService;
        private readonly IRuntimeStateService _runtimeStateService;
        private readonly SynchronizationContext? _uiContext;
        private CancellationTokenSource? _cts;
        private string _currentInput = string.Empty;
        private bool _isBusy;
        private bool _isRuntimeActive;
        private Func<AIAssistantProjectStateSnapshot?>? _captureProjectState;
        private Func<AIAssistantProjectStateSnapshot, Task<bool>>? _restoreProjectState;
        private AIAssistantProjectStateSnapshot? _stateBeforeLastPrompt;
        private AIAssistantProjectStateSnapshot? _stateAfterLastPrompt;
        private bool _isPromptStateUndone;
        private readonly AiAssistantHistoryStore _historyStore = AiAssistantHistoryStore.Instance;
        private AiAssistantHistoryDocument? _currentHistory;
        private string? _currentHistoryFilePath;
        private bool _canGoBackToHistory;
        private int _estimatedUsedTokens;

        public ObservableCollection<AssistantTranscriptLine> Transcript { get; } = new ObservableCollection<AssistantTranscriptLine>();
        public RelayCommand NewHistoryCommand { get; }
        public RelayCommand BackToHistoryCommand { get; }
        public RelayCommand SubmitCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand CopyTranscriptEntryCommand { get; }
        public RelayCommand OpenTranscriptEntryInEditorCommand { get; }
        public RelayCommand OpenCommunicationProtocolDirectoryCommand { get; }
        public RelayCommand UndoProjectStateCommand { get; }
        public RelayCommand RedoProjectStateCommand { get; }
        public RelayCommand ResetTokenUsageCommand { get; }

        public string CurrentInput
        {
            get => _currentInput;
            set
            {
                if (_currentInput != value)
                {
                    _currentInput = value;
                    OnPropertyChanged(nameof(CurrentInput));
                    SubmitCommand.Invalidate();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    OnPropertyChanged(nameof(CanEditInput));
                    SubmitCommand.Invalidate();
                    StopCommand.Invalidate();
                    NewHistoryCommand.Invalidate();
                    BackToHistoryCommand.Invalidate();
                    RefreshUndoRedoState();
                }
            }
        }

        public bool CanEditInput => !IsBusy;
        public bool CanGoBackToHistory
        {
            get => !IsBusy && _canGoBackToHistory;
            set
            {
                if (_canGoBackToHistory == value)
                    return;

                _canGoBackToHistory = value;
                OnPropertyChanged(nameof(CanGoBackToHistory));
                BackToHistoryCommand.Invalidate();
            }
        }

        public bool ShowProviderConfigurationWarning => !_isProviderConfigured;
        public bool ShowIntroHeader => Transcript.Count == 0;
        public int EstimatedUsedTokens
        {
            get => _estimatedUsedTokens;
            private set
            {
                if (_estimatedUsedTokens == value)
                    return;

                _estimatedUsedTokens = Math.Max(0, value);
                OnPropertyChanged(nameof(EstimatedUsedTokens));
                OnPropertyChanged(nameof(HasEstimatedUsedTokens));
                OnPropertyChanged(nameof(EstimatedUsedTokensText));
                ResetTokenUsageCommand?.Invalidate();
            }
        }

        public bool HasEstimatedUsedTokens => EstimatedUsedTokens > 0;
        public string EstimatedUsedTokensText => string.Format(
            CultureInfo.CurrentCulture,
            FlowBloxResourceUtil.GetLocalizedString("TokenMonitor_Format", typeof(Resources.AiAssistantChatView)),
            EstimatedUsedTokens);

        public bool CanUndoProjectState =>
            !IsBusy &&
            _stateBeforeLastPrompt != null &&
            _stateAfterLastPrompt != null &&
            !_isPromptStateUndone;

        public bool CanRedoProjectState =>
            !IsBusy &&
            _stateBeforeLastPrompt != null &&
            _stateAfterLastPrompt != null &&
            _isPromptStateUndone;

        public event EventHandler<FlowBlocksChangedEventArgs>? FlowBlocksChanged;
        public event EventHandler<FlowBlocksConnectionsChangedEventArgs>? FlowBlocksConnectionsChanged;
        public event EventHandler<FlowBlocksLayoutChangedEventArgs>? BeforeFlowBlocksLayoutChanged;
        public event EventHandler<FlowBlocksLayoutChangedEventArgs>? FlowBlocksLayoutChanged;
        public event EventHandler? NewHistoryRequested;
        public event EventHandler? HistoryRequested;
        private bool _isProviderConfigured;

        public AiAssistantChatViewModel()
        {
            _uiContext = SynchronizationContext.Current;
            _messageBoxService = FlowBloxServiceLocator.Instance.GetService<IFlowBloxMessageBoxService>();
            _runtimeStateService = FlowBloxServiceLocator.Instance.GetService<IRuntimeStateService>();
            var toolApi = new DefaultToolApi
            {
                ToolExecutionConfirmationCallback = ConfirmToolExecutionRequest
            };
            _service = new AiAssistantService(
                new AiProviderExecutor(),
                toolApi,
                FlowBloxLogManager.Instance.GetLogger());
            _service.FlowBlocksChanged += Service_FlowBlocksChanged;
            _service.FlowBlocksConnectionsChanged += Service_FlowBlocksConnectionsChanged;
            _service.BeforeFlowBlocksLayoutChanged += Service_BeforeFlowBlocksLayoutChanged;
            _service.FlowBlocksLayoutChanged += Service_FlowBlocksLayoutChanged;
            _service.TranscriptLineAdded += Service_TranscriptLineAdded;
            _service.EstimatedUsedTokensChanged += Service_EstimatedUsedTokensChanged;

            NewHistoryCommand = new RelayCommand(() => NewHistoryRequested?.Invoke(this, EventArgs.Empty), () => !IsBusy);
            BackToHistoryCommand = new RelayCommand(RequestHistoryOverview, () => CanGoBackToHistory);
            SubmitCommand = new RelayCommand(async () => await SubmitAsync(), CanSubmit);
            StopCommand = new RelayCommand(Stop, () => IsBusy);
            CopyTranscriptEntryCommand = new RelayCommand(CopyTranscriptEntry);
            OpenTranscriptEntryInEditorCommand = new RelayCommand(OpenTranscriptEntryInEditor);
            OpenCommunicationProtocolDirectoryCommand = new RelayCommand(OpenCommunicationProtocolDirectory);
            UndoProjectStateCommand = new RelayCommand(async () => await UndoProjectStateAsync(), () => CanUndoProjectState);
            RedoProjectStateCommand = new RelayCommand(async () => await RedoProjectStateAsync(), () => CanRedoProjectState);
            ResetTokenUsageCommand = new RelayCommand(ResetTokenUsage, () => HasEstimatedUsedTokens);

            if (_runtimeStateService != null)
            {
                _runtimeStateService.StateChanged += RuntimeStateService_StateChanged;
                _isRuntimeActive = _runtimeStateService.IsRuntimeActive;
            }

            RefreshProviderConfigurationState();
        }

        private bool CanSubmit()
        {
            return !IsBusy && !_isRuntimeActive && !string.IsNullOrWhiteSpace(CurrentInput);
        }

        private async Task SubmitAsync()
        {
            var input = CurrentInput?.Trim();
            if (string.IsNullOrWhiteSpace(input) || IsBusy)
                return;

            EnsureCurrentHistory();
            var stateBeforePrompt = _captureProjectState?.Invoke();
            var promptCompleted = false;

            AddTranscriptLine(new AssistantTranscriptLine
            {
                Kind = AssistantTranscriptKind.User,
                Text = input,
                Timestamp = DateTime.Now
            });

            IsBusy = true;
            _runtimeStateService?.SetRuntimeStartBlocked(true);
            _cts = new CancellationTokenSource();

            try
            {
                await _service.GenerateProjectAsync(input, _cts.Token);
                promptCompleted = true;
            }
            catch (OperationCanceledException)
            {
                AddTranscriptLine(new AssistantTranscriptLine
                {
                    Kind = AssistantTranscriptKind.Status,
                    Text = "Stopped.",
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                AddTranscriptLine(new AssistantTranscriptLine
                {
                    Kind = AssistantTranscriptKind.Error,
                    Text = ex.Message,
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                SaveCurrentHistory();

                if (promptCompleted && stateBeforePrompt != null)
                {
                    var stateAfterPrompt = _captureProjectState?.Invoke();
                    if (stateAfterPrompt != null)
                    {
                        _stateBeforeLastPrompt = stateBeforePrompt;
                        _stateAfterLastPrompt = stateAfterPrompt;
                        _isPromptStateUndone = false;
                        RefreshUndoRedoState();
                    }
                }

                CurrentInput = string.Empty;
                IsBusy = false;
                _runtimeStateService?.SetRuntimeStartBlocked(false);
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void RuntimeStateService_StateChanged(object? sender, Events.RuntimeStateChangedEventArgs e)
        {
            SynchronizationContextHelper.PostToUi(_uiContext, () =>
            {
                if (_isRuntimeActive == e.IsRuntimeActive)
                    return;

                _isRuntimeActive = e.IsRuntimeActive;
                SubmitCommand.Invalidate();
            });
        }

        private void Stop()
        {
            if (!IsBusy)
                return;

            _cts?.Cancel();
        }

        private void RequestHistoryOverview()
        {
            if (HasUnsavedFirstMessageDraft() && !ConfirmDiscardFirstMessageDraft())
                return;

            HistoryRequested?.Invoke(this, EventArgs.Empty);
        }

        private bool HasUnsavedFirstMessageDraft()
        {
            return Transcript.Count == 0 && !string.IsNullOrWhiteSpace(CurrentInput);
        }

        private bool ConfirmDiscardFirstMessageDraft()
        {
            var decision = _messageBoxService?.ShowMessageBox(
                FlowBloxResourceUtil.GetLocalizedString("Message_DiscardUnsentDraft_Description", typeof(Resources.AiAssistantChatView)),
                FlowBloxResourceUtil.GetLocalizedString("Message_DiscardUnsentDraft_Title", typeof(Resources.AiAssistantChatView)),
                FlowBloxMessageBoxTypes.Question);

            return decision == FlowBloxMessageBoxDialogResult.Yes;
        }

        private async Task UndoProjectStateAsync()
        {
            if (!CanUndoProjectState || _restoreProjectState == null || _stateBeforeLastPrompt == null)
                return;

            if (await _restoreProjectState.Invoke(_stateBeforeLastPrompt))
            {
                _isPromptStateUndone = true;
                RefreshUndoRedoState();
            }
        }

        private async Task RedoProjectStateAsync()
        {
            if (!CanRedoProjectState || _restoreProjectState == null || _stateAfterLastPrompt == null)
                return;

            if (await _restoreProjectState.Invoke(_stateAfterLastPrompt))
            {
                _isPromptStateUndone = false;
                RefreshUndoRedoState();
            }
        }

        private void Service_FlowBlocksChanged(object? sender, FlowBlocksChangedEventArgs e)
        {
            FlowBlocksChanged?.Invoke(this, e);
        }

        private void Service_FlowBlocksConnectionsChanged(object? sender, FlowBlocksConnectionsChangedEventArgs e)
        {
            FlowBlocksConnectionsChanged?.Invoke(this, e);
        }

        private void Service_BeforeFlowBlocksLayoutChanged(object? sender, FlowBlocksLayoutChangedEventArgs e)
        {
            BeforeFlowBlocksLayoutChanged?.Invoke(this, e);
        }

        private void Service_FlowBlocksLayoutChanged(object? sender, FlowBlocksLayoutChangedEventArgs e)
        {
            FlowBlocksLayoutChanged?.Invoke(this, e);
        }

        private void Service_TranscriptLineAdded(object? sender, AssistantTranscriptLine line)
        {
            AddTranscriptLine(line);
        }

        private void Service_EstimatedUsedTokensChanged(object? sender, int estimatedUsedTokens)
        {
            if (_uiContext != null && _uiContext != SynchronizationContext.Current)
            {
                _uiContext.Post(_ => EstimatedUsedTokens = estimatedUsedTokens, null);
                return;
            }

            EstimatedUsedTokens = estimatedUsedTokens;
        }

        private void ResetTokenUsage()
        {
            _service.ResetEstimatedUsedTokens();
            SaveCurrentHistory();
        }

        private bool ConfirmToolExecutionRequest(ToolRequest request)
        {
            if (request == null || !string.Equals(request.ToolName, "ExecuteInputFileCommand", StringComparison.OrdinalIgnoreCase))
                return true;

            var key = request.Arguments?.Value<string>("key") ?? string.Empty;
            var message = string.Format(
                FlowBloxResourceUtil.GetLocalizedString("Message_ExecuteInputFileCommand_Confirm_Description", typeof(Resources.AiAssistantChatView)),
                key);

            var title = FlowBloxResourceUtil.GetLocalizedString("Message_ExecuteInputFileCommand_Confirm_Title", typeof(Resources.AiAssistantChatView));
            var decision = _messageBoxService?.ShowMessageBox(message, title, FlowBloxMessageBoxTypes.Question);

            return decision == FlowBloxMessageBoxDialogResult.Yes;
        }

        private void CopyTranscriptEntry(object parameter)
        {
            if (parameter is not AssistantTranscriptLine line)
                return;

            var content = GetTranscriptContent(line);
            if (string.IsNullOrWhiteSpace(content))
                return;

            Clipboard.SetText(content);
        }

        private void OpenTranscriptEntryInEditor(object parameter)
        {
            if (parameter is not AssistantTranscriptLine line)
                return;

            var content = GetTranscriptContent(line);
            if (string.IsNullOrWhiteSpace(content))
                return;

            var subject = $"AIAssistant_{line.Timestamp:yyyyMMdd_HHmmss}_{line.Kind}";
            FlowBloxEditingHelper.OpenUsingEditor(content, subject);
        }

        private void OpenCommunicationProtocolDirectory(object _)
        {
            try
            {
                var options = FlowBloxOptions.GetOptionInstance();
                var directory = options.GetOption("AI.CommuncationProtocolDir")?.Value;
                if (string.IsNullOrWhiteSpace(directory))
                {
                    directory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "FlowBlox",
                        "logs",
                        "ai_assistant_protocol");
                }

                if (!Directory.Exists(directory))
                {
                    _messageBoxService?.ShowMessageBox(
                        string.Format(
                            FlowBloxResourceUtil.GetLocalizedString("Message_CommunicationProtocolDirectory_NotFound_Description", typeof(Resources.AiAssistantChatView)),
                            directory),
                        FlowBloxResourceUtil.GetLocalizedString("Message_CommunicationProtocolDirectory_NotFound_Title", typeof(Resources.AiAssistantChatView)),
                        FlowBloxMessageBoxTypes.Warning);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = directory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _messageBoxService?.ShowMessageBox(
                    string.Format(
                        FlowBloxResourceUtil.GetLocalizedString("Message_CommunicationProtocolDirectory_OpenFailed_Description", typeof(Resources.AiAssistantChatView)),
                        ex.Message),
                    FlowBloxResourceUtil.GetLocalizedString("Message_CommunicationProtocolDirectory_OpenFailed_Title", typeof(Resources.AiAssistantChatView)),
                    FlowBloxMessageBoxTypes.Error);
            }
        }

        public void StartNewHistory()
        {
            if (IsBusy)
                return;

            var snapshot = _captureProjectState?.Invoke();
            _service.ResetSession();
            Transcript.Clear();
            NotifyTranscriptStateChanged();
            _currentHistory = new AiAssistantHistoryDocument
            {
                HistoryGuid = Guid.NewGuid(),
                ProjectGuid = snapshot?.ProjectGuid ?? Guid.Empty,
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
            _currentHistoryFilePath = null;
            CurrentInput = string.Empty;
            RefreshNavigationState();
        }

        public void OpenHistory(AiAssistantHistoryListItem item)
        {
            if (IsBusy || item == null)
                return;

            var history = _historyStore.Load(item.FilePath);
            if (history == null)
                return;

            _currentHistory = history;
            _currentHistoryFilePath = item.FilePath;
            Transcript.Clear();
            foreach (var line in history.Transcripts ?? new List<AssistantTranscriptLine>())
                Transcript.Add(line);

            _service.RestoreSession(history);
            EstimatedUsedTokens = _service.EstimatedUsedTokens;
            NotifyTranscriptStateChanged();
            CurrentInput = string.Empty;
        }

        private void EnsureCurrentHistory()
        {
            if (_currentHistory != null)
                return;

            var snapshot = _captureProjectState?.Invoke();
            _currentHistory = new AiAssistantHistoryDocument
            {
                HistoryGuid = Guid.NewGuid(),
                ProjectGuid = snapshot?.ProjectGuid ?? Guid.Empty,
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        private void SaveCurrentHistory()
        {
            if (_currentHistory == null || Transcript.Count == 0)
                return;

            _service.UpdateHistorySessionMetadata(_currentHistory);
            _currentHistory.Transcripts = Transcript.ToList();
            _currentHistoryFilePath = _historyStore.CreateOrUpdateHistory(_currentHistory, _currentHistoryFilePath);
            RefreshNavigationState();
        }

        private void RefreshHistories()
        {
            var projectGuid = _captureProjectState?.Invoke()?.ProjectGuid;
            _historyStore.Initialize(projectGuid);
            _historyStore.Refresh(projectGuid);
            RefreshNavigationState();
        }

        private void RefreshNavigationState()
        {
            OnPropertyChanged(nameof(CanGoBackToHistory));
            BackToHistoryCommand.Invalidate();
            NewHistoryCommand.Invalidate();
        }
        private static string GetTranscriptContent(AssistantTranscriptLine line)
        {
            if (line == null)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"Timestamp: {line.Timestamp:O}");
            sb.AppendLine($"Kind: {line.Kind}");

            if (!string.IsNullOrWhiteSpace(line.Text))
            {
                sb.AppendLine();
                sb.AppendLine("Text:");
                sb.AppendLine(line.Text);
            }

            if (!string.IsNullOrWhiteSpace(line.InternalContent))
            {
                sb.AppendLine();
                sb.AppendLine("Details:");
                sb.AppendLine(line.InternalContent);
            }

            return sb.ToString().TrimEnd();
        }

        private void AddTranscriptLine(AssistantTranscriptLine line)
        {
            if (line == null)
                return;

            if (_uiContext != null && _uiContext != SynchronizationContext.Current)
            {
                _uiContext.Post(_ =>
                {
                    Transcript.Add(line);
                    NotifyTranscriptStateChanged();
                }, null);
                return;
            }

            Transcript.Add(line);
            NotifyTranscriptStateChanged();
        }

        public void ResetForProjectInitialization()
        {
            Stop();
            _service.ResetSession();
            Transcript.Clear();
            NotifyTranscriptStateChanged();
            _currentHistory = null;
            _currentHistoryFilePath = null;
            RefreshHistories();
            CurrentInput = string.Empty;
            IsBusy = false;
            _runtimeStateService?.SetRuntimeStartBlocked(false);
            _stateBeforeLastPrompt = null;
            _stateAfterLastPrompt = null;
            _isPromptStateUndone = false;
            RefreshProviderConfigurationState();
            RefreshHistories();
            RefreshUndoRedoState();
        }

        private void NotifyTranscriptStateChanged()
        {
            OnPropertyChanged(nameof(ShowIntroHeader));
        }

        public void ConfigureProjectStateAccess(
            Func<AIAssistantProjectStateSnapshot?> captureProjectState,
            Func<AIAssistantProjectStateSnapshot, Task<bool>> restoreProjectState)
        {
            _captureProjectState = captureProjectState;
            _restoreProjectState = restoreProjectState;
            RefreshHistories();
            var snapshot = _captureProjectState?.Invoke();
            if (_currentHistory != null && _currentHistory.ProjectGuid == Guid.Empty && Transcript.Count == 0)
                _currentHistory.ProjectGuid = snapshot?.ProjectGuid ?? Guid.Empty;

            RefreshUndoRedoState();
        }

        public AssistantConfiguration GetConfiguration(out string error) => _service.GetConfiguration(out error);

        public bool SaveConfiguration(AssistantConfiguration configuration, out string error) =>
            SaveConfigurationInternal(configuration, out error);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RefreshUndoRedoState()
        {
            OnPropertyChanged(nameof(CanUndoProjectState));
            OnPropertyChanged(nameof(CanRedoProjectState));
            UndoProjectStateCommand.Invalidate();
            RedoProjectStateCommand.Invalidate();
        }

        public void RefreshProviderConfigurationState()
        {
            var configuration = _service.GetConfiguration(out _);
            var isConfigured = configuration?.Provider != null;

            if (_isProviderConfigured == isConfigured)
                return;

            _isProviderConfigured = isConfigured;
            OnPropertyChanged(nameof(ShowProviderConfigurationWarning));
        }

        private bool SaveConfigurationInternal(AssistantConfiguration configuration, out string error)
        {
            var success = _service.SaveConfiguration(configuration, out error);
            if (success)
                RefreshProviderConfigurationState();
            RefreshHistories();

            return success;
        }

        public void Dispose()
        {
            if (_runtimeStateService != null)
                _runtimeStateService.StateChanged -= RuntimeStateService_StateChanged;

            _service.FlowBlocksChanged -= Service_FlowBlocksChanged;
            _service.FlowBlocksConnectionsChanged -= Service_FlowBlocksConnectionsChanged;
            _service.BeforeFlowBlocksLayoutChanged -= Service_BeforeFlowBlocksLayoutChanged;
            _service.FlowBlocksLayoutChanged -= Service_FlowBlocksLayoutChanged;
            _service.TranscriptLineAdded -= Service_TranscriptLineAdded;
            _service.EstimatedUsedTokensChanged -= Service_EstimatedUsedTokensChanged;
        }
    }
}