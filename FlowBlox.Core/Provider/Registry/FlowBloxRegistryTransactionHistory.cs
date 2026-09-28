namespace FlowBlox.Core.Provider.Registry
{
    public static class FlowBloxRegistryTransactionHistory
    {
        private const int MaximumHistoryCount = 500;
        private static readonly object _sync = new();
        private static readonly List<TransactionHistoryEntry> _entries = new();
        private static long _nextTransactionId;

        public static event EventHandler TransactionsChanged;

        public static int ActiveTransactionCount
        {
            get
            {
                lock (_sync)
                    return _entries.Count(x => x.State == FlowBloxRegistryTransactionState.Active);
            }
        }

        public static bool HasInactiveTransactions
        {
            get
            {
                lock (_sync)
                    return _entries.Any(x => x.State != FlowBloxRegistryTransactionState.Active);
            }
        }

        public static IReadOnlyList<FlowBloxRegistryTransactionSnapshot> GetHistory()
        {
            lock (_sync)
            {
                var currentRegistry = _entries
                    .LastOrDefault(x => x.State == FlowBloxRegistryTransactionState.Active)
                    ?.Registry;

                return _entries
                    .Select(x => new FlowBloxRegistryTransactionSnapshot
                    {
                        Id = x.Id,
                        RegistryType = x.RegistryType,
                        TargetType = x.TargetType,
                        IsDetached = x.IsDetached,
                        IsNested = x.IsNested,
                        Depth = x.Depth,
                        OpenedAt = x.OpenedAt,
                        ClosedAt = x.ClosedAt,
                        State = x.State,
                        IsCurrent = ReferenceEquals(x.Registry, currentRegistry)
                    })
                    .ToList();
            }
        }

        public static void ClearInactiveTransactions()
        {
            var changed = false;
            lock (_sync)
            {
                changed = _entries.RemoveAll(x =>
                    x.State != FlowBloxRegistryTransactionState.Active) > 0;
            }

            if (changed)
                OnTransactionsChanged();
        }

        internal static void RegisterOpened(
            FlowBloxRegistry registry,
            object target,
            bool detached,
            bool nested,
            int depth)
        {
            lock (_sync)
            {
                _entries.Add(new TransactionHistoryEntry
                {
                    Id = ++_nextTransactionId,
                    Registry = registry,
                    RegistryType = registry.GetType().Name,
                    TargetType = target?.GetType().FullName,
                    IsDetached = detached,
                    IsNested = nested,
                    Depth = depth,
                    OpenedAt = DateTime.Now,
                    State = FlowBloxRegistryTransactionState.Active
                });
                TrimHistory();
            }

            OnTransactionsChanged();
        }

        internal static void RegisterCompleted(
            FlowBloxRegistry registry,
            FlowBloxRegistryTransactionState state)
        {
            lock (_sync)
            {
                var entry = _entries.LastOrDefault(x =>
                    ReferenceEquals(x.Registry, registry) &&
                    x.State == FlowBloxRegistryTransactionState.Active);
                if (entry == null)
                    return;

                entry.State = state;
                entry.ClosedAt = DateTime.Now;
            }

            OnTransactionsChanged();
        }

        private static void TrimHistory()
        {
            while (_entries.Count > MaximumHistoryCount)
            {
                var completedEntry = _entries.FirstOrDefault(x =>
                    x.State != FlowBloxRegistryTransactionState.Active);
                if (completedEntry == null)
                    return;

                _entries.Remove(completedEntry);
            }
        }

        private static void OnTransactionsChanged() =>
            TransactionsChanged?.Invoke(null, EventArgs.Empty);

        private sealed class TransactionHistoryEntry
        {
            public long Id { get; init; }
            public FlowBloxRegistry Registry { get; init; }
            public string RegistryType { get; init; }
            public string TargetType { get; init; }
            public bool IsDetached { get; init; }
            public bool IsNested { get; init; }
            public int Depth { get; init; }
            public DateTime OpenedAt { get; init; }
            public DateTime? ClosedAt { get; set; }
            public FlowBloxRegistryTransactionState State { get; set; }
        }
    }
}
