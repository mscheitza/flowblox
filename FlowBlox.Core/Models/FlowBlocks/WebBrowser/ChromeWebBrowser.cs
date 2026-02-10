using OpenQA.Selenium.Chrome;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public class ChromeWebBrowser : WebBrowserBase, IDisposable
    {
        public ChromeWebBrowser(string serializedSettings): base()
        {
            var settings = DeserializeToSettings<ChromeWebBrowserSettingsDto>(serializedSettings);
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = settings.HideCommandPromptWindow;

            var chromeOptions = new ChromeOptions();
            if (settings.Arguments != null)
            {
                foreach (var arg in settings.Arguments)
                {
                    if (!string.IsNullOrWhiteSpace(arg))
                    {
                        chromeOptions.AddArgument(arg.Trim());
                    }
                }
            }

            this.driver = new ChromeDriver(chromeDriverService, chromeOptions);

            InitWebDriverWait();
        }
    }
}