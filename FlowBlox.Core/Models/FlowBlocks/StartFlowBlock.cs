using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [Display(Name = "StartFlowBlock_DisplayName", Description = "StartFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class StartFlowBlock : BaseFlowBlock
    {
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.ray_start_arrow, 16, SKColors.LimeGreen);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.ray_start_arrow, 32, SKColors.LimeGreen);

        public StartFlowBlock() : base ()
        {

        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.None;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.ControlFlow;

        public override bool CanGoBack => false;

        public override string NamePrefix => "Start";

        public override bool CreateNumericNameSuffix => false;

        public override bool Execute(Runtime.BaseRuntime runtime, object Data)
        {
            return this.Invoke(runtime, Data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(Data);
                ExecuteNextFlowBlocks(runtime);
            });
        }
    }
}
