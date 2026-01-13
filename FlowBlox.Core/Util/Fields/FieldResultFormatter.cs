using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using System;
using System.Globalization;

namespace FlowBlox.Core.Util.Fields
{
    public static class FieldResultFormatter
    {
        public static string FormatResult(FieldElement resultField, object value)
        {
            if (resultField == null)
                throw new ArgumentNullException(nameof(resultField));

            if (value == null)
                return string.Empty;

            var typeElement = resultField.FieldType;
            var efectiveFieldType = typeElement?.FieldType ?? FieldTypes.Text;
            return efectiveFieldType switch
            {
                FieldTypes.Integer => FormatInteger(value),
                FieldTypes.Long => FormatLong(value),
                FieldTypes.Float => FormatFloat(typeElement, value),
                FieldTypes.Double => FormatDouble(typeElement, value),
                FieldTypes.Boolean => FormatBoolean(value),
                FieldTypes.DateTime => FormatDateTime(typeElement, value),
                FieldTypes.Text => value.ToString(),
                _ => value.ToString()
            };
        }

        private static string FormatInteger(object value)
        {
            return Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatLong(object value)
        {
            return Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatBoolean(object value)
        {
            return Convert.ToBoolean(value).ToString().ToLowerInvariant();
        }

        private static string FormatFloat(TypeElement typeElement, object value)
        {
            var format = typeElement.GetNumberFormat();
            return Convert.ToSingle(value).ToString(format);
        }

        private static string FormatDouble(TypeElement typeElement, object value)
        {
            var format = typeElement.GetNumberFormat();
            return Convert.ToDouble(value).ToString(format);
        }

        private static string FormatDateTime(TypeElement typeElement, object value)
        {
            if (typeElement == null)
                throw new InvalidOperationException("DateTime formatting requires a TypeElement configuration.");

            if (string.IsNullOrWhiteSpace(typeElement.DateFormat))
                throw new InvalidOperationException("Date format is not defined for the result field.");

            DateTime dt = value switch
            {
                DateTime d => d,
                _ => Convert.ToDateTime(value, CultureInfo.InvariantCulture)
            };

            return dt.ToString(typeElement.DateFormat, CultureInfo.InvariantCulture);
        }
    }
}
