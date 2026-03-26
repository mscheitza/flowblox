using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks;
using FlowBlox.Core.Models.FlowBlocks.Base;
using Newtonsoft.Json.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal abstract class ConnectDisconnectFlowBlocksHandlerBase : ToolHandlerBase
    {
        protected abstract bool Connect { get; }

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var registry = ToolHandlerUtilities.GetRegistry();
            var fromName = args.Value<string>("from");
            var toName = args.Value<string>("to");

            var from = registry.GetFlowBlocks()
                .FirstOrDefault(x => string.Equals(x.Name, fromName, StringComparison.OrdinalIgnoreCase));
            var to = registry.GetFlowBlocks()
                .FirstOrDefault(x => string.Equals(x.Name, toName, StringComparison.OrdinalIgnoreCase));

            if (from == null || to == null)
            {
                return Task.FromResult(ToolHandlerUtilities.Fail("from/to flow block was not found."));
            }

            if (Connect)
            {
                if (!CanConnect(from, to, out var canConnectReason))
                {
                    return Task.FromResult(ToolHandlerUtilities.Fail(canConnectReason));
                }

                if (from is RecursiveCallFlowBlock recursiveCallFrom)
                {
                    var wasChanged = !ReferenceEquals(recursiveCallFrom.TargetFlowBlock, to);
                    recursiveCallFrom.TargetFlowBlock = to;

                    return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
                    {
                        ["connected"] = true,
                        ["mode"] = "invoke",
                        ["changed"] = wasChanged,
                        ["from"] = from.Name,
                        ["to"] = to.Name
                    }));
                }

                var alreadyConnected = to.ReferencedFlowBlocks.Contains(from);
                if (!alreadyConnected)
                {
                    to.ReferencedFlowBlocks.Add(from);
                }

                return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
                {
                    ["connected"] = true,
                    ["mode"] = "reference",
                    ["alreadyConnected"] = alreadyConnected,
                    ["from"] = from.Name,
                    ["to"] = to.Name
                }));
            }

            if (from is RecursiveCallFlowBlock recursiveCallFromOnDisconnect)
            {
                var disconnectedInvoke = ReferenceEquals(recursiveCallFromOnDisconnect.TargetFlowBlock, to);
                if (disconnectedInvoke)
                {
                    recursiveCallFromOnDisconnect.TargetFlowBlock = null;
                }

                return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
                {
                    ["disconnected"] = true,
                    ["mode"] = "invoke",
                    ["wasConnected"] = disconnectedInvoke,
                    ["from"] = from.Name,
                    ["to"] = to.Name
                }));
            }

            var wasConnected = to.ReferencedFlowBlocks.Contains(from);
            if (wasConnected)
            {
                to.ReferencedFlowBlocks.Remove(from);
            }

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["disconnected"] = true,
                ["mode"] = "reference",
                ["wasConnected"] = wasConnected,
                ["from"] = from.Name,
                ["to"] = to.Name
            }));
        }

        private static bool CanConnect(BaseFlowBlock from, BaseFlowBlock to, out string reason)
        {
            reason = string.Empty;

            if (from == to)
            {
                reason = "Cannot connect a flow block to itself.";
                return false;
            }

            if (to.GetInputCardinality() == FlowBlockCardinalities.None)
            {
                reason = $"Flow block '{to.Name}' does not accept input references.";
                return false;
            }

            if (to.GetInputCardinality() == FlowBlockCardinalities.One &&
                to.ReferencedFlowBlocks.Count > 0 &&
                !to.ReferencedFlowBlocks.Contains(from))
            {
                reason = $"Flow block '{to.Name}' allows only one input reference.";
                return false;
            }

            if (to.ReferencedFlowBlocks.Contains(from))
            {
                reason = $"Flow block '{to.Name}' is already connected to '{from.Name}'.";
                return false;
            }

            if (from.ReferencedFlowBlocks.Contains(to))
            {
                reason = $"Reverse edge exists ('{from.Name}' <- '{to.Name}'); two-node cycle is not allowed.";
                return false;
            }

            return true;
        }
    }
}
