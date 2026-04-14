namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeDebugProtocolEntry
    {
        public string Elapsed { get; set; } = "00:00.000";
        public string EventType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string FlowBlockName { get; set; }
        public int? FieldValueChangeId { get; set; }
        public int? GeneratedResultId { get; set; }
    }
}
