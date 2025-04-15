using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
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
