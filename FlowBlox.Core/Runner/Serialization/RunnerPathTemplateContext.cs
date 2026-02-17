namespace FlowBlox.Core.Runner.Serialization
{
    public sealed class RunnerPathTemplateContext
    {
        public string ProjectName { get; init; }
        public string ContentForHash { get; init; }
        public DateTime UtcNow { get; init; } = DateTime.UtcNow;
    }
}
