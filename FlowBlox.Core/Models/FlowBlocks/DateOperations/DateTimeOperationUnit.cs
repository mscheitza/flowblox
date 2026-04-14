using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.DateOperations
{
    public enum DateTimeOperationUnit
    {
        [Display(Name = "DateTimeOperationUnit_Years", ResourceType = typeof(FlowBloxTexts))]
        Years,

        [Display(Name = "DateTimeOperationUnit_Months", ResourceType = typeof(FlowBloxTexts))]
        Months,

        [Display(Name = "DateTimeOperationUnit_Days", ResourceType = typeof(FlowBloxTexts))]
        Days,

        [Display(Name = "DateTimeOperationUnit_Hours", ResourceType = typeof(FlowBloxTexts))]
        Hours,

        [Display(Name = "DateTimeOperationUnit_Minutes", ResourceType = typeof(FlowBloxTexts))]
        Minutes,

        [Display(Name = "DateTimeOperationUnit_Seconds", ResourceType = typeof(FlowBloxTexts))]
        Seconds
    }
}
