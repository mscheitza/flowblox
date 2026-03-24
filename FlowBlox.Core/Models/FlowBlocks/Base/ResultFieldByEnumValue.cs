using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public class ResultFieldByEnumValue<TEnum> : FlowBloxReactiveObject
        where TEnum : struct, Enum
    {
        [Display(Name = "Global_ResultField", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(
            Factory = UIFactory.Association,
            SelectionDisplayMember = nameof(FieldElement.Name),
            Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        [Required]
        public FieldElement ResultField { get; set; }

        [Required()]
        [Display(Name = "ResultFieldByEnumValue_EnumValue", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public TEnum? EnumValue { get; set; }

        public bool IsDeletable(out List<IFlowBloxComponent> dependencies)
        {
            if (ResultField == null)
            {
                dependencies = null;
                return true;
            }

            return ResultField.IsDeletable(out dependencies);
        }
    }
}
