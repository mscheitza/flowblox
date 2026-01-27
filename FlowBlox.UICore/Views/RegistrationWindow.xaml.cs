using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : MetroWindow
    {
        private RegistrationViewModel _viewModel;

        public RegistrationWindow(string apiUrl)
        {
            InitializeComponent();
            _viewModel = new RegistrationViewModel(this, apiUrl);
            DataContext = _viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }

        private void PasswordRepeatBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                _viewModel.PasswordRepeat = passwordBox.Password;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
