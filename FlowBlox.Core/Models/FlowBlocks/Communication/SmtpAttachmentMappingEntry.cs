using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Communication
{
    [Display(Name = "SmtpAttachmentMappingEntry_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class SmtpAttachmentMappingEntry : FlowBloxReactiveObject
    {
        [Required]
        [Display(Name = "SmtpAttachmentMappingEntry_FileName", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string FileName { get; set; }

        [Required]
        [Display(Name = "Global_FieldElement", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.ComboBox,
            SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName),
            SelectionFilterMethod = nameof(BaseFlowBlock.GetPossibleFieldElements))]
        public FieldElement Field { get; set; }

        [Display(Name = "PropertyNames_EncodingName", Description = "PropertyNames_EncodingName_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public DotNetEncodingNames EncodingName { get; set; } = DotNetEncodingNames.Default;
    }
}
