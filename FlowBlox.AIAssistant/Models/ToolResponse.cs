using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Models
{
    public class ToolResponse
    {
        public bool Ok { get; set; }
        public JObject Result { get; set; } = new JObject();
        public string Error { get; set; } = string.Empty;
        public JArray Log { get; set; } = new JArray();
    }
}
