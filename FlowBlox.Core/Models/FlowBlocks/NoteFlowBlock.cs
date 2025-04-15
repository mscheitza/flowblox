using FlowBlox.Core.Attributes;
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
    [Display(Name = "NoteFlowBlock_DisplayName", Description = "NoteFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class NoteFlowBlock : BaseFlowBlock
    {
        [Display(Name = "NoteFlowBlock_Note", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockTextBox(MultiLine = true)]
        public string Note { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.note_text, 16, SKColors.Gold);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.note_text, 32, SKColors.Gold);

        public NoteFlowBlock()
            : base()
        {
            
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.None;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Additional;

        public override bool Execute(Runtime.BaseRuntime runtime, object data)
        {
            throw new NotSupportedException("An element of type \"NoteElement\" cannot be executed.");
        }
    }
}
