using FlowBlox.Core.Models.Runtime;
using FlowBlox.UICore.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class ProblemsViewControl : UserControl
    {
        public ProblemsViewControl()
        {
            InitializeComponent();
        }

        private ProblemsViewModel ViewModel => DataContext as ProblemsViewModel;

        public void Append(ProblemTrace problemTrace)
        {
            ViewModel?.Append(problemTrace);
        }

        private void ProblemTraceListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel?.OpenProblemTraceCommand.CanExecute(null) == true)
                ViewModel.OpenProblemTraceCommand.Execute(null);
        }
    }
}