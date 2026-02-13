using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    [Display(Name = "LogicalCondition", ResourceType = typeof(FlowBloxTexts))]
    public abstract class LogicalCondition : FlowBloxReactiveObject
    {
        private LogicalOperator _logicalOperator;

        [Display(Name = "LogicalCondition_LogicalOperator", ResourceType = typeof(FlowBloxTexts), Order = -10)]
        public LogicalOperator LogicalOperator
        {
            get => _logicalOperator;
            set
            {
                _logicalOperator = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(ShortDisplayName));
            }
        }

        public abstract bool Check();

        [Display(Name = "PropertyNames_Name", ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(Visible = false, ReadOnly = true)]
        public abstract string DisplayName { get; }

        public abstract string ShortDisplayName { get; }

        public override string ToString() => DisplayName;
    }
}
