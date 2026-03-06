using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Models.Base;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class UpdateManagedObjectHandler : ToolHandlerBase
    {
        public override string Name => "UpdateManagedObject";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Updates simple/reactive properties on a managed object via JSON-path syntax /Property/0/Nested.",
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

            var managedObject = registry.GetManagedObjects()
                .OfType<FlowBloxComponent>()
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (managedObject == null)
                return Task.FromResult(ToolHandlerUtilities.Fail($"Managed object '{name}' was not found."));

            var response = ToolHandlerUtilities.ApplyUpdates(args, managedObject, registry, "managedObject", managedObject.Name);
            return Task.FromResult(response);
        }
    }
}
