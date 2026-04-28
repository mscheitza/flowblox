using FlowBlox.Core.Models.Project;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using SettingsResources = FlowBlox.UICore.Resources.ProjectPanelGridSettingsWindow;

namespace FlowBlox.UICore.Views
{
    public partial class ProjectPanelGridSettingsWindow : MetroWindow
    {
        private static readonly Regex NumericInputRegex = new("^[0-9]+$");

        private ProjectPanelGridSettingsViewModel ViewModel => (ProjectPanelGridSettingsViewModel)DataContext;

        public ProjectPanelGridSettingsWindow(FlowBloxProject project)
        {
            InitializeComponent();
            DataContext = new ProjectPanelGridSettingsViewModel(project);
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !NumericInputRegex.IsMatch(e.Text);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.TrySave(out var validationMessage))
            {
                MessageBox.Show(
                    this,
                    validationMessage,
                    SettingsResources.Validation_Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
