using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class DockContentSettings
    {
        public DockContentSettings()
        {
            this.Visible = true;
        }

        public int? Width { get; set; }
        public int? Height { get; set; }
        public DockState DockState { get; set; }
        public bool Visible { get; set; }
    }
}
