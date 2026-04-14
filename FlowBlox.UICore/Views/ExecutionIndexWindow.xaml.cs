using MahApps.Metro.Controls;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ExecutionIndexResources = FlowBlox.UICore.Resources.ExecutionIndexWindow;

namespace FlowBlox.UICore.Views
{
    public partial class ExecutionIndexWindow : MetroWindow
    {
        private static readonly Regex NumericInputRegex = new("^[0-9]+$");

        public int Result { get; private set; } = -1;

        public ExecutionIndexWindow(int executionIndex)
        {
            InitializeComponent();

            if (executionIndex >= 0)
            {
                ExecutionIndexTextBox.Text = executionIndex.ToString();
                Result = executionIndex;
            }

            ApplyButton.IsEnabled = executionIndex >= 0;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            ExecutionIndexTextBox.Focus();
            Keyboard.Focus(ExecutionIndexTextBox);
            ExecutionIndexTextBox.CaretIndex = ExecutionIndexTextBox.Text.Length;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void RemoveExecutionIndexButton_Click(object sender, RoutedEventArgs e)
        {
            Result = -1;
            DialogResult = true;
            Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseExecutionIndex(out var executionIndex))
            {
                MessageBox.Show(
                    this,
                    ExecutionIndexResources.Validation_InvalidIndex_Message,
                    ExecutionIndexResources.Validation_InvalidIndex_Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            Result = executionIndex;
            DialogResult = true;
            Close();
        }

        private void ExecutionIndexTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !NumericInputRegex.IsMatch(e.Text);
        }

        private void ExecutionIndexTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyButton.IsEnabled = TryParseExecutionIndex(out _);
        }

        private bool TryParseExecutionIndex(out int executionIndex)
        {
            if (int.TryParse(ExecutionIndexTextBox.Text, out executionIndex) && executionIndex >= 0)
            {
                return true;
            }

            executionIndex = -1;
            return false;
        }
    }
}
