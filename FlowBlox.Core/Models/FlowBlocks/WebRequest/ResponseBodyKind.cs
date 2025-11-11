using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.WebRequest
{
    public enum ResponseBodyKind
    {
        [Display(Name = "Empty")]
        Empty = 0,

        [Display(Name = "Text")]
        Text = 1,

        [Display(Name = "Bytes")]
        Bytes = 2
    }
}