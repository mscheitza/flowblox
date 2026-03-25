using SkiaSharp;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util.Fields;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.Core
{
    [Display(Name = "VocabMergesConfiguration_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    [PluralDisplayName("VocabMergesConfiguration_DisplayName_Plural", typeof(FlowBloxTexts))]
    public class VocabMergesConfiguration : ManagedObject
    {
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.source_merge, 16, new SKColor(2, 132, 199));

        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.source_merge, 32, new SKColor(2, 132, 199));
        private Microsoft.ML.Tokenizers.Tokenizer _tokenizer;

        [ConditionallyRequired(CheckReadOnly = true)]
        [DependsOnProperty(MemberNames = [nameof(VocabFile), nameof(MergesFile)])]
        [Display(Name = "VocabMergesConfiguration_TokenizerDirectory", Description = "VocabMergesConfiguration_TokenizerDirectory_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFolderSelection | UIOptions.EnableFieldSelection,
            ReadOnlyMethod = nameof(IsTokenizerDirectoryReadOnly))]
        public string TokenizerDirectory { get; set; }

        [ConditionallyRequired(CheckReadOnly = true)]
        [DependsOnProperty(MemberName = nameof(TokenizerDirectory))]
        [Display(Name = "VocabMergesConfiguration_VocabFile", Description = "VocabMergesConfiguration_VocabFile_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFileSelection | UIOptions.EnableFieldSelection,
            ReadOnlyMethod = nameof(IsVocabAndMergesReadOnly))]
        public string VocabFile { get; set; }

        [ConditionallyRequired(CheckReadOnly = true)]
        [DependsOnProperty(MemberName = nameof(TokenizerDirectory))]
        [Display(Name = "VocabMergesConfiguration_MergesFile", Description = "VocabMergesConfiguration_MergesFile_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFileSelection | UIOptions.EnableFieldSelection,
            ReadOnlyMethod = nameof(IsVocabAndMergesReadOnly))]
        public string MergesFile { get; set; }

        public bool IsTokenizerDirectoryReadOnly()
        {
            return !string.IsNullOrWhiteSpace(VocabFile) ||
                   !string.IsNullOrWhiteSpace(MergesFile);
        }

        public bool IsVocabAndMergesReadOnly() => !string.IsNullOrWhiteSpace(TokenizerDirectory);

        public void Initialize<T>(Func<string, string, T> factoryMethod) where T : Microsoft.ML.Tokenizers.Tokenizer
        {
            if (_tokenizer != null)
                return;

            string vocabPath;
            string mergesPath;

            if (!string.IsNullOrWhiteSpace(TokenizerDirectory))
            {
                string tokenizerDirectory = FlowBloxFieldHelper.ReplaceFieldsInString(TokenizerDirectory);
                if (!Directory.Exists(tokenizerDirectory))
                    throw new DirectoryNotFoundException($"The tokenizer directory \"{tokenizerDirectory}\" could not be found. Please ensure the folder exists and is correctly linked.");

                vocabPath = Path.Combine(tokenizerDirectory, "vocab.json");
                mergesPath = Path.Combine(tokenizerDirectory, "merges.txt");
            }
            else
            {
                vocabPath = FlowBloxFieldHelper.ReplaceFieldsInString(VocabFile);
                mergesPath = FlowBloxFieldHelper.ReplaceFieldsInString(MergesFile);
            }

            if (!File.Exists(vocabPath) || !File.Exists(mergesPath))
                throw new FileNotFoundException("Tokenizer files not found: vocab.json or merges.txt");

            _tokenizer = factoryMethod.Invoke(vocabPath, mergesPath);
        }

        public Microsoft.ML.Tokenizers.Tokenizer Tokenizer
        {
            get
            {
                return _tokenizer;
            }
        }
    }
}


