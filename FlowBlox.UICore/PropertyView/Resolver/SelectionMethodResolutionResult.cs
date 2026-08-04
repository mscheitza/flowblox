using System.Reflection;

namespace FlowBlox.UICore.PropertyView.Resolver
{
    public class SelectionMethodResolutionResult : SelectionMemberResolutionResult
    {
        public MethodInfo Method { get; init; }
    }
}
