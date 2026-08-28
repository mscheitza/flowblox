using FlowBlox.AppWindow.Contents;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class RuntimeViewPanelFactory : DockContentFactoryBase<RuntimeView>
    {
        public RuntimeViewPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override RuntimeView Create()
        {
            var dockContent = new RuntimeView
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 25, 0, 20),
                DockAreas = DockAreas.DockBottom
            };

            var key = typeof(RuntimeView).FullName;
            return Create(key, dockContent);
        }

        protected override DockContentSettings GetDefaults()
        {
            return new DockContentSettings
            {
                DockState = DockState.DockBottom
            };
        }
    }
}