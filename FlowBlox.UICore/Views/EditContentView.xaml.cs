using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
