using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für PasswordResetWindow.xaml
    /// </summary>
    public partial class PasswordResetWindow : MetroWindow
    {
        public PasswordResetWindow(string apiUrl)
        {
            InitializeComponent();
            this.DataContext = new PasswordResetViewModel(this, apiUrl);
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is PasswordResetViewModel viewModel)
                viewModel.NewPassword = ((PasswordBox)sender).Password;
        }

        private void NewPasswordRepeatBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is PasswordResetViewModel viewModel)
                viewModel.NewPasswordRepeat = ((PasswordBox)sender).Password;
        }
    }
}
