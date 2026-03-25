using FlowBlox.AppWindow.Contents;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class ManagedObjectsViewPanelFactory : DockContentFactoryBase<ManagedObjectsView>
    {
        public ManagedObjectsViewPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override ManagedObjectsView Create()
        {
            var dockContent = new ManagedObjectsView
            {
                Dock = DockStyle.Fill,
                DockAreas = DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockBottom
            };

            var key = typeof(ManagedObjectsView).FullName;
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
