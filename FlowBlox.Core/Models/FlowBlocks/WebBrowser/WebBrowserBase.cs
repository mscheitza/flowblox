using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Net;
using System.Xml;
using Cookie = OpenQA.Selenium.Cookie;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public abstract class WebBrowserBase
    {
        protected WebDriverWait webDriverWait;
        protected IWebDriver driver;

        protected WebBrowserBase()
        {
            
        }

        protected void InitWebDriverWait()
        {
            this.webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(WebBrowserConstants.DefaultTimeout));
        }

        /// <summary>
        /// Convenience accessor for executing JavaScript on the current driver.
        /// </summary>
        protected IJavaScriptExecutor JsExecutor => (IJavaScriptExecutor)driver;

        public string Name { get; set; }

        protected TResult DeserializeToSettings<TResult>(string serializedSettings)
            where TResult : SeleniumWebBrowserSettingsDto
        {
            TResult settings;
            try
            {
                settings = JsonConvert.DeserializeObject<TResult>(serializedSettings);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "Option 'WebBrowser.ServiceSettings' contains invalid JSON. " +
                    "Please fix the JSON or reset it to default.", ex);
            }

            if (settings == null)
            {
                throw new InvalidOperationException(
                    "Deserialization of 'WebBrowser.ServiceSettings' returned null. " +
                    "Please check the JSON configuration or reset it to default.");
            }

            return settings;
        }

        public virtual int Timeout
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

        public virtual void Open(string url)
        {
            driver.Navigate().GoToUrl(url);
            WaitForPageLoad();
        }

        public virtual void Close()
        {
            driver.Close();
        }

        private IWebElement FindElement(string selector, WebEventSelectionMode mode, int retryCount = 0)
        {
            return mode == WebEventSelectionMode.XPath ?
                    driver.FindElement(By.XPath(selector)) :
                    driver.FindElement(By.CssSelector(selector));
        }

        private IEnumerable<IWebElement> FindElements(string selector, WebEventSelectionMode mode, int retryCount = 0)
        {
            return mode == WebEventSelectionMode.XPath ?
                driver.FindElements(By.XPath(selector)) :
                driver.FindElements(By.CssSelector(selector));
        }

        public int RetryLimit { get; set; }

        public int RetryTimeunit { get; set; }

        /// <summary>
        /// Executes a browser action with retry support.
        /// Retries common transient Selenium errors (element not found, not interactable, stale, intercepted).
        /// </summary>
        protected WebBrowserActionResult Invoke(Func<WebBrowserActionResult> executor, int retryCount = 0)
        {
            try
            {
                return executor();
            }
            catch (NoSuchElementException ex)
            {
                return HandleRetryOrFail(executor, retryCount, ex, WebBrowserActionStatus.ElementNotFound);
            }
            catch (ElementNotInteractableException ex)
            {
                return HandleRetryOrFail(executor, retryCount, ex, WebBrowserActionStatus.ElementNotApplicable);
            }
            catch (StaleElementReferenceException ex)
            {
                return HandleRetryOrFail(executor, retryCount, ex, WebBrowserActionStatus.ElementStale);
            }
            catch (WebDriverException ex)
            {
                return HandleRetryOrFail(executor, retryCount, ex, WebBrowserActionStatus.UnknownError);
            }
        }

        private WebBrowserActionResult HandleRetryOrFail(
            Func<WebBrowserActionResult> executor,
            int retryCount,
            Exception exception,
            WebBrowserActionStatus status)
        {
            if (retryCount < RetryLimit)
            {
                WaitForPageLoad();
                Thread.Sleep(RetryTimeunit);
                return Invoke(executor, retryCount + 1);
            }

            return new WebBrowserActionResult
            {
                Success = false,
                Exception = exception,
                Status = status
            };
        }

        public virtual WebBrowserActionResult ClickOnElement(string selector, WebEventSelectionMode mode)
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

        public virtual WebBrowserActionResult ClickOnAllElements(string selector, WebEventSelectionMode mode)
        {
            return this.Invoke(() =>
            {
                foreach (var elem in FindElements(selector, mode))
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

        public virtual WebBrowserActionResult ScrollMax()
        {
            JsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            WaitForPageLoad();
            return new WebBrowserActionResult()
            {
                Success = true
            };
        }

        public string TabHandle => driver.WindowHandles.Last();

        public virtual int Height => driver.Manage().Window.Size.Height;

        public virtual string DOMContent => driver.PageSource;

        public virtual void Dispose()
        {
            this.driver?.Quit();
            this.driver?.Dispose();
        }

        public virtual WebBrowserActionResult EnterTextForElement(string selector, string text, WebEventSelectionMode mode)
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

        public virtual void DeleteAllCookies()
        {
            this.driver.Manage().Cookies.DeleteAllCookies();
        }

        public virtual List<Cookie> GetAllCookies()
        {
            return driver.Manage().Cookies.AllCookies.ToList();
        }

        public virtual void SetCookies(List<OpenQA.Selenium.Cookie> cookies)
        {
            foreach (var cookie in cookies)
            {
                var adjustedCookie = new OpenQA.Selenium.Cookie(
                    cookie.Name,
                    cookie.Value,
                    cookie.Domain,
                    cookie.Path,
                    cookie.Expiry);

                driver.Manage().Cookies.AddCookie(adjustedCookie);
            }
        }

        public virtual WebBrowserActionResult UpdateDOM(string selector, string content, WebEventSelectionMode mode)
        {
            return this.Invoke(() =>
            {
                var elem = FindElement(selector, mode);

                // Escape the content string for JavaScript
                string escapedContent = content.Replace("\\", "\\\\")
                    .Replace("'", "\\'")
                    .Replace("\r\n", "\\n")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\n");

                JsExecutor.ExecuteScript($"arguments[0].innerHTML = '{escapedContent}'", elem);
                return new WebBrowserActionResult()
                {
                    Success = true
                };
            });
        }

        public virtual WebBrowserActionResult UploadFile(string selector, WebEventSelectionMode mode, string filePath)
        {
            return this.Invoke(() =>
            {
                IWebElement fileInput = null;
                if (mode == WebEventSelectionMode.CssSelector)
                {
                    fileInput = driver.FindElement(By.CssSelector(selector));
                }
                else if (mode == WebEventSelectionMode.XPath)
                {
                    fileInput = driver.FindElement(By.XPath(selector));
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

        public virtual WebBrowserActionResult SwitchToUrl(string url)
        {
            try
            {
                driver.Navigate().GoToUrl(url);
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

        public virtual WebBrowserContentActionResult GetContent(string selector, WebEventSelectionMode mode, bool innerContent)
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