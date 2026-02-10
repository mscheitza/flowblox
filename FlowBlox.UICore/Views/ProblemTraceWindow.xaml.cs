using FlowBlox.Core.Models.Runtime;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für ProblemTraceWindow.xaml
    /// </summary>
    public partial class ProblemTraceWindow : MetroWindow
    {
        public ProblemTraceWindow(ProblemTrace problemTrace)
        {
            InitializeComponent();
            var viewModel = (ProblemTraceViewModel)DataContext;
            viewModel.SelectedProblemTrace = problemTrace;
        }
    }
}
