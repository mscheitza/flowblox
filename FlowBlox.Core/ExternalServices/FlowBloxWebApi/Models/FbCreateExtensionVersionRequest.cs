namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbCreateExtensionVersionRequest
    {
        public Guid ExtensionGuid { get; set; }

        public string Version { get; set; }
    }
}