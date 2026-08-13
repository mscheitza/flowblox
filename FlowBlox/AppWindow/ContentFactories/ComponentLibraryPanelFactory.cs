using FlowBlox.AppWindow.Contents;
using FlowBlox.Core.Util.Resources;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class ComponentLibraryPanelFactory : DockContentFactoryBase<ComponentLibraryView>
    {
        private string _displayName;

        public ComponentLibraryPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
            _displayName = FlowBloxResourceUtil.GetLocalizedString($"{nameof(ComponentLibraryView) + "_Text"}", typeof(FlowBloxMainUITexts));
        }

        public override ComponentLibraryView Create()
        {
            var dockContent = new ComponentLibraryView
            {
                Dock = DockStyle.Fill,
                Text = _displayName,
                Name = _displayName,
                DockAreas = DockAreas.DockLeft | DockAreas.DockRight
            };
            var key = typeof(ComponentLibraryView).FullName;
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
