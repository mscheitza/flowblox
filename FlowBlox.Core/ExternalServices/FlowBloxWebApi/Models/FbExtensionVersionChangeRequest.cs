namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbExtensionVersionChangeRequest
    {
        public string ExtensionGuid { get; set; }

        public string Version { get; set; }

        public string Changes { get; set; }

        public byte[] Content { get; set; }

        public string RuntimeVersion { get; set; }

        public bool Released { get; set; }

        public bool Active { get; set; }

        public bool BackwardsCompatible { get; set; }

        public List<FbVersionDependency> Dependencies { get; set; }
        
    }
}
