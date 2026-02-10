using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.AI.TokenSelector
{
    public enum TokenSelectionStrategy
    {
        [Display( Name = "ArgMax")]
        ArgMax,
        [Display(Name = "Sample")]
        Sample
    }
}
