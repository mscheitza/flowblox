using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.WebRequest
{
    public class ConfigurableWebRequestResult
    {
        public bool Success { get; set; }
        public string UrlCalled { get; set; }
        public string Content { get; set; }
    }
}
