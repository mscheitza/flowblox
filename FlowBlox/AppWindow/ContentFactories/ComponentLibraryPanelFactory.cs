using FlowBlox.AppWindow.Contents;
using FlowBlox.Core.Util.Resources;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class ComponentLibraryPanelFactory : DockContentFactoryBase<ComponentLibraryPanel>
    {
        private string _displayName;

        public ComponentLibraryPanelFactory(DockPanel dockPanel) : base(dockPanel)
        {
            _displayName = FlowBloxResourceUtil.GetLocalizedString($"{nameof(ComponentLibraryPanel) + "_Text"}", typeof(FlowBloxMainUITexts));
        }

        public override ComponentLibraryPanel Create()
        {
            var dockContent = new ComponentLibraryPanel
            {
                Dock = DockStyle.Fill,
                Name = _displayName,
                DockAreas = DockAreas.DockLeft | DockAreas.DockRight
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
