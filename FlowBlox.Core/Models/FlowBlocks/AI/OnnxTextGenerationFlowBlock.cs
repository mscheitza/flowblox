using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer;
using FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.AiTokenizer;
using FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    [Display(Name = "OnnxTextGenerationFlowBlock_DisplayName", Description = "OnnxTextGenerationFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class OnnxTextGenerationFlowBlock : OnnxBaseFlowBlock
    {
        #region Tab: Default

        [Display(Name = "OnnxTextGenerationFlowBlock_AiTokenizer", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.All, SelectionFilterMethod = nameof(GetPossibleAiTokenizers))]
        [Required]
        public AiTokenizerBase AiTokenizer { get; set; }

        private IEnumerable<AiTokenizerBase> GetPossibleAiTokenizers()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            return registry.GetManagedObjects<AiTokenizerBase>();
        }

        [Display(Name = "OnnxTextGenerationFlowBlock_Prompt", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBlockTextBox(IsCodingMode = true, MultiLine = true)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [Required]
        public string Prompt { get; set; }

        #endregion

        #region Tab: Extended settings

        [Display(Name = "OnnxTextGenerationFlowBlock_TokenSelectionStrategy",
            Description = "OnnxTextGenerationFlowBlock_TokenSelectionStrategy_Tooltip",
            GroupName = "OnnxBaseFlowBlock_Groups_ExtendedSettings",
            ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public TokenSelectionStrategy TokenSelectionStrategy { get; set; }

        [Display(Name = "OnnxTextGenerationFlowBlock_MaxTokens",
            GroupName = "OnnxBaseFlowBlock_Groups_ExtendedSettings",
            ResourceType = typeof(FlowBloxTexts), Order = 2)]
        public int MaxTokens { get; set; }

        #endregion

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.text_long, 16, SKColors.CadetBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.text_long, 32, SKColors.CadetBlue);

        public OnnxTextGenerationFlowBlock()
        {
            MaxTokens = 100;
        }

        public override void RuntimeStarted(BaseRuntime runtime)
        {
            AiTokenizer.DisposeTokenizer();
            base.RuntimeStarted(runtime);
        }

        protected override void OnReferencedFieldNameChanged(FieldElement field, string oldFQFieldName, string newFQFieldName)
        {
            this.Prompt = this.Prompt.Replace(oldFQFieldName, newFQFieldName);
            base.OnReferencedFieldNameChanged(field, oldFQFieldName, newFQFieldName);
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                if (string.IsNullOrEmpty(ModelPath) || !File.Exists(ModelPath))
                {
                    CreateNotification(runtime, OnnxAiNotifications.ModelFileMissing);
                    return;
                }

                if (AiTokenizer == null)
                {
                    CreateNotification(runtime, OnnxAiNotifications.TokenizerMissing);
                    return;
                }

                string promptText = FlowBloxFieldHelper.ReplaceFieldsInString(Prompt);

                try
                {
                    AiTokenizer.Initialize();
                }
                catch (Exception ex)
                {
                    CreateNotification(runtime, OnnxAiNotifications.AITokenizerInitializationFailed, ex);
                    return;
                }

                string result;
                try
                {
                    result = ExecutePrompt(runtime, promptText);
                }
                catch (Exception ex)
                {
                    CreateNotification(runtime, OnnxAiNotifications.PromptExecutionFailed, ex);
                    return;
                }

                runtime.Report($"ONNX-AI result: {result}");
                GenerateResult(runtime, result);
            });
        }

        public string ExecutePrompt(BaseRuntime runtime, string promptText)
        {
            var inputIds = AiTokenizer.Encode(promptText).ToList();
            var generatedIds = new List<long>();
            var selector = TokenSelectorFactory.Create(TokenSelectionStrategy);
            while (true) 
            {
                int length = inputIds.Count;
                var inputTensor = new DenseTensor<long>([1, length]);
                var positionTensor = new DenseTensor<long>([1, length]);
                var attentionMaskTensor = new DenseTensor<long>([1, length]);

                for (int i = 0; i < length; i++)
                {
                    inputTensor[0, i] = inputIds[i];
                    positionTensor[0, i] = i;
                    attentionMaskTensor[0, i] = 1;
                }

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                    NamedOnnxValue.CreateFromTensor("position_ids", positionTensor),
                    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
                };

                using var results = _session.Run(inputs);
                var logits = results.First().AsTensor<float>();

                int vocabSize = logits.Dimensions[2];
                int lastIndex = logits.Dimensions[1] - 1;

                var nextToken = selector.SelectNextToken(logits, lastIndex);
                if (nextToken == AiTokenizer.EOSToken)
                    break;

                runtime.Report($"ONNX Model append token: " + nextToken);
                inputIds.Add(nextToken);
                generatedIds.Add(nextToken);
            }

            return AiTokenizer.Decode(generatedIds);
        }

        public override List<string> GetDisplayableProperties()
        {
            var props = base.GetDisplayableProperties();
            props.Add(nameof(ModelPath));
            return props;
        }
    }

    public enum OnnxAiNotifications
    {
        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Model path is empty or file does not exist.")]
        ModelFileMissing,

        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Tokenizer instance is null.")]
        TokenizerMissing,

        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Failed to execute prompt using ONNX model.")]
        PromptExecutionFailed,

        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "General execution error in the ONNX AI flow block.")]
        ExecutionFailed,

        [FlowBlockNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Could not initialize AI tokenizer.")]
        AITokenizerInitializationFailed
    }
}
