using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public enum InputBehavior
    {
        [Display(Name = "InputBehavior_Cross", ResourceType = typeof(FlowBloxTexts))]
        Cross,
        [Display(Name = "InputBehavior_CrossValid", ResourceType = typeof(FlowBloxTexts))]
        CrossValid,
        [Display(Name = "InputBehavior_First", ResourceType = typeof(FlowBloxTexts))]
        First,
        [Display(Name = "InputBehavior_FirstValid", ResourceType = typeof(FlowBloxTexts))]
        FirstValid,
        [Display(Name = "InputBehavior_Last", ResourceType = typeof(FlowBloxTexts))]
        Last,
        [Display(Name = "InputBehavior_LastValid", ResourceType = typeof(FlowBloxTexts))]
        LastValid
    }
}
