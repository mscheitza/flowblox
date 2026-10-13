namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    internal sealed class CollectorCore
    {
        public bool CanDisplayAssociatedIterationContextHint(
            BaseFlowBlock collector,
            bool fallback)
        {
            if (collector.ReferencedFlowBlocks.OfType<BaseResultFlowBlock>().Count() == 1 &&
                collector.AssociatedIterationContext == null)
            {
                return true;
            }

            return fallback;
        }

        public BaseFlowBlock ResolveIterationContext(
            BaseFlowBlock collector,
            BaseFlowBlock fallback)
        {
            if (collector.AssociatedIterationContext != null)
                return collector.AssociatedIterationContext;

            var referencedResultFlowBlocks = collector.ReferencedFlowBlocks
                .OfType<BaseResultFlowBlock>()
                .ToList();

            if (referencedResultFlowBlocks.Count == 1)
            {
                var referencedFlowBlock = referencedResultFlowBlocks.Single();
                if (referencedFlowBlock.ReferencedFlowBlocks.Count == 1)
                    return referencedFlowBlock.ReferencedFlowBlocks.Single();

                return referencedFlowBlock.IterationContext;
            }

            return fallback;
        }
    }
}
