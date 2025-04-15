using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für PasswordResetWindow.xaml
    /// </summary>
    public partial class PasswordResetWindow : MetroWindow
    {
        public PasswordResetWindow()
        {
            InitializeComponent();
            this.DataContext = new PasswordResetViewModel(this);
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
