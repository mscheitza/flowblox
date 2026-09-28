using MahApps.Metro.Controls;
using System.Windows;
using FlowBlox.UICore.Enums;
using FlowBlox.UICore.Manager;
using FlowBloxSampleExtension.Models.FlowBlocks.SampleCategory;
using FlowBloxSampleExtension.UI.ViewModels;
using System.ComponentModel;

namespace FlowBloxSampleExtension.UI.Views
{
    public partial class SampleWindow : MetroWindow
    {
        private readonly SampleWindowViewModel _viewModel;
        private readonly FlowBloxTransactionEventHandler _transactionEventHandler;

        public PropertyWindowCommitStatus CommitStatus => _transactionEventHandler.CommitStatus;

        public SampleWindow(SampleExtensionFlowBlock component)
        {
            InitializeComponent();

            _viewModel = (SampleWindowViewModel)DataContext;

            // Custom windows own their transaction lifecycle. The handler coordinates deep-copy
            // commits, Apply/reopen and cancellation. workingCopyOpened rebinds the ViewModel to
            // every fresh working copy, while canCommit provides the shared save/apply condition.
            _transactionEventHandler = new FlowBloxTransactionEventHandler(
                component,
                workingCopyOpened: workingCopy => _viewModel.Open((SampleExtensionFlowBlock)workingCopy),
                canCommit: () => _viewModel.CanCommit,
                window: this);

            Closing += SampleWindow_Closing;
            _transactionEventHandler.Open();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => _transactionEventHandler.Cancel();

        private void ApplyButton_Click(object sender, RoutedEventArgs e) => _transactionEventHandler.Apply();

        private void SaveButton_Click(object sender, RoutedEventArgs e) => _transactionEventHandler.Save();

        private void SampleWindow_Closing(object? sender, CancelEventArgs e) =>
            _transactionEventHandler.HandleClosing(sender!, e);
    }
}
