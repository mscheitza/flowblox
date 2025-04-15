using FlowBlox.Core.Factories;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;
using FlowBlox.Core.Util;
using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
    }
}
