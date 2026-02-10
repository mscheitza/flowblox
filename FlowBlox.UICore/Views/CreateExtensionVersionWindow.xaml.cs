using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für CreateExtensionVersionWindow.xaml
    /// </summary>
    public partial class CreateExtensionVersionWindow : MetroWindow
    {
        public CreateExtensionVersionWindow(string userToken, FbExtensionResult extensionResult)
        {
            InitializeComponent();
            this.DataContext = new CreateExtensionVersionViewModel(this, userToken, extensionResult);
        }
    }
}
