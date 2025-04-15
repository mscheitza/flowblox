using System.Windows.Forms;
using FlowBlox.Core;
using FlowBlox.Core.Util.Controls;

namespace FlowBlox.Views
{
    internal partial class LoadProjectView : Form
    {
        public LoadProjectView(string projectName)
        {
            InitializeComponent();
			FlowBloxStyle.ApplyStyle(this);
            FlowBloxUILocalizationUtil.Localize(this);

            this.Text = Text.Replace("$Project", projectName);
            this.lblHeader.Text = lblHeader.Text.Replace("$Project", projectName);
        }
    }
}
