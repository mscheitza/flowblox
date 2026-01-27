using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        public LoginWindow(string apiUrl = null)
        {
            InitializeComponent();
            DataContext = new LoginWindowViewModel(this, apiUrl);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginWindowViewModel viewModel)
            {
                var passwordBox = sender as PasswordBox;
                viewModel.Password = passwordBox?.Password;
            }
        }
    }
}