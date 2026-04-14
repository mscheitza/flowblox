using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Microsoft.ML.OnnxRuntimeGenAI;
using Newtonsoft.Json;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    [Display(Name = "OnnxRuntimeGenAIFlowBlock_DisplayName", Description = "OnnxRuntimeGenAIFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    [FlowBloxSpecialExplanation("OnnxGenAIFlowBlock_SpecialExplanation_ManagedResource", Icon = SpecialExplanationIcon.Information)]
    public class OnnxGenAIFlowBlock : BaseSingleResultFlowBlock
    {
        private Model _model;
        private Microsoft.ML.OnnxRuntimeGenAI.Tokenizer _tokenizer;
        #region Tab: Default

        [Required]
        [Display(Name = "OnnxRuntimeGenAIFlowBlock_ModelFolder", Description = "OnnxRuntimeGenAIFlowBlock_ModelFolder_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFolderSelection | UIOptions.EnableFieldSelection)]
        public string ModelFolder { get; set; }

        [Display(Name = "OnnxRuntimeGenAIFlowBlock_Prompt", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBloxTextBox(IsCodingMode = true, MultiLine = true)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [Required]
        public string Prompt { get; set; }

        #endregion

        #region Tab: Extended settings

        [JsonIgnore()]
        [DeepCopierIgnore()]
        [Display(Name = "OnnxRuntimeGenAIFlowBlock_AiExecutionProvider", Description = "OnnxRuntimeGenAIFlowBlock_AiExecutionProvider_Tooltip",
            GroupName = "OnnxRuntimeGenAIFlowBlock_Groups_ExtendedSettings", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.ComboBox, ReadOnly = true)]
        [Required]
        public AiExecutionProviders AiExecutionProvider
        {
            get
            {
                var providerOption = FlowBloxOptions.GetOptionInstance().GetOption("AI.Onnx.Provider");
                if (providerOption == null)
                    return AiExecutionProviders.Default;

                var providerName = providerOption.Value?.Trim();
                if (string.IsNullOrWhiteSpace(providerName))
                    return AiExecutionProviders.Default;

                if (Enum.TryParse<AiExecutionProviders>(providerName, ignoreCase: true, out var parsed))
                    return parsed;

                return AiExecutionProviders.Default;
            }
        }

        [Display(Name = "OnnxRuntimeGenAIFlowBlock_TokenSelectionStrategy", Description = "OnnxRuntimeGenAIFlowBlock_TokenSelectionStrategy_Tooltip",
            GroupName = "OnnxRuntimeGenAIFlowBlock_Groups_ExtendedSettings", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.ComboBox)]
        public TokenSelectionStrategy TokenSelectionStrategy { get; set; }

        [Display(Name = "OnnxRuntimeGenAIFlowBlock_MaxNewTokens", Description = "OnnxRuntimeGenAIFlowBlock_MaxNewTokens_Tooltip",
            GroupName = "OnnxRuntimeGenAIFlowBlock_Groups_ExtendedSettings",
            ResourceType = typeof(FlowBloxTexts), Order = 2)]
        public int MaxNewTokens { get; set; }

        [Display(Name = "OnnxRuntimeGenAIFlowBlock_Temperature", Description = "OnnxRuntimeGenAIFlowBlock_Temperature_Tooltip",
            GroupName = "OnnxRuntimeGenAIFlowBlock_Groups_ExtendedSettings", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        public float Temperature { get; set; }

        [Display(Name = "OnnxRuntimeGenAIFlowBlock_TopP", Description = "OnnxRuntimeGenAIFlowBlock_TopP_Tooltip",
            GroupName = "OnnxRuntimeGenAIFlowBlock_Groups_ExtendedSettings", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        public float TopP { get; set; }

        [Display(Name = "OnnxRuntimeGenAIFlowBlock_TopK", Description = "OnnxRuntimeGenAIFlowBlock_TopK_Tooltip",
            GroupName = "OnnxRuntimeGenAIFlowBlock_Groups_ExtendedSettings", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        public int TopK { get; set; }

        [Display(Name = "OnnxRuntimeGenAIFlowBlock_UseChatTemplate", Description = "OnnxRuntimeGenAIFlowBlock_UseChatTemplate_Tooltip",
            GroupName = "OnnxRuntimeGenAIFlowBlock_Groups_ExtendedSettings", ResourceType = typeof(FlowBloxTexts), Order = 6)]
        public bool UseChatTemplate { get; set; }

        [Display(Name = "OnnxRuntimeGenAIFlowBlock_SystemPrompt", Description = "OnnxRuntimeGenAIFlowBlock_SystemPrompt_Tooltip",
            GroupName = "OnnxRuntimeGenAIFlowBlock_Groups_ExtendedSettings", ResourceType = typeof(FlowBloxTexts), Order = 7)]
        [FlowBloxTextBox(IsCodingMode = true, MultiLine = true)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        public string SystemPrompt { get; set; }

        [ActivationCondition(MemberName = nameof(UseChatTemplate), Value = true)]
        [ConditionallyRequired()]
        [Display(Name = "OnnxRuntimeGenAIFlowBlock_ChatTemplate", Description = "OnnxRuntimeGenAIFlowBlock_ChatTemplate_Tooltip",
            GroupName = "OnnxRuntimeGenAIFlowBlock_Groups_ExtendedSettings", ResourceType = typeof(FlowBloxTexts), Order = 8)]
        [FlowBloxTextBox(IsCodingMode = true, MultiLine = true)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection, ToolboxCategory = nameof(FlowBloxToolboxCategory.ChatTemplates))]
        public string ChatTemplate { get; set; }

        #endregion

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.text_long, 16, SKColors.CadetBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.text_long, 32, SKColors.CadetBlue);
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;
        public override FlowBlockCategory GetCategory() => FlowBlockCategory.AI;

        public OnnxGenAIFlowBlock()
        {
            MaxNewTokens = 200;

            Temperature = 0.0f;
            TopP = 1.0f;
            TopK = 1;

            UseChatTemplate = true;
            SystemPrompt = "You are a helpful assistant.";
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                if (string.IsNullOrWhiteSpace(ModelFolder) || !Directory.Exists(ModelFolder))
                {
                    CreateNotification(runtime, OnnxRuntimeGenAiNotifications.ModelFolderMissing);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Prompt))
                {
                    CreateNotification(runtime, OnnxRuntimeGenAiNotifications.PromptMissing);
                    return;
                }

                string userPrompt = FlowBloxFieldHelper.ReplaceFieldsInString(Prompt);
                string systemPrompt = FlowBloxFieldHelper.ReplaceFieldsInString(SystemPrompt ?? string.Empty);

                string finalPrompt = UseChatTemplate
                    ? ApplyChatTemplate(systemPrompt, userPrompt)
                    : userPrompt;

                string result;
                try
                {
                    result = RunGenAI(runtime, ModelFolder, finalPrompt, MaxNewTokens);
                }
                catch (Exception ex)
                {
                    CreateNotification(runtime, OnnxRuntimeGenAiNotifications.ExecutionFailed, ex);
                    return;
                }

                runtime.Report($"ONNX Runtime GenAI result: {result}");
                GenerateResult(runtime, result);
            });
        }

        private string ApplyChatTemplate(string systemPrompt, string userPrompt)
        {
            if (string.IsNullOrEmpty(this.ChatTemplate))
                throw new ArgumentNullException(nameof(this.ChatTemplate));

            return ChatTemplate
                .Replace("{SystemPrompt}", systemPrompt ?? string.Empty)
                .Replace("{UserPrompt}", userPrompt ?? string.Empty);
        }

        public override void RuntimeStarted(BaseRuntime runtime)
        {
            base.RuntimeStarted(runtime);

            // Ensure ONNX runtime native binaries are loaded:
            FlowBloxOnnxRuntimeLoader.Instance.EnsureLoaded(AiExecutionProvider, runtime);

            // Ensure GenAI native binaries are loaded:
            FlowBloxOnnxRuntimeGenAiLoader.Instance.EnsureLoaded(AiExecutionProvider, runtime);

            if (string.IsNullOrWhiteSpace(ModelFolder))
                throw new InvalidOperationException("ONNX Runtime GenAI initialization failed: ModelFolder is null or empty.");

            if (!Directory.Exists(ModelFolder))
                throw new InvalidOperationException($"ONNX Runtime GenAI initialization failed: ModelFolder does not exist: '{ModelFolder}'");

            runtime.Report($"Loading ONNX model from folder: {ModelFolder}");

            try
            {
                _model = new Model(ModelFolder);
                _tokenizer = new Microsoft.ML.OnnxRuntimeGenAI.Tokenizer(_model);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ONNX Runtime GenAI initialization failed while loading model from '{ModelFolder}'.", ex);
            }
        }

        public override void RuntimeFinished(BaseRuntime runtime)
        {
            _tokenizer?.Dispose();
            _model?.Dispose();

            base.RuntimeFinished(runtime);
        }

        private string RunGenAI(BaseRuntime runtime, string modelFolder, string prompt, int maxNewTokens)
        {
            
            using var stream = _tokenizer.CreateStream();

            // Encode -> Sequences
            using var inputSeq = _tokenizer.Encode(prompt);
            if (inputSeq == null || inputSeq.NumSequences <= 0)
                return string.Empty;

            // Try to get prompt length
            int promptLen = 0;
            try
            {
                var promptTokens = inputSeq[0];
                promptLen = promptTokens.Length;
            }
            catch
            {
                // Some bindings expose sequence differently
                // Keep promptLen=0 and use fallback below
                promptLen = 0;
            }

            using var genParams = new GeneratorParams(_model);

            // max_length = prompt + newTokens
            // If promptLen is unknown, use a safe cap
            int maxLength = (promptLen > 0)
                ? (promptLen + Math.Max(0, maxNewTokens))
                : Math.Max(256, maxNewTokens + 2048);

            genParams.SetSearchOption("max_length", (double)maxLength);

            ApplySearchOptions(genParams);

            using var generator = new Generator(_model, genParams);

            // attach prompt sequences to generator
            generator.AppendTokenSequences(inputSeq);

            var sb = new StringBuilder(capacity: Math.Max(256, maxNewTokens * 4));

            // We decode only newly generated tokens (skip prompt tokens)
            int lastEmittedLen = 0;
            while (!generator.IsDone())
            {
                generator.GenerateNextToken();

                var seq = generator.GetSequence(0);
                if (seq == null || seq.Length == 0)
                    continue;

                // First time: if promptLen is 0 (unknown), assume entire first seq is prompt+first gen token.
                // We still avoid duplicating by tracking lastEmittedLen.
                int start = Math.Max(promptLen, lastEmittedLen);

                if (seq.Length <= start)
                {
                    lastEmittedLen = seq.Length;
                    continue;
                }

                for (int i = start; i < seq.Length; i++)
                {
                    int token = seq[i];
                    sb.Append(stream.Decode(token));
                }

                lastEmittedLen = seq.Length;

                // hard stop: if user sets MaxNewTokens and generator doesn't early-stop
                if (maxNewTokens > 0 && promptLen > 0)
                {
                    int generatedSoFar = Math.Max(0, seq.Length - promptLen);
                    if (generatedSoFar >= maxNewTokens)
                        break;
                }
            }

            return sb.ToString();
        }

        private void ApplySearchOptions(GeneratorParams genParams)
        {
            // - If Temperature <= 0 or strategy is ArgMax => deterministic
            // - Else sampling with top_k/top_p/temperature
            bool deterministic =
                TokenSelectionStrategy == TokenSelectionStrategy.ArgMax ||
                Temperature <= 0.0f;

            if (deterministic)
            {
                genParams.SetSearchOption("do_sample", false);
                genParams.SetSearchOption("top_k", 1.0);
                genParams.SetSearchOption("top_p", 1.0);
                genParams.SetSearchOption("temperature", 0.0);
                return;
            }

            genParams.SetSearchOption("do_sample", true);
            genParams.SetSearchOption("top_k", (double)Math.Max(1, TopK));
            genParams.SetSearchOption("top_p", (double)Math.Clamp(TopP, 0.0f, 1.0f));
            genParams.SetSearchOption("temperature", (double)Math.Max(0.0f, Temperature));
        }

        public override List<string> GetDisplayableProperties()
        {
            var props = base.GetDisplayableProperties();
            props.Add(nameof(ModelFolder));
            return props;
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(OnnxRuntimeGenAiNotifications));
                return notificationTypes;
            }
        }
    }

    public enum OnnxRuntimeGenAiNotifications
    {
        [FlowBloxNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Model folder path is empty or does not exist.")]
        ModelFolderMissing,

        [FlowBloxNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Prompt is empty.")]
        PromptMissing,

        [FlowBloxNotification(NotificationType = NotificationType.Error)]
        [Display(Name = "Failed to execute prompt using ONNX Runtime GenAI.")]
        ExecutionFailed
    }
}
