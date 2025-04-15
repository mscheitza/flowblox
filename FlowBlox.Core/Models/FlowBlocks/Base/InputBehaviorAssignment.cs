using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    [Serializable()]
    public class InputBehaviorAssignment : FlowBloxReactiveObject
    {
        public InputBehaviorAssignment()
        {
            this.Behavior = InputBehavior.Cross;
        }

        [Required()]
        [Display(Name = "Global_FlowBlock", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ComboBox, ReadOnly = true)]
        public BaseFlowBlock FlowBlock { get; set; }

        [Display(Name = "InputBehaviorAssignment_Behavior", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public InputBehavior Behavior { get; set; }
    }
}
