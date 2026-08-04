using System.Reflection;

namespace FlowBlox.UICore.PropertyView.Resolver
{
    public class SelectionPropertyResolutionResult : SelectionMemberResolutionResult
    {
        public PropertyInfo Property { get; init; }
    }
}
