using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Logging;
using FlowBlox.UICore.Resources;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.Utilities;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class AIAssistantProjectStateSnapshot
    {
        public Guid ProjectGuid { get; init; }
        public string ProjectName { get; init; }
        public string ProjectJson { get; init; }
        public string ExtensionsJson { get; init; }
        public string ProjectSpaceGuid { get; init; }
        public int? ProjectSpaceVersion { get; init; }
        public string ProjectSpaceEndpointUri { get; init; }
    }

    public class AIAssistantViewModel : INotifyPropertyChanged
    {
        private readonly AiAssistantService _service;
        private readonly IFlowBloxMessageBoxService _messageBoxService;
        private readonly SynchronizationContext? _uiContext;
        private CancellationTokenSource? _cts;
        private string _currentInput = string.Empty;
        private bool _isBusy;
        private Func<AIAssistantProjectStateSnapshot?>? _captureProjectState;
        private Func<AIAssistantProjectStateSnapshot, bool>? _restoreProjectState;
        private AIAssistantProjectStateSnapshot? _stateBeforeLastPrompt;
        private AIAssistantProjectStateSnapshot? _stateAfterLastPrompt;
        private bool _isPromptStateUndone;

        public ObservableCollection<AssistantTranscriptLine> Transcript { get; } = new ObservableCollection<AssistantTranscriptLine>();

        public RelayCommand SubmitCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand CopyTranscriptEntryCommand { get; }
        public RelayCommand OpenTranscriptEntryInEditorCommand { get; }
        public RelayCommand OpenCommunicationProtocolDirectoryCommand { get; }
        public RelayCommand ResetCommunicationStateCommand { get; }
        public RelayCommand UndoProjectStateCommand { get; }
        public RelayCommand RedoProjectStateCommand { get; }

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
                    CancelCommand.Invalidate();
                    ResetCommunicationStateCommand.Invalidate();
                    RefreshUndoRedoState();
                }
            }
        }

        public bool CanEditInput => !IsBusy;
        public bool ShowProviderConfigurationWarning => !_isProviderConfigured;
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
        private bool _isProviderConfigured;

        public AIAssistantViewModel()
        {
            _uiContext = SynchronizationContext.Current;
            _messageBoxService = FlowBloxServiceLocator.Instance.GetService<IFlowBloxMessageBoxService>();
            _service = new AiAssistantService(
                new AiProviderExecutor(),
                new DefaultToolApi(),
                FlowBloxLogManager.Instance.GetLogger());
            _service.FlowBlocksChanged += Service_FlowBlocksChanged;
            _service.TranscriptLineAdded += Service_TranscriptLineAdded;

            SubmitCommand = new RelayCommand(async () => await SubmitAsync(), CanSubmit);
            CancelCommand = new RelayCommand(Cancel, () => IsBusy);
            CopyTranscriptEntryCommand = new RelayCommand(CopyTranscriptEntry);
            OpenTranscriptEntryInEditorCommand = new RelayCommand(OpenTranscriptEntryInEditor);
            OpenCommunicationProtocolDirectoryCommand = new RelayCommand(OpenCommunicationProtocolDirectory);
            ResetCommunicationStateCommand = new RelayCommand(ResetCommunicationState, () => !IsBusy);
            UndoProjectStateCommand = new RelayCommand(UndoProjectState, () => CanUndoProjectState);
            RedoProjectStateCommand = new RelayCommand(RedoProjectState, () => CanRedoProjectState);

            RefreshProviderConfigurationState();
        }

        private bool CanSubmit()
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(CurrentInput);
        }

        private async Task SubmitAsync()
        {
            var input = CurrentInput?.Trim();
            if (string.IsNullOrWhiteSpace(input) || IsBusy)
                return;

            var stateBeforePrompt = _captureProjectState?.Invoke();
            var promptCompleted = false;

            AddTranscriptLine(new AssistantTranscriptLine
            {
                Kind = AssistantTranscriptKind.User,
                Text = input,
                Timestamp = DateTime.Now
            });

            IsBusy = true;
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
                    Text = "Cancelled.",
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
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void Cancel()
        {
            if (!IsBusy)
                return;

            _cts?.Cancel();
        }

        private void UndoProjectState()
        {
            if (!CanUndoProjectState || _restoreProjectState == null || _stateBeforeLastPrompt == null)
                return;

            if (_restoreProjectState.Invoke(_stateBeforeLastPrompt))
            {
                _isPromptStateUndone = true;
                RefreshUndoRedoState();
            }
        }

        private void RedoProjectState()
        {
            if (!CanRedoProjectState || _restoreProjectState == null || _stateAfterLastPrompt == null)
                return;

            if (_restoreProjectState.Invoke(_stateAfterLastPrompt))
            {
                _isPromptStateUndone = false;
                RefreshUndoRedoState();
            }
        }

        private void Service_FlowBlocksChanged(object? sender, FlowBlocksChangedEventArgs e)
        {
            FlowBlocksChanged?.Invoke(this, e);
        }

        private void Service_TranscriptLineAdded(object? sender, AssistantTranscriptLine line)
        {
            AddTranscriptLine(line);
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
                            FlowBloxResourceUtil.GetLocalizedString("Message_CommunicationProtocolDirectory_NotFound_Description", typeof(AIAssistantControl)),
                            directory),
                        FlowBloxResourceUtil.GetLocalizedString("Message_CommunicationProtocolDirectory_NotFound_Title", typeof(AIAssistantControl)),
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
                        FlowBloxResourceUtil.GetLocalizedString("Message_CommunicationProtocolDirectory_OpenFailed_Description", typeof(AIAssistantControl)),
                        ex.Message),
                    FlowBloxResourceUtil.GetLocalizedString("Message_CommunicationProtocolDirectory_OpenFailed_Title", typeof(AIAssistantControl)),
                    FlowBloxMessageBoxTypes.Error);
            }
        }

        private void ResetCommunicationState()
        {
            if (IsBusy)
                return;

            var result = _messageBoxService?.ShowMessageBox(
                FlowBloxResourceUtil.GetLocalizedString(
                    "Message_ResetCommunicationState_Question",
                    typeof(AIAssistantControl)),
                FlowBloxResourceUtil.GetLocalizedString(
                    "Message_ResetCommunicationState_Title",
                    typeof(AIAssistantControl)),
                FlowBloxMessageBoxTypes.Question);

            if (result != FlowBloxMessageBoxDialogResult.Yes)
                return;

            ResetForProjectInitialization();
        }

        private static string GetTranscriptContent(AssistantTranscriptLine line)
        {
            if (!string.IsNullOrWhiteSpace(line.InternalContent))
                return line.InternalContent;

            return line.Text ?? string.Empty;
        }

        private void AddTranscriptLine(AssistantTranscriptLine line)
        {
            if (line == null)
                return;

            if (_uiContext != null && _uiContext != SynchronizationContext.Current)
            {
                _uiContext.Post(_ => Transcript.Add(line), null);
                return;
            }

            Transcript.Add(line);
        }

        public void ResetForProjectInitialization()
        {
            Cancel();
            _service.ResetSession();
            Transcript.Clear();
            CurrentInput = string.Empty;
            IsBusy = false;
            _stateBeforeLastPrompt = null;
            _stateAfterLastPrompt = null;
            _isPromptStateUndone = false;
            RefreshProviderConfigurationState();
            RefreshUndoRedoState();
        }

        public void ConfigureProjectStateAccess(
            Func<AIAssistantProjectStateSnapshot?> captureProjectState,
            Func<AIAssistantProjectStateSnapshot, bool> restoreProjectState)
        {
            _captureProjectState = captureProjectState;
            _restoreProjectState = restoreProjectState;
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

            return success;
        }
    }
}
