namespace FlowBlox.UICore.Formatting;

public static class ContentFormattingProviderFactory
{
    private static readonly IContentFormattingProvider JsonProvider = new JsonContentFormattingProvider();
    private static readonly IContentFormattingProvider XmlProvider = new XmlContentFormattingProvider();

    public static IContentFormattingProvider? GetFormattingProvider(string? contentType) =>
        contentType?.Trim().ToUpperInvariant() switch
        {
            "JSON" => JsonProvider,
            "XML" or "HTML" => XmlProvider,
            _ => null
        };
}
