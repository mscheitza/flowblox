using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Models.FlowBlocks.Calculation
{
    public static class FlowBloxMathExpressionHelper
    {
        public static string ReplaceFieldsInMathExpression(
            string expression,
            out Dictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Math expression must not be empty.", nameof(expression));

            parameters = new Dictionary<string, object>();
            int index = 0;
            foreach (var field in FlowBloxRegistryProvider.GetRegistry().GetFieldElements())
            {
                if (!expression.Contains(field.FullyQualifiedName))
                    continue;

                object value = GetMathCompatibleValue(field);

                string paramName = $"p{index}";
                parameters[paramName] = value;

                expression = expression.Replace(field.FullyQualifiedName, paramName);
                index++;
            }

            return expression;
        }

        private static object GetMathCompatibleValue(FieldElement field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (field.Value == null)
                return 0;

            var effectiveFieldType = field.FieldType?.FieldType ?? FieldTypes.Text;
            return effectiveFieldType switch
            {
                FieldTypes.Integer => Convert.ToInt32(field.Value),
                FieldTypes.Long => Convert.ToInt64(field.Value),
                FieldTypes.Float => Convert.ToSingle(field.Value),
                FieldTypes.Double => Convert.ToDouble(field.Value),
                FieldTypes.Boolean => Convert.ToBoolean(field.Value),

                FieldTypes.Text =>
                    throw new InvalidOperationException(
                        $"Field '{field.FullyQualifiedName}' has no field type or is of type Text and cannot be used in a math expression."),

                FieldTypes.ByteArray =>
                    throw new InvalidOperationException(
                        $"Field '{field.FullyQualifiedName}' has type ByteArray and cannot be used in a math expression."),

                FieldTypes.DateTime =>
                    throw new InvalidOperationException(
                        $"Field '{field.FullyQualifiedName}' has type DateTime and cannot be used in a math expression."),

                _ => throw new NotSupportedException(
                        $"FieldType '{effectiveFieldType}' is not supported in math expressions.")
            };
        }

    }

}
