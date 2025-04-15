using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector
{
    public enum PositionSelectionStrategy
    {
        [Display(Name = "ArgMax")]
        ArgMax
    }
}
