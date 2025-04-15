using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
