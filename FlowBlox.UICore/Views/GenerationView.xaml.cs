using FlowBlox.Core.Models.FlowBlocks.Additions;
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
    /// Interaktionslogik für GenerationView.xaml
    /// </summary>
    public partial class GenerationView : MetroWindow
    {
        public GenerationView(BaseFlowBlock currentFlowBlock)
        {
            InitializeComponent();
            var viewModel = (GenerationViewModel)DataContext;
            viewModel.CurrentFlowBlock = currentFlowBlock;
            _ = viewModel.Generate();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
