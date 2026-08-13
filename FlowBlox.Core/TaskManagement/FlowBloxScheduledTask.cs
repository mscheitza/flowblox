using FlowBlox.Core.Enums;

namespace FlowBlox.Core.TaskManagement
{
    public sealed class FlowBloxScheduledTask
    {
        public string TaskName { get; set; }
        public string DisplayName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public FlowBloxTaskScheduleType ScheduleType { get; set; } = FlowBloxTaskScheduleType.Manual;
        public DateTime? StartAt { get; set; }
        public TimeSpan? Interval { get; set; }
        public DateTime? NextRunTime { get; set; }
        public DateTime? LastRunTime { get; set; }
        public int? LastResult { get; set; }
        public bool IsRunning { get; set; }
        public string ProjectName { get; set; }
        public string ProjectFile { get; set; }
        public string ProjectSpaceGuid { get; set; }
        public int? ProjectSpaceVersion { get; set; }
        public string TaskDirectory { get; set; }
        public string RequestFilePath { get; set; }
        public string ResponseFilePathTemplate { get; set; }
        public Dictionary<string, string> UserFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
