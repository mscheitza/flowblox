using FlowBlox.Core.Models.Project;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für CreateOrUpdatePSProjectWindow.xaml
    /// </summary>
    public partial class CreateOrUpdatePSProjectWindow : MetroWindow
    {
        public CreateOrUpdatePSProjectWindow(FlowBloxProject project)
        {
            InitializeComponent();
            DataContext = new CreateOrUpdatePSProjectViewModel(this, project);
        }
    }
}