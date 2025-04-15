namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.QATokenizer
{
    public abstract class QATokenizerBase : TokenizerBase
    {
        public abstract QATokenizedInput Encode(string question, string context);

        public abstract string Decode(IEnumerable<long> tokens);
    }
}
