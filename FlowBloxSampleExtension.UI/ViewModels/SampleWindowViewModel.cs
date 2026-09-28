namespace FlowBloxSampleExtension.UI.ViewModels
{
    using FlowBloxSampleExtension.Models.FlowBlocks.SampleCategory;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class SampleWindowViewModel : INotifyPropertyChanged
    {
        private SampleExtensionFlowBlock? _component;
        private bool _isDirty;

        public string? OutputText
        {
            get => _component?.OutputText;
            set
            {
                if (_component == null || _component.OutputText == value)
                    return;

                _component.OutputText = value;
                IsDirty = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCommit));
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty == value)
                    return;

                _isDirty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCommit));
            }
        }

        public bool CanCommit => IsDirty && !string.IsNullOrWhiteSpace(OutputText);

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Open(SampleExtensionFlowBlock component)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
            IsDirty = false;
            OnPropertyChanged(nameof(OutputText));
            OnPropertyChanged(nameof(CanCommit));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
