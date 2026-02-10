using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Util
{
    public static class ValidationUtil
    {
        public static bool ValidateProperty<T>(T instance, string propertyName, out string message)
        {
            message = string.Empty;

            var propertyInfo = typeof(T).GetProperty(propertyName);

            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property {propertyName} not found");
            }

            var value = propertyInfo.GetValue(instance);
            var validationContext = new ValidationContext(instance) { MemberName = propertyName };
            var results = new System.Collections.Generic.List<ValidationResult>();

            bool isValid = Validator.TryValidateProperty(value, validationContext, results);

            if (!isValid && results.Any())
            {
                message = results.First().ErrorMessage;
            }

            return isValid;
        }
    }
}
