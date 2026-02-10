using System.ComponentModel;
using System.Runtime.CompilerServices;
using FlowBlox.UICore.Utilities;

namespace FlowBlox.UICore.ViewModels
{
    public class MultiValueSelectionDialogViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        private List<DisplayItem> _items;
        private bool _isItemSelected;
        private GenericSelectionHandler _genericSelectionHandler;
        private DisplayItem _selectedObject;

        public MultiValueSelectionDialogViewModel(string title, string description, GenericSelectionHandler genericSelectionHandler)
        {
            Title = title;
            Description = description;
            Items = genericSelectionHandler.Items;
            SelectedItem = Items.FirstOrDefault();
            _genericSelectionHandler = genericSelectionHandler;
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public List<DisplayItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public DisplayItem SelectedItem
        {
            get => _selectedObject;
            set
            {
                SetProperty(ref _selectedObject, value);
                IsItemSelected = value != null;
            }
        }

        public bool IsItemSelected
        {
            get => _isItemSelected;
            set => SetProperty(ref _isItemSelected, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<TProp>(ref TProp storage, TProp value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TProp>.Default.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        
    }
}
