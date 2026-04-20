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
                ["name"] = "string",
                ["usageHint"] = "Precondition: remove all blocking references first. For EnableFieldSelection string values, remove placeholders that reference fields from this flow block. " + 
                    "Unlink direct FlowBlock/ManagedObject/FieldElement references. " + 
                    "Remove obsolete reactive list entries (for example column mappings) that still reference this block/fields. " + 
                    "If delete is blocked, inspect result.blockedBy."
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
                var blockedBy = ToolHandlerUtilities.BuildBlockedBy(
                    dependencies?.Where(x => !ReferenceEquals(x, flowBlock)));

                return Task.FromResult(ToolHandlerUtilities.Fail(
                    $"FlowBlock '{name}' cannot be deleted because blocking references still exist.",
                    new JObject
                    {
                        ["deleted"] = false,
                        ["name"] = name,
                        ["blockedBy"] = blockedBy
                    }));
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
