namespace FlowBlox.SequenceDetection.Model
{
    public class SequenceDetectionInputEntry
    {
        public SequenceDetectionInputEntry(string content, string match, int count)
        {
            Content = content;
            Match = match;
            Count = count;
        }

        public string Content { get; set; }

        public string Match { get; set; }

        public int Count { get; set; }
    }
}
