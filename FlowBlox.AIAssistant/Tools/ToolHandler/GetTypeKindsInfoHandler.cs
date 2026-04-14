using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
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
                ["excludeBaseTypes"] = "optional: string[] (base type full names to skip inherited members)",
                ["usageHint"] =
                    $"Best practice for traffic reduction: fetch base kinds first (FlowBlock: '{typeof(BaseFlowBlock).FullName}', Managed Object: '{typeof(ManagedObject).FullName}'), " +
                    $"then call specific kinds with excludeBaseTypes to skip already known inherited members."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var excludedBaseTypes = ToolHandlerUtilities.ResolveExcludedBaseTypes(
                args["excludeBaseTypes"] ?? args["excludeBaseType"]);
            var response = ToolHandlerUtilities.CreateUnifiedTypeInfoResponse(
                args.Value<string>("typeFullName"),
                excludedBaseTypes);

            return Task.FromResult(response);
        }
    }
}
