namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.QATokenizer
{
    public class QATokenizedInput
    {
        public long[] InputIds { get; set; }
        public long[] AttentionMask { get; set; }
        public long[] TokenTypeIds { get; set; }
    }
}
