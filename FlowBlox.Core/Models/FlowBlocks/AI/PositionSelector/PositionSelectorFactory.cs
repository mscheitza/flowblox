using FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector;

namespace FlowBlox.Core.Models.FlowBlocks.AI.PositionSelector
{
    public static class PositionSelectorFactory
    {
        public static ISequencePositionSelector CreatePositionSelector(PositionSelectionStrategy strategy)
        {
            return strategy switch
            {
                PositionSelectionStrategy.ArgMax => new ArgMaxPositionSelector(),
                _ => throw new NotImplementedException()
            };
        }
    }
}
