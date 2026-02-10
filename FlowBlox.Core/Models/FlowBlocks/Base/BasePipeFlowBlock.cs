using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.FlowBlocks;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public abstract class BasePipeFlowBlock : BaseSingleResultFlowBlock
    {
        private FieldElement _inputField;
        
        [Display(Name = "Global_InputField", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Association, SelectionFilterMethod = nameof(GetPossibleFieldElements), SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName), Operations = UIOperations.Link | UIOperations.Unlink)]
        [Required()]
        public FieldElement InputField
        {
            get => _inputField;
            set => SetRequiredInputField(ref _inputField, value);
        }

        public override List<FieldElement> GetPossibleFieldElements() => FlowBlockHelper.GetFieldElementsOfAccoiatedFlowBlocks(this);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;
    }
}
