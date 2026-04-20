using FlowBlox.Core.Models.FlowBlocks.WebBrowser;
using OpenQA.Selenium.Firefox;
using System.Drawing;

namespace FlowBlox.Core.Models.FlowBlocks.Web.InternalWebBrowser
{
    public class FirefoxWebBrowser : WebBrowserBase
    {
        public FirefoxWebBrowser(string serializedSettings)
        {
            var settings = DeserializeToSettings<FirefoxWebBrowserSettingsDto>(serializedSettings);
            var firefoxService = FirefoxDriverService.CreateDefaultService();
            firefoxService.HideCommandPromptWindow = settings.HideCommandPromptWindow;

            var firefoxOptions = new FirefoxOptions();
            if (settings.Headless)
                firefoxOptions.AddArgument("-headless");

            firefoxOptions.AcceptInsecureCertificates = settings.AcceptInsecureCertificates;

            if (settings.Arguments != null)
            {
                foreach (var arg in settings.Arguments)
                {
                    if (!string.IsNullOrWhiteSpace(arg))
                    {
                        firefoxOptions.AddArgument(arg.Trim());
                    }
                }
            }

            driver = new FirefoxDriver(firefoxService, firefoxOptions);
            driver.Manage().Window.Size = new Size(settings.WindowWidth, settings.WindowHeight);

            InitWebDriverWait();
        }
    }
}