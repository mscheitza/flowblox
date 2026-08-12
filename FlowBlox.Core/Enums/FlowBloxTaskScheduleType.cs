using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum FlowBloxTaskScheduleType
    {
        [Display(Name = "FlowBloxTaskScheduleType_Manual", ResourceType = typeof(FlowBloxTexts))]
        Manual = 0,

        [Display(Name = "FlowBloxTaskScheduleType_AtStartup", ResourceType = typeof(FlowBloxTexts))]
        AtStartup = 1,

        [Display(Name = "FlowBloxTaskScheduleType_Daily", ResourceType = typeof(FlowBloxTexts))]
        Daily = 2,

        [Display(Name = "FlowBloxTaskScheduleType_Interval", ResourceType = typeof(FlowBloxTexts))]
        Interval = 3
    }
}
