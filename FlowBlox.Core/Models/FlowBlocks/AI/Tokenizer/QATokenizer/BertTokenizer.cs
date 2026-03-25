using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using Models.FlowBlocks.AI;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.QATokenizer
{
    [Display(Name = "BertTokenizer_DisplayName", Description = "BertTokenizer_Description", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("BertTokenizer_DisplayName_Plural", typeof(FlowBloxTexts))]
    public class BertTokenizer : QATokenizerBase
    {
        private Microsoft.ML.Tokenizers.BertTokenizer _tokenizer;

        public BertTokenizer()
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

        [Display(Name = "BertTokenizer_VocabFile", Description = "BertTokenizer_VocabFile_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFileSelection)]
        [Required]
        public string VocabFile { get; set; }

        public override void Initialize()
        {
            if (string.IsNullOrEmpty(VocabFile) || !File.Exists(VocabFile))
                throw new FileNotFoundException($"The BERT vocabulary file was not found at: {VocabFile}");
            
            _tokenizer = Microsoft.ML.Tokenizers.BertTokenizer.Create(VocabFile);
        }

        public override void DisposeTokenizer()
        {
            _tokenizer = null;
        }

        public override QATokenizedInput Encode(string question, string context)
        {
            if (_tokenizer == null)
                throw new InvalidOperationException("Tokenizer not initialized.");

            var questionIds = _tokenizer.EncodeToIds(question);
            var contextIds = _tokenizer.EncodeToIds(context);

            var inputIds = new List<long>();
            var tokenTypeIds = new List<long>();

            // [CLS]
            inputIds.Add(_tokenizer.ClassificationTokenId);
            tokenTypeIds.Add(0);

            // Question tokens
            inputIds.AddRange(questionIds.Select(id => (long)id));
            tokenTypeIds.AddRange(Enumerable.Repeat(0L, questionIds.Count));

            // [SEP]
            inputIds.Add(_tokenizer.SeparatorTokenId);
            tokenTypeIds.Add(0);

            // Context tokens
            inputIds.AddRange(contextIds.Select(id => (long)id));
            tokenTypeIds.AddRange(Enumerable.Repeat(1L, contextIds.Count));

            // [SEP]
            inputIds.Add(_tokenizer.SeparatorTokenId);
            tokenTypeIds.Add(1);

            var attentionMask = Enumerable.Repeat(1L, inputIds.Count).ToArray();

            return new QATokenizedInput
            {
                InputIds = inputIds.ToArray(),
                AttentionMask = attentionMask,
                TokenTypeIds = tokenTypeIds.ToArray()
            };
        }

        public override string Decode(IEnumerable<long> tokens)
        {
            if (_tokenizer == null)
                throw new InvalidOperationException("Tokenizer is not initialized.");

            return _tokenizer.Decode(tokens.Select(t => (int)t).ToArray());
        }
    }
}
