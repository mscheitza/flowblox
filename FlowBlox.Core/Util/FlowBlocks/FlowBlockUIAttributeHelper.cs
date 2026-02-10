using FlowBlox.Core.Attributes;

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
