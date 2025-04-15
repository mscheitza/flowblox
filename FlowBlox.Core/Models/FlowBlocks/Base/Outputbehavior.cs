using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public enum OutputBehavior
    {
        [Display(Name = "OutputBehavior_All", ResourceType = typeof(FlowBloxTexts))]
        All,
        [Display(Name = "OutputBehavior_First", ResourceType = typeof(FlowBloxTexts))]
        First,
        [Display(Name = "OutputBehavior_Last", ResourceType = typeof(FlowBloxTexts))]
        Last,
        [Display(Name = "OutputBehavior_Range", ResourceType = typeof(FlowBloxTexts))]
        Range
    }
}
