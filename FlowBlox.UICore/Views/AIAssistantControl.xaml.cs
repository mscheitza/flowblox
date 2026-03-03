using FlowBlox.UICore.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class AIAssistantControl : UserControl
    {
        public AIAssistantControl()
        {
            InitializeComponent();
            PromptTextBox.PreviewKeyDown += PromptTextBox_PreviewKeyDown;
        }

        private void PromptTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not AIAssistantViewModel vm)
                return;

            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (vm.SubmitCommand.CanExecute(null))
                {
                    vm.SubmitCommand.Execute(null);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (vm.CancelCommand.CanExecute(null))
                {
                    vm.CancelCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}
