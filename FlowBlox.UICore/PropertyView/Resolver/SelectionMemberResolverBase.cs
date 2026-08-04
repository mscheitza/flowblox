using System.Reflection;

namespace FlowBlox.UICore.PropertyView.Resolver
{
    public abstract class SelectionMemberResolverBase<TMember, TResult>
        where TMember : MemberInfo
        where TResult : SelectionMemberResolutionResult
    {
        protected const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance |
                                                    System.Reflection.BindingFlags.Public |
                                                    System.Reflection.BindingFlags.NonPublic;

        protected static TResult ResolveMemberFromTargetOrParent(
            object target,
            object parent,
            string memberName,
            Func<Type, string, TMember> resolveMember,
            Func<object, TMember, TResult> createResult)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName))
                return null;

            var member = resolveMember(target.GetType(), memberName);
            if (member != null)
                return createResult(target, member);

            if (parent == null)
                return null;

            member = resolveMember(parent.GetType(), memberName);
            return member != null
                ? createResult(parent, member)
                : null;
        }
    }
}
