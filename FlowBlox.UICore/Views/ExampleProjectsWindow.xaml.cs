using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class ExampleProjectsWindow : MetroWindow
    {
        public ExampleProjectsWindow()
        {
            InitializeComponent();
            DataContext = new ExampleProjectsViewModel(this);
        }

        private void ProjectsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ExampleProjectsViewModel viewModel && viewModel.OpenCommand.CanExecute(null))
                viewModel.OpenCommand.Execute(null);
        }
    }
}
