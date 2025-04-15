using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum UserFieldTypes
    {
        [Display(Name = "UserFieldTypes_None", ResourceType = typeof(FlowBloxTexts))]
        None,

        [Display(Name = "UserFieldTypes_Input", ResourceType = typeof(FlowBloxTexts))]
        Input,

        [Display(Name = "UserFieldTypes_Memory", ResourceType = typeof(FlowBloxTexts))]
        Memory
    }
}