using FlowBlox.AppWindow.Contents;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class FieldViewPanelFactory : DockContentFactoryBase<FieldView>
    {
        public FieldViewPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
        }

        public override FieldView Create()
        {
            var dockContent = new FieldView
            {
                Dock = DockStyle.Fill,
                DockAreas = DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockBottom
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
