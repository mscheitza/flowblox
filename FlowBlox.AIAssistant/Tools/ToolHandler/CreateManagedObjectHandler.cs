using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.FlowBlocks.Base;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class CreateManagedObjectHandler : ToolHandlerBase
    {
        public override string Name => "CreateManagedObject";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Creates managed object.",
            new JObject
            {
                ["typeFullName"] = "string",
                ["name"] = "string?"
            });

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var type = ToolHandlerUtilities.ResolveType(args.Value<string>("typeFullName"));
            if (type == null
                || !typeof(IManagedObject).IsAssignableFrom(type)
                || typeof(BaseFlowBlock).IsAssignableFrom(type)
                || type.IsAbstract)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail("typeFullName invalid."));
            }

            var managedObject = Activator.CreateInstance(type) as IManagedObject;
            if (managedObject == null)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail("Could not create managed object."));
            }

            var requestedName = args.Value<string>("name");
            if (managedObject is FlowBloxComponent component && !string.IsNullOrWhiteSpace(requestedName))
            {
                component.Name = requestedName;
            }

            var registry = ToolHandlerUtilities.GetRegistry();
            registry.PostProcessManagedObjectCreated(managedObject);
            registry.RegisterManagedObject(managedObject);

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["created"] = true,
                ["name"] = (managedObject as FlowBloxComponent)?.Name ?? string.Empty,
                ["typeFullName"] = managedObject.GetType().FullName
            }));
        }
    }
}
