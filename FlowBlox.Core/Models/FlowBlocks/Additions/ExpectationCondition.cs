using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    public class ExpectationCondition : ComparisonCondition
    {
        private ExpectationConditionTarget _expectationConditionTarget;

        [Display(Name = "ExpectationCondition_ExpectationConditionTarget", Description = "ExpectationCondition_ExpectationConditionTarget_Description", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public ExpectationConditionTarget ExpectationConditionTarget
        {
            get => _expectationConditionTarget;
            set
            {
                if (_expectationConditionTarget != value)
                {
                    _expectationConditionTarget = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _index;

        [Display(Name = "ExpectationCondition_Index", Description = "ExpectationCondition_Index_Description", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public int Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public override string ToString()
        {
            var targetText = ExpectationConditionTarget.GetDisplayName();

            if (ExpectationConditionTarget == ExpectationConditionTarget.ValueAtIndex)
                targetText = $"{targetText} {Index}";

            return $"{targetText} {base.ToString()}";
        }
    }
}
