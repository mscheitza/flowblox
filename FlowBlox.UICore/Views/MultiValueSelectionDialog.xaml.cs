using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für MultiValueSelectionDialog.xaml
    /// </summary>
    public partial class MultiValueSelectionDialog : Window
    {
        private MultiValueSelectionDialogViewModel _viewModel;

        public MultiValueSelectionDialog(string title, string description, GenericSelectionHandler genericSelectionHandler)
        {
            InitializeComponent();
            _viewModel = new MultiValueSelectionDialogViewModel(title, description, genericSelectionHandler);
            DataContext = _viewModel;
        }

        public DisplayItem SelectedItem => _viewModel.SelectedItem;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
