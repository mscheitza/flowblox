using System;
using System.Collections.ObjectModel;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbExtensionChangeRequest
    {
        public string ExtensionGuid { get; set; }

        public string Description { get; set; }

        public bool Active { get; set; }
    }
}
