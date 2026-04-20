using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.Logic
{
    [Serializable()]
    [Display(Name = "DistributorFlowBlock_DisplayName", Description = "DistributorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class DistributorFlowBlock : BaseResultFlowBlock
    {
        private FieldElement _inputField;

        [Display(Name = "Global_InputField", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Association, 
            SelectionFilterMethod = nameof(GetPossibleInputFields), 
            SelectionDisplayMember = nameof(FieldElement.FullyQualifiedName), Operations = UIOperations.Link | UIOperations.Unlink)]
        [Required()]
        public FieldElement InputField 
        {
            get => _inputField;
            set => SetRequiredInputField(ref _inputField, value);
        }

        public List<FieldElement> GetPossibleInputFields() => FlowBloxFieldsResolver.GetFieldsOfAssociatedFlowBlocks(this);

        [Display(Name = "DistributorFlowBlock_DisributedFields", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.ListView, Operations = UIOperations.Create | UIOperations.Delete)]
        [FlowBloxListView(LVColumnMemberNames = new[] { nameof(FieldElement.FlowBlockName), nameof(FieldElement.Name) })]
        public ObservableCollection<FieldElement> DisributedFields { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.source_merge, 16, SKColors.SaddleBrown);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.source_merge, 32, SKColors.SaddleBrown);

        public DistributorFlowBlock() : base ()
        {
            DisributedFields = new ObservableCollection<FieldElement>();
        }

        public override List<FieldElement> Fields => DisributedFields.ToList();

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Logic;

        public override bool Execute(Runtime.BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);
                runtime.Report($"Source field value \"{InputField.StringValue}\" will be distributed to the following fields: {string.Join(", ", Fields.Select(x => x.Name))}");
                var resultEntry = Fields.ToDictionary(x => x, y => InputField.StringValue);
                GenerateResult(runtime, new List<Dictionary<FieldElement, string>>() { resultEntry });
            });
        }
    }
}
