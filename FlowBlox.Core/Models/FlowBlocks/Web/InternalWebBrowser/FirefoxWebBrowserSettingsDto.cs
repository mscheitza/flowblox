namespace FlowBlox.Core.Models.FlowBlocks.Web.InternalWebBrowser
{
    /// <summary>
    /// Selenium settings specialized for Firefox (Gecko) browsers.
    /// </summary>
    public class FirefoxWebBrowserSettingsDto : SeleniumWebBrowserSettingsDto
    {
        /// <summary>
        /// Runs Firefox in headless mode when set to true.
        /// </summary>
        public bool Headless { get; set; } = true;

        /// <summary>
        /// Initial browser window width.
        /// </summary>
        public int WindowWidth { get; set; } = 1920;

        /// <summary>
        /// Initial browser window height.
        /// </summary>
        public int WindowHeight { get; set; } = 1200;

        /// <summary>
        /// If true, Firefox will accept insecure certificates.
        /// </summary>
        public bool AcceptInsecureCertificates { get; set; } = true;

        /// <summary>
        /// Creates a default settings instance optimized for headless Firefox usage.
        /// </summary>
        public static FirefoxWebBrowserSettingsDto CreateDefault()
        {
            return new FirefoxWebBrowserSettingsDto
            {
                HideCommandPromptWindow = true,
                Headless = true,
                WindowWidth = 1920,
                WindowHeight = 1200,
                AcceptInsecureCertificates = true,
                Arguments = new List<string>()
            };
        }
    }
}
