using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetTypeKindsInfoHandler : ToolHandlerBase
    {
        public override string Name => "GetTypeKindsInfo";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns compact type kind metadata for FlowBloxReactiveObject types (FlowBlocks, ManagedObjects, nested ReactiveObjects) and Enums. Property flags: N=nullable, W=writable, S=simple, E=enum, C=collection.",
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