using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools.Util
{
    internal static class ProtocolEntryWindowing
    {
        public static JArray TakeLastInOriginalOrder(IEnumerable<JToken>? entries, int maxEntries)
        {
            var values = entries?.ToList() ?? [];
            var take = Math.Max(1, maxEntries);
            return new JArray(values
                .Skip(Math.Max(0, values.Count - take))
                .Select(value => value.DeepClone()));
        }
    }
}
