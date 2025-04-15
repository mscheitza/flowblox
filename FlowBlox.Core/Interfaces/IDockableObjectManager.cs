using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Interfaces
{
    public interface IDockableObjectManager : IObjectManager
    {
        bool IsActive { get; }
    }
}
