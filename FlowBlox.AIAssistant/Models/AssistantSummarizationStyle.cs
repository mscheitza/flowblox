using System.ComponentModel.DataAnnotations;
using FlowBlox.Core;

namespace FlowBlox.AIAssistant.Models
{
    public enum AssistantSummarizationStyle
    {
        [Display(Name = "AssistantSummarizationStyle_DetailedProjectMemory", ResourceType = typeof(FlowBloxTexts))]
        DetailedProjectMemory = 0,

        [Display(Name = "AssistantSummarizationStyle_StructuredHistoricalMemory", ResourceType = typeof(FlowBloxTexts))]
        StructuredHistoricalMemory = 1
    }
}
