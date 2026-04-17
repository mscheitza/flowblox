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
                ["typeFullName"] = "string",
                ["includeAlreadySent"] = "optional: bool (default: false). If true, properties that were already described in this session are returned with full details again."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var includeAlreadySent = args.Value<bool?>("includeAlreadySent") ?? false;
            var response = ToolHandlerUtilities.CreateUnifiedTypeInfoResponse(
                args.Value<string>("typeFullName"),
                includeAlreadySent);

            return Task.FromResult(response);
        }
    }
}
