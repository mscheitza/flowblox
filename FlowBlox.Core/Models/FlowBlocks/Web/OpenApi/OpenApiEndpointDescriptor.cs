namespace FlowBlox.Core.Models.FlowBlocks.Web.OpenApi
{
    internal sealed class OpenApiEndpointDescriptor
    {
        public string DisplayName { get; init; }
        public string Group { get; init; }
        public string Method { get; init; }
        public string Path { get; init; }
        public string Summary { get; init; }
        public string ServerUrl { get; init; }
        public string ContentType { get; init; }
        public string PayloadExample { get; init; }
        public List<OpenApiParameterValue> Parameters { get; init; } = [];
    }
}
