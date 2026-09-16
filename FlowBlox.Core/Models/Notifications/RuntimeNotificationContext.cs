using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Runtime.Debugging;

namespace FlowBlox.Core.Models.Notifications
{
    public sealed class RuntimeNotificationContext
    {
        public RuntimeNotificationType Type { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.Now;
        public DateTime RuntimeStartedAt { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TriggeringErrorMessage { get; set; } = string.Empty;
        public Exception Exception { get; set; }
        public string FlowBlockName { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public IReadOnlyCollection<string> IpAddresses { get; set; } = Array.Empty<string>();
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string RuntimeLogFilePath { get; set; } = string.Empty;
        public RuntimeCancellationContext Cancellation { get; set; }
        public bool IsDebugging { get; set; }
    }
}
