using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.SequenceFlow.ExecuteProject
{
    [Display(Name = "ExecuteProjectParameterMappingEntry_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class ExecuteProjectParameterMappingEntry : FlowBloxReactiveObject
    {
        [Required]
        [Display(Name = "ExecuteProjectParameterMappingEntry_TargetType", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        public ExecuteProjectTargetType TargetType { get; set; } = ExecuteProjectTargetType.UserField;

        [Required]
        [Display(Name = "ExecuteProjectParameterMappingEntry_TargetKey", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public string TargetKey { get; set; }

        [Required]
        [Display(Name = "ExecuteProjectParameterMappingEntry_TargetValue", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string TargetValue { get; set; }
    }
}