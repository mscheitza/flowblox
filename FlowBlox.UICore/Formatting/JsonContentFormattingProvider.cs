using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.UICore.Formatting;

internal sealed class JsonContentFormattingProvider : IContentFormattingProvider
{
    public string Format(string content)
    {
        try
        {
            return JToken.Parse(content ?? string.Empty).ToString(Newtonsoft.Json.Formatting.Indented);
        }
        catch (JsonException exception)
        {
            throw new FormatException("The content is not valid JSON.", exception);
        }
    }
}
