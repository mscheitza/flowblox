using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FlowBlox.Core.Attributes
{
    /// <summary>
    /// Requires the decorated property when another property has the configured value.
    /// Unlike <see cref="ActivationConditionAttribute"/>, this attribute only affects
    /// validation and does not change the property's UI activation state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredIfAttribute : ValidationAttribute
    {
        public RequiredIfAttribute(string memberName, object value)
        {
            MemberName = memberName;
            Value = value;
        }

        public string MemberName { get; }

        public object Value { get; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var conditionProperty = validationContext.ObjectType.GetProperty(
                MemberName,
                BindingFlags.Public | BindingFlags.Instance);

            if (conditionProperty == null)
            {
                throw new InvalidOperationException(
                    $"RequiredIf member '{MemberName}' was not found on type '{validationContext.ObjectType.FullName}'.");
            }

            var conditionValue = conditionProperty.GetValue(validationContext.ObjectInstance);
            if (!Equals(conditionValue, Value) || !string.IsNullOrWhiteSpace(value?.ToString()))
                return ValidationResult.Success;

            var displayName = TryGetDisplayName(validationContext) ?? validationContext.MemberName;
            var message = string.Format(
                FlowBloxResourceUtil.GetLocalizedString(
                    "RequiredIfAttribute_ValidationFailed_Message",
                    typeof(FlowBloxTexts)),
                displayName);

            return new ValidationResult(message, [validationContext.MemberName!]);
        }

        private static string? TryGetDisplayName(ValidationContext validationContext)
        {
            try
            {
                return validationContext.DisplayName;
            }
            catch (InvalidOperationException)
            {
                // Kann auftreten, wenn ein DisplayAttribute auf einen nicht vorhandenen Resource-Key verweist.
                return null;
            }
        }
    }
}
