using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.Fields;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public abstract class BasePipeFlowBlock : BaseSingleResultFlowBlock
    {
        private FieldElement _inputField;
        
        [Display(Name = "Global_InputField", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Association, SelectionFilterMethod = nameof(GetPossibleInputFields), SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName), Operations = UIOperations.Link | UIOperations.Unlink)]
        [Required()]
        public FieldElement InputField
        {
            get => _inputField;
            set => SetRequiredInputField(ref _inputField, value);
        }

        public virtual List<FieldElement> GetPossibleInputFields() => FlowBloxFieldsResolver.GetFieldsOfAssociatedFlowBlocks(this);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;
    }
}
