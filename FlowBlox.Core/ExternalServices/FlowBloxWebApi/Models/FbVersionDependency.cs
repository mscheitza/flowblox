namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbVersionDependency
    {
        public string ExtensionName { get; set; }

        public Guid ExtensionGuid { get; set; }

        public string Version { get; set; }
    }
}
