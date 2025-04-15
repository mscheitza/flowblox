using FlowBlox.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Models.Base;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    [Display(Name="Condition", ResourceType = typeof(FlowBloxTexts))]
    public class Condition : FlowBloxReactiveObject
    {
        private ComparisonOperator _operator;

        [Display(Name = "Condition_Operator", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public ComparisonOperator Operator
        {
            get => _operator;
            set
            {
                _operator = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        private string _value;

        [Display(Name = "Condition_Value", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(MultiLine = true)]
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        private string GetRuntimeValue()
        {
            string runtimeValue = this.Value;
            runtimeValue = FlowBloxFieldHelper.ReplaceFieldsInString(runtimeValue);
            return runtimeValue;
        }

        public bool Check(object value)
        {
            if (value is int || 
                value is int?)
            {
                return Check((int?)value);
            }
            else if (value is DateTime ||
                     value is DateTime?)
            {
                return Check((DateTime?)value);
            }
            else
            {
                return Check(value?.ToString());
            }
        }

        public bool Check(string valueToCheck)
        {
            var comparisonValue = GetRuntimeValue();
            
            // Operatoren mit möglichen null-Werten auf beiden Seiten:
            switch (Operator)
            {
                case ComparisonOperator.HasValue:
                    return !string.IsNullOrEmpty(valueToCheck);
                case ComparisonOperator.HasNoValue:
                    return string.IsNullOrEmpty(valueToCheck);
                case ComparisonOperator.Equals:
                    return valueToCheck == comparisonValue;
                case ComparisonOperator.NotEquals:
                    return valueToCheck != comparisonValue;
                case ComparisonOperator.GreaterThan:
                    return string.Compare(valueToCheck, comparisonValue) > 0;
                case ComparisonOperator.LowerThan:
                    return string.Compare(valueToCheck, comparisonValue) < 0;
                case ComparisonOperator.GreaterThanOrEquals:
                    return string.Compare(valueToCheck, comparisonValue) >= 0;
                case ComparisonOperator.LowerThanOrEquals:
                    return string.Compare(valueToCheck, comparisonValue) <= 0;
            }

            if (comparisonValue == null)
                return false;

            // Operatoren mit erlaubten null-Werten auf der linken Seite:
            switch (Operator)
            {
                case ComparisonOperator.Contains:
                    return valueToCheck != null && valueToCheck.Contains(comparisonValue);
                case ComparisonOperator.NotContains:
                    return valueToCheck == null || !valueToCheck.Contains(comparisonValue);
            }

            if (valueToCheck == null)
                return false;

            // Operatoren ohne erlaubte null-Werte auf einer von beiden Seiten:
            switch (Operator)
            {
                case ComparisonOperator.RegexIsTrue:
                    return Regex.IsMatch(valueToCheck, comparisonValue);
                case ComparisonOperator.RegexIsFalse:
                    return !Regex.IsMatch(valueToCheck, comparisonValue);
                default:
                    throw new NotSupportedException("Dieser Operator wird nicht unterstützt");
            }
        }

        public bool Check(int? valueToCheck)
        {
            int comparisonValue;
            if (!int.TryParse(GetRuntimeValue(), out comparisonValue))
                throw new InvalidOperationException("Ungültiger Vergleichswert für Integer-Operation");

            switch (Operator)
            {
                case ComparisonOperator.HasValue:
                    return valueToCheck.HasValue;
                case ComparisonOperator.HasNoValue:
                    return !valueToCheck.HasValue;
                case ComparisonOperator.Equals:
                    return valueToCheck == comparisonValue;
                case ComparisonOperator.NotEquals:
                    return valueToCheck != comparisonValue;
                case ComparisonOperator.GreaterThan:
                    return valueToCheck > comparisonValue;
                case ComparisonOperator.LowerThan:
                    return valueToCheck < comparisonValue;
                case ComparisonOperator.GreaterThanOrEquals:
                    return valueToCheck >= comparisonValue;
                case ComparisonOperator.LowerThanOrEquals:
                    return valueToCheck <= comparisonValue;
                default:
                    throw new NotSupportedException("Dieser Operator wird nicht unterstützt");
            }
        }

        public bool Check(DateTime? valueToCheck)
        {
            DateTime comparisonValue;
            if (!DateTime.TryParse(GetRuntimeValue(), out comparisonValue))
                throw new InvalidOperationException("Ungültiger Vergleichswert für DateTime-Operation");

            switch (Operator)
            {
                case ComparisonOperator.HasValue:
                    return valueToCheck.HasValue;
                case ComparisonOperator.HasNoValue:
                    return valueToCheck.HasValue;
                case ComparisonOperator.Equals:
                    return valueToCheck == comparisonValue;
                case ComparisonOperator.NotEquals:
                    return valueToCheck != comparisonValue;
                case ComparisonOperator.GreaterThan:
                    return valueToCheck > comparisonValue;
                case ComparisonOperator.LowerThan:
                    return valueToCheck < comparisonValue;
                case ComparisonOperator.GreaterThanOrEquals:
                    return valueToCheck >= comparisonValue;
                case ComparisonOperator.LowerThanOrEquals:
                    return valueToCheck <= comparisonValue;
                default:
                    throw new NotSupportedException("Dieser Operator wird nicht unterstützt");
            }
        }

        [Display(Name = "PropertyNames_Name", ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(Visible = false)]
        public virtual string DisplayName
        {
            get
            {
                if (Operator == ComparisonOperator.HasValue ||
                    Operator == ComparisonOperator.HasNoValue)
                {
                    return Operator.GetDisplayName();
                }
                else
                {
                    return $"{Operator.GetDisplayName()} \"{Value}\"";
                }
            }
        }

        public virtual string ShortDisplayName
        {
            get
            {
                return DisplayName;
            }
        }

        public override string ToString() => DisplayName;
    }
}
