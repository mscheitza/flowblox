using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.TextOperations
{
    /// <summary>
    /// A single URI part.
    /// </summary>
    [Display(Name = "ConcatUriFlowBlock_UriPart_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public sealed class UriPartDefinition : FlowBloxReactiveObject
    {
        [Display(Name = "ConcatUriFlowBlock_UriPart_Value", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox(IsCodingMode = true)]
        public string Value { get; set; }
    }
}
