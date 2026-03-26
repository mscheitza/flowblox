namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeDebugProblemEntry
    {
        public string Elapsed { get; set; } = "00:00.000";
        public string FlowBlockName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Exception { get; set; } = string.Empty;
    }
}
