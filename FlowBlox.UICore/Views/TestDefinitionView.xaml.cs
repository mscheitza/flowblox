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
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FlowBlox.UICore.Models.FieldSelection;
using FlowBlox.UICore.Manager;
using FlowBlox.UICore.Factory;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaktionslogik für TestDefinitionView.xaml
    /// </summary>
    public partial class TestDefinitionView : MetroWindow
    {
        private readonly BaseFlowBlock _requestedCurrentFlowBlock;
        private readonly FlowBloxTransactionEventHandler _transactionEventHandler;

        public PropertyWindowCommitStatus CommitStatus => _transactionEventHandler.CommitStatus;

        public TestDefinitionView(FlowBloxTestDefinition testDefinition, BaseFlowBlock currentFlowBlock)
        {
            InitializeComponent();
            TransactionMonitorWindowFactory.Register(this);

            _requestedCurrentFlowBlock = currentFlowBlock;
            _transactionEventHandler = new FlowBloxTransactionEventHandler(
                testDefinition ?? throw new ArgumentNullException(nameof(testDefinition)),
                workingCopyOpened: InitializeWorkingCopy,
                canCommit: () => DataContext is TestDefinitionViewModel { CanApply: true },
                window: this);
            _transactionEventHandler.Open();
            Loaded += TestDefinitionView_Loaded;
            Closing += TestDefinitionView_Closing;
        }

        private void InitializeWorkingCopy(object workingCopy)
        {
            var transientTestDefinition = (FlowBloxTestDefinition)workingCopy;
            var testConfigurationSynchronizer = new FlowBloxTestDefinitionSynchronizer();
            testConfigurationSynchronizer.Synchronize(transientTestDefinition, _requestedCurrentFlowBlock);

            var latestFlowBlockResolver = new FlowBloxTestDefinitionLatestFlowBlockResolver();
            var effectiveCurrentFlowBlock = _requestedCurrentFlowBlock ??
                latestFlowBlockResolver.ResolveLatestFlowBlock(transientTestDefinition);

            var viewModel = (TestDefinitionViewModel)DataContext;
            viewModel.OwnerWindow = this;
            viewModel.HasExplicitFlowBlockContext = _requestedCurrentFlowBlock != null;
            viewModel.TestDefinition = transientTestDefinition;
            viewModel.CurrentFlowBlock = effectiveCurrentFlowBlock;
            viewModel.AcceptChanges("Test definition view initialized");
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void TestDefinitionView_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                datasetsScrollViewer?.UpdateLayout();
                datasetsScrollViewer?.ScrollToBottom();
            }), System.Windows.Threading.DispatcherPriority.Background);
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
            => _transactionEventHandler.Save();

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
            => _transactionEventHandler.Apply();

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => _transactionEventHandler.Cancel();

        private void TestDefinitionView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            => _transactionEventHandler.HandleClosing(sender, e);

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
