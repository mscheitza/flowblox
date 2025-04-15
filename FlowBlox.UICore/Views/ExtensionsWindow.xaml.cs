using FlowBlox.Core.Models.Project;
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
    /// Interaktionslogik für ExtensionsWindow.xaml
    /// </summary>
    public partial class ExtensionsWindow : MetroWindow
    {
        public ExtensionsWindow(FlowBloxProject project)
        {
            InitializeComponent();
            DataContext = new ExtensionsViewModel(this, project);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SearchButton.IsEnabled)
                {
                    if (SearchButton.Command != null && SearchButton.Command.CanExecute(null))
                        SearchButton.Command.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
