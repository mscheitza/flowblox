using FlowBlox.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp;

namespace FlowBlox.Core.Util.FlowBlocks
{
    public static class FlowBlockUIAttributeHelper
    {
        public static bool IsDynamicallyReadOnly(object target, FlowBlockUIAttribute? flowBlockUI)
        {
            if (target == null) 
                throw new ArgumentNullException(nameof(target));

            if (flowBlockUI?.ReadOnlyMethod == null)
                return false;

            var readOnlySelector = target.GetType().GetMethod(flowBlockUI.ReadOnlyMethod);
            if (readOnlySelector != null)
            {
                var result = readOnlySelector.Invoke(target, Array.Empty<object>());
                if (result is bool readOnly)
                    return readOnly;
            }
            return false;
        }
    }
}
