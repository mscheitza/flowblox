namespace FlowBlox.UICore.ViewModels.ProjectPanel
{
    public sealed class FlowBlockRenderRowViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isSelected;

        public FlowBlockRenderRowViewModel(
            string kind,
            string label,
            string value,
            string tooltip = null,
            object target = null,
            string preselectedProperty = null,
            object preselectedInstance = null)
        {
            Kind = kind;
            Label = label;
            Value = value;
            Tooltip = tooltip ?? value;
            Target = target;
            PreselectedProperty = preselectedProperty;
            PreselectedInstance = preselectedInstance;
        }

        public string Kind { get; }
        public string Label { get; }
        public string Value { get; }
        public string Tooltip { get; }
        public object Target { get; }
        public string PreselectedProperty { get; }
        public object PreselectedInstance { get; }
        public bool CanNavigate => Target != null;
        public bool CanCopyValue => !string.IsNullOrEmpty(Value);
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
