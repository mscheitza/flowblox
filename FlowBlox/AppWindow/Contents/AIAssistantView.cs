using FlowBlox.UICore.Views;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class AIAssistantView : DockContent
    {
        private readonly ElementHost _elementHost;

        public AIAssistantView()
        {
            Text = "AI Assistant";
            Name = nameof(AIAssistantView);
            DockAreas = DockAreas.DockRight | DockAreas.DockLeft | DockAreas.DockBottom;

            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = new AIAssistantControl()
            };

            Controls.Add(_elementHost);
        }
    }
}
