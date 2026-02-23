namespace FlowBlox.Core.Models.Project
{
    public class FlowBloxProjectLocalData
    {
        public string ProjectSpaceGuid { get; set; }
        public int? ProjectSpaceVersion { get; set; }
        public Dictionary<string, string> LocalUserFieldValues { get; set; } = new();
    }
}