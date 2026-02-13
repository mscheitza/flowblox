using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.Runtime;
using System.ComponentModel.DataAnnotations;
using System.Data;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.Resources;
using System.Collections.ObjectModel;
using SkiaSharp;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [FlowBlockUIGroup("DecisionFlowBlock_Groups_Decisions", 0)]
    [Display(Name = "DecisionFlowBlock_DisplayName", Description = "DecisionFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class DecisionFlowBlock : BaseSingleResultFlowBlock
    {
        [Display(Name = "DecisionFlowBlock_Decisions", ResourceType = typeof(FlowBloxTexts), GroupName = "DecisionFlowBlock_Groups_Decisions", Order = 0)]
        [CustomValidation(typeof(DecisionFlowBlock), nameof(ValidateDecisions))]
        [FlowBlockUI(Factory = UIFactory.GridView, DisplayLabel = false)]
        [FlowBlockDataGrid(
            GridColumnMemberNames = new[]
            {
                nameof(FieldComparisonCondition.FieldElement),
                nameof(FieldComparisonCondition.Operator),
                nameof(FieldComparisonCondition.Value)
            })]
        public ObservableCollection<FieldComparisonCondition> Decisions { get; set; }

        public static ValidationResult ValidateDecisions(List<FieldComparisonCondition> decisions, ValidationContext context)
        {
            var duplicates = decisions
                .GroupBy(x => x.FieldElement)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                var errorMessage = string.Format(FlowBloxResourceUtil.GetLocalizedString("DecisionFlowBlock_Validation_DuplicateFields"), string.Join(", ", duplicates.Select(x => x.Name).ToArray()));
                return new ValidationResult(errorMessage, [context.MemberName]);
            }

            return ValidationResult.Success;
        }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.source_branch, 16, SKColors.Goldenrod);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.source_branch, 32, SKColors.Goldenrod);

        public DecisionFlowBlock()
        {
            this.Decisions = new ObservableCollection<FieldComparisonCondition>();
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Logic;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                string result = null;
                foreach(var decision in this.Decisions)
                {
                    if (decision.Compare())
                    {
                        result = decision.FieldElement.StringValue;
                        break;
                    }
                }
                GenerateResult(runtime, result);
            });
        }
    }
}