using FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
