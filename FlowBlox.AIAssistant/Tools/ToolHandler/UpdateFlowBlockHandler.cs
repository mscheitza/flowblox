using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class UpdateFlowBlockHandler : ToolHandlerBase
    {
        public override string Name => "UpdateFlowBlock";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Updates simple/reactive properties on a flow block via JSON-path syntax /Property/0/Nested. Do not update managed objects indirectly.",
            new JObject
            {
                ["name"] = "string",
                ["path"] = "string?",
                ["value"] = "any?",
                ["updates"] = "[{path,value}]?",
                ["reactiveObjectUpdateHint"] = "For FlowBloxReactiveObject-only nested types, do not send full JSON object/array as value. Use path syntax per property/index (Get-or-Create), e.g. /MappingEntries/0/ColumnName and /MappingEntries/0/Field.",
                ["resolverExamples"] = new JArray
                {
                    new JObject { ["resolveFlowBlockByName"] = "FlowBlockName" },
                    new JObject { ["resolveManagedObjectByName"] = "ManagedObjectName" },
                    new JObject { ["resolveFieldElementByFQName"] = "$FlowBlock::FieldName" }
                }
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var registry = ToolHandlerUtilities.GetRegistry();
            var name = args.Value<string>("name");

            var flowBlock = registry.GetFlowBlocks()
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (flowBlock == null)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail($"FlowBlock '{name}' was not found."));
            }

            var response = ToolHandlerUtilities.ApplyUpdates(args, flowBlock, registry, "flowBlock", flowBlock.Name);
            return Task.FromResult(response);
        }
    }
}
