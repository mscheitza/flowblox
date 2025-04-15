using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Resources;

namespace FlowBlox.Core.Models.Components.Modifier
{
    [Display(Name = "SubstringModifier", ResourceType = typeof(FlowBloxTexts))]
    public class SubstringModifier : ModifierBase
    {
        [Required()]
        [Display(Name = "SubstringModifier_Index", Order = 0, ResourceType = typeof(FlowBloxTexts))]
        public int Index { get; set; }

        [Required()]
        [Display(Name = "SubstringModifier_Length", Order = 1, ResourceType = typeof(FlowBloxTexts))]
        public int Length { get; set; }

        public override string Modify(BaseRuntime runtime, string value)
        {
            return value.Substring(Index, Length);
        }

        public override string ToString()
        {
            var format = FlowBloxResourceUtil.GetLocalizedString(nameof(SubstringModifier), nameof(ObjectDisplayName));
            return string.Format(format, this.Index, this.Index + this.Length);
        }
    }
}
