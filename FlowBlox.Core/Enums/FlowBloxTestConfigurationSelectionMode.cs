using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum FlowBloxTestConfigurationSelectionMode
    {
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_Keep", ResourceType = typeof(FlowBloxTexts))]
        Keep,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_UserInput", ResourceType = typeof(FlowBloxTexts))]
        UserInput,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_UserInput_ExpectedValue", ResourceType = typeof(FlowBloxTexts))]
        UserInput_ExpectedValue,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_First", ResourceType = typeof(FlowBloxTexts))]
        First,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_Index", ResourceType = typeof(FlowBloxTexts))]
        Index,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_Last", ResourceType = typeof(FlowBloxTexts))]
        Last
    }
}
