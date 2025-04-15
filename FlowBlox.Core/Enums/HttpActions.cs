using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Enums
{
    public enum HttpActions
    {
        [Display(Name = "GET")]
        GET,
        [Display(Name = "POST")]
        POST,
        [Display(Name = "PUT")]
        PUT,
        [Display(Name = "DELETE")]
        DELETE,
    }
}
