using FlowBlox.Core.Provider.Registry;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Resources;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class TransactionMonitorViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Dispatcher _dispatcher;
        private int _activeTransactionCount;
        private string _currentTransaction;

        public ObservableCollection<TransactionMonitorEntryViewModel> Transactions { get; } = new();

        public RelayCommand ClearInactiveTransactionsCommand { get; }

        public int ActiveTransactionCount
        {
            get => _activeTransactionCount;
            private set
            {
                _activeTransactionCount = value;
                OnPropertyChanged();
            }
        }

        public string CurrentTransaction
        {
            get => _currentTransaction;
            private set
            {
                _currentTransaction = value;
                OnPropertyChanged();
            }
        }

        public TransactionMonitorViewModel(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            ClearInactiveTransactionsCommand = new RelayCommand(
                FlowBloxRegistryTransactionHistory.ClearInactiveTransactions,
                () => FlowBloxRegistryTransactionHistory.HasInactiveTransactions);
            FlowBloxRegistryTransactionHistory.TransactionsChanged += RegistryTransactionsChanged;
            Refresh();
        }

        private void RegistryTransactionsChanged(object sender, EventArgs e)
        {
            if (_dispatcher.HasShutdownStarted || _dispatcher.HasShutdownFinished)
                return;

            if (_dispatcher.CheckAccess())
                Refresh();
            else
                _dispatcher.BeginInvoke(Refresh);
        }

        private void Refresh()
        {
            var snapshots = FlowBloxRegistryTransactionHistory.GetHistory();
            var current = snapshots.LastOrDefault(x => x.IsCurrent);

            ActiveTransactionCount = FlowBloxRegistryTransactionHistory.ActiveTransactionCount;
            CurrentTransaction = current == null
                ? TransactionMonitorWindow.Current_Root
                : $"#{current.Id} · {GetShortTypeName(current.TargetType)} · {TransactionMonitorWindow.Depth} {current.Depth}";

            Transactions.Clear();
            foreach (var snapshot in snapshots.OrderByDescending(x => x.Id))
                Transactions.Add(TransactionMonitorEntryViewModel.FromSnapshot(snapshot));

            ClearInactiveTransactionsCommand.Invalidate();
        }

        private static string GetShortTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return TransactionMonitorWindow.Target_Unknown;

            var lastSeparator = typeName.LastIndexOf('.');
            return lastSeparator >= 0 ? typeName[(lastSeparator + 1)..] : typeName;
        }

        public void Dispose()
        {
            FlowBloxRegistryTransactionHistory.TransactionsChanged -= RegistryTransactionsChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
