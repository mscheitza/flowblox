using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.AI.Enums;
using FlowBlox.Core.Models.FlowBlocks.AI.PositionSelector;
using FlowBlox.Core.Models.FlowBlocks.AI.Tokenizer.QATokenizer;
using FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    [Display(Name = "OnnxQuestionAnsweringFlowBlock_DisplayName", Description = "OnnxQuestionAnsweringFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class OnnxQuestionAnsweringFlowBlock : OnnxBaseFlowBlock
    {
        #region Tab: Default

        [Display(Name = "OnnxQuestionAnsweringFlowBlock_QATokenizer", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.All, SelectionFilterMethod = nameof(GetPossibleQATokenizers))]
        [Required]
        public QATokenizerBase QATokenizer { get; set; }

        private IEnumerable<QATokenizerBase> GetPossibleQATokenizers()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            return registry.GetManagedObjects<QATokenizerBase>();
        }

        [Display(Name = "OnnxQuestionAnsweringFlowBlock_Question", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(MultiLine = false)]
        [Required]
        public string Question { get; set; }

        [Display(Name = "OnnxQuestionAnsweringFlowBlock_Context", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(MultiLine = true)]
        [Required]
        public string Context { get; set; }

        #endregion

        #region Tab: Extended settings

        [Display(Name = "OnnxQuestionAnsweringFlowBlock_PositionSelectionStrategy",
            Description = "OnnxQuestionAnsweringFlowBlock_PositionSelectionStrategy_Tooltip",
            GroupName = "OnnxBaseFlowBlock_Groups_ExtendedSettings",
            ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public PositionSelectionStrategy PositionSelectionStrategy { get; set; }

        #endregion

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.comment_question, 16, SKColors.MediumVioletRed);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.comment_question, 32, SKColors.MediumVioletRed);

        public override void RuntimeStarted(BaseRuntime runtime)
        {
            QATokenizer.DisposeTokenizer();
            base.RuntimeStarted(runtime);
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
                    CreateNotification(runtime, OnnxNotifications.ModelFileMissing);
                    return;
                }

                if (QATokenizer == null)
                {
                    CreateNotification(runtime, OnnxNotifications.TokenizerMissing);
                    return;
                }

                try
                {
                    QATokenizer.Initialize();
                }
                catch (Exception ex)
                {
                    CreateNotification(runtime, OnnxNotifications.AITokenizerInitializationFailed, ex);
                    return;
                }

                string context = FlowBloxFieldHelper.ReplaceFieldsInString(Context);
                string question = FlowBloxFieldHelper.ReplaceFieldsInString(Question);

                try
                {
                    var result = PredictAnswer(runtime, question, context);
                    runtime.Report("ONNX QA result: " + result);
                    GenerateResult(runtime, result);
                }
                catch (Exception ex)
                {
                    CreateNotification(runtime, OnnxNotifications.PromptExecutionFailed, ex);
                    return;
                }
            });
        }

        private string PredictAnswer(BaseRuntime runtime, string question, string context)
        {
            var positionSelector = PositionSelectorFactory.CreatePositionSelector(PositionSelectionStrategy);

            var tokenizedInput = QATokenizer.Encode(question, context);

            var length = tokenizedInput.InputIds.Length;

            var inputTensor = new DenseTensor<long>(new[] { 1, length });
            var attentionTensor = new DenseTensor<long>(new[] { 1, length });
            var tokenTypeTensor = new DenseTensor<long>(new[] { 1, length });

            for (int i = 0; i < length; i++)
            {
                inputTensor[0, i] = tokenizedInput.InputIds[i];
                attentionTensor[0, i] = tokenizedInput.AttentionMask[i];
                tokenTypeTensor[0, i] = tokenizedInput.TokenTypeIds[i];
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionTensor)
            };

            var modelInputs = _session.InputMetadata.Keys;
            bool modelSupportsTokenTypeIds = modelInputs.Contains("token_type_ids");
            if (modelSupportsTokenTypeIds)
            {
                inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeTensor));
            }

            using var results = _session.Run(inputs);
            var startLogits = results.First(r => r.Name == "start_logits").AsTensor<float>();
            var endLogits = results.First(r => r.Name == "end_logits").AsTensor<float>();

            int start = positionSelector.SelectPosition(startLogits);
            int end = positionSelector.SelectPosition(endLogits);

            if (start > end || start < 0 || end >= tokenizedInput.InputIds.Length)
                return string.Empty;

            var selectedTokenIds = tokenizedInput.InputIds[start..(end + 1)];
            return QATokenizer.Decode(selectedTokenIds);
        }
    }
}
