using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    public class ExpectationCondition : Condition
    {
        [Display(Name = "ExpectationCondition_ExpectationConditionTarget", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public ExpectationConditionTarget ExpectationConditionTarget { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", ExpectationConditionTarget.GetDisplayName(), base.ToString());
        }
    }
}
