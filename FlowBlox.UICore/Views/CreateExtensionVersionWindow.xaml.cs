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
