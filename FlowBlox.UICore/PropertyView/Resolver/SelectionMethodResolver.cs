using System.Reflection;

namespace FlowBlox.UICore.PropertyView.Resolver
{
    public class SelectionMethodResolver : SelectionMemberResolverBase<MethodInfo, SelectionMethodResolutionResult>
    {
        public static SelectionMethodResolutionResult ResolveSelectionFilterMethodFromTargetOrParent(
            object target,
            object parent,
            string selectionFilterMethod)
        {
            return ResolveMemberFromTargetOrParent(
                target,
                parent,
                selectionFilterMethod,
                ResolveMethod,
                CreateResult);
        }

        private static MethodInfo ResolveMethod(Type type, string name)
        {
            return type.GetMethod(name, BindingFlags);
        }

        private static SelectionMethodResolutionResult CreateResult(object invocationTarget, MethodInfo method)
        {
            return new SelectionMethodResolutionResult
            {
                InvocationTarget = invocationTarget,
                Method = method
            };
        }
    }
}
