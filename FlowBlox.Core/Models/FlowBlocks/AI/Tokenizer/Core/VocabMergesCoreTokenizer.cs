using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util.Fields;
using Microsoft.ML.Tokenizers;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.Core
{
    [Display(Name = "VocabMergesConfiguration_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class VocabMergesConfiguration : ManagedObject
    {
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

        protected override void OnReferencedFieldNameChanged(FieldElement field, string oldFQFieldName, string newFQFieldName)
        {
            this.VocabFile = this.VocabFile.Replace(oldFQFieldName, newFQFieldName);
            this.MergesFile = this.MergesFile.Replace(oldFQFieldName, newFQFieldName);
            this.TokenizerDirectory = this.TokenizerDirectory.Replace(oldFQFieldName, newFQFieldName);
            base.OnReferencedFieldNameChanged(field, oldFQFieldName, newFQFieldName);
        }

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
