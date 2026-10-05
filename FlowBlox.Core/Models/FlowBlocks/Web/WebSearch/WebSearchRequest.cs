namespace FlowBlox.Core.Models.FlowBlocks.Web.WebSearch
{
    public sealed class WebSearchRequest
    {
        public string Query { get; init; } = string.Empty;
        public int MaxResults { get; init; } = 10;
        public string Country { get; init; } = string.Empty;
        public string SearchLanguage { get; init; } = string.Empty;
        public WebSearchSafeSearch SafeSearch { get; init; } = WebSearchSafeSearch.Moderate;
    }
}
