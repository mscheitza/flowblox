using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.FlowBlocks;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.FlowBlocks.Format;

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
            get { return _inputField; }
            set
            {
                if (_inputField != null)
                    SetFieldRequirement(_inputField, false);

                _inputField = value;

                if (_inputField != null)
                    SetFieldRequirement(_inputField, true);

                OnPropertyChanged(nameof(InputField));
            }
        }

        public override List<FieldElement> GetPossibleFieldElements() => FlowBlockHelper.GetFieldElementsOfAccoiatedFlowBlocks(this);

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;
    }
}
