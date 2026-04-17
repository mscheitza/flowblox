using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestDefinitionLatestFlowBlockResolver
    {
        private readonly Lazy<FlowBloxRegistry> _registry;

        public FlowBloxTestDefinitionLatestFlowBlockResolver()
        {
            _registry = new Lazy<FlowBloxRegistry>(FlowBloxRegistryProvider.GetRegistry);
        }

        public BaseFlowBlock ResolveLatestFlowBlock(
            FlowBloxTestDefinition testDefinition,
            IEnumerable<BaseFlowBlock> orderedFlowBlocks)
        {
            if (testDefinition == null)
                throw new ArgumentNullException(nameof(testDefinition));

            if (orderedFlowBlocks == null)
                throw new ArgumentNullException(nameof(orderedFlowBlocks));

            var orderedFlowBlockList = orderedFlowBlocks.ToList();
            if (!orderedFlowBlockList.Any())
                return null;

            var testDefinitionFlowBlocks = testDefinition.Entries
                .Where(x => x.FlowBlock != null)
                .Select(x => x.FlowBlock)
                .Distinct()
                .ToList();

            if (!testDefinitionFlowBlocks.Any())
                return null;

            var orderedIndexByFlowBlock = orderedFlowBlockList
                .Select((flowBlock, index) => new { flowBlock, index })
                .ToDictionary(x => x.flowBlock, x => x.index);

            return testDefinitionFlowBlocks
                .Where(flowBlock => orderedIndexByFlowBlock.ContainsKey(flowBlock))
                .OrderBy(flowBlock => orderedIndexByFlowBlock[flowBlock])
                .LastOrDefault();
        }

        public BaseFlowBlock ResolveLatestFlowBlock(FlowBloxTestDefinition testDefinition)
        {
            if (testDefinition == null)
                throw new ArgumentNullException(nameof(testDefinition));

            var orderedFlowBlocks = _registry.Value
                .GetFlowBlocksRecursiveOrderedByExecutionFlow(_registry.Value.GetStartFlowBlock());

            return ResolveLatestFlowBlock(testDefinition, orderedFlowBlocks);
        }
    }
}