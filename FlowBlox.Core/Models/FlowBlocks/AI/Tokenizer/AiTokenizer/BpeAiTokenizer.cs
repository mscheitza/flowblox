using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.Core;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Fields;
using Microsoft.ML.Tokenizers;
using Models.FlowBlocks.AI;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.AiTokenizer
{
    [Display(Name = "BpeAiTokenizer_DisplayName", Description = "BpeAiTokenizer_Description", ResourceType = typeof(FlowBloxTexts))]
    public class BpeAiTokenizer : AiTokenizerBase
    {
        private Microsoft.ML.Tokenizers.Tokenizer _tokenizer;

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


        [Display(Name = "BpeAiTokenizer_TokenizerConfiguration", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.All, SelectionFilterMethod = nameof(GetPossibleTokenizerConfigurations))]
        [Required]
        public VocabMergesConfiguration TokenizerConfiguration { get; set; }

        public IEnumerable<VocabMergesConfiguration> GetPossibleTokenizerConfigurations() => FlowBloxRegistryProvider.GetRegistry().GetManagedObjects<VocabMergesConfiguration>();

        public override void Initialize()
        {
            if (_tokenizer != null)
                return;

            TokenizerConfiguration.Initialize(BpeTokenizer.Create);

            _tokenizer = TokenizerConfiguration.Tokenizer;
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

            var tokenList = tokens.Select(id => (int)id).ToList();
            return TranslateOut(_tokenizer.Decode(tokenList));
        }
    }
}