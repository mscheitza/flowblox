using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.UICore.Enums;

namespace FlowBlox.UICore.Models
{
    public sealed class FieldSelectionWindowArgs
    {
        public BaseFlowBlock FlowBlock { get; set; }

        public FieldSelectionMode SelectionMode { get; set; } = FieldSelectionMode.Fields;

        public FieldSelectionMode[] AllowedFieldSelectionModes { get; set; } = [FieldSelectionMode.Fields, FieldSelectionMode.ProjectProperties, FieldSelectionMode.Options];

        public bool MultiSelect { get; set; } = true;

        public bool IsRequired { get; set; } = true;
        
        public bool HideRequired { get; set; } = false;

        public IEnumerable<FieldElement> FieldElements { get; set; }

        public IEnumerable<FlowBloxProjectPropertyElement> ProjectPropertyElements { get; set; }

        public IEnumerable<OptionElement> OptionElements { get; set; }

        public IEnumerable<FlowBloxInputFilePlaceholderElement> InputFileElements { get; set; }

        public IEnumerable<FlowBloxGenerationStrategyPlaceholderElement> GenerationStrategyDataElements { get; set; }
    }
}
