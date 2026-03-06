using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetTypeKindsInfoHandler : ToolHandlerBase
    {
        public override string Name => "GetTypeKindsInfo";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns type kind metadata for FlowBloxReactiveObject types (FlowBlocks, ManagedObjects, nested ReactiveObjects) and Enums.",
            new JObject
            {
                ["typeFullName"] = "string"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var response = ToolHandlerUtilities.CreateUnifiedTypeInfoResponse(
                args.Value<string>("typeFullName"));

            return Task.FromResult(response);
        }
    }
}
