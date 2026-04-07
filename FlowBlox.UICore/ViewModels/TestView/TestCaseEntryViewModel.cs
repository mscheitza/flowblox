using FlowBlox.Core.Models.FlowBlocks.Additions;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.ViewModels.TestView
{
    public sealed class TestCaseEntryViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _requiredFor = string.Empty;
        private string _definedAt = string.Empty;
        private bool _requiredForExecution;
        private TestCaseStatus _status;
        private string? _protocolPath;
        private bool _isSelected;

        public FlowBloxTestDefinition TestDefinition { get; }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string RequiredFor
        {
            get => _requiredFor;
            set
            {
                if (_requiredFor == value)
                    return;

                _requiredFor = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string DefinedAt
        {
            get => _definedAt;
            set
            {
                if (_definedAt == value)
                    return;

                _definedAt = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public bool RequiredForExecution
        {
            get => _requiredForExecution;
            set
            {
                if (_requiredForExecution == value)
                    return;

                _requiredForExecution = value;
                OnPropertyChanged();
            }
        }

        public TestCaseStatus Status
        {
            get => _status;
            set
            {
                if (_status == value)
                    return;

                _status = value;
                OnPropertyChanged();
            }
        }

        public string? ProtocolPath
        {
            get => _protocolPath;
            set
            {
                if (_protocolPath == value)
                    return;

                _protocolPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public TestCaseEntryViewModel(FlowBloxTestDefinition testDefinition)
        {
            TestDefinition = testDefinition ?? throw new ArgumentNullException(nameof(testDefinition));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
