using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public class ChromeWebBrowser : WebBrowserBase, IDisposable
    {
        protected ChromeDriver browser;
        protected WebDriverWait webDriverWait;

        public ChromeWebBrowser()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--window-size=1920,1200");
            chromeOptions.AddArguments("--ignore-certificate-errors");
            chromeOptions.AddArguments("--no-sandbox");
            chromeOptions.AddArguments("--disable-dev-shm-usage");
            this.browser = new ChromeDriver(chromeDriverService, chromeOptions);
            this.webDriverWait = new WebDriverWait(browser, TimeSpan.FromSeconds(30));
        }

        public override int Timeout
        {
            get
            {
                return this.webDriverWait.Timeout.Seconds;
            }
            set
            {
                this.webDriverWait.Timeout = TimeSpan.FromSeconds(value);
            }
        }

        public override void Open(string url)
        {
            browser.Navigate().GoToUrl(url);
            WaitForPageLoad();
        }

        public override void Close()
        {
            browser.Close();
        }

        private const int RetryLimit = 3;
        private const int RetryTimeunit = 100;

        private IWebElement FindElement(string selector, WebEventSelectionMode mode, int retryCount = 0)
        {
            try
            {
                return mode == WebEventSelectionMode.XPath ?
                    browser.FindElement(By.XPath(selector)) :
                    browser.FindElement(By.CssSelector(selector));
            }
            catch (Exception)
            {
                if (retryCount < RetryLimit)
                {
                    WaitForPageLoad();
                    Thread.Sleep(RetryTimeunit);
                    return FindElement(selector, mode, retryCount + 1);
                }
                else
                {
                    throw;
                }
            }
        }

        private IEnumerable<IWebElement> FindElements(string selector, WebEventSelectionMode mode, int retryCount = 0)
        {
            try
            {
                return mode == WebEventSelectionMode.XPath ?
                    browser.FindElements(By.XPath(selector)) :
                    browser.FindElements(By.CssSelector(selector));
            }
            catch (Exception)
            {
                if (retryCount < RetryLimit)
                {
                    WaitForPageLoad();
                    Thread.Sleep(RetryTimeunit);
                    return FindElements(selector, mode, retryCount + 1);
                }
                else
                {
                    throw;
                }
            }
        }

        private WebBrowserActionResult Invoke(Func<WebBrowserActionResult> executor)
        {
            try
            {
                return executor.Invoke();
            }
            catch (ElementNotInteractableException e)
            {
                return new WebBrowserActionResult()
                {
                    Success = false,
                    Exception = e,
                    Status = WebBrowserActionStatus.ElementNotApplicable
                };
            }
            catch (NoSuchElementException e)
            {
                return new WebBrowserActionResult() 
                { 
                    Success = false,
                    Exception = e,
                    Status = WebBrowserActionStatus.ElementNotFound
                };
            }
        }

        public override WebBrowserActionResult ClickOnElement(string selector, WebEventSelectionMode mode)
        {
            return this.Invoke(() =>
            {
                var elem = FindElement(selector, mode);
                elem.Click();
                WaitForPageLoad();
                return new WebBrowserActionResult() 
                { 
                    Success = true 
                };
            });
        }

        public override WebBrowserActionResult ClickOnAllElements(string selector, WebEventSelectionMode mode)
        {
            return this.Invoke(() =>
            {
                foreach(var elem in FindElements(selector, mode))
                {
                    elem.Click();
                    WaitForPageLoad();
                }
                
                return new WebBrowserActionResult()
                {
                    Success = true
                };
            });
        }

        public override WebBrowserActionResult ScrollMax()
        {
            browser.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            WaitForPageLoad();
            return new WebBrowserActionResult() 
            { 
                Success = true 
            };
        }

        public string TabHandle => browser.WindowHandles.Last();

        public override int Height => browser.Manage().Window.Size.Height;

        public override string DOMContent => browser.PageSource;

        public void Dispose()
        {
            this.browser.Dispose();
        }

        public override WebBrowserActionResult EnterTextForElement(string selector, string text, WebEventSelectionMode mode)
        {
            return this.Invoke(() =>
            {
                var elem = FindElement(selector, mode);
                elem.SendKeys(text);
                return new WebBrowserActionResult() 
                { 
                    Success = true 
                };
            });
        }

        public override void DeleteAllCookies()
        {
            this.browser.Manage().Cookies.DeleteAllCookies();
        }

        public override List<Cookie> GetAllCookies()
        {
            return browser.Manage().Cookies.AllCookies.ToList();
        }

        public override void SetCookies(List<OpenQA.Selenium.Cookie> cookies)
        {
            foreach (var cookie in cookies)
            {
                var adjustedCookie = new OpenQA.Selenium.Cookie(
                    cookie.Name,
                    cookie.Value,
                    cookie.Domain,
                    cookie.Path,
                    cookie.Expiry);
                
                browser.Manage().Cookies.AddCookie(adjustedCookie);
            }
        }

        public override WebBrowserActionResult UpdateDOM(string selector, string content, WebEventSelectionMode mode)
        {
            return this.Invoke(() =>
            {
                var elem = FindElement(selector, mode);

                // Escape the content string for JavaScript
                string escapedContent = content.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\r", "\\n");

                browser.ExecuteScript($"arguments[0].innerHTML = '{escapedContent}'", elem);
                return new WebBrowserActionResult()
                {
                    Success = true
                };
            });
        }


        public override WebBrowserActionResult UploadFile(string selector, WebEventSelectionMode mode, string filePath)
        {
            return this.Invoke(() =>
            {
                IWebElement fileInput = null;
                if (mode == WebEventSelectionMode.CssSelector)
                {
                    fileInput = browser.FindElement(By.CssSelector(selector));
                }
                else if (mode == WebEventSelectionMode.XPath)
                {
                    fileInput = browser.FindElement(By.XPath(selector));
                }

                if (fileInput != null && fileInput.GetAttribute("type") == "file")
                {
                    fileInput.SendKeys(filePath);
                    return new WebBrowserActionResult()
                    {
                        Success = true
                    };
                }
                else
                {
                    return new WebBrowserActionResult()
                    {
                        Success = false,
                        Status = WebBrowserActionStatus.ElementNotFound
                    };
                }
            });
        }

        public override WebBrowserActionResult SwitchToUrl(string url)
        {
            try
            {
                browser.Navigate().GoToUrl(url);
                WaitForPageLoad();
                return new WebBrowserActionResult() 
                { 
                    Success = true 
                };
            }
            catch (Exception e)
            {
                return new WebBrowserActionResult() 
                { 
                    Success = false, 
                    Status = WebBrowserActionStatus.UrlNotAccessible, 
                    Exception = e 
                };
            }
        }

        private void WaitForPageLoad()
        {
            webDriverWait.Until(driver =>
            {
                bool isPageLoaded = ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");
                bool isBodyPresent = ((IJavaScriptExecutor)driver).ExecuteScript("return document.body != null").Equals(true);
                return isPageLoaded && isBodyPresent;
            });
        }

        public override WebBrowserContentActionResult GetContent(string selector, WebEventSelectionMode mode, bool innerContent)
        {
            string content = "";
            var actionResult = this.Invoke(() =>
            {
                var elem = FindElement(selector, mode);
                content = innerContent ? elem.GetAttribute("innerHTML") : elem.GetAttribute("outerHTML");
                return new WebBrowserActionResult()
                {
                    Success = true
                };
            });
            return new WebBrowserContentActionResult()
            {
                Content = content,
                Exception = actionResult.Exception,
                Status = actionResult.Status,
                Success = actionResult.Success
            };
        }
    }
}