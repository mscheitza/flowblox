using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _propertyName;
        private readonly object _desiredValue;

        public RequiredIfAttribute(string propertyName, object desiredValue)
        {
            _propertyName = propertyName;
            _desiredValue = desiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_propertyName);
            if (property == null)
                return new ValidationResult($"Unknown property: {_propertyName}", [validationContext.MemberName]);

            var propertyValue = property.GetValue(validationContext.ObjectInstance);

            if (propertyValue != null && propertyValue.Equals(_desiredValue))
            {
                if (value == null)
                {
                    return new ValidationResult(ErrorMessage, [validationContext.MemberName]);
                }
            }

            return ValidationResult.Success;
        }
    }
}
