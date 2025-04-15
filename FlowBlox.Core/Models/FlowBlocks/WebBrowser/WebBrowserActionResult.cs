using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public enum WebBrowserActionStatus
    {
        [FlowBlockNotification(NotificationType = NotificationType.Warning)]
        [Display(Name = "Element not found")]
        ElementNotFound,
        [FlowBlockNotification(NotificationType = NotificationType.Warning)]
        [Display(Name = "Element is not applicable")]
        ElementNotApplicable,
        [FlowBlockNotification(NotificationType = NotificationType.Warning)]
        [Display(Name = "URL could not be accessed")]
        UrlNotAccessible
    }

    public class WebBrowserActionResult
    {
        public bool Success { get; set; }
        public Exception Exception { get; internal set; }
        public WebBrowserActionStatus? Status { get; internal set;}
    }

    public class WebBrowserContentActionResult : WebBrowserActionResult
    {
        public string Content;
    }
}
