using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für CreateExtensionWindow.xaml
    /// </summary>
    public partial class CreateExtensionWindow : MetroWindow
    {
        public CreateExtensionWindow(string userToken)
        {
            InitializeComponent();
            this.DataContext = new CreateExtensionViewModel(this, userToken);
        }
    }
}
