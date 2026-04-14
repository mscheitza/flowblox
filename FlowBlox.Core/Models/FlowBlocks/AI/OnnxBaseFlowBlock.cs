using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.AI.Enums;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.DeepCopier;
using Microsoft.ML.OnnxRuntime;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    [FlowBloxUIGroup("OnnxBaseFlowBlock_Groups_ExtendedSettings", 10)]
    [FlowBloxSpecialExplanation("OnnxBaseFlowBlock_SpecialExplanation_ManagedResource", Icon = SpecialExplanationIcon.Information)]
    public abstract class OnnxBaseFlowBlock : BaseSingleResultFlowBlock
    {
        #region Tab: Default

        [Display(Name = "OnnxTextGenerationFlowBlock_ModelPath", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFileSelection)]
        [Required]
        public string ModelPath { get; set; }

        [Display(Name = "OnnxBaseFlowBlock_AssociatedOnnxFlowBlock", Description = "OnnxBaseFlowBlock_AssociatedOnnxFlowBlock_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
            SelectionFilterMethod = nameof(GetPossibleOnnxFlowBlocks),
            SelectionDisplayMember = nameof(Name))]
        public OnnxBaseFlowBlock AssociatedOnnxFlowBlock { get; set; }

        public List<OnnxBaseFlowBlock> GetPossibleOnnxFlowBlocks()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFlowBlocks<OnnxBaseFlowBlock>()
                .Except([this])
                .ToList();
        }

        #endregion

        #region Tab: Extended settings

        [JsonIgnore()]
        [DeepCopierIgnore()]
        [Display(Name = "OnnxBaseFlowBlock_AiExecutionProvider",
            Description = "OnnxBaseFlowBlock_AiExecutionProvider_Tooltip",
            GroupName = "OnnxBaseFlowBlock_Groups_ExtendedSettings",
            ResourceType = typeof(FlowBloxTexts), Order = 0)]
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

        #endregion

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.AI;

        protected InferenceSession _session;

        public override void RuntimeStarted(BaseRuntime runtime)
        {
            CreateInferenceSession(runtime);

            base.RuntimeStarted(runtime);
        }

        private void CreateInferenceSession(BaseRuntime runtime)
        {
            if (_session != null)
                _session.Dispose();

            var associatedOnnxFlowBlock = this.AssociatedOnnxFlowBlock;
            if (associatedOnnxFlowBlock?._session != null)
            {
                _session = associatedOnnxFlowBlock._session;
                return;
            }

            runtime.Report($"Loading ONNX model from file: {ModelPath}");

            AiExecutionProviders aiExecutionProvider = this.AiExecutionProvider;

            SessionOptions sessionOptions;

            try
            {
                // Ensure ONNX runtime native binaries are loaded:
                FlowBloxOnnxRuntimeLoader.Instance.EnsureLoaded(aiExecutionProvider, runtime);

                sessionOptions = new SessionOptions();

                switch (aiExecutionProvider)
                {
                    case AiExecutionProviders.OpenVINO:
                        sessionOptions.AppendExecutionProvider_OpenVINO("GPU_FP32");
                        runtime.Report("Using OpenVINO Execution Provider (GPU_FP32).");
                        break;

                    case AiExecutionProviders.DirectML:
                        sessionOptions.AppendExecutionProvider_DML();
                        runtime.Report("Using DirectML Execution Provider.");
                        break;

                    case AiExecutionProviders.CUDA:
                        sessionOptions.AppendExecutionProvider_CUDA();
                        runtime.Report("Using CUDA Execution Provider.");
                        break;

                    case AiExecutionProviders.Default:
                    default:
                        runtime.Report("Using default CPU Execution Provider.");
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize ONNX Execution Provider '{aiExecutionProvider}'.", ex);
            }

            _session = new InferenceSession(ModelPath, sessionOptions);
        }

        public override void RuntimeFinished(BaseRuntime runtime)
        {
            if (_session != null)
                _session.Dispose();

            base.RuntimeFinished(runtime);
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(OnnxNotifications));
                return notificationTypes;
            }
        }
    }
}

