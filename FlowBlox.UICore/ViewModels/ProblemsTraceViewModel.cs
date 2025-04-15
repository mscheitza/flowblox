using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util;
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
            }
        }

        public ICommand OpenEditorCommand { get; }

        public ProblemTraceViewModel()
        {
            OpenEditorCommand = new RelayCommand(OpenEditor);
        }

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
