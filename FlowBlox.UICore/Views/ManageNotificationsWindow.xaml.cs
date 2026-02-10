using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für ManageNotificationSuppressionsWindow.xaml
    /// </summary>
    public partial class ManageNotificationsWindow : MetroWindow
    {
        public ManageNotificationsWindow(BaseFlowBlock flowBlock)
        {
            InitializeComponent();
            DataContext = new ManageNotificationOverridesViewModel(flowBlock, this);
        }
    }
}
