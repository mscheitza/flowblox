using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum SpecialSeparator
    {
        [Display(Name = "SpecialSeparator_Tab", ResourceType = typeof(FlowBloxTexts))]
        Tab,

        [Display(Name = "SpecialSeparator_NewLine", ResourceType = typeof(FlowBloxTexts))]
        NewLine
    }
}
