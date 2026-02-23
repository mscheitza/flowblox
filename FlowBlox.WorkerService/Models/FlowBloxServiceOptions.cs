namespace FlowBlox.WorkerService.Models
{
    public sealed class FlowBloxServiceOptions
    {
        public string? ServiceName { get; set; } = "FlowBlox.Service";
        public string? DisplayName { get; set; } = "FlowBlox Project Service";
        public string? Description { get; set; } = "Executes a FlowBlox project in the background.";

        public bool RunOnStart { get; set; } = true;

        /// <summary>
        /// Local project file path (.fbprj).
        /// Used when ProjectSpaceGuid is not set.
        /// </summary>
        public string? ProjectFile { get; set; }

        /// <summary>
        /// Optional: Load project from Project Space by GUID.
        /// If set, the runner will resolve the project from Project Space.
        /// </summary>
        public string? ProjectSpaceGuid { get; set; }

        /// <summary>
        /// Optional: Stable Project Space version number.
        /// Requires ProjectSpaceGuid to be set.
        /// If null, the latest project version will be used.
        /// </summary>
        public int? ProjectSpaceVersion { get; set; }

        public bool Restart { get; set; } = false;
        public int RestartDelaySeconds { get; set; } = 10;

        public bool AbortOnError { get; set; } = true;
        public bool AbortOnWarning { get; set; } = false;

        public string? OutputFile { get; set; }

        public Dictionary<string, string>? UserFields { get; set; }
        public Dictionary<string, string>? OptionOverrides { get; set; }
    }
}
