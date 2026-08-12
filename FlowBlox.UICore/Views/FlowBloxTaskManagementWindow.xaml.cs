using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    public partial class FlowBloxTaskManagementWindow : MetroWindow
    {
        public FlowBloxTaskManagementWindow()
        {
            InitializeComponent();
            DataContext = new FlowBloxTaskManagementViewModel(this);
        }
    }
}