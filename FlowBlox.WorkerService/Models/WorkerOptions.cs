namespace FlowBlox.WorkerService.Models
{
    public sealed class FlowBloxServiceOptions
    {
        public string? ServiceName { get; set; } = "FlowBlox.Service";
        public string? DisplayName { get; set; } = "FlowBlox Project Service";
        public string? Description { get; set; } = "Executes a FlowBlox project in the background.";

        public bool RunOnStart { get; set; } = true;

        public string? ProjectFile { get; set; }

        public bool Restart { get; set; } = false;
        public int RestartDelaySeconds { get; set; } = 10;

        public bool AbortOnError { get; set; } = true;
        public bool AbortOnWarning { get; set; } = false;

        public string? OutputFile { get; set; }

        public Dictionary<string, string>? UserFields { get; set; }
        public Dictionary<string, string>? OptionOverrides { get; set; }
    }
}
