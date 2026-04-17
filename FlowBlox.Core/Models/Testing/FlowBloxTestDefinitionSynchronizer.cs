using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;

namespace FlowBlox.Core.Models.Testing
{
    public class FlowBloxTestDefinitionSynchronizer
    {
        private readonly Lazy<FlowBloxRegistry> _registry;
        private readonly FlowBloxTestDefinitionLatestFlowBlockResolver _latestResolver;

        public FlowBloxTestDefinitionSynchronizer()
        {
            _registry = new Lazy<FlowBloxRegistry>(FlowBloxRegistryProvider.GetRegistry);
            _latestResolver = new FlowBloxTestDefinitionLatestFlowBlockResolver();
        }

        public void Synchronize(FlowBloxTestDefinition testDefinition, BaseFlowBlock currentFlowBlock = null)
        {
            if (testDefinition == null)
                throw new ArgumentNullException(nameof(testDefinition));

            var orderedFlowBlocks = _registry.Value
                .GetFlowBlocksRecursiveOrderedByExecutionFlow(_registry.Value.GetStartFlowBlock())
                .ToList();

            var targetFlowBlock = ResolveTargetFlowBlock(testDefinition, currentFlowBlock, orderedFlowBlocks);

            AppendUserFields(testDefinition);

            if (targetFlowBlock == null)
                return;

            var capturedFlowBlocks = CaptureFlowBlocksUpToTarget(targetFlowBlock);
            var setExecuteForCurrentFlowBlock = currentFlowBlock != null;

            SynchronizeEntries(testDefinition, capturedFlowBlocks, orderedFlowBlocks, targetFlowBlock, setExecuteForCurrentFlowBlock);
            AppendConfigurations(testDefinition, capturedFlowBlocks);
        }

        private BaseFlowBlock ResolveTargetFlowBlock(FlowBloxTestDefinition testDefinition, BaseFlowBlock currentFlowBlock, List<BaseFlowBlock> orderedFlowBlocks)
        {
            if (currentFlowBlock != null)
                return currentFlowBlock;

            return _latestResolver.ResolveLatestFlowBlock(testDefinition, orderedFlowBlocks);
        }

        private List<BaseFlowBlock> CaptureFlowBlocksUpToTarget(BaseFlowBlock targetFlowBlock)
        {
            var flowBloxCapture = new FlowBloxTestCapture();
            flowBloxCapture.CreateCapture(_registry.Value.GetStartFlowBlock(), targetFlowBlock);
            return flowBloxCapture.GetCapturedFlowBlocks() ?? new List<BaseFlowBlock>();
        }

        private void SynchronizeEntries(
            FlowBloxTestDefinition testDefinition,
            List<BaseFlowBlock> capturedFlowBlocks,
            List<BaseFlowBlock> orderedFlowBlocks,
            BaseFlowBlock targetFlowBlock,
            bool setExecuteForCurrentFlowBlock)
        {
            if (capturedFlowBlocks == null)
                throw new ArgumentNullException(nameof(capturedFlowBlocks));

            if (orderedFlowBlocks == null)
                throw new ArgumentNullException(nameof(orderedFlowBlocks));

            var orderedIndexByFlowBlock = orderedFlowBlocks
                .Select((flowBlock, index) => new { flowBlock, index })
                .ToDictionary(x => x.flowBlock, x => x.index);

            foreach (var flowBlock in capturedFlowBlocks)
            {
                EnsureEntryExists(testDefinition, flowBlock);
            }

            var reorderedEntries = testDefinition.Entries
                .Where(x => x.FlowBlock != null)
                .OrderBy(x =>
                {
                    if (orderedIndexByFlowBlock.TryGetValue(x.FlowBlock, out var directIndex))
                        return directIndex;

                    var originalRef = _registry.Value.GetOriginalRef(x.FlowBlock) as BaseFlowBlock;
                    if (originalRef != null && orderedIndexByFlowBlock.TryGetValue(originalRef, out var originalIndex))
                        return originalIndex;

                    return int.MaxValue;
                })
                .ToList();

            var nonFlowBlockEntries = testDefinition.Entries
                .Where(x => x.FlowBlock == null)
                .ToList();

            testDefinition.Entries.Clear();

            foreach (var nonFlowBlockEntry in nonFlowBlockEntries)
            {
                testDefinition.Entries.Add(nonFlowBlockEntry);
            }

            foreach (var reorderedEntry in reorderedEntries)
            {
                if (setExecuteForCurrentFlowBlock && reorderedEntry.FlowBlock == targetFlowBlock)
                    reorderedEntry.Execute = true;

                testDefinition.Entries.Add(reorderedEntry);
            }
        }

        private FlowBlockTestDataset EnsureEntryExists(FlowBloxTestDefinition testDefinition, BaseFlowBlock flowBlock)
        {
            var entry = testDefinition.Entries.SingleOrDefault(x => x.FlowBlock == flowBlock);
            if (entry != null)
                return entry;

            var sameOriginalReferenceEntry = testDefinition.Entries
                .FirstOrDefault(x => x.FlowBlock == _registry.Value.GetOriginalRef(flowBlock));

            var sameNameEntry = testDefinition.Entries.FirstOrDefault(x =>
                x.FlowBlock != null &&
                string.Equals(x.FlowBlock.Name, flowBlock.Name, StringComparison.Ordinal));

            if (sameOriginalReferenceEntry != null || sameNameEntry != null)
            {
                var detectedCases = new List<string>();
                if (sameOriginalReferenceEntry != null)
                    detectedCases.Add("same original reference");
                if (sameNameEntry != null)
                    detectedCases.Add("same flow-block name");

                throw new InvalidOperationException(
                    $"Flow-block could not be resolved by direct reference, but a flow-block with {string.Join(" and ", detectedCases)} was found. " +
                    $"Flow-block: \"{flowBlock.Name}\".");
            }

            entry = new FlowBlockTestDataset()
            {
                ParentTestDefinition = testDefinition,
                FlowBlock = flowBlock,
                Execute = false,
                FlowBloxTestConfigurations = new List<FlowBloxFieldTestConfiguration>()
            };

            testDefinition.Entries.Add(entry);
            return entry;
        }

        private void AppendUserFields(FlowBloxTestDefinition testDefinition)
        {
            foreach (var userField in _registry.Value.GetUserFields())
            {
                var entry = testDefinition.Entries.SingleOrDefault(x => x.FlowBlock == null);
                if (entry == null)
                {
                    entry = new FlowBlockTestDataset()
                    {
                        ParentTestDefinition = testDefinition,
                        FlowBloxTestConfigurations = new List<FlowBloxFieldTestConfiguration>()
                    };

                    testDefinition.Entries.Add(entry);
                }

                if (!entry.FlowBloxTestConfigurations.Any(x => x.FieldElement == userField))
                {
                    entry.FlowBloxTestConfigurations.Add(new FlowBloxFieldTestConfiguration()
                    {
                        FieldElement = userField
                    });
                }
            }
        }

        private void AppendConfigurations(FlowBloxTestDefinition testDefinition, List<BaseFlowBlock> capturedFlowBlocks)
        {
            foreach (var flowBlock in capturedFlowBlocks)
            {
                var entry = testDefinition.Entries.SingleOrDefault(x => x.FlowBlock == flowBlock);
                if (entry == null)
                    continue;

                if (flowBlock is BaseResultFlowBlock resultFlowBlock)
                {
                    foreach (var field in resultFlowBlock.Fields)
                    {
                        if (!entry.FlowBloxTestConfigurations.Any(x => x.FieldElement == field))
                        {
                            entry.FlowBloxTestConfigurations.Add(new FlowBloxFieldTestConfiguration()
                            {
                                FieldElement = field
                            });
                        }
                    }
                }
            }
        }
    }
}