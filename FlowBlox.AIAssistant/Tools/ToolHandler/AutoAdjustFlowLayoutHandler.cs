using FlowBlox.AIAssistant.Models;
using FlowBlox.Core.DependencyInjection;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.FlowBlocks;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace FlowBlox.AIAssistant.Tools
{
    internal sealed class AutoAdjustFlowLayoutHandler : ToolHandlerBase
    {
        public override string Name => "AutoAdjustFlowLayout";

        public override ToolDefinition Definition => ToolHandlerUtilities.CreateDefinition(
            Name,
            "Automatically re-centers and aligns flow blocks based on their graph connections.",
            new JObject());

        public override Task<ToolResponse> HandleAsync(JObject args, CancellationToken ct)
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            if (registry?.GetStartFlowBlock() == null)
            {
                return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
                {
                    ["updated"] = 0,
                    ["total"] = registry?.GetFlowBlocks()?.Count() ?? 0,
                    ["components"] = 0,
                    ["message"] = "No start flow block was found. Automatic alignment was not performed."
                }));
            }

            var result = FlowBlockAutoLayoutAdjuster.AdjustCurrentRegistryLayout();
            var moveActions = FlowBlockAutoLayoutAdjuster.GetRecordedMoveActions();
            FlowBloxServiceLocator.Instance
                .GetService<IFlowBloxActionHistoryService>()
                ?.RegisterAutoLayoutMoves(moveActions);

            return Task.FromResult(ToolHandlerUtilities.Ok(new JObject
            {
                ["updated"] = result.UpdatedFlowBlocks,
                ["total"] = result.TotalFlowBlocks,
                ["components"] = result.ComponentsProcessed
            }));
        }
    }
}
