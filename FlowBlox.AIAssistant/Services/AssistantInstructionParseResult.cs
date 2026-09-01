using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Services
{
    internal sealed class AssistantInstructionParseResult
    {
        public AssistantInstruction Instruction { get; init; }
        public JObject JsonObject { get; init; }
        public Exception Exception { get; init; }
        public string ResponseContent { get; init; } 
        public bool Success => Instruction != null;
    }
}