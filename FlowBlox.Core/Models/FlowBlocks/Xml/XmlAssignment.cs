using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Xml
{
    public class XmlAssignment : FlowBloxReactiveObject
    {
        [Display(Name = "XmlAssignment_XPath", ResourceType = typeof(FlowBloxTexts))]
        public string XPath { get; set; }

        [Display(Name = "XmlAssignment_Value", ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        public string Value { get; set; }

        [Display(Name = "XmlAssignment_FieldValue", ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(Factory = UIFactory.ComboBox, SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName),
            SelectionFilterMethod = nameof(BaseFlowBlock.GetPossibleFieldElements))]
        public FieldElement FieldValue { get; set; }
    }
}
