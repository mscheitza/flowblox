using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Models.Base;
using System.Globalization;
using FlowBlox.Core.Extensions;
using Newtonsoft.Json;

namespace FlowBlox.Core.Models.Components
{
    [Display(Name = "TypeElement_DisplayName", ResourceType = typeof(FlowBloxTexts), Order = 0)]
    public class TypeElement : FlowBloxReactiveObject
    {
        public TypeElement()
        {
            var culture = CultureInfo.CurrentCulture;
            DecimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
            GroupSeparator = culture.NumberFormat.NumberGroupSeparator;
            DateFormat = GetDefaultDateTimeFormat(culture);
            IsNullable = true;
        }

        private static string GetDefaultDateTimeFormat(CultureInfo culture)
        {
            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            var datePart = culture.DateTimeFormat.ShortDatePattern?.Trim();
            var timePart = culture.DateTimeFormat.LongTimePattern?.Trim();

            if (string.IsNullOrWhiteSpace(datePart) && string.IsNullOrWhiteSpace(timePart))
                return "o";

            if (string.IsNullOrWhiteSpace(datePart))
                return timePart;

            if (string.IsNullOrWhiteSpace(timePart))
                return datePart;

            return $"{datePart} {timePart}";
        }

        [Required]
        [Display(Name = "TypeElement_FieldType", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public FieldTypes FieldType { get; set; }

        [ActivationCondition(MemberName = nameof(FieldType), Value = FieldTypes.DateTime)]
        [ConditionallyRequired()]
        [Display(Name = "TypeElement_DateFormat", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBloxUI(ToolboxCategory = nameof(FlowBloxToolboxCategory.Format))]
        public string DateFormat { get; set; }

        [ActivationCondition(MemberName = nameof(FieldType), Values = new object[] { FieldTypes.Float, FieldTypes.Double })]
        [ConditionallyRequired()]
        [Display(Name = "TypeElement_DecimalSeparator", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        public string DecimalSeparator { get; set; }

        [ActivationCondition(MemberName = nameof(FieldType), Values = new object[] { FieldTypes.Float, FieldTypes.Double })]
        [ConditionallyRequired()]
        [Display(Name = "TypeElement_GroupSeparator", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        public string GroupSeparator { get; set; }

        [ActivationCondition(MemberName = nameof(HasNullableInfo), Value = true)]
        [DependsOnProperty(MemberName = nameof(FieldType))]
        [Display(Name = "TypeElement_IsNullable", ResourceType = typeof(FlowBloxTexts), Order = 5)]
        public bool IsNullable { get; set; }

        public FieldElement GetReferencedField()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFieldElements()
                .FirstOrDefault(x => x.FieldType == this);
        }

        private bool IsFieldElementSet(out FieldElement fieldElement)
        {
            fieldElement = GetReferencedField();
            if (fieldElement == null || string.IsNullOrWhiteSpace(fieldElement.StringValue))
            {
                if (HasNullableInfo && !IsNullable)
                    throw new InvalidOperationException(
                        $"No value set for field of type '{FieldType}'. The field is marked as non-nullable.");

                return false;
            }

            return true;
        }

        [JsonIgnore]
        public bool? ValueBoolean
        {
            get
            {
                if (!IsFieldElementSet(out FieldElement fieldElement))
                    return default;

                if (bool.TryParse(fieldElement.StringValue.Trim(), out bool result))
                    return result;

                throw new FormatException($"Failed to convert value '{fieldElement.StringValue}' to boolean.");
            }
        }

        public NumberFormatInfo GetNumberFormat()
        {
            var referencedField = GetReferencedField();
            var fieldInfo = referencedField != null ? $" (for field '{referencedField}')" : string.Empty;

            if (string.IsNullOrWhiteSpace(DecimalSeparator))
                throw new InvalidOperationException($"Decimal separator is not defined{fieldInfo}.");

            if (string.IsNullOrWhiteSpace(GroupSeparator))
                throw new InvalidOperationException($"Group separator is not defined{fieldInfo}.");

            return new NumberFormatInfo
            {
                NumberDecimalSeparator = DecimalSeparator,
                NumberGroupSeparator = GroupSeparator
            };
        }

        [JsonIgnore]
        public int? ValueInteger
        {
            get
            {
                if (!IsFieldElementSet(out FieldElement fieldElement))
                    return default;

                if (int.TryParse(fieldElement.StringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                    return result;

                throw new FormatException($"Failed to convert value '{fieldElement.StringValue}' to integer.");
            }
        }

        [JsonIgnore]
        public long? ValueLong
        {
            get
            {
                if (!IsFieldElementSet(out FieldElement fieldElement))
                    return default;

                if (long.TryParse(fieldElement.StringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
                    return result;

                throw new FormatException($"Failed to convert value '{fieldElement.StringValue}' to long.");
            }
        }

        [JsonIgnore]
        public float? ValueFloat
        {
            get
            {
                if (!IsFieldElementSet(out FieldElement fieldElement))
                    return default;

                var format = GetNumberFormat();
                if (float.TryParse(fieldElement.StringValue, NumberStyles.Float | NumberStyles.AllowThousands, format, out float result))
                    return result;

                throw new FormatException($"Failed to convert value '{fieldElement.StringValue}' to float using decimal separator '{DecimalSeparator}' and group separator '{GroupSeparator}'.");
            }
        }

        [JsonIgnore]
        public double? ValueDouble
        {
            get
            {
                if (!IsFieldElementSet(out FieldElement fieldElement))
                    return default;

                var format = GetNumberFormat();
                if (double.TryParse(fieldElement.StringValue, NumberStyles.Float | NumberStyles.AllowThousands, format, out double result))
                    return result;

                throw new FormatException($"Failed to convert value '{fieldElement.StringValue}' to double using decimal separator '{DecimalSeparator}' and group separator '{GroupSeparator}'.");
            }
        }

        [JsonIgnore]
        public DateTime? ValueDateTime
        {
            get
            {
                if (!IsFieldElementSet(out FieldElement fieldElement))
                    return default;

                try
                {
                    return DateTime.ParseExact(fieldElement.StringValue, DateFormat, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    throw new FormatException($"Failed to convert value '{fieldElement.StringValue}' to DateTime using format '{DateFormat}'.");
                }
            }
        }

        [JsonIgnore]
        public byte[] ValueByteArray
        {
            get
            {
                if (!IsFieldElementSet(out FieldElement fieldElement))
                    return [];

                try
                {
                    return Convert.FromBase64String(fieldElement.StringValue);
                }
                catch (FormatException)
                {
                    throw new FormatException($"Failed to convert value '{fieldElement.StringValue}' to byte array (Base64 expected).");
                }
            }
        }

        [JsonIgnore]
        public bool HasNullableInfo
        {
            get
            {
                return
                    FieldType == FieldTypes.Integer ||
                    FieldType == FieldTypes.Long ||
                    FieldType == FieldTypes.Float ||
                    FieldType == FieldTypes.Double ||
                    FieldType == FieldTypes.Boolean ||
                    FieldType == FieldTypes.DateTime;
            }
        }

        public override string ToString()
        {
            var typeName = FieldType.GetDisplayName();
            string details = string.Empty;

            switch (FieldType)
            {
                case FieldTypes.DateTime:
                    details = $" (Format: {DateFormat ?? "n/a"})";
                    break;
                case FieldTypes.Double:
                case FieldTypes.Float:
                    details = $" (Decimal: '{DecimalSeparator ?? "n/a"}', Group: '{GroupSeparator ?? "n/a"}')";
                    break;
            }

            string nullableInfo = HasNullableInfo
                ? $" (Nullable: {IsNullable.ToString().ToLowerInvariant()})"
                : string.Empty;

            return $"{typeName}{details}{nullableInfo}";
        }
    }
}

