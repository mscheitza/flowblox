using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Views;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class ProblemsView : DockContent
    {
        private readonly ElementHost _elementHost;
        private readonly ProblemsViewControl _problemsViewControl;

        public ProblemsView()
        {
            Text = FlowBloxResourceUtil.GetLocalizedString("ProblemsView_Text", typeof(FlowBloxMainUITexts));
            Name = Text;
            DockAreas = DockAreas.DockBottom;
            Padding = new Padding(0, 0, 0, 25);

            _problemsViewControl = new ProblemsViewControl();
            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Child = _problemsViewControl
            };

            Controls.Add(_elementHost);
        }

        internal void Append(ProblemTrace problemTrace)
        {
            _problemsViewControl.Append(problemTrace);
        }
    }
}