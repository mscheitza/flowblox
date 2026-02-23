using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Logging;
using FlowBlox.Core.Util;
using Newtonsoft.Json;
using System.Text;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi
{
    public partial class FlowBloxWebApiService
    {
        public async Task<ApiResponse<List<FbProjectVersionResult>>> GetProjectVersionsAsync(string userToken, Guid projectGuid)
        {
            string url = UriHelper.ConcatUri(_baseUrl, $"project_version.php?guid={projectGuid}");

            if (!string.IsNullOrEmpty(userToken))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<List<FbProjectVersionResult>>
                    {
                        Success = true,
                        ResultObject = JsonConvert.DeserializeObject<List<FbProjectVersionResult>>(json) ?? new()
                    };
                }

                return await CreateErrorApiResponse<List<FbProjectVersionResult>>(response);
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse<List<FbProjectVersionResult>> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<int>> CreateProjectVersionAsync(string userToken, FbCreateProjectVersionRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "project_version.php");
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<FbCreateProjectVersionResult>(body);

                    return new ApiResponse<int>
                    {
                        Success = true,
                        ResultObject = result?.VersionNumber ?? 0
                    };
                }

                return await CreateErrorApiResponse<int>(response);
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse<int> { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse> UpdateProjectVersionMetadataAsync(string userToken, FbProjectVersionChangeRequest request)
        {
            string url = UriHelper.ConcatUri(_baseUrl, "project_version.php");
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            try
            {
                var response = await _httpClient.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                    return new ApiResponse { Success = true };

                return await CreateErrorApiResponse(response);
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<string>> GetProjectVersionContentAsync(string userToken, Guid projectGuid, int version)
        {
            // Content is retrieved via a separate endpoint to keep DTOs small.
            string url = UriHelper.ConcatUri(_baseUrl, $"project_version_content.php?guid={projectGuid}&version={version}");

            if (!string.IsNullOrEmpty(userToken))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var resp = JsonConvert.DeserializeObject<FbGetProjectVersionContentResponse>(json);

                    return new ApiResponse<string>
                    {
                        Success = true,
                        ResultObject = resp?.Content
                    };
                }

                return await CreateErrorApiResponse<string>(response);
            }
            catch (Exception ex)
            {
                FlowBloxLogManager.Instance.GetLogger().Exception(ex);
                return new ApiResponse<string> { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}
