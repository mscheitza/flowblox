using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Enums
{
    public enum FlowBloxTestConfigurationSelectionMode
    {
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_UserInput", ResourceType = typeof(FlowBloxTexts))]
        UserInput,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_UserInput_ExistingValue", ResourceType = typeof(FlowBloxTexts))]
        UserInput_ExistingValue,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_First", ResourceType = typeof(FlowBloxTexts))]
        First,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_Index", ResourceType = typeof(FlowBloxTexts))]
        Index,
        [Display(Name = "FlowBloxTestConfigurationSelectionMode_Last", ResourceType = typeof(FlowBloxTexts))]
        Last
    }
}
