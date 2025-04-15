using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector
{
    public static class TokenSelectorFactory
    {
        public static INextTokenSelector Create(TokenSelectionStrategy strategy)
        {
            return strategy switch
            {
                TokenSelectionStrategy.ArgMax => new ArgMaxTokenSelector(),
                TokenSelectionStrategy.Sample => new SampleTokenSelector(),
                _ => throw new NotImplementedException($"Strategy '{strategy}' is not implemented.")
            };
        }
    }
}
