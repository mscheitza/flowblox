using FlowBlox.AppWindow.Contents;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class TestViewPanelFactory : DockContentFactoryBase<TestView>
    {
        public TestViewPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override TestView Create()
        {
            var dockContent = new TestView
            {
                Dock = DockStyle.Fill,
                DockAreas = DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockBottom
            };

            var key = typeof(TestView).FullName;
            return Create(key, dockContent);
        }

        protected override DockContentSettings GetDefaults()
        {
            return new DockContentSettings
            {
                DockState = DockState.DockBottom,
                Visible = true
            };
        }
    }
}
