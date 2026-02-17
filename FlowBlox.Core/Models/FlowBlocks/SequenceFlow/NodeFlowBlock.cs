using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.SequenceFlow
{
    [Display(Name = "NodeFlowBlock_DisplayName", Description = "NodeFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class NodeFlowBlock : BaseFlowBlock
    {
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.vector_polyline, 16, SKColors.SteelBlue);

        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.vector_polyline, 32, SKColors.SteelBlue);

        public NodeFlowBlock() : base()
        {
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.ControlFlow;

        public override bool CanGoBack => false;

        public override string NamePrefix => "Node";

        public override bool CreateNumericNameSuffix => false;

        public override bool Execute(Runtime.BaseRuntime runtime, object Data)
        {
            return Invoke(runtime, Data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(Data);
                ExecuteNextFlowBlocks(runtime);
            });
        }
    }
}