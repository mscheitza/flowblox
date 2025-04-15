using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.Project
{
    public class FlowBloxProjectReloadResult
    {
        public bool Success { get; set; }

        public List<string> RemainingAssemblies { get; set; }

        public List<string> UnloadableExtensions { get; set; }

        public FlowBloxProjectReloadResult()
        {
            Success = true;
            RemainingAssemblies = new List<string>();
            UnloadableExtensions = new List<string>();
        }
    }
}
