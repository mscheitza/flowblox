using FlowBlox.Core.Models.Project;
using FlowBlox.UICore.Factory.Adapter;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.Utilities;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UICoreTextBoxHelper = FlowBlox.UICore.Utilities.TextBoxHelper;
using FlowBlox.UICore.Models.FieldSelection;

namespace FlowBlox.UICore.Views
{
    public partial class ManageInputFilesWindow : MetroWindow
    {
        public ManageInputFilesWindow(FlowBloxProject project)
        {
            InitializeComponent();
            DataContext = new ManageInputFilesViewModel(this, project);
        }

        private void CommandTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F || Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (sender is not TextBox textBox)
                return;

            if (textBox.DataContext is not FlowBloxInputFile inputFile)
                return;

            OpenInputFilePlaceholderSelection(textBox, inputFile);
            e.Handled = true;
        }

        private void InsertInputFilePlaceholderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            TextBox textBox = null;
            if (button.Parent is DockPanel dockPanel)
                textBox = dockPanel.Children.OfType<TextBox>().FirstOrDefault();

            textBox ??= FindVisualChild<TextBox>(button);
            if (textBox == null || textBox.DataContext is not FlowBloxInputFile inputFile)
                return;

            OpenInputFilePlaceholderSelection(textBox, inputFile);
        }

        private void OpenInputFilePlaceholderSelection(TextBox textBox, FlowBloxInputFile inputFile)
        {
            if (DataContext is not ManageInputFilesViewModel vm)
                return;

            var args = new FieldSelectionWindowArgs
            {
                SelectionMode = FieldSelectionMode.InputFiles,
                AllowedFieldSelectionModes = [FieldSelectionMode.InputFiles],
                MultiSelect = false,
                InputFileElements = vm.GetInputFilePlaceholderElements(inputFile)
            };

            var win = new FieldSelectionWindow(args)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (win.ShowDialog() != true || win.Result == null)
                return;

            UICoreTextBoxHelper.ApplyInputFileElementsToTextBox(win.Result.SelectedInputFiles, new WpfTextBoxAdapter(textBox));
        }

        private static T FindVisualChild<T>(DependencyObject start) where T : DependencyObject
        {
            if (start == null)
                return null;

            var childCount = VisualTreeHelper.GetChildrenCount(start);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(start, i);
                if (child is T typedChild)
                    return typedChild;

                var nested = FindVisualChild<T>(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }
    }
}

