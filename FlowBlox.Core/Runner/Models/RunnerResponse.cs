namespace FlowBlox.Core.Runner.Contracts
{
    public sealed class RunnerResponse
    {
        public RunnerResponse()
        {
            Warnings = new List<string>();
            Outputs = new(StringComparer.OrdinalIgnoreCase);
        }

        public bool Success { get; set; }
        public int ExitCode { get; set; }

        public List<string> Warnings { get; set; }
        public string ErrorMessage { get; set; }
        public string Exception { get; set; }

        public string ProjectName { get; set; }
        public DateTime StartedUtc { get; set; }
        public DateTime FinishedUtc { get; set; }
        public string LogfilePath { get; set; }

        public Dictionary<string, List<ProjectOutputDatasetDto>> Outputs { get; set; } 
    }
}
