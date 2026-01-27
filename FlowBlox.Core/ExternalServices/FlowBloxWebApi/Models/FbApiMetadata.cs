using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbApiMetadata
    {
        public string ApiName { get; set; }
        public string Version { get; set; }

        public FbApiCapabilities Capabilities { get; set; }
    }

    public class FbApiCapabilities
    {
        public bool CanLogin { get; set; }
        public bool CanRegister { get; set; }
        public bool CanRequestPasswordReset { get; set; }
    }
}
