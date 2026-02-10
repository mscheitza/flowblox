using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum SpecialCharacter
    {
        [Display(Name = "SpecialCharacter_LineBreak", ResourceType = typeof(FlowBloxTexts))]
        LineBreak,

        [Display(Name = "SpecialCharacter_Tab", ResourceType = typeof(FlowBloxTexts))]
        Tab,

        [Display(Name = "SpecialCharacter_Space", ResourceType = typeof(FlowBloxTexts))]
        Space,

        [Display(Name = "SpecialCharacter_CarriageReturn", ResourceType = typeof(FlowBloxTexts))]
        CarriageReturn
    }
}
