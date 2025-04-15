using System;
using System.Net;
using System.Xml;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public abstract class WebBrowserBase
    {
        public string Name { get; set; }
        public abstract int Timeout { get; set; }
        public abstract void Open(string url);
        public abstract WebBrowserActionResult ClickOnElement(string selector, WebEventSelectionMode mode);
        public abstract WebBrowserActionResult ScrollMax();
        public abstract int Height { get; }
        public abstract string DOMContent { get; }
        public abstract WebBrowserContentActionResult GetContent(string selector, WebEventSelectionMode mode, bool innerContent);
        public abstract WebBrowserActionResult EnterTextForElement(string selector, string text, WebEventSelectionMode mode);
        public abstract void DeleteAllCookies();
        public abstract WebBrowserActionResult UpdateDOM(string selector, string content, WebEventSelectionMode mode);
        public abstract WebBrowserActionResult UploadFile(string selector, WebEventSelectionMode mode, string filePath);
        public abstract WebBrowserActionResult ClickOnAllElements(string selector, WebEventSelectionMode mode);
        public abstract WebBrowserActionResult SwitchToUrl(string url);
        public abstract void SetCookies(List<OpenQA.Selenium.Cookie> cookies);
        public abstract List<OpenQA.Selenium.Cookie> GetAllCookies();
        public abstract void Close();
    }
}