using FlowBlox.Core.Util;
using FlowBlox.Core.Logging;
using Newtonsoft.Json;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;

namespace FlowBlox.Core.Authentication
{
    public class FlowBloxAutoLoginExecutor
    {
        private readonly Lazy<FlowBloxWebApiService> _flowBloxWebApiService;

        public FlowBloxAutoLoginExecutor()
        {
            _flowBloxWebApiService = new Lazy<FlowBloxWebApiService>(() =>
            {
                var webApiServiceUrl = FlowBloxOptions.GetOptionInstance().OptionCollection["General.ExtensionApiServiceBaseUrl"].Value;
                return new FlowBloxWebApiService(webApiServiceUrl);
            });
        }

        public async Task TryAutoLoginAsync()
        {
            try
            {
                var loginDataString = FlowBloxOptions.GetOptionInstance().GetOption("Account.LoginData").Value;
                var loginData = !string.IsNullOrEmpty(loginDataString) ?
                    JsonConvert.DeserializeObject<FlowBloxLoginData>(loginDataString) :
                    null;

                if (loginData != null)
                {
                    var token = await _flowBloxWebApiService.Value.LoginAsync(new FbLoginData
                    {
                        Username = loginData.UserName,
                        Password = loginData.Password
                    });

                    if (!string.IsNullOrEmpty(token))
                    {
                        var userData = await _flowBloxWebApiService.Value.GetUserInfoAsync(token);
                        if (userData != null)
                        {
                            FlowBloxAccountManager.Instance.ActiveUser = userData;
                            FlowBloxAccountManager.Instance.UserToken = token;

                            FlowBloxLogManager.Instance.GetLogger().Info("Auto-login successful.");
                        }
                        else
                        {
                            FlowBloxLogManager.Instance.GetLogger().Error("Failed to retrieve user data.");
                        }
                    }
                    else
                    {
                        FlowBloxLogManager.Instance.GetLogger().Error("Auto-login failed: Invalid credentials.");
                    }
                }
                else
                {
                    FlowBloxLogManager.Instance.GetLogger().Error("No stored login data available for auto-login.");
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Error($"An error occurred during auto-login: {ex.Message}", ex);
            }
        }
    }
}
