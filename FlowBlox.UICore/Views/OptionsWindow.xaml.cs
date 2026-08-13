using FlowBlox.Core.Models.Components;
using FlowBlox.UICore.Resources;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels.Options;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using OptionsWindowResources = FlowBlox.UICore.Resources.OptionsWindow;

namespace FlowBlox.UICore.Views
{
    public partial class OptionsWindow : MetroWindow
    {
        private bool _isUpdatingPassword;

        public OptionsWindow()
            : this(null)
        {
        }

        public OptionsWindow(OptionElement selectedOption)
        {
            InitializeComponent();
            DataContext = new OptionsWindowViewModel(selectedOption);

            if (DataContext is OptionsWindowViewModel viewModel)
            {
                viewModel.ErrorOccurred += ViewModel_ErrorOccurred;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private OptionsWindowViewModel ViewModel => DataContext as OptionsWindowViewModel;

        private void OptionsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ViewModel != null)
                ViewModel.SelectedNode = e.NewValue as OptionTreeNodeViewModel;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditValueWindow(false, false)
            {
                Owner = this,
                Description = OptionsWindowResources.AddOption_Description
            };

            if (dialog.ShowDialog() != true)
                return;

            var error = ViewModel?.AddOption(dialog.GetValue());
            if (!string.IsNullOrWhiteSpace(error))
                await MessageBoxHelper.ShowMessageBoxAsync(this, MessageBoxType.Error, error);
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmed = await MessageBoxHelper.ShowQuestionAsync(
                this,
                OptionsWindowResources.ResetOptions_Message);

            if (confirmed == true)
                ViewModel?.ResetOptions();
        }

        private async void ViewModel_ErrorOccurred(object sender, string message)
        {
            await MessageBoxHelper.ShowMessageBoxAsync(this, MessageBoxType.Error, message);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OptionsWindowViewModel.Value) ||
                e.PropertyName == nameof(OptionsWindowViewModel.IsPasswordType))
                SyncPasswordFromViewModel();
        }

        private void SyncPasswordFromViewModel()
        {
            if (ViewModel?.IsPasswordType != true)
                return;

            if (ValuePasswordBox.Password == ViewModel.Value)
                return;

            _isUpdatingPassword = true;
            ValuePasswordBox.Password = ViewModel.Value ?? string.Empty;
            _isUpdatingPassword = false;
        }

        private void ValuePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingPassword || ViewModel?.IsPasswordType != true)
                return;

            ViewModel.Value = ValuePasswordBox.Password;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is OptionsWindowViewModel viewModel)
            {
                viewModel.ErrorOccurred -= ViewModel_ErrorOccurred;
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            base.OnClosed(e);
        }
    }
}
