using FlowBlox.AIAssistant.Models;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class DeleteFlowBlockHandler : ToolHandlerBase
    {
        public override string Name => "DeleteFlowBlock";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Deletes flow block by name.",
            new JObject
            {
                ["name"] = "string"
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

            if (!flowBlock.IsDeletable(out var dependencies))
            {
                var dependencyNames = string.Join(", ", dependencies.Select(x => x.Name));
                return Task.FromResult(ToolHandlerUtilities.Fail($"FlowBlock '{name}' cannot be deleted. Dependencies: {dependencyNames}"));
            }

            registry.RemoveFlowBlock(flowBlock);

            foreach (var other in registry.GetFlowBlocks())
            {
                if (other.ReferencedFlowBlocks.Contains(flowBlock))
                {
                    other.ReferencedFlowBlocks.Remove(flowBlock);
                }
            }

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["deleted"] = true,
                ["name"] = name
            }));
        }
    }
}
