using FlowBlox.AIAssistant.Models;
using FlowBlox.AIAssistant.Services;
using FlowBlox.AIAssistant.Tools;
using FlowBlox.Core.Logging;
using FlowBlox.UICore.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace FlowBlox.UICore.ViewModels
{
    public class AIAssistantViewModel : INotifyPropertyChanged
    {
        private readonly AiAssistantService _service;
        private CancellationTokenSource _cts;
        private string _currentInput = string.Empty;
        private bool _isBusy;
        private string _lastGeneratedProjectJson = string.Empty;

        public ObservableCollection<AssistantTranscriptLine> Transcript { get; } = new ObservableCollection<AssistantTranscriptLine>();

        public RelayCommand SubmitCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand CopyJsonCommand { get; }

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
                    CopyJsonCommand.Invalidate();
                }
            }
        }

        public bool CanEditInput => !IsBusy;

        public string LastGeneratedProjectJson
        {
            get => _lastGeneratedProjectJson;
            private set
            {
                if (_lastGeneratedProjectJson != value)
                {
                    _lastGeneratedProjectJson = value;
                    OnPropertyChanged(nameof(LastGeneratedProjectJson));
                    CopyJsonCommand.Invalidate();
                }
            }
        }

        public AIAssistantViewModel()
        {
            _service = new AiAssistantService(
                new AiProviderExecutor(),
                new DefaultToolApi(),
                new DefaultOptionsProvider(),
                FlowBloxLogManager.Instance.GetLogger());

            SubmitCommand = new RelayCommand(async () => await SubmitAsync(), CanSubmit);
            CancelCommand = new RelayCommand(Cancel, () => IsBusy);
            CopyJsonCommand = new RelayCommand(CopyJson, CanCopyJson);
        }

        private bool CanSubmit()
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(CurrentInput);
        }

        private bool CanCopyJson()
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(LastGeneratedProjectJson);
        }

        private async Task SubmitAsync()
        {
            var input = CurrentInput?.Trim();
            if (string.IsNullOrWhiteSpace(input) || IsBusy)
                return;

            Transcript.Add(new AssistantTranscriptLine
            {
                Kind = AssistantTranscriptKind.User,
                Text = $"User: {input}",
                Timestamp = DateTime.Now
            });

            IsBusy = true;
            _cts = new CancellationTokenSource();

            try
            {
                var result = await _service.GenerateProjectAsync(input, _cts.Token);
                foreach (var line in result.TranscriptLines)
                    Transcript.Add(line);

                if (result.Success)
                    LastGeneratedProjectJson = result.ProjectJson;
            }
            catch (OperationCanceledException)
            {
                Transcript.Add(new AssistantTranscriptLine
                {
                    Kind = AssistantTranscriptKind.Status,
                    Text = "Assistant: Cancelled.",
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Transcript.Add(new AssistantTranscriptLine
                {
                    Kind = AssistantTranscriptKind.Error,
                    Text = $"Assistant: {ex.Message}",
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
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

        private void CopyJson()
        {
            if (string.IsNullOrWhiteSpace(LastGeneratedProjectJson))
                return;

            Clipboard.SetText(LastGeneratedProjectJson);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
