using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Web.InternalWebRequest
{
    public enum WebRequestDestinations
    {
        [Display(Name = "Content")]
        Content = 0,

        [Display(Name = "File Name")]
        FileName = 1,

        [Display(Name = "Url")]
        Url = 2
    }
}
