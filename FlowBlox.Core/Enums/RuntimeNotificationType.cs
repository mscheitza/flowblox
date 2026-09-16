using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum RuntimeNotificationType
    {
        [Display(Name = "RuntimeNotificationType_RuntimeError", ResourceType = typeof(FlowBloxTexts))]
        RuntimeError = 0,

        [Display(Name = "RuntimeNotificationType_RuntimeAborted", ResourceType = typeof(FlowBloxTexts))]
        RuntimeAborted = 1,

        [Display(Name = "RuntimeNotificationType_RuntimeCompletedSuccessfully", ResourceType = typeof(FlowBloxTexts))]
        RuntimeCompletedSuccessfully = 2,

        [Display(Name = "RuntimeNotificationType_RuntimeStartedSuccessfully", ResourceType = typeof(FlowBloxTexts))]
        RuntimeStartedSuccessfully = 3,

        [Display(Name = "RuntimeNotificationType_RuntimeStartFailed", ResourceType = typeof(FlowBloxTexts))]
        RuntimeStartFailed = 4
    }
}
