using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum ExpectationConditionTarget
    {
        [Display(Name = "ExpectationConditionTarget_FirstValue", ResourceType = typeof(FlowBloxTexts))]
        FirstValue,
        [Display(Name = "ExpectationConditionTarget_ValueAtIndex", ResourceType = typeof(FlowBloxTexts))]
        ValueAtIndex,
        [Display(Name = "ExpectationConditionTarget_AnyValue", ResourceType = typeof(FlowBloxTexts))]
        AnyValue,
        [Display(Name = "ExpectationConditionTarget_LastValue", ResourceType = typeof(FlowBloxTexts))]
        LastValue,
        [Display(Name = "ExpectationConditionTarget_NumberOfDatasets", ResourceType = typeof(FlowBloxTexts))]
        NumberOfDatasets
    }
}
