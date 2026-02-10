using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public class ResultFieldByEnumValue<TEnum>
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
    }
}
