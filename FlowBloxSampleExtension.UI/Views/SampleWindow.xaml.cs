using MahApps.Metro.Controls;
using System.Windows;

namespace FlowBloxSampleExtension.UI.Views
{
    public partial class SampleWindow : MetroWindow
    {
        public SampleWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}