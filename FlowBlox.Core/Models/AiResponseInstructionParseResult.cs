using Newtonsoft.Json.Linq;

namespace FlowBlox.Core.Models
{
    public sealed class AiResponseInstructionParseResult
    {
        public AiResponseInstruction Instruction { get; init; }
        public JObject JsonObject { get; init; }
        public Exception Exception { get; init; }
        public string ResponseContent { get; init; }
        public bool Success => Instruction != null;
    }
}
