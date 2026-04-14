using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public enum WebDownloadMode
    {
        [Display(Name = "WebDownloadMode_Auto", ResourceType = typeof(FlowBloxTexts))]
        Auto = 0,

        [Display(Name = "WebDownloadMode_BrowserNative", ResourceType = typeof(FlowBloxTexts))]
        BrowserNative = 1,

        [Display(Name = "WebDownloadMode_HttpRequest", ResourceType = typeof(FlowBloxTexts))]
        HttpRequest = 2
    }
}
