using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Models
{
    public sealed class AiResponseInstruction
    {
        public string AssistantMessage { get; set; } = string.Empty;
        public string InternalContent { get; set; } = string.Empty;
        public bool Final { get; set; }
        public List<AiResponseToolCall> ToolCalls { get; set; } = new();
    }

    public sealed class AiResponseToolCall
    {
        public string ToolName { get; set; } = string.Empty;
        public JObject Arguments { get; set; } = new JObject();
    }
}
