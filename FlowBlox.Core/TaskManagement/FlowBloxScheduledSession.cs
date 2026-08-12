namespace FlowBlox.Core.TaskManagement
{
    public sealed class FlowBloxScheduledSession
    {
        public DateTime CreatedAt { get; set; }
        public string SessionId { get; set; }
        public string SessionDirectory { get; set; }
        public string LogFilePath { get; set; }
        public string ResponseFilePath { get; set; }
        public FlowBloxScheduledSessionStatus Status { get; set; }
        public string StatusText { get; set; }
        public string StatusIconKind { get; set; }
        public string StatusBrush { get; set; }
        public string Message { get; set; }
    }
}
