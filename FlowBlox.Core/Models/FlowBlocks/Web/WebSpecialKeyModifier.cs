using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public enum WebSpecialKeyModifier
    {
        [Display(Name = "WebSpecialKeyModifier_None", ResourceType = typeof(FlowBloxTexts))]
        None,

        [Display(Name = "WebSpecialKeyModifier_Ctrl", ResourceType = typeof(FlowBloxTexts))]
        Ctrl,

        [Display(Name = "WebSpecialKeyModifier_Shift", ResourceType = typeof(FlowBloxTexts))]
        Shift,

        [Display(Name = "WebSpecialKeyModifier_Alt", ResourceType = typeof(FlowBloxTexts))]
        Alt,

        [Display(Name = "WebSpecialKeyModifier_CtrlShift", ResourceType = typeof(FlowBloxTexts))]
        CtrlShift,

        [Display(Name = "WebSpecialKeyModifier_CtrlAlt", ResourceType = typeof(FlowBloxTexts))]
        CtrlAlt,

        [Display(Name = "WebSpecialKeyModifier_ShiftAlt", ResourceType = typeof(FlowBloxTexts))]
        ShiftAlt,

        [Display(Name = "WebSpecialKeyModifier_CtrlShiftAlt", ResourceType = typeof(FlowBloxTexts))]
        CtrlShiftAlt
    }
}

