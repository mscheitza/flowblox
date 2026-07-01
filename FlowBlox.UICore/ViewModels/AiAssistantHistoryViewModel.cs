using FlowBlox.AIAssistant.History;
using FlowBlox.UICore.Commands;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class AiAssistantHistoryViewModel : INotifyPropertyChanged
    {
        private readonly AiAssistantHistoryStore _historyStore = AiAssistantHistoryStore.Instance;
        private Func<AIAssistantProjectStateSnapshot?>? _captureProjectState;

        public ObservableCollection<AiAssistantHistoryListItem> Histories => _historyStore.Histories;
        public RelayCommand NewHistoryCommand { get; }
        public RelayCommand OpenHistoryCommand { get; }

        public bool HasHistories => Histories.Count > 0;

        public event EventHandler? NewHistoryRequested;
        public event EventHandler<AiAssistantHistoryListItem>? HistoryOpenRequested;
        public event PropertyChangedEventHandler? PropertyChanged;

        public AiAssistantHistoryViewModel()
        {
            NewHistoryCommand = new RelayCommand(() => NewHistoryRequested?.Invoke(this, EventArgs.Empty));
            OpenHistoryCommand = new RelayCommand(OpenHistory, parameter => parameter is AiAssistantHistoryListItem);
            Histories.CollectionChanged += Histories_CollectionChanged;
        }

        public void ConfigureProjectStateAccess(Func<AIAssistantProjectStateSnapshot?> captureProjectState)
        {
            _captureProjectState = captureProjectState;
            Refresh();
        }

        public void Refresh()
        {
            var projectGuid = _captureProjectState?.Invoke()?.ProjectGuid;
            _historyStore.Initialize(projectGuid);
            _historyStore.Refresh(projectGuid);
            OnPropertyChanged(nameof(HasHistories));
        }

        private void OpenHistory(object parameter)
        {
            if (parameter is AiAssistantHistoryListItem item)
                HistoryOpenRequested?.Invoke(this, item);
        }

        private void Histories_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasHistories));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
