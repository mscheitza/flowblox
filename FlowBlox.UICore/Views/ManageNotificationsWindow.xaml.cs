using FlowBlox.Core.Models.FlowBlocks.Base;
using MahApps.Metro.Controls;
using FlowBlox.UICore.ViewModels;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik fuer ManageNotificationSuppressionsWindow.xaml
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
