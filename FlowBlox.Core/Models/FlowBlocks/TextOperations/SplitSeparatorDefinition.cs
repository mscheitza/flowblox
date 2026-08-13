using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.TextOperations
{
    public class SplitSeparatorDefinition : FlowBloxReactiveObject
    {
        [Display(Name = "SplitSeparatorDefinition_Separator", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [CustomValidation(typeof(SplitSeparatorDefinition), nameof(ValidateSeparator))]
        public string Separator { get; set; }

        [Display(Name = "SplitSeparatorDefinition_SpecialSeparator", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.ComboBox)]
        [CustomValidation(typeof(SplitSeparatorDefinition), nameof(ValidateSeparator))]
        public SpecialSeparator? SpecialSeparator { get; set; }

        public string ResolveSeparator()
        {
            if (SpecialSeparator == Enums.SpecialSeparator.Tab)
                return "\t";

            if (SpecialSeparator == Enums.SpecialSeparator.NewLine)
                return Environment.NewLine;

            return Separator;
        }

        public static ValidationResult ValidateSeparator(object value, ValidationContext validationContext)
        {
            var separatorDefinition = (SplitSeparatorDefinition)validationContext.ObjectInstance;

            if (string.IsNullOrEmpty(separatorDefinition.Separator) && !separatorDefinition.SpecialSeparator.HasValue)
                return new ValidationResult(FlowBloxResourceUtil.GetLocalizedString("SplitSeparatorDefinition_Validation_NoSeparatorDefined"), [validationContext.MemberName]);

            if (!string.IsNullOrEmpty(separatorDefinition.Separator) && separatorDefinition.SpecialSeparator.HasValue)
                return new ValidationResult(FlowBloxResourceUtil.GetLocalizedString("SplitSeparatorDefinition_Validation_ManySeparatorsDefined"), [validationContext.MemberName]);

            return ValidationResult.Success;
        }
    }
}
