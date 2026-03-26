namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public enum RuntimeCancellationKind
    {
        Unknown = 0,
        UserCancellation = 1,
        DebuggingTargetReached = 2,
        DebuggingTimeout = 3,
        AbortOnWarningOrError = 4
    }
}
