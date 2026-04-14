namespace FlowBlox.Core.Models.Runtime.Debugging
{
    public sealed class RuntimeGeneratedResultFieldValueMapping
    {
        public string FieldFullyQualifiedName { get; set; } = string.Empty;
        public string Value { get; set; }
    }

    public sealed class RuntimeGeneratedResultDataset
    {
        public int DatasetIndex { get; set; }
        public List<RuntimeGeneratedResultFieldValueMapping> FieldValueMappings { get; set; } = new();
    }

    public sealed class RuntimeGeneratedResultDetails
    {
        public int Id { get; set; }
        public string Elapsed { get; set; } = "00:00.000";
        public string FlowBlockName { get; set; } = string.Empty;
        public int DatasetCount { get; set; }
        public List<RuntimeGeneratedResultDataset> Datasets { get; set; } = new();
    }
}
