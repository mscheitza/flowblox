namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeCancellationContext
    {
        public DateTime UtcTimestamp { get; set; } = DateTime.UtcNow;
        public RuntimeCancellationKind CancellationKind { get; set; } = RuntimeCancellationKind.Unknown;
        public string Reason { get; set; } = string.Empty;
    }
}
