using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class GetComponentSnapshotHandler : ToolHandlerBase
    {
        public override string Name => "GetComponentSnapshot";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Returns a lightweight JSON snapshot for a specific component. Use only for targeted follow-up steps (for example: after linking a test definition to inspect generated entries) to avoid unnecessary traffic.",
            new JObject
            {
                ["kind"] = "string (flowBlock|managedObject|field)",
                ["name"] = "string",
                ["usageHint"] = "Null properties are omitted in snapshot output to reduce traffic. If a property is missing, treat it as null."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var kind = args.Value<string>("kind")?.Trim();
            var name = args.Value<string>("name")?.Trim();

            if (string.IsNullOrWhiteSpace(kind))
                return Task.FromResult(ToolHandlerUtilities.Fail("kind is required."));
            if (string.IsNullOrWhiteSpace(name))
                return Task.FromResult(ToolHandlerUtilities.Fail("name is required."));

            var registry = ToolHandlerUtilities.GetRegistry();
            FlowBloxReactiveObject? component = null;
            var normalizedKind = kind.ToLowerInvariant();
            string componentName;

            switch (normalizedKind)
            {
                case "flowblock":
                    component = ToolHandlerUtilities.ResolveFlowBlockByName(registry, name);
                    componentName = (component as BaseFlowBlock)?.Name ?? name;
                    break;
                case "managedobject":
                    component = ToolHandlerUtilities.ResolveManagedObjectByName(registry, name) as FlowBloxReactiveObject;
                    componentName = (component as IManagedObject)?.Name ?? name;
                    break;
                case "field":
                    component = ToolHandlerUtilities.ResolveFieldElementByFQName(registry, name);
                    componentName = (component as FieldElement)?.FullyQualifiedName ?? name;
                    break;
                default:
                    return Task.FromResult(ToolHandlerUtilities.Fail("kind must be 'flowBlock', 'managedObject', or 'field'."));
            }

            if (component == null)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail($"{kind} '{name}' was not found."));
            }

            var snapshotJson = JsonConvert.SerializeObject(component, JsonSettings.ComponentSnapshotExport());
            var snapshotToken = JToken.Parse(snapshotJson);

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["kind"] = normalizedKind,
                ["name"] = componentName,
                ["typeFullName"] = component.GetType().FullName,
                ["snapshot"] = snapshotToken
            }));
        }
    }
}
