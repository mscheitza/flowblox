namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeFieldValueChangeDetails
    {
        public int Id { get; set; }
        public string Elapsed { get; set; } = "00:00.000";
        public string FieldName { get; set; } = string.Empty;
        public string SourceFlowBlockName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public bool IsStored { get; set; }
        public string Note { get; set; }
    }
}
