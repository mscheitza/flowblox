using FlowBlox.UICore.Views;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.Contents
{
    public class TestView : DockContent
    {
        private readonly ElementHost _elementHost;
        private readonly TestViewControl _testViewControl;

        public TestView()
        {
            Text = FlowBlox.UICore.Resources.TestView.Title;
            Name = nameof(TestView);
            DockAreas = DockAreas.DockRight | DockAreas.DockLeft | DockAreas.DockBottom;
            Padding = new Padding(0, 25, 0, 25);

            _testViewControl = new TestViewControl();
            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Child = _testViewControl
            };

            Controls.Add(_elementHost);
        }

        internal void OnAfterUIRegistryInitialized()
        {
            _testViewControl.OnAfterUIRegistryInitialized();
            UpdateUI();
        }

        internal void UpdateUI()
        {
            _testViewControl.UpdateRuntimeState(AppWindow.Instance.IsRuntimeActive);
        }
    }
}
