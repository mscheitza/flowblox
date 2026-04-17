using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Enums;
using FlowBlox.UICore.Repositories;
using FlowBlox.UICore.Commands;
using FlowBlox.Core.Util;
using System.Windows;
using FlowBlox.UICore.Resources;
using FlowBlox.UICore.Models.Toolbox;
using FlowBlox.UICore.Events;

namespace FlowBlox.UICore.ViewModels
{
    public class ToolboxWindowViewModel : INotifyPropertyChanged
    {
        private ToolboxElement _selectedToolboxElement;
        private bool _isSelectionMode;
        private string? _passedToolboxType;
        private ToolboxRepository _repository;
        private Visibility _noToolboxElementsFoundVisibility;
        private Visibility _detailViewVisibility;
        private List<FlowBloxToolboxCategoryItem> _toolboxTypes;
        private ToolboxScope _toolboxScope;
        private List<ToolboxScope> _toolboxScopes;

        public ToolboxWindowViewModel()
        {
            Init();
        }

        private void Init()
        {
            AddCommand = new RelayCommand(AddToolboxElement);
            RemoveCommand = new RelayCommand(RemoveToolboxElement, () => CanRemove);
            SaveCommand = new RelayCommand(SaveToolboxElement, () => CanSave);
            SelectCommand = new RelayCommand(SelectToolboxElement, () => CanSelect);

            _repository = new ToolboxRepository(FlowBloxOptions.GetOptionInstance());

            if (_passedToolboxType != null)
                _toolboxTypes = new List<FlowBloxToolboxCategoryItem>() { FlowBloxToolboxCategory.GetCategoryOrDefault(_passedToolboxType) };
            else
                _toolboxTypes = FlowBloxToolboxCategory.GetAllCategories()
                    .ToList();

            _toolboxScopes = Enum.GetValues(typeof(ToolboxScope))
                .Cast<ToolboxScope>()
                .ToList();

            _detailViewVisibility = Visibility.Collapsed;

            ToolboxElements.CollectionChanged += ToolboxElements_CollectionChanged;

            LoadToolboxElements();
        }

        private void ToolboxElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _noToolboxElementsFoundVisibility = this.ToolboxElements.Count > 0 ? Visibility.Hidden : Visibility.Visible;
            OnPropertyChanged(nameof(NoToolboxElementsFoundVisibility));
        }

        public ToolboxWindowViewModel(bool isSelectionMode, string? passedToolboxType)
        {
            _isSelectionMode = isSelectionMode;
            _passedToolboxType = passedToolboxType;
            Init();
        }

        public ObservableCollection<ToolboxElement> ToolboxElements { get; set; } = new ObservableCollection<ToolboxElement>();

        public ToolboxElement SelectedToolboxElement
        {
            get => _selectedToolboxElement;
            set
            {
                _selectedToolboxElement = value;
                OnPropertyChanged(nameof(SelectedToolboxElement));
                OnPropertyChanged(nameof(CanRemove));
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(CanSelect));
                DetailViewVisibility = _selectedToolboxElement != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public List<FlowBloxToolboxCategoryItem> ToolboxTypes
        {
            get => _toolboxTypes;
            set
            {
                _toolboxTypes = value;
                OnPropertyChanged(nameof(ToolboxTypes));
            }
        }

        public List<ToolboxScope> ToolboxScopes
        {
            get => _toolboxScopes;
            set
            {
                _toolboxScopes = value;
                OnPropertyChanged(nameof(ToolboxScopes));
            }
        }

        public ToolboxScope ToolboxScope
        {
            get => _toolboxScope;
            set
            {
                _toolboxScope = value;
                OnPropertyChanged(nameof(ToolboxScope));
                LoadToolboxElements();
            }
        }

        public Visibility NoToolboxElementsFoundVisibility
        {
            get => _noToolboxElementsFoundVisibility;
            set
            {
                _noToolboxElementsFoundVisibility = value;
                OnPropertyChanged(nameof(NoToolboxElementsFoundVisibility));
            }
        }

        public Visibility DetailViewVisibility
        {
            get => _detailViewVisibility;
            set
            {
                _detailViewVisibility = value;
                OnPropertyChanged(nameof(DetailViewVisibility));
            }
        }

        public ICommand AddCommand { get; private set; }
        public ICommand RemoveCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }

        public bool CanRemove => SelectedToolboxElement != null && SelectedToolboxElement.IsEditable;

        public bool CanSave 
        { 
            get
            {
                return SelectedToolboxElement != null &&
                       !string.IsNullOrEmpty(SelectedToolboxElement.Name) &&
                       !string.IsNullOrEmpty(SelectedToolboxElement.Content) &&
                       SelectedToolboxElement.IsEditable;
            }
        }

        public bool CanSelect 
        {
            get
            {
                return _isSelectionMode &&
                       SelectedToolboxElement != null &&
                       !string.IsNullOrEmpty(SelectedToolboxElement.Name) &&
                       !string.IsNullOrEmpty(SelectedToolboxElement.Content);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler RequestClose;
        public event EventHandler<NotificationEventArgs> ShowNotification;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadToolboxElements()
        {
            ToolboxElements.Clear();
            var elements = _repository.Query(_passedToolboxType, toolboxScope: _toolboxScope);
            foreach (var element in elements)
            {
                ToolboxElements.Add(element);
            }
        }

        private void AddToolboxElement()
        {
            var newElement = _repository.Create();
            ToolboxElements.Add(newElement);
            SelectedToolboxElement = newElement;
        }

        private void RemoveToolboxElement()
        {
            if (SelectedToolboxElement != null)
            {
                _repository.Delete(SelectedToolboxElement);
                ToolboxElements.Remove(SelectedToolboxElement);
                SelectedToolboxElement = null;
            }
        }

        private void SaveToolboxElement()
        {
            if (SelectedToolboxElement != null)
            {
                _repository.Update(SelectedToolboxElement);
                ShowNotification?.Invoke(this, new NotificationEventArgs(ToolboxWindow.Message_SaveSuccessful));
            }
        }

        public void SelectToolboxElement()
        {
            if (_isSelectionMode)
                RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
