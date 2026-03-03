using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Models
{
    public class ToolDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public JObject ArgumentsSchema { get; set; } = new JObject();
    }
}
