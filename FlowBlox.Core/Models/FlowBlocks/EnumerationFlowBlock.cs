using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util;
using System.Drawing;
using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.Resources;
using System.Collections.ObjectModel;
using FlowBlox.Core.Util.FlowBlocks;
using SkiaSharp;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [Display(Name = "EnumerationFlowBlock_DisplayName", Description = "EnumerationFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class EnumerationFlowBlock : BaseSingleResultFlowBlock
    {
        [Display(Name = "EnumerationFlowBlock_Parameters", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ListView, Operations = UIOperations.Link | UIOperations.Unlink,
            UiOptions = UIOptions.FieldSelectionDefaultNotRequired,
            SelectionFilterMethod = nameof(GetPossibleFieldElements))]
        [FlowBlockListView(LVColumnMemberNames = new[] { nameof(FieldElement.FlowBlockName), nameof(FieldElement.Name) })]
        public ObservableCollection<FieldElement> Parameters { get; set; } = new();

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.format_list_bulleted, 16, SKColors.Teal);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.format_list_bulleted, 32, SKColors.Teal);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.TextOperations;

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                var values = Parameters.Select(p => p.StringValue)
                    .ExceptNullOrEmpty()
                    .ToArray();

                GenerateResult(runtime, values);
            });
        }
    }
}
