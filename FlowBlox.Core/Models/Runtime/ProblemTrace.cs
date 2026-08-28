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
        private string _displayValue;

        public string FullyQualifiedName { get; set; }
        public string Value { get; set; }
        public int ExecutionIndex { get; set; }
        public string DisplayValue
        {
            get => _displayValue ?? Value;
            set => _displayValue = value;
        }

        public FieldValue(string fullyQualifiedName, string value)
            : this(fullyQualifiedName, value, value)
        {
        }

        public FieldValue(string fullyQualifiedName, string value, string displayValue)
            : this(fullyQualifiedName, value, displayValue, 0)
        {
        }

        public FieldValue(string fullyQualifiedName, string value, string displayValue, int executionIndex)
        {
            FullyQualifiedName = fullyQualifiedName;
            Value = value;
            DisplayValue = displayValue;
            ExecutionIndex = executionIndex;
        }
    }
}