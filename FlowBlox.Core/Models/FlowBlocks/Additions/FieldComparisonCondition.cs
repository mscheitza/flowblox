using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Provider;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Additions
{
    [Display(Name = "FieldCondition_DisplayName", ResourceType = typeof(FlowBloxTexts))]
    public class FieldComparisonCondition : ComparisonCondition
    {
        [Required]
        [Display(Name = "Global_FieldElement", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.ComboBox,
            SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName),
            SelectionFilterMethod = nameof(FlowBloxComponent.GetPossibleFieldElements))]
        public FieldElement FieldElement { get; set; }

        public virtual bool Compare() => Compare(FieldElement?.Value);

        public override string DisplayName => GetDisplayName(f => f?.FullyQualifiedName);

        public override string ShortDisplayName => GetDisplayName(f => f?.Name);

        private string GetDisplayName(Func<FieldElement, string> selector)
        {
            string baseText = base.DisplayName;

            if (!string.IsNullOrEmpty(baseText) && char.IsUpper(baseText[0]))
                baseText = char.ToLower(baseText[0]) + baseText.Substring(1);

            string name = selector(FieldElement);

            return !string.IsNullOrEmpty(name)
                ? $"{name} {baseText}"
                : baseText;
        }
    }
}
