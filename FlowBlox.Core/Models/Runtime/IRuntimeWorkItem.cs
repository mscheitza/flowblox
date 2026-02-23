using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.Runtime
{
    public interface IRuntimeWorkItem
    {
        void Run(BaseRuntime runtime);
    }
}
