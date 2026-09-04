using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Events
{
    public sealed class ReferencedFlowBlocksChangedEventArgs : EventArgs
    {
        public ReferencedFlowBlocksChangedEventArgs(
            BaseFlowBlock target,
            IReadOnlyCollection<BaseFlowBlock> addedFlowBlocks,
            IReadOnlyCollection<BaseFlowBlock> removedFlowBlocks)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            AddedFlowBlocks = addedFlowBlocks ?? Array.Empty<BaseFlowBlock>();
            RemovedFlowBlocks = removedFlowBlocks ?? Array.Empty<BaseFlowBlock>();
        }

        public BaseFlowBlock Target { get; }

        public IReadOnlyCollection<BaseFlowBlock> AddedFlowBlocks { get; }

        public IReadOnlyCollection<BaseFlowBlock> RemovedFlowBlocks { get; }

        public IEnumerable<BaseFlowBlock> AffectedFlowBlocks =>
            new[] { Target }
                .Concat(AddedFlowBlocks)
                .Concat(RemovedFlowBlocks)
                .Where(x => x != null)
                .Distinct();
    }
}
