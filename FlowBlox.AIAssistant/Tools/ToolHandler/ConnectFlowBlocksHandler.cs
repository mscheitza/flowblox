using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class ConnectFlowBlocksHandler : ConnectDisconnectFlowBlocksHandlerBase
    {
        public override string Name => "ConnectFlowBlocks";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Connects from -> to.",
            new JObject
            {
                ["from"] = "string",
                ["to"] = "string"
            });

        protected override bool Connect => true;
    }
}
