namespace FlowBlox.Core.Util.DeepCopier
{
    public interface IDeepCopyStrategy
    {
        List<DeepCopyAction> GetDeepCopyActions(object target);
    }
}
