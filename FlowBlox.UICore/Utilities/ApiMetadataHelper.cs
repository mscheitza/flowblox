using FlowBlox.Core.ExternalServices.FlowBloxWebApi;
using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;
using FlowBlox.Core.Logging;

namespace FlowBlox.UICore.Utilities
{
    public static class ApiMetadataHelper
    {
        /// <summary>
        /// Loads API metadata. Returns null if unavailable.
        /// </summary>
        public static async Task<FbApiMetadata> LoadApiMetadataAsync(FlowBloxWebApiService webApiService)
        {
            if (webApiService == null)
                return null;

            var resp = await webApiService.GetApiMetadataAsync();
            if (!resp.Success)
            {
                FlowBloxLogManager.Instance.GetLogger().Error($"Failed to load API metadata. {resp.ErrorMessage}");
                return null;
            }

            return resp.ResultObject;
        }
    }
}
