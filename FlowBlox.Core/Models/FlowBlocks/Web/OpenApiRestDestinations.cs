using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Web
{
    public enum OpenApiRestDestinations
    {
        [Display(Name = "OpenApiRestDestinations_Payload", ResourceType = typeof(FlowBloxTexts))]
        Payload,
        [Display(Name = "OpenApiRestDestinations_StatusCode", ResourceType = typeof(FlowBloxTexts))]
        StatusCode,
        [Display(Name = "OpenApiRestDestinations_Status", ResourceType = typeof(FlowBloxTexts))]
        Status,
        [Display(Name = "OpenApiRestDestinations_ErrorMessage", ResourceType = typeof(FlowBloxTexts))]
        ErrorMessage,
        [Display(Name = "OpenApiRestDestinations_ResponseHeaders", ResourceType = typeof(FlowBloxTexts))]
        ResponseHeaders,
        [Display(Name = "OpenApiRestDestinations_Url", ResourceType = typeof(FlowBloxTexts))]
        Url
    }
}
