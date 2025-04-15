using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Util.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ActivationConditionAttribute : ValidationAttribute
    {
        public string MemberName { get; set; }

        public object Value { get; set; }

        public object[] Values { get; set; }

        public bool IsRequired { get; set; }

        public string ActivationMethod { get; set; }

        public bool IsActive(object target)
        {
            if (!string.IsNullOrWhiteSpace(ActivationMethod))
            {
                var method = target.GetType().GetMethod(ActivationMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                    throw new InvalidOperationException($"Activation method '{ActivationMethod}' not found on type '{target.GetType().FullName}'.");

                if (method.ReturnType != typeof(bool) || method.GetParameters().Length > 0)
                    throw new InvalidOperationException($"Activation method '{ActivationMethod}' must return bool and have no parameters.");

                var result = (bool)method.Invoke(target, null);
                return result;
            }
            else
            {
                var property = target.GetType().GetProperty(MemberName);

                if (property == null)
                    return false;

                var propertyValue = property.GetValue(target);

                if (propertyValue == null)
                    return false;

                if (Values != null && Values.Length > 0)
                    return Values.Contains(propertyValue);

                return propertyValue.Equals(Value);
            }
        }

        private string GetIsRequiredValidationMessage(string displayName)
        {
            return string.Format(FlowBloxResourceUtil.GetLocalizedString("ActivationConditionAttribute_Messages_IsRequired"), displayName);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (IsRequired &&
                IsActive(validationContext.ObjectInstance))
            {
                // IsRequired
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return new ValidationResult(GetIsRequiredValidationMessage(validationContext.DisplayName), [validationContext.MemberName]);
            }
            return ValidationResult.Success;
        }
    }
}
