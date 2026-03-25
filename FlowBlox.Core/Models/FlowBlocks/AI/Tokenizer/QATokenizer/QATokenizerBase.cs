using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.QATokenizer
{
    [Display(Name = "QATokenizerBase_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("QATokenizerBase_DisplayName_Plural", typeof(FlowBloxTexts))]
    public abstract class QATokenizerBase : TokenizerBase
    {
        public abstract QATokenizedInput Encode(string question, string context);

        public abstract string Decode(IEnumerable<long> tokens);
    }
}
