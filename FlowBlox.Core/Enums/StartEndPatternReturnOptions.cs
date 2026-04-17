using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum StartEndPatternReturnOptions
    {
        [Display(Name = "StartEndPatternReturnOptions_StartPattern", ResourceType = typeof(FlowBloxTexts))]
        StartPattern = 0,

        [Display(Name = "StartEndPatternReturnOptions_EndPattern", ResourceType = typeof(FlowBloxTexts))]
        EndPattern = 1,

        [Display(Name = "StartEndPatternReturnOptions_StartAndEndPattern", ResourceType = typeof(FlowBloxTexts))]
        StartAndEndPattern = 2
    }
}
