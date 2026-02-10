using FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Repository
{
    public class ExtensionRepository
    {
        private readonly Dictionary<string, FbExtensionResult> _extensions;
        private readonly FlowBloxWebApiService _webApiService;

        public ExtensionRepository(FlowBloxWebApiService webApiService)
        {
            _extensions = new Dictionary<string, FbExtensionResult>();
            _webApiService = webApiService;
        }

        public void AddExtension(FbExtensionResult extension)
        {
            if (!_extensions.ContainsKey(extension.Name))
            {
                _extensions[extension.Name] = extension;
            }
        }

        public async Task<FbExtensionResult> GetExtensionByNameAsync(string name)
        {
            if (_extensions.TryGetValue(name, out var extension))
                return extension;

            var request = new FbExtensionRequest { Name = name };
            var resp = await _webApiService.GetExtensionAsync(request);

            if (!resp.Success || resp.ResultObject == null)
                return null;

            extension = resp.ResultObject;
            _extensions[name] = extension;

            return extension;
        }

    }
}
