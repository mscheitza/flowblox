using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Enums
{
    public enum LogicalOperator
    {
        [Display(Name = "LogicalOperator_And", ResourceType = typeof(FlowBloxTexts))]
        And = 0,

        [Display(Name = "LogicalOperator_Or", ResourceType = typeof(FlowBloxTexts))]
        Or = 1
    }
}