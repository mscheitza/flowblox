using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public class ResultFieldByEnumValue<TEnum> : FlowBloxReactiveObject
        where TEnum : struct, Enum
    {
        [Display(Name = "Global_ResultField", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(
            Factory = UIFactory.Association,
            SelectionDisplayMember = nameof(FieldElement.Name),
            Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        [Required]
        public FieldElement ResultField { get; set; }

        [Required()]
        [Display(Name = "ResultFieldByEnumValue_EnumValue", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.ComboBox)]
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

        public override string ToString()
        {
            var notSetText = FlowBloxResourceUtil.GetLocalizedString("ResultFieldByEnumValue_NotSet", typeof(FlowBloxTexts));
            var format = FlowBloxResourceUtil.GetLocalizedString("ResultFieldByEnumValue_ObjectDisplayName", typeof(FlowBloxTexts));

            var enumValue = EnumValue?.ToString() ?? notSetText;
            var resultField = ResultField?.FullyQualifiedName ?? notSetText;

            return string.Format(format, enumValue, resultField);
        }
    }
}
