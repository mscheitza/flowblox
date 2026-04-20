using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Models.Base;

namespace FlowBlox.Core.Models.FlowBlocks.IO
{
    public class TableColumnDefinition : FieldRequiredDefinitionBase
    {
        [Required()]
        [Display(Name = "Global_FieldElement", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.ComboBox,
            SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName), 
            SelectionFilterMethod = nameof(FlowBloxComponent.GetPossibleFieldElements))]
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
