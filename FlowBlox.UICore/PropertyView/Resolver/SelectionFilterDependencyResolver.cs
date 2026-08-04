using System.Reflection;

namespace FlowBlox.UICore.PropertyView.Resolver
{
    public class SelectionFilterDependencyResolver : SelectionMemberResolverBase<PropertyInfo, SelectionPropertyResolutionResult>
    {
        public static SelectionPropertyResolutionResult ResolveSelectionFilterDependencyFromTargetOrParent(
            object target,
            object parent,
            string selectionFilterDependency)
        {
            return ResolveMemberFromTargetOrParent(
                target,
                parent,
                selectionFilterDependency,
                ResolveProperty,
                CreateResult);
        }

        private static PropertyInfo ResolveProperty(Type type, string name)
        {
            return type.GetProperty(name, BindingFlags);
        }

        private static SelectionPropertyResolutionResult CreateResult(object invocationTarget, PropertyInfo property)
        {
            return new SelectionPropertyResolutionResult
            {
                InvocationTarget = invocationTarget,
                Property = property
            };
        }
    }
}
