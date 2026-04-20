using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class DeleteManagedObjectHandler : ToolHandlerBase
    {
        public override string Name => "DeleteManagedObject";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Deletes managed object.",
            new JObject
            {
                ["name"] = "string",
                ["usageHint"] = "Precondition: remove all blocking references first. For EnableFieldSelection string values, remove placeholders that reference this managed object or its fields. Unlink direct FlowBlock/ManagedObject/FieldElement references. Remove obsolete reactive list entries that still depend on it. If delete is blocked, inspect result.blockedBy."
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var registry = ToolHandlerUtilities.GetRegistry();
            var name = args.Value<string>("name");

            var managedObject = registry.GetManagedObjects()
                .OfType<FlowBloxComponent>()
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (managedObject == null)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail($"Managed object '{name}' was not found."));
            }

            if (!managedObject.IsDeletable(out var dependencies))
            {
                var blockedBy = ToolHandlerUtilities.BuildBlockedBy(
                    dependencies?.Where(x => !ReferenceEquals(x, managedObject)));

                return Task.FromResult(ToolHandlerUtilities.Fail(
                    $"Managed object '{name}' cannot be deleted because blocking references still exist.",
                    new JObject
                    {
                        ["deleted"] = false,
                        ["name"] = name,
                        ["blockedBy"] = blockedBy
                    }));
            }

            registry.RemoveManagedbject((IManagedObject)managedObject);

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["deleted"] = true,
                ["name"] = name
            }));
        }
    }
}
