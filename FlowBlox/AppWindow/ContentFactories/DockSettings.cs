using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
