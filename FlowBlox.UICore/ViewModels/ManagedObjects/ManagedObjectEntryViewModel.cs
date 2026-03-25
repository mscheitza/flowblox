using FlowBlox.Core.Interfaces;
using FlowBlox.UICore.ViewModels.PropertyWindow;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace FlowBlox.UICore.ViewModels.ManagedObjects
{
    public sealed class ManagedObjectEntryViewModel : INotifyPropertyChanged
    {
        public IManagedObject ManagedObject { get; }

        public ImageSource Icon { get; }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _displayableProperties;
        public string DisplayableProperties
        {
            get => _displayableProperties;
            set { _displayableProperties = value; OnPropertyChanged(); }
        }

        private string _usedIn;
        public string UsedIn
        {
            get => _usedIn;
            set { _usedIn = value; OnPropertyChanged(); }
        }

        public ObservableCollection<UIActionViewModel> Actions { get; } = new();

        public ManagedObjectEntryViewModel(IManagedObject managedObject, ImageSource icon)
        {
            ManagedObject = managedObject;
            Icon = icon;
            _name = managedObject?.Name ?? string.Empty;
            _displayableProperties = string.Empty;
            _usedIn = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
