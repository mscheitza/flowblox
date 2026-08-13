using FlowBlox.Core.Models.Components;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowBlox.UICore.ViewModels.Options
{
    public sealed class OptionTreeNodeViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded;

        public OptionTreeNodeViewModel(string displayName, OptionElement optionElement = null)
        {
            DisplayName = displayName;
            OptionElement = optionElement;
        }

        public string DisplayName { get; }

        public OptionElement OptionElement { get; }

        public ObservableCollection<OptionTreeNodeViewModel> Children { get; } = new();

        public bool IsOption => OptionElement != null;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value)
                    return;

                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
