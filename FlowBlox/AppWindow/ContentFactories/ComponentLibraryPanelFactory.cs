using FlowBlox.AppWindow.Contents;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class ComponentLibraryPanelFactory : DockContentFactoryBase<ComponentLibraryPanel>
    {
        public ComponentLibraryPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override ComponentLibraryPanel Create()
        {
            var dockContent = new ComponentLibraryPanel
            {
                Dock = DockStyle.Fill
            };
            var key = typeof(ComponentLibraryPanel).FullName;
            return Create(key, dockContent);
        }

        protected override DockContentSettings GetDefaults()
        {
            return new DockContentSettings
            {
                DockState = DockState.DockLeft
            };
        }
    }
}
