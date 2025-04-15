using FlowBlox.AppWindow.Contents;
using FlowBlox.Views;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class ProblemsViewPanelFactory : DockContentFactoryBase<DockContentUserControlWrapper<ProblemsView>>
    {
        public ProblemsViewPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override DockContentUserControlWrapper<ProblemsView> Create()
        {
            var dockContent = new DockContentUserControlWrapper<ProblemsView>
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 25)
            };
            var key = typeof(ProblemsView).FullName;
            Create(key, dockContent);
            return dockContent;
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
