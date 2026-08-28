using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Interfaces;
using FlowBlox.UICore.ViewModels.ProblemsView;
using FlowBlox.UICore.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class ProblemsViewModel : INotifyPropertyChanged
    {
        private readonly IDialogService _dialogService;
        private ProblemTraceEntryViewModel _selectedProblemTrace;

        public ProblemsViewModel()
        {
            _dialogService = FlowBloxServiceLocator.Instance.GetService<IDialogService>();
            OpenProblemTraceCommand = new RelayCommand(OpenSelectedProblemTrace, CanOpenSelectedProblemTrace);
        }

        public ObservableCollection<ProblemTraceEntryViewModel> ProblemTraces { get; } = new ObservableCollection<ProblemTraceEntryViewModel>();

        public ProblemTraceEntryViewModel SelectedProblemTrace
        {
            get => _selectedProblemTrace;
            set
            {
                if (ReferenceEquals(_selectedProblemTrace, value))
                    return;

                _selectedProblemTrace = value;
                OnPropertyChanged(nameof(SelectedProblemTrace));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool HasProblemTraces => ProblemTraces.Count > 0;

        public ICommand OpenProblemTraceCommand { get; }

        public void Append(ProblemTrace problemTrace)
        {
            if (problemTrace == null)
                return;

            ProblemTraces.Add(new ProblemTraceEntryViewModel(problemTrace));
            OnPropertyChanged(nameof(HasProblemTraces));
        }

        private bool CanOpenSelectedProblemTrace()
        {
            return SelectedProblemTrace?.ProblemTrace != null;
        }

        private void OpenSelectedProblemTrace()
        {
            var problemTrace = SelectedProblemTrace?.ProblemTrace;
            if (problemTrace == null)
                return;

            var window = new ProblemTraceWindow(problemTrace);
            if (_dialogService != null)
                _dialogService.ShowWPFDialog(window, true);
            else
                window.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
