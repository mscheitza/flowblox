namespace FlowBlox.SequenceDetection
{
    public class SequenceDetectionPattern
    {
        public SequenceDetectionPattern()
        {

        }

        public SequenceDetectionPattern(SequenceDetectionPattern? parent)
        {
            if (parent != null)
                parent.Next = this;

            this.Parent = parent;
        }

        public string? Sequence { get; set; }
        public string? ValueTerminationSequence { get; set; }
        public string? IterationTerminationSequence { get; set; }
        public SequenceDetectionPattern Root => GetRoot(this);
        public SequenceDetectionPattern? Parent { get; set; }
        public SequenceDetectionPattern? Next { get; set; }
        public int TotalLength => GetTotalLength(this);
        public bool HasIterationTerminationSequence => GetHasIterationTerminationSequence(this);

        private int GetTotalLength(SequenceDetectionPattern sequenceDetectionPattern)
        {
            return Sequence!.Length + (sequenceDetectionPattern.Next != null ? GetTotalLength(sequenceDetectionPattern.Next) : 0);
        }
            
        private bool GetHasIterationTerminationSequence(SequenceDetectionPattern pattern)
        {
            return !string.IsNullOrEmpty(pattern!.IterationTerminationSequence) || 
                    (pattern.Next != null && GetHasIterationTerminationSequence(pattern.Next));
        }

        private SequenceDetectionPattern GetRoot(SequenceDetectionPattern pattern)
        {
            return pattern.Parent == null ? pattern : GetRoot(pattern.Parent);
        }
    }
}