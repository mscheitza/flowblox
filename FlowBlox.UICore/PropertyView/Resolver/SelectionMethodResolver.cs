using System.Reflection;

namespace FlowBlox.UICore.PropertyView.Resolver
{
    public class SelectionMethodResolutionResult
    {
        public MethodInfo Method { get; init; }

        public object InvocationTarget { get; init; }
    }

    public static class SelectionMethodResolver
    {
        public static SelectionMethodResolutionResult ResolveSelectionFilterMethodFromTargetOrParent(
            object target,
            object parent,
            string selectionFilterMethod)
        {
            if (target == null || string.IsNullOrWhiteSpace(selectionFilterMethod))
                return null;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var method = target.GetType().GetMethod(selectionFilterMethod, flags);
            if (method != null)
            {
                return new SelectionMethodResolutionResult
                {
                    Method = method,
                    InvocationTarget = target
                };
            }

            if (parent == null)
                return null;

            method = parent.GetType().GetMethod(selectionFilterMethod, flags);
            if (method == null)
                return null;

            return new SelectionMethodResolutionResult
            {
                Method = method,
                InvocationTarget = parent
            };
        }
    }
}
