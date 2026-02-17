using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [Display(Name = "Global_FieldElement", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        public FieldElement SourceField { get; set; }

        public bool IsDeletable(out List<IFlowBloxComponent> dependencies)
        {
            if (SourceField == null)
            {
                dependencies = null;
                return true;
            }

            return SourceField.IsDeletable(out dependencies);
        }
    }
}
