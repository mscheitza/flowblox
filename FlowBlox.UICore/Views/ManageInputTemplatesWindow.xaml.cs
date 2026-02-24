using FlowBlox.Core.Models.Project;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

namespace FlowBlox.UICore.Views
{
    public partial class ManageInputTemplatesWindow : MetroWindow
    {
        public ManageInputTemplatesWindow(FlowBloxProject project)
        {
            InitializeComponent();
            DataContext = new ManageInputTemplatesViewModel(this, project);
        }
    }
}