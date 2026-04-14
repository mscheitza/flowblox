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
        UrlNotAccessible,

        [FlowBlockNotification(NotificationType = NotificationType.Warning)]
        [Display(Name = "Element is stale (DOM changed)")]
        ElementStale,

        [FlowBlockNotification(NotificationType = NotificationType.Warning)]
        [Display(Name = "Unknown browser error")]
        UnknownError,

        [FlowBlockNotification(NotificationType = NotificationType.Warning)]
        [Display(Name = "Download URL could not be determined")]
        DownloadUrlMissing,

        [FlowBlockNotification(NotificationType = NotificationType.Warning)]
        [Display(Name = "File download failed")]
        FileDownloadFailed
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

    public class WebBrowserDownloadActionResult : WebBrowserActionResult
    {
        public string DownloadPath { get; set; }
    }
}
