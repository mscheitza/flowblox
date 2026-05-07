using FlowBlox.UICore.ViewModels;
using MahApps.Metro.Controls;

namespace FlowBlox.UICore.Views
{
    public partial class DisplayContentView : MetroWindow
    {
        public DisplayContentView(string contentText)
        {
            InitializeComponent();
            DataContext = new DisplayContentViewModel(this, contentText);
        }
    }
}
