using FlowBlox.Core.Models.FlowBlocks.Base;
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
