namespace FlowBlox.Core.Provider.Registry
{
    public enum FlowBloxRegistryTransactionState
    {
        Active,
        Committed,
        Cancelled
    }

    public sealed class FlowBloxRegistryTransactionSnapshot
    {
        public long Id { get; init; }

        public string RegistryType { get; init; }

        public string TargetType { get; init; }

        public bool IsDetached { get; init; }

        public bool IsNested { get; init; }

        public int Depth { get; init; }

        public DateTime OpenedAt { get; init; }

        public DateTime? ClosedAt { get; init; }

        public FlowBloxRegistryTransactionState State { get; init; }

        public bool IsCurrent { get; init; }
    }
}
