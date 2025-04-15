using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.Components.Modifier
{
    public abstract class RegexModifierBase : ModifierBase
    {
        [Required()]
        [Display(Name = "RegexMatchModifier_Search", Order = 0, ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(ToolboxCategory = FlowBloxToolboxCategory.Regex, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(IsCodingMode = true)]
        public string Search { get; set; }
    }
}
