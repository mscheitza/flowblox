using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace FlowBlox.UICore.ViewModels.ManagedObjects
{
    public sealed class ManagedObjectTypeNodeViewModel : INotifyPropertyChanged
    {
        public Type ManagedObjectType { get; }

        public string DisplayName { get; }

        public ImageSource Icon { get; }

        public bool CanCreateInstance { get; }

        public bool IsCategoryOnly { get; }
        public bool IsAllManagedObjectsRoot { get; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value)
                    return;

                _isExpanded = value;
                Raise();
            }
        }

        public ObservableCollection<ManagedObjectTypeNodeViewModel> Children { get; } = new();

        public ManagedObjectTypeNodeViewModel(
            Type managedObjectType,
            string displayName,
            ImageSource icon,
            bool canCreateInstance,
            bool isCategoryOnly = false,
            bool isAllManagedObjectsRoot = false,
            bool isExpanded = false)
        {
            ManagedObjectType = managedObjectType;
            DisplayName = displayName;
            Icon = icon;
            CanCreateInstance = canCreateInstance;
            IsCategoryOnly = isCategoryOnly;
            IsAllManagedObjectsRoot = isAllManagedObjectsRoot;
            IsExpanded = isExpanded;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Raise([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
