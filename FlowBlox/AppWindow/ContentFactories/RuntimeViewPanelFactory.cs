using FlowBlox.AppWindow.Contents;
using FlowBlox.Views;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class RuntimeViewPanelFactory : DockContentFactoryBase<DockContentUserControlWrapper<RuntimeView>>
    {
        public RuntimeViewPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override DockContentUserControlWrapper<RuntimeView> Create()
        {
            var dockContent = new DockContentUserControlWrapper<RuntimeView>
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 25),
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
