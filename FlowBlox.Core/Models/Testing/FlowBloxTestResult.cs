namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestResult
    {
        public FlowBloxTestResult(bool success, Dictionary<string, string> fieldValueAssignments)
        {
            Success = success;
            FieldValueAssignments = fieldValueAssignments;
        }

        public bool Success { get; set; }

        public Dictionary<string, string> FieldValueAssignments { get; set; }
    }
}
