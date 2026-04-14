using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public enum WebSpecialKey
    {
        [Display(Name = "WebSpecialKey_Enter", ResourceType = typeof(FlowBloxTexts))]
        Enter,

        [Display(Name = "WebSpecialKey_Tab", ResourceType = typeof(FlowBloxTexts))]
        Tab,

        [Display(Name = "WebSpecialKey_Escape", ResourceType = typeof(FlowBloxTexts))]
        Escape,

        [Display(Name = "WebSpecialKey_Backspace", ResourceType = typeof(FlowBloxTexts))]
        Backspace,

        [Display(Name = "WebSpecialKey_Delete", ResourceType = typeof(FlowBloxTexts))]
        Delete,

        [Display(Name = "WebSpecialKey_ArrowUp", ResourceType = typeof(FlowBloxTexts))]
        ArrowUp,

        [Display(Name = "WebSpecialKey_ArrowDown", ResourceType = typeof(FlowBloxTexts))]
        ArrowDown,

        [Display(Name = "WebSpecialKey_ArrowLeft", ResourceType = typeof(FlowBloxTexts))]
        ArrowLeft,

        [Display(Name = "WebSpecialKey_ArrowRight", ResourceType = typeof(FlowBloxTexts))]
        ArrowRight,

        [Display(Name = "WebSpecialKey_Space", ResourceType = typeof(FlowBloxTexts))]
        Space
    }
}

