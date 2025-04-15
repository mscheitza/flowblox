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

    [Display(Name = "LlamaAiTokenizer_DisplayName", Description = "LlamaAiTokenizer_Description", ResourceType = typeof(FlowBloxTexts))]
    public class LlamaAiTokenizer : AiTokenizerBase
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

        [Required()]
        [Display(Name = "LlamaAiTokenizer_TokenizerModelPath", Description = "LlamaAiTokenizer_TokenizerModelPath_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFileSelection | UIOptions.EnableFieldSelection)]
        public string TokenizerModelPath { get; set; }

        public override void Initialize()
        {
            if (_tokenizer != null)
                return;

            if (!File.Exists(TokenizerModelPath))
                throw new FileNotFoundException($"The tokenizer model file was not found at the specified path: {TokenizerModelPath}");

            var tokenizerModelStream = File.OpenRead(TokenizerModelPath);

            _tokenizer = LlamaTokenizer.Create(tokenizerModelStream);
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