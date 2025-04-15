using System.ComponentModel;
using System.Windows;

namespace FlowBlox.UICore.ViewModels.PropertyView
{
    public class PropertyControlViewModel : INotifyPropertyChanged
    {
        public string PropertyName { get; set; }
        public object Target { get; set; }
        public string Label { get; set; }
        public object Value { get; set; }
        public Type ValueType { get; set; }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public bool AllowHasChangesUpdate { get; set; }

        private bool _hasChanges = false;
        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                if (AllowHasChangesUpdate && _hasChanges != value)
                {
                    _hasChanges = value;
                    OnPropertyChanged(nameof(HasChanges));
                }
            }
        }

        public FrameworkElement Control { get; set; }

        public bool UseLabel { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}