using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Provider;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    [Display(Name = "FieldCondition_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class FieldCondition : Condition
    {
        [Required()]
        [Display(Name = "Global_FieldElement", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ComboBox, 
            SelectionDisplayMember = nameof(Components.FieldElement.FullyQualifiedName), 
            SelectionFilterMethod = nameof(GetPossibleFieldElements))]
        public FieldElement FieldElement { get; set; }

        public static List<FieldElement> GetPossibleFieldElements()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var fieldElements = registry.GetFieldElements(true).ToList();
            return fieldElements;
        }

        public override string DisplayName => GetDisplayName(fieldElement => fieldElement?.FullyQualifiedName);

        public override string ShortDisplayName => GetDisplayName(fieldElement => fieldElement.Name);

        private string GetDisplayName(Func<FieldElement, string> nameSelector)
        {
            string baseText = base.DisplayName;

            if (!string.IsNullOrEmpty(baseText) && char.IsUpper(baseText[0]))
                baseText = char.ToLower(baseText[0]) + baseText.Substring(1);

            string fQFieldName = nameSelector.Invoke(FieldElement);

            if (!string.IsNullOrEmpty(fQFieldName))
                return $"{fQFieldName} {baseText}";
            else
                return baseText;
        }
    }
}