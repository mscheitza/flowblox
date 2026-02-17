namespace FlowBlox.Core.Runner.Contracts
{
    public sealed class ProjectOutputDatasetDto
    {
        public ProjectOutputDatasetDto()
        {
            Values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public string OutputName { get; set; }
        public DateTime CreatedUtc { get; set; }
        public Dictionary<string, object> Values { get; set; } 
    }
}
