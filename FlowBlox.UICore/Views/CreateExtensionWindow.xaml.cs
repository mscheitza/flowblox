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
