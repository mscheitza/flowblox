using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.Models.FieldSelection;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using InsertTextOrFieldWindowResources = FlowBlox.UICore.Resources.InsertTextOrFieldWindow;

namespace FlowBlox.UICore.Views
{
    public partial class InsertTextOrFieldWindow : MetroWindow
    {
        private readonly BaseFlowBlock _flowBlock;
        private readonly string _noFieldSelectedText;
        private FieldElement? _selectedField;
        private bool _selectedFieldRequired;

        public InsertTextOrFieldWindow(BaseFlowBlock flowBlock, string parameterName)
        {
            InitializeComponent();

            _flowBlock = flowBlock;
            _noFieldSelectedText = InsertTextOrFieldWindowResources.NoFieldSelected;

            ParameterTextBlock.Text = parameterName;
            SelectedFieldTextBox.Text = _noFieldSelectedText;
            SelectedValue = string.Empty;
            UpdateUI();
        }

        public string SelectedValue { get; private set; }

        public FieldElement? GetSelectedField()
        {
            if (!string.IsNullOrWhiteSpace(ValueTextBox.Text))
                return null;

            if (SelectedFieldTextBox.Text == _noFieldSelectedText)
                return null;

            return _selectedField;
        }

        public bool IsSelectedFieldRequired() => _selectedFieldRequired;

        private string GetValueBySelection()
        {
            if (!string.IsNullOrWhiteSpace(ValueTextBox.Text))
                return ValueTextBox.Text;

            if (SelectedFieldTextBox.Text != _noFieldSelectedText)
                return SelectedFieldTextBox.Text;

            return string.Empty;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedValue = GetValueBySelection();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            var fieldSelectionEnabled = string.IsNullOrWhiteSpace(ValueTextBox.Text);
            SelectedFieldTextBox.IsEnabled = fieldSelectionEnabled;
            SelectFieldButton.IsEnabled = fieldSelectionEnabled;
        }

        private void SelectFieldButton_Click(object sender, RoutedEventArgs e)
        {
            var args = new FieldSelectionWindowArgs
            {
                FlowBlock = _flowBlock,
                SelectionMode = FieldSelectionMode.Fields,
                MultiSelect = false,
                IsRequired = true
            };

            var selectionDialog = new FieldSelectionWindow(args)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (selectionDialog.ShowDialog() == true &&
                selectionDialog.Result?.SelectedFields?.Count == 1)
            {
                var selectedField = selectionDialog.Result.SelectedFields[0];
                SelectedFieldTextBox.Text = selectedField.FullyQualifiedName;
                _selectedField = selectedField;
                _selectedFieldRequired = selectionDialog.Result.IsRequired;
            }
        }
    }
}
