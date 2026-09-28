using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    public partial class TransactionMonitorWindow : MetroWindow
    {
        private readonly TransactionMonitorViewModel _viewModel;

        public TransactionMonitorWindow()
        {
            InitializeComponent();
            _viewModel = new TransactionMonitorViewModel(Dispatcher);
            DataContext = _viewModel;
            Closed += (_, _) => _viewModel.Dispose();
        }
    }
}
