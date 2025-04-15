using FlowBlox.Core.Enums;
using Microsoft.ML.Tokenizers;
using Models.FlowBlocks.AI;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.AiTokenizer
{
    [Display(Name = "TiktokenAiTokenizer_DisplayName", Description = "TiktokenAiTokenizer_Description", ResourceType = typeof(FlowBloxTexts))]
    public class TiktokenAiTokenizer : AiTokenizerBase
    {
        private TiktokenTokenizer _tokenizer;

        [Display(Name = "TiktokenAiTokenizer_ModelName", Description = "TiktokenAiTokenizer_ModelName_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [Required]
        public string ModelName { get; set; }

        public override void OnAfterCreate()
        {
            this.TokenTranslations.Add(new TokenTranslation()
            {
                SpecialCharacter = SpecialCharacter.Space,
                SourceCharacter = "Ġ"
            });
            this.TokenTranslations.Add(new TokenTranslation()
            {
                SpecialCharacter = SpecialCharacter.LineBreak,
                SourceCharacter = "Ċ"
            });
            base.OnAfterCreate();
        }

        public override void Initialize()
        {
            _tokenizer = TiktokenTokenizer.CreateForModel(ModelName);
        }

        public override void DisposeTokenizer()
        {
            _tokenizer = null;
        }

        public override List<long> Encode(string text)
        {
            if (_tokenizer == null)
                throw new InvalidOperationException("Tokenizer not initialized.");

            return _tokenizer.EncodeToIds(TranslateIn(text))
                .Select(Convert.ToInt64)
                .ToList();
        }

        public override string Decode(IEnumerable<long> tokens)
        {
            if (_tokenizer == null)
                throw new InvalidOperationException("Tokenizer not initialized.");

            return TranslateOut(_tokenizer.Decode(tokens.Select(t => (int)t).ToArray()));
        }
    }
}