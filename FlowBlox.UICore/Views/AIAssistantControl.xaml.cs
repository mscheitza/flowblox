using FlowBlox.UICore.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowBlox.UICore.Views
{
    public partial class AIAssistantControl : UserControl
    {
        public event EventHandler? ConfigurationRequested;

        public AIAssistantControl()
        {
            InitializeComponent();
            PromptTextBox.PreviewKeyDown += PromptTextBox_PreviewKeyDown;
            SettingsButton.Click += SettingsButton_Click;
        }

        private void SettingsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ConfigurationRequested?.Invoke(this, EventArgs.Empty);
        }

        private void PromptTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not AIAssistantViewModel vm)
                return;

            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var caretIndex = PromptTextBox.CaretIndex;
                PromptTextBox.Text = PromptTextBox.Text.Insert(caretIndex, Environment.NewLine);
                PromptTextBox.CaretIndex = caretIndex + Environment.NewLine.Length;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
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
