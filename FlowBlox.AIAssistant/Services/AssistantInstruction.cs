using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Services
{
    internal sealed class AssistantInstruction
    {
        public string AssistantMessage { get; set; } = string.Empty;
        public string InternalContent { get; set; } = string.Empty;
        public bool Final { get; set; }
        public List<AssistantToolCall> ToolCalls { get; set; } = new();
    }

    internal sealed class AssistantToolCall
    {
        public string ToolName { get; set; } = string.Empty;
        public JObject Arguments { get; set; } = new JObject();
    }
}