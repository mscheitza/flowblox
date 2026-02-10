using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    /// <summary>
    /// Interaction logic for EditContentView.xaml
    /// </summary>
    public partial class EditContentView : MetroWindow
    {
        private EditContentViewModel _viewModel;

        public EditContentView(string content)
        {
            InitializeComponent();
            _viewModel = new EditContentViewModel(this);
            _viewModel.ContentText = content;
            this.DataContext = _viewModel;
        }

        public string ContentText
        {
            get
            {
                return _viewModel.ContentText;
            }
        }
    }
}
