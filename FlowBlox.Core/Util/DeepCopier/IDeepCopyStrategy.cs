using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Util.DeepCopier
{
    public interface IDeepCopyStrategy
    {
        List<DeepCopyAction> GetDeepCopyActions(object target);
    }
}
