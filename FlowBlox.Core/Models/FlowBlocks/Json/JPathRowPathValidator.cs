using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Json
{
    public static class JPathRowPathValidator
    {
        public static ValidationResult ValidateRowPath(string rowPath, ValidationContext validationContext)
        {
            var fallbackProperty = validationContext.ObjectType.GetProperty("FallbackValue");
            var fallbackValue = fallbackProperty?.GetValue(validationContext.ObjectInstance)?.ToString();
            if (string.IsNullOrWhiteSpace(fallbackValue) || !string.IsNullOrWhiteSpace(rowPath))
                return ValidationResult.Success;

            var message = string.Format(
                FlowBloxResourceUtil.GetLocalizedString(
                    "RequiredIfAttribute_ValidationFailed_Message",
                    typeof(FlowBloxTexts)),
                validationContext.DisplayName);
            return new ValidationResult(message, [validationContext.MemberName]);
        }
    }
}
