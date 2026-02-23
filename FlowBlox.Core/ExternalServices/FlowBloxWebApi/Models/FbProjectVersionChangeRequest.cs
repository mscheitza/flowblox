namespace FlowBlox.Core.ExternalServices.FlowBloxWebApi.Models
{
    public class FbProjectVersionChangeRequest
    {
        public string ProjectGuid { get; set; }
        public int Version { get; set; }
        public string Comment { get; set; }
    }
}
