using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Enums
{
    [Flags()]
    public enum FlowBlockFlags
    {
        None = 0,
        RequirementsNotMet = 1
    }
}
