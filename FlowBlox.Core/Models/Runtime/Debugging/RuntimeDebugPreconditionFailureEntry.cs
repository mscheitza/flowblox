namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeDebugPreconditionFailureEntry
    {
        public string Elapsed { get; set; } = "00:00.000";
        public string FlowBlockName { get; set; } = string.Empty;
        public List<string> Messages { get; set; } = new();
    }
}
