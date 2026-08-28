using FlowBlox.Core.Models.Runtime;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Utilities;
using System.ComponentModel;
using System.Windows.Input;

namespace FlowBlox.UICore.ViewModels
{
    public class ProblemTraceViewModel : INotifyPropertyChanged
    {
        private ProblemTrace _selectedProblemTrace;

        public ProblemTrace SelectedProblemTrace
        {
            get { return _selectedProblemTrace; }
            set
            {
                _selectedProblemTrace = value;
                OnPropertyChanged(nameof(SelectedProblemTrace));
                OnPropertyChanged(nameof(HasFieldValues));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool HasFieldValues => SelectedProblemTrace?.FieldValues?.Count > 0;

        public ICommand OpenEditorCommand { get; }

        public ProblemTraceViewModel()
        {
            OpenEditorCommand = new RelayCommand(OpenEditor, CanOpenEditor);
        }

        private bool CanOpenEditor(object parameter)
            => parameter is FieldValue fieldValue && !string.IsNullOrWhiteSpace(fieldValue.Value);

        private void OpenEditor(object parameter)
        {
            var fieldValue = parameter as FieldValue;
            if (fieldValue == null)
                return;

            FlowBloxEditingHelper.OpenUsingEditor(fieldValue.Value, fieldValue.FullyQualifiedName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
