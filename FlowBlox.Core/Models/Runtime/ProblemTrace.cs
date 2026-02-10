namespace FlowBlox.Core.Models.Runtime
{
    public class ProblemTraceSummary
    {
        public ProblemTraceSummary()
        {
            this.Traces = new List<ProblemTrace>();
        }

        public List<ProblemTrace> Traces { get; set; }
    }

    public class ProblemTrace
    {
        public DateTime Timestamp { get; set; }
        public string Criticality { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public List<FieldValue> FieldValues { get; set; }

        public ProblemTrace()
        {
            this.Timestamp = DateTime.Now;
            this.FieldValues = new List<FieldValue>();
        }
    }

    public class FieldValue
    {
        public string FullyQualifiedName { get; set; }
        public string Value { get; set; }

        public FieldValue(string fullyQualifiedName, string value)
        {
            FullyQualifiedName = fullyQualifiedName;
            Value = value;
        }
    }
}
