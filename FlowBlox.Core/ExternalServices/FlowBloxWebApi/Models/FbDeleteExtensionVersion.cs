using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbDeleteExtensionVersionRequest
    {
        public Guid ExtensionGuid { get; set; }

        public string VersionNumber { get; set; }
    }
}
