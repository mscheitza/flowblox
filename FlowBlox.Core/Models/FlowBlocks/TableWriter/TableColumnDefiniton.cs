using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBlox.Core.Models.FlowBlocks.TableWriter
{
    public class TableColumnDefinition : FieldRequiredDefinitionBase
    {
        [Required()]
        [Display(Name = "Global_FieldElement", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ComboBox,
            SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName), 
            SelectionFilterMethod = nameof(BaseFlowBlock.GetPossibleFieldElements))]
        public override FieldElement Field { get; set; }

        [Display(Name = "TableColumnDefinition_IsRequired", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        public override bool IsRequired { get; set; }

        [Display(Name = "TableColumnDefinition_IsKeyColumn", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        public bool IsKeyColumn { get; set; }

        [Required()]
        [Display(Name = "TableColumnDefinition_ColumnName", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        public string ColumnName { get; set; }
    }
}
