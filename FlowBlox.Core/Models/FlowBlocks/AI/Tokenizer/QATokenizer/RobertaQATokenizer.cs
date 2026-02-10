using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.Core;
using FlowBlox.Core.Provider;
using Microsoft.ML.Tokenizers;
using Models.FlowBlocks.AI;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.QATokenizer
{
    [Display(Name = "RobertaQATokenizer_DisplayName", Description = "RobertaQATokenizer_Description", ResourceType = typeof(FlowBloxTexts))]
    public class RobertaQATokenizer : QATokenizerBase
    {
        private Microsoft.ML.Tokenizers.Tokenizer _tokenizer;

        public RobertaQATokenizer() : base()
        {
            
        }

        public override void OnAfterCreate()
        {
            this.TokenTranslations.Add(new TokenTranslation()
            {
                SpecialCharacter = SpecialCharacter.Space,
                SourceCharacter = "Ġ"
            });
            base.OnAfterCreate();
        }


        [Display(Name = "RobertaQATokenizer_TokenizerConfiguration", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.All, SelectionFilterMethod = nameof(GetPossibleTokenizerConfigurations))]
        [Required]
        public VocabMergesConfiguration TokenizerConfiguration { get; set; }

        public IEnumerable<VocabMergesConfiguration> GetPossibleTokenizerConfigurations() => FlowBloxRegistryProvider.GetRegistry().GetManagedObjects<VocabMergesConfiguration>();

        public override void Initialize()
        {
            if (_tokenizer != null)
                return;

            TokenizerConfiguration.Initialize((vocabFile, mergesFile) =>
            {
                return BpeTokenizer.Create(vocabFile, mergesFile, PreTokenizer.CreateWhiteSpace());
            });
            _tokenizer = TokenizerConfiguration.Tokenizer;
        }

        public override void DisposeTokenizer()
        {
            _tokenizer = null;
        }

        public override QATokenizedInput Encode(string question, string context)
        {
            if (_tokenizer == null)
                throw new InvalidOperationException("Tokenizer not initialized.");

            // RoBERTa format: <s> question </s></s> context </s>
            var input = $"<s>{question}</s></s>{context}</s>";
            var encoded = _tokenizer.EncodeToIds(TranslateIn(input));
            var ids = encoded.Select(e => (long)e).ToArray();
            var mask = Enumerable.Repeat(1L, ids.Length).ToArray();

            // Token type IDs for Question Answering
            var tokenTypes = new long[ids.Length];

            // Find the boundary between question and context
            var separatorCount = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == 2) // </s> token ID (may vary)
                {
                    separatorCount++;
                    if (separatorCount >= 2) // Context starts after the second </s>
                    {
                        for (int j = i + 1; j < ids.Length; j++)
                            tokenTypes[j] = 1;
                        break;
                    }
                }
            }

            return new QATokenizedInput
            {
                InputIds = ids,
                AttentionMask = mask,
                TokenTypeIds = tokenTypes
            };
        }

        public override string Decode(IEnumerable<long> tokens)
        {
            if (_tokenizer == null)
                throw new InvalidOperationException("Tokenizer not initialized.");

            return TranslateOut(_tokenizer.Decode(tokens.Select(t => (int)t).ToArray()));
        }
    }
}