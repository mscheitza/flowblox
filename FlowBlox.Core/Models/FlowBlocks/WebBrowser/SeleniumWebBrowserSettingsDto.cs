namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    /// <summary>
    /// Generic Selenium web browser settings that can be used for Chrome, Firefox, etc.
    /// This DTO is intended to be stored as JSON in options.
    /// </summary>
    public class SeleniumWebBrowserSettingsDto
    {
        /// <summary>
        /// Hides the driver command prompt window (if supported by the driver service).
        /// </summary>
        public bool HideCommandPromptWindow { get; set; }

        /// <summary>
        /// Raw command line arguments passed to the browser.
        /// Example: "--headless", "--disable-gpu", "--window-size=1920,1200".
        /// </summary>
        public List<string> Arguments { get; set; } = new List<string>();
    }
}
