using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbProjectChangeRequest
    {
        public string ProjectGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public FbProjectVisibility? Visibility { get; set; }
        public string ContentBase64 { get; set; }
    }
}
