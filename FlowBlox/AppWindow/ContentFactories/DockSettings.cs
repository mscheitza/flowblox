using System.Collections.Generic;

namespace FlowBlox.AppWindow.ContentFactories
{
    public class DockSettings
    {
        public DockSettings()
        {
            this.DockContentSettings = new Dictionary<string, DockContentSettings>();
        }

        public Dictionary<string, DockContentSettings> DockContentSettings { get; set; }
    }

}
