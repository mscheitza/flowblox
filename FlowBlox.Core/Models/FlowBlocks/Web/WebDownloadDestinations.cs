using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public enum WebDownloadDestinations
    {
        [Display(Name = "WebDownloadDestinations_DOMContent", ResourceType = typeof(FlowBloxTexts))]
        DOMContent = 0,

        [Display(Name = "WebDownloadDestinations_DownloadPath", ResourceType = typeof(FlowBloxTexts))]
        DownloadPath = 1
    }
}
