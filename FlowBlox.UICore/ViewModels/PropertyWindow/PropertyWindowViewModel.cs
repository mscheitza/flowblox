using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Grid.Elements.Util;
using FlowBlox.UICore.Commands;
using FlowBlox.UICore.Provider;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels.PropertyWindow;
using FlowBlox.UICore.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace FlowBlox.UICore.ViewModels.PropertyView
{
    public class PropertyWindowViewModel : INotifyPropertyChanged
    {
        private readonly MetroWindow _window;

        public bool DisplaySaveButton { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand SaveWithoutVerificationCommand { get; }

        public RelayCommand CancelCommand { get; }

        public BitmapImage HeaderIcon { get; }

        public string HeaderTitle { get; }

        public string HeaderDescription { get; }

        private bool _canSave = true;
        public bool CanSaveChanges() => PropertyViewModel?.IsDirty == true;

        private PropertyViewModel _propertyViewModel;
        public PropertyViewModel PropertyViewModel
        {
            get
            {
                return _propertyViewModel;
            }
            private set
            {
                _propertyViewModel = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UIActionViewModel> UIActions { get; }

        public ObservableCollection<PropertyWindowSpecialExplanationEntryViewModel> SpecialExplanations { get; } = new();

        public PropertyWindowViewModel()
        {
            SaveCommand = new RelayCommand(Save, CanSaveChanges);
            SaveWithoutVerificationCommand = new RelayCommand(SaveWithoutVerification, CanSaveChanges);
            CancelCommand = new RelayCommand(Cancel);
            UIActions = new ObservableCollection<UIActionViewModel>();
        }

        public PropertyWindowViewModel(MetroWindow window, PropertyWindowArgs propertyWindowArgs) : this()
        {
            _window = window;

            var propertyViewModel = new PropertyViewModel(window);
            propertyViewModel.Open(
                propertyWindowArgs.Target,
                propertyWindowArgs.Parent,
                propertyWindowArgs.DeepCopy,
                propertyWindowArgs.ReadOnly,
                propertyWindowArgs.PreselectedProperty,
                propertyWindowArgs.PreselectedInstance,
                propertyWindowArgs.Detached);

            this.PropertyViewModel = propertyViewModel;

            SubscribeToIsDirtyChanged(propertyViewModel);

            if (propertyWindowArgs.IsNew)
                propertyViewModel.IsDirty = true;

            HeaderTitle = FlowBloxComponentHelper.GetDisplayName(propertyWindowArgs.Target);
            HeaderDescription = FlowBloxComponentHelper.GetDescription(propertyWindowArgs.Target);
            var headerIcon = FlowBloxComponentHelper.GetIcon32(propertyWindowArgs.Target);
            HeaderIcon = SkiaWpfImageHelper.ConvertToImageSource(headerIcon);
            DisplaySaveButton = propertyWindowArgs.CanSave;
            LoadSpecialExplanations(propertyWindowArgs.Target);
            _ = LoadUIActions(propertyWindowArgs.Target);
        }

        private void LoadSpecialExplanations(object target)
        {
            SpecialExplanations.Clear();
            if (target == null)
                return;

            var explanationEntries = target.GetType()
                .GetCustomAttributes(typeof(FlowBloxSpecialExplanationAttribute), true)
                .OfType<FlowBloxSpecialExplanationAttribute>()
                .Select(x => new
                {
                    SpecialExplanation = x.GetResolvedSpecialExplanation(),
                    x.Icon,
                    x.Color
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.SpecialExplanation))
                .ToList();

            foreach (var x in explanationEntries)
            {
                var (iconKind, iconForeground) = ResolveSpecialExplanationIcon(x.Icon, x.Color);
                SpecialExplanations.Add(new PropertyWindowSpecialExplanationEntryViewModel
                {
                    Explanation = x.SpecialExplanation,
                    IconKind = iconKind,
                    IconForeground = iconForeground
                });
            }
        }

        private static (PackIconMaterialKind IconKind, string Foreground) ResolveSpecialExplanationIcon(
            SpecialExplanationIcon icon,
            string color)
        {
            var defaultValue = icon switch
            {
                SpecialExplanationIcon.Hint => (PackIconMaterialKind.LightbulbOnOutline, "#D97706"),
                SpecialExplanationIcon.Warning => (PackIconMaterialKind.AlertCircleOutline, "#B91C1C"),
                SpecialExplanationIcon.Success => (PackIconMaterialKind.CheckCircleOutline, "#2E7D32"),
                SpecialExplanationIcon.Important => (PackIconMaterialKind.AlertOctagonOutline, "#C2410C"),
                _ => (PackIconMaterialKind.InformationOutline, "#3A6EA5")
            };

            if (!string.IsNullOrWhiteSpace(color))
            {
                return (defaultValue.Item1, color);
            }

            return defaultValue;
        }

        private void SubscribeToIsDirtyChanged(PropertyViewModel propertyViewModel)
        {
            propertyViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PropertyViewModel.IsDirty) && propertyViewModel.IsDirty)
                {
                    SaveCommand.Invalidate();
                    SaveWithoutVerificationCommand.Invalidate();
                }

            };
        }

        private async Task LoadUIActions(object target)
        {
            if (target is not IFlowBloxComponent component)
                return;

            List<UIActionViewModel> actions;

            var provider = new WpfUIActionsProvider();
            try
            {
                actions = provider.GetToolStripItemsForComponent(component);
            }
            catch(Exception e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);

                await MessageBoxHelper.ShowMessageBoxAsync(
                    _window,
                    MessageBoxType.Error,
                    FlowBloxResourceUtil.GetLocalizedString("Message_ComponentActionsLoadingFailure", typeof(Resources.PropertyWindow)));

                return;
            }

            UIActions.Clear();
            foreach (var action in actions)
                UIActions.Add(action);
        }

        private async void Save()
        {
            var success = await _propertyViewModel.SaveAsync(_window);
            if (success)
            {
                _window.DialogResult = true;
                _window.Close();
            }
        }

        private async void SaveWithoutVerification()
        {
            var success = await _propertyViewModel.SaveAsync(_window, true);
            if (success)
            {
                _window.DialogResult = true;
                _window.Close();
            }
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        public void Rollback() => _propertyViewModel.Cancel();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

