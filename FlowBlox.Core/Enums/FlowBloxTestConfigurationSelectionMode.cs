using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum FlowBloxTestConfigurationSelectionMode
    {
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_Keep", Description = "FlowBloxTestConfigurationSelectionMode_Keep_Description", ResourceType = typeof(FlowBloxTexts))]
        Keep,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_UserInput", Description = "FlowBloxTestConfigurationSelectionMode_UserInput_Description", ResourceType = typeof(FlowBloxTexts))]
        UserInput,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_UserInput_ExpectedValue", Description = "FlowBloxTestConfigurationSelectionMode_UserInput_ExpectedValue_Description", ResourceType = typeof(FlowBloxTexts))]
        UserInput_ExpectedValue,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_First", Description = "FlowBloxTestConfigurationSelectionMode_First_Description", ResourceType = typeof(FlowBloxTexts))]
        First,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_Index", Description = "FlowBloxTestConfigurationSelectionMode_Index_Description", ResourceType = typeof(FlowBloxTexts))]
        Index,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_Last", Description = "FlowBloxTestConfigurationSelectionMode_Last_Description", ResourceType = typeof(FlowBloxTexts))]
        Last
    }
}
