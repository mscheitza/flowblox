using FlowBlox.Core;
using FlowBlox.Core.Models.FlowBlocks.CollectionOperations;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Attributes;
using FlowBlox.UICore.Interfaces;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.UICore.Operations
{
    public class CollectionQueryFlowBlockUIActions : ComponentUIActions<CollectionQueryFlowBlock>
    {
        public CollectionQueryFlowBlockUIActions(CollectionQueryFlowBlock component) : base(component) { }
        public SKImage SynchronizeOutputFieldsIcon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.format_list_bulleted, 16, SKColors.Teal);

        [UIActionMetadata(OnlyShowInPropertyWindow = true)]
        [Display(Name = "CollectionQueryFlowBlockUIActions_SynchronizeOutputFields", Description = "CollectionQueryFlowBlockUIActions_SynchronizeOutputFields_Tooltip", ResourceType = typeof(FlowBloxTexts))]
        public void SynchronizeOutputFields() => Component.SynchronizeOutputFields();
    }
}
