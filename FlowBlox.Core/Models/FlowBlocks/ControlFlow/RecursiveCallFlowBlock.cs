using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.ControlFlow.Recursion
{
    [FlowBloxSpecialExplanation("RecursiveCallFlowBlock_SpecialExplanation_TargetInvocation", Icon = SpecialExplanationIcon.Information)]
    [Display(Name = "RecursiveCallFlowBlock_DisplayName", Description = "RecursiveCallFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class RecursiveCallFlowBlock : RecursiveTargetFlowBlockBase
    {
        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.call_merge, 16, SKColors.DarkOrange);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.call_merge, 32, SKColors.DarkOrange);

        public override bool Execute(Runtime.BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {

                if (TargetFlowBlock == null)
                    throw new InvalidOperationException("A target flow block must be defined.");

                TargetFlowBlock.Repeat(runtime);
            });
        }

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(TargetFlowBlock));
            return properties;
        }
    }
}
