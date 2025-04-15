using FlowBlox.AppWindow.Contents;
using FlowBlox.Views;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class FieldViewPanelFactory : DockContentFactoryBase<DockContentUserControlWrapper<FieldView>>
    {
        public FieldViewPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override DockContentUserControlWrapper<FieldView> Create()
        {
            var dockContent = new DockContentUserControlWrapper<FieldView>
            {
                Dock = DockStyle.Fill
            };
            var key = typeof(FieldView).FullName;
            return Create(key, dockContent);
        }

        protected override DockContentSettings GetDefaults()
        {
            return new DockContentSettings
            {
                DockState = DockState.DockRight
            };
        }
    }
}
