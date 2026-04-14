using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Testing;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Factory.Adapter;
using FlowBlox.UICore.Models;
using FlowBlox.UICore.Utilities;
using FlowBlox.Core.Util;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für TestDefinitionView.xaml
    /// </summary>
    public partial class TestDefinitionView : MetroWindow
    {
        public TestDefinitionView(FlowBloxTestDefinition testDefinition, BaseFlowBlock currentFlowBlock)
        {
            InitializeComponent();
            var viewModel = (TestDefinitionViewModel)DataContext;

            var testConfigurationUsageResolver = new FlowBloxTestDefinitionUsageResolver();
            var usages = testConfigurationUsageResolver.ResolveUsages(testDefinition);

            if (currentFlowBlock != null)
            {
                var testConfigurationAppender = new FlowBloxTestDefinitionAppender();
                testConfigurationAppender.Append(testDefinition, currentFlowBlock);
                usages.AddIfNotExists(currentFlowBlock);
            }

            viewModel.OwnerWindow = this;
            viewModel.TestDefinition = testDefinition;
            viewModel.CurrentFlowBlock = currentFlowBlock;
            viewModel.TestDefinitionUsages = usages;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TestDefinitionViewModel.TestResultsColumns))
            {
                if (DataContext is TestDefinitionViewModel viewModel)
                {
                    UpdateTestResultsColumns(viewModel.TestResultsColumns);
                }
            }
        }

        private void UpdateTestResultsColumns(ObservableCollection<DataGridColumn> columns)
        {
            testResultsDataGrid.Columns.Clear();
            foreach (var column in columns)
            {
                testResultsDataGrid.Columns.Add(column);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void InsertFieldPlaceholderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            var parentGrid = FindVisualParent<System.Windows.Controls.Grid>(button);
            if (parentGrid == null)
                return;

            var textBox = FindVisualChild<TextBox>(parentGrid, "UserInputTextBox");
            if (textBox == null)
                return;

            var args = new FieldSelectionWindowArgs
            {
                SelectionMode = FieldSelectionMode.Options,
                IsRequired = false,
                HideRequired = true,
                AllowedFieldSelectionModes = [FieldSelectionMode.ProjectProperties, FieldSelectionMode.Options]
            };

            var adapter = new WpfTextBoxAdapter(textBox);
            Utilities.TextBoxHelper.ShowFieldSelectionDialog(target: null, args, adapter, this);
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var current = child;
            while (current != null)
            {
                if (current is T match)
                    return match;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static T FindVisualChild<T>(DependencyObject parent, string elementName = null) where T : FrameworkElement
        {
            if (parent == null)
                return null;

            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed && (string.IsNullOrWhiteSpace(elementName) || typed.Name == elementName))
                    return typed;

                var nested = FindVisualChild<T>(child, elementName);
                if (nested != null)
                    return nested;
            }

            return null;
        }
    }
}
