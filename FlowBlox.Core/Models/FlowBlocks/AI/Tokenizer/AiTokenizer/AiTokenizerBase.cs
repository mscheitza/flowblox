using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer;
using Models.FlowBlocks.AI;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.AiTokenizer
{
    public abstract class AiTokenizerBase : TokenizerBase
    {
        protected AiTokenizerBase()
        {
            EOSToken = 50256;
        }

        [Display(Name = "AiTokenizerBase_EOSToken", Description = "AiTokenizerBase_EOSToken_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        [Required]
        public int EOSToken { get; set; }

        public abstract List<long> Encode(string text);

        public abstract string Decode(IEnumerable<long> tokens);
    }
}