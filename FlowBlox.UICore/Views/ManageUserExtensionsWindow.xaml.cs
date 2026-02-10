using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für ManageUserExtensionsWindow.xaml
    /// </summary>
    public partial class ManageUserExtensionsWindow : MetroWindow
    {
        public ManageUserExtensionsWindow(string userToken, FbUserData userData)
        {
            InitializeComponent();
            this.DataContext = new ManageUserExtensionsViewModel(this, userToken, userData);
        }

        public void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var viewModel = (ManageUserExtensionsViewModel)this.DataContext;
            if (e.NewValue is FbExtensionResult extension)
            {
                viewModel.SelectedItem = extension;
            }
            else if (e.NewValue is FbVersionResult version)
            {
                viewModel.SelectedItem = version;
            }
            else
            {
                viewModel.SelectedItem = null;
            }
        }
    }
}
