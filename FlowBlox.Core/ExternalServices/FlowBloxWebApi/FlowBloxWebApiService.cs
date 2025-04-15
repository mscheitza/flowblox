using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Util;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi
{
    public class FlowBloxWebApiService
    {
        protected string _baseUrl;
        protected HttpClient _httpClient;

        public FlowBloxWebApiService(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
        }

        protected class ErrorResponse
        {
            public string Error { get; set; }

            public string ErrorCode { get; set; }
        }

        public class RegisterResult
        {
            public bool Success { get; set; }

            public string ErrorMessage { get; set; }

            public string ErrorCode { get; set; }
        }

        public class ApiResponse
        {
            public bool Success { get; set; }

            public string ErrorMessage { get; set; }
        }

        public async Task<RegisterResult> Register(FbRegistrationData registrationData)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "register.php");
            try
            {
                var json = JsonConvert.SerializeObject(registrationData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string resp = await response.Content.ReadAsStringAsync();

                    return new RegisterResult { Success = true };
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                    return new RegisterResult
                    {
                        Success = false,
                        ErrorMessage = error?.Error,
                        ErrorCode = error?.ErrorCode
                    };
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new RegisterResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<FbCaptchaResponse> GetCaptchaAsync()
        {
            string url = UriHelper.ConcatUri(_baseUrl, "captcha.php");
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var captchaResult = JsonConvert.DeserializeObject<CaptchaResult>(jsonData);

                    if (!string.IsNullOrEmpty(captchaResult.CaptchaImage))
                    {
                        // Entferne den Prefix "data:image/png;base64," falls vorhanden
                        var base64Data = captchaResult.CaptchaImage.Split(',')[1];

                        return new FbCaptchaResponse
                        {
                            CaptchaId = captchaResult.CaptchaId,
                            CaptchaImageBase64 = base64Data
                        };
                    }

                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return null;
            }
        }

        private class CaptchaResult
        {
            public string CaptchaId { get; set; }
            public string CaptchaImage { get; set; }
        }


        public async Task<string> LoginAsync(FbLoginData loginData)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "login.php");
            try
            {
                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                string responseData = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonConvert.DeserializeObject<FbTokenResponse>(responseData);
                    return tokenResponse.Token;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return null;
            }
        }

        public async Task<FbUserData> GetUserInfoAsync(string token)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "userinfo.php?token=" + token);
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonConvert.DeserializeObject<FbUserData>(jsonData);
                    return userInfo;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return null;
            }
        }

        public async Task<ApiResponse> GeneratePasswordResetCodeAsync(FbPasswordResetRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "generatePasswordResetCode.php");
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                    return new ApiResponse
                    {
                        Success = false,
                        ErrorMessage = error?.Error
                    };
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse> ChangePasswordAsync(FbChangePasswordRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "changePassword.php");
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                    return new ApiResponse
                    {
                        Success = false,
                        ErrorMessage = error?.Error
                    };
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FbExtensionResult> GetExtensionAsync(FbExtensionRequest request)
        {
            string query = string.IsNullOrEmpty(request.Name) ? $"guid={request.Guid}" : $"name={request.Name}";
            string url = UriHelper.ConcatUri(_baseUrl, $"extension.php?{query}");

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FbExtensionResult>(responseBody);
            }
            catch (HttpRequestException e)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(e);
                return null;
            }
        }

        public async Task<List<FbExtensionResult>> GetExtensionsAsync(string searchForName = "", string searchForUsername = "")
        {
            // Konstruiere die URL basierend auf den vorhandenen Parametern
            string url = string.IsNullOrEmpty(searchForUsername)
                ? UriHelper.ConcatUri(_baseUrl, $"extensions.php?searchForName={searchForName}")
                : UriHelper.ConcatUri(_baseUrl, $"extensions.php?searchForUsername={searchForUsername}");

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<FbExtensionResult>>(jsonResponse);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return null;
            }
        }

        public async Task<ApiResponse> CreateExtensionAsync(string userToken, FbCreateExtensionRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "extension.php");
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string resp = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<FbCreateExtensionResult>(resp);

                    // Erfolgsmeldung zurückgeben
                    return new ApiResponse
                    {
                        Success = true
                    };
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                    return new ApiResponse
                    {
                        Success = false,
                        ErrorMessage = error?.Error
                    };
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<ApiResponse> CreateExtensionVersionAsync(string userToken, FbCreateExtensionVersionRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "version.php");
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                    return new ApiResponse
                    {
                        Success = false,
                        ErrorMessage = error?.Error
                    };
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse> UpdateExtensionAsync(string userToken, FbExtensionChangeRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "extension.php");
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            try
            {
                HttpResponseMessage response = await _httpClient.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                    return new ApiResponse
                    {
                        Success = false,
                        ErrorMessage = error?.Error
                    };
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse> UpdateExtensionVersionAsync(string userToken, FbExtensionVersionChangeRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "version.php");
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            try
            {
                HttpResponseMessage response = await _httpClient.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse 
                    { 
                        Success = true 
                    };
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                    return new ApiResponse
                    {
                        Success = false,
                        ErrorMessage = error?.Error
                    };
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse> DeleteExtensionAsync(string userToken, FbDeleteExtensionRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "extension.php");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Content = content,
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(url)
                };

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                    return new ApiResponse
                    {
                        Success = false,
                        ErrorMessage = error?.Error
                    };
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse> DeleteExtensionVersionAsync(string userToken, FbDeleteExtensionVersionRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "version.php");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Content = content,
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(url)
                };

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse { Success = true };
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResponse);
                    return new ApiResponse
                    {
                        Success = false,
                        ErrorMessage = error?.Error
                    };
                }
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> HasVersionContentAsync(Guid extensionGuid, string version)
        {
            string url = UriHelper.ConcatUri(_baseUrl, $"version_content.php?guid={extensionGuid}&version={version}&mode=hasContent");

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var hasContentResponse = JsonConvert.DeserializeObject<FbHasVersionContentResponse>(jsonResponse);

                return hasContentResponse?.HasContent ?? false;
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return false;
            }
        }

        public async Task<string> GetVersionContentAsync(Guid extensionGuid, string version)
        {
            string url = UriHelper.ConcatUri(_baseUrl, $"version_content.php?guid={extensionGuid}&version={version}");

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var contentResponse = JsonConvert.DeserializeObject<FbGetVersionContentResponse>(jsonResponse);

                return contentResponse?.Content;
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return null;
            }
        }

        
    }
}
