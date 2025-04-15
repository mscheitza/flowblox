using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.FlowBlocks.Xml;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using Microsoft.ML.OnnxRuntime;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FlowBlox.Core.Models.FlowBlocks.AI
{
    [FlowBlockUIGroup("OnnxBaseFlowBlock_Groups_ExtendedSettings", 10)]
    public abstract class OnnxBaseFlowBlock : BaseSingleResultFlowBlock
    {
        #region Tab: Default

        [Display(Name = "OnnxTextGenerationFlowBlock_ModelPath", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFileSelection)]
        [Required]
        public string ModelPath { get; set; }

        [Display(Name = "OnnxBaseFlowBlock_AssociatedOnnxFlowBlock", Description = "OnnxBaseFlowBlock_AssociatedOnnxFlowBlock_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
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

        [Display(Name = "OnnxBaseFlowBlock_AiExecutionProvider",
            GroupName = "OnnxBaseFlowBlock_Groups_ExtendedSettings",
            ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        [Required]
        public AiExecutionProviders AiExecutionProvider { get; set; }

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

            var sessionOptions = new SessionOptions();

            AiExecutionProviders aiExecutionProvider = this.AiExecutionProvider;

            try
            {
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
            _session.Dispose();

            base.RuntimeFinished(runtime);
        }
    }
}
