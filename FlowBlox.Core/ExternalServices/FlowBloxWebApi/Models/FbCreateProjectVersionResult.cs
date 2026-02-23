using Newtonsoft.Json;

namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbCreateProjectVersionResult
    {
        public bool Success { get; set; }
        public string ProjectGuid { get; set; }
        public int VersionNumber { get; set; }
    }
}
