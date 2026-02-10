namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    /// <summary>
    /// Selenium settings specialized for Chrome-based browsers.
    /// </summary>
    public class ChromeWebBrowserSettingsDto : SeleniumWebBrowserSettingsDto
    {
        /// <summary>
        /// Creates a default settings instance optimized for headless Chrome usage
        /// in typical CI / Docker scenarios.
        /// </summary>
        public static ChromeWebBrowserSettingsDto CreateDefault()
        {
            return new ChromeWebBrowserSettingsDto
            {
                HideCommandPromptWindow = true,
                Arguments = new List<string>
                {
                    "--headless",
                    "--disable-gpu",
                    "--window-size=1920,1200",
                    "--ignore-certificate-errors",
                    "--no-sandbox",
                    "--disable-dev-shm-usage"
                }
            };
        }
    }
}
