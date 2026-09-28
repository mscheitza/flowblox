using FlowBlox.Core.Provider.Registry;
using FlowBlox.UICore.Resources;

namespace FlowBlox.UICore.ViewModels
{
    public sealed class TransactionMonitorEntryViewModel
    {
        public long Id { get; init; }

        public bool IsCurrent { get; init; }

        public string TargetType { get; init; }

        public string RegistryType { get; init; }

        public string Mode { get; init; }

        public int Depth { get; init; }

        public string OpenedAt { get; init; }

        public string ClosedAt { get; init; }

        public string State { get; init; }

        public static TransactionMonitorEntryViewModel FromSnapshot(
            FlowBloxRegistryTransactionSnapshot snapshot)
        {
            return new TransactionMonitorEntryViewModel
            {
                Id = snapshot.Id,
                IsCurrent = snapshot.IsCurrent,
                TargetType = GetShortTypeName(snapshot.TargetType),
                RegistryType = snapshot.RegistryType,
                Mode = snapshot.IsDetached
                    ? TransactionMonitorWindow.Mode_Detached
                    : TransactionMonitorWindow.Mode_Standard,
                Depth = snapshot.Depth,
                OpenedAt = snapshot.OpenedAt.ToString("HH:mm:ss.fff"),
                ClosedAt = snapshot.ClosedAt?.ToString("HH:mm:ss.fff") ?? "—",
                State = GetStateText(snapshot.State)
            };
        }

        private static string GetShortTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return TransactionMonitorWindow.Target_Unknown;

            var lastSeparator = typeName.LastIndexOf('.');
            return lastSeparator >= 0 ? typeName[(lastSeparator + 1)..] : typeName;
        }

        private static string GetStateText(FlowBloxRegistryTransactionState state) => state switch
        {
            FlowBloxRegistryTransactionState.Active => TransactionMonitorWindow.State_Active,
            FlowBloxRegistryTransactionState.Committed => TransactionMonitorWindow.State_Committed,
            FlowBloxRegistryTransactionState.Cancelled => TransactionMonitorWindow.State_Cancelled,
            _ => state.ToString()
        };
    }
}
