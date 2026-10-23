using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class DemoPromptsWindow : MetroWindow
    {
        public string? SelectedPrompt { get; set; }

        public DemoPromptsWindow()
        {
            InitializeComponent();
            DataContext = new DemoPromptsViewModel(this);
        }

        private void PromptsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DemoPromptsViewModel viewModel && viewModel.UseCommand.CanExecute(null))
                viewModel.UseCommand.Execute(null);
        }
    }
}
