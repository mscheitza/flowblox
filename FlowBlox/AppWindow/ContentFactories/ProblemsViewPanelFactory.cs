using FlowBlox.AppWindow.Contents;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class ProblemsViewPanelFactory : DockContentFactoryBase<ProblemsView>
    {
        public ProblemsViewPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override ProblemsView Create()
        {
            var dockContent = new ProblemsView
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 25, 0, 20),
                DockAreas = DockAreas.DockBottom
            };

            var key = typeof(ProblemsView).FullName;
            return Create(key, dockContent);
        }

        protected override DockContentSettings GetDefaults()
        {
            return new DockContentSettings
            {
                DockState = DockState.DockBottom,
                Visible = false
            };
        }
    }
}