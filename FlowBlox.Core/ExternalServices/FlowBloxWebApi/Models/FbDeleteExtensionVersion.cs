namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbDeleteExtensionVersionRequest
    {
        public Guid ExtensionGuid { get; set; }

        public string VersionNumber { get; set; }
    }
}
