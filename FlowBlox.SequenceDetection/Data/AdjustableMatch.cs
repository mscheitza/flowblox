namespace FlowBlox.SequenceDetection.Data
{
    internal class AdjustableMatch
    {
        public AdjustableMatch(string value, int index, int length)
        {
            this.Value = value;
            this.Index = index;
            this.Length = length;
        }

        public string Value { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }

        internal AdjustableMatch CopyAndAdjust(int adjustIndex)
        {
            return new AdjustableMatch(Value, Index + adjustIndex, Length);
        }
    }
}
