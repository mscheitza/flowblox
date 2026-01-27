using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class PSProjectsWindow : MetroWindow
    {
        public PSProjectsWindow()
        {
            InitializeComponent();
            DataContext = new PSProjectsViewModel(this);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is PSProjectsViewModel vm)
                vm.SearchCommand.Execute(null);
        }

        private void ProjectsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PSProjectsViewModel vm && vm.IsProjectSelected)
                vm.OpenProjectCommand.Execute(null);
        }
    }
}
