using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaction logic for EditContentView.xaml.
    /// </summary>
    public partial class EditContentView : MetroWindow
    {
        private readonly EditContentViewModel _viewModel;

        public EditContentView(string content, bool enableFieldSelection = false, bool isReadOnly = false)
        {
            InitializeComponent();

            _viewModel = new EditContentViewModel(this)
            {
                ContentText = content,
                EnableFieldSelection = enableFieldSelection,
                IsReadOnly = isReadOnly
            };

            DataContext = _viewModel;
        }

        public string ContentText => _viewModel.ContentText;
    }
}
