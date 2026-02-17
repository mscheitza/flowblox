using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.DeepCopier;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    // TODO: Wichtig!
    // Beim Close manchmal eine Null-Reference Exception in Group-Viewn
    // Wenn man einfach 2 Elemente anlegt oder Field dann kommen Exceptions beim Speichern (Conditions ohne Field)

    [Display(Name = "LogicalComparisonCondition_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public abstract class LogicalComparisonCondition : LogicalCondition
    {
        private ComparisonOperator _operator;
        private string _value;

        [JsonIgnore]
        [DeepCopierIgnore]
        internal ComparisonCore ComparisonCore { get; } = new ComparisonCore();

        [Display(Name = "ComparisonCondition_Operator", ResourceType = typeof(FlowBloxTexts), Order = 10)]
        public ComparisonOperator Operator
        {
            get => _operator;
            set
            {
                _operator = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(ShortDisplayName));
            }
        }

        [Display(Name = "ComparisonCondition_Value", ResourceType = typeof(FlowBloxTexts), Order = 20)]
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
                OnPropertyChanged(nameof(ShortDisplayName));
            }
        }

        public virtual bool Compare(object leftValue) => ComparisonCore.Check(Operator, Value, leftValue);

        protected string GetComparisonDisplayName()
        {
            if (Operator == ComparisonOperator.HasValue || 
                Operator == ComparisonOperator.HasNoValue)
                return Operator.GetDisplayName();

            return $"{Operator.GetDisplayName()} \"{Value}\"";
        }
    }
}