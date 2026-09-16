using System.ComponentModel.DataAnnotations;
using FlowBlox.Core;

namespace FlowBlox.AIAssistant.Models
{
    public enum AssistantOptionsAccessLevel
    {
        [Display(Name = "AssistantOptionsAccessLevel_None", ResourceType = typeof(FlowBloxTexts))]
        None,

        [Display(Name = "AssistantOptionsAccessLevel_ReadOnly", ResourceType = typeof(FlowBloxTexts))]
        ReadOnly,

        [Display(Name = "AssistantOptionsAccessLevel_ReadWrite", ResourceType = typeof(FlowBloxTexts))]
        ReadWrite
    }
}
