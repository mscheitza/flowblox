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
                ["name"] = "string"
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
                var dependencyNames = string.Join(", ", dependencies.Select(x => x.Name));
                return Task.FromResult(ToolHandlerUtilities.Fail($"Managed object '{name}' cannot be deleted. Dependencies: {dependencyNames}"));
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
