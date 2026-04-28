using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using System.Data;
using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Util.Resources;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Format;
using System.Collections.ObjectModel;
using SkiaSharp;
using System.Text;

namespace FlowBlox.Core.Models.FlowBlocks.TextOperations
{
    [Display(Name = "FormatFlowBlock_DisplayName", Description = "FormatFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class FormatFlowBlock : BaseSingleResultFlowBlock
    {
        public FormatFlowBlock()
        {
            FormatParameterDefinitions = new ObservableCollection<FormatParameterDefinition>();
        }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.format_line_style, 16, SKColors.ForestGreen);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.format_line_style, 32, SKColors.ForestGreen);


        public string FieldName { get; set; }

        private string _formatExpression;

        [Display(Name = "FormatFlowBlock_FormatExpression", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxTextBox(MultiLine = true, IsCodingMode = true)]
        [CustomValidation(typeof(FormatFlowBlock), nameof(ValidateFormatExpression))]
        [Required()]
        public string FormatExpression { get; set; }

        private ObservableCollection<FormatParameterDefinition> _formatParameterDefinitions;

        [Display(Name = "FormatFlowBlock_FormatParameters", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.GridView)]
        public ObservableCollection<FormatParameterDefinition> FormatParameterDefinitions
        {
            get
            {
                return _formatParameterDefinitions;
            }
            set
            {
                _formatParameterDefinitions = value;
                SetFieldRequirements(_formatParameterDefinitions);
            }
        }

        public static ValidationResult ValidateFormatExpression(string formatExpression, ValidationContext validationContext)
        {
            var block = (FormatFlowBlock)validationContext.ObjectInstance;
            if (string.IsNullOrWhiteSpace(formatExpression))
                return ValidationResult.Success;

            CompositeFormat compositeFormat;
            try
            {
                compositeFormat = CompositeFormat.Parse(formatExpression);
            }
            catch (FormatException)
            {
                var invalidFormatMessage = FlowBloxResourceUtil.GetLocalizedString("FormatFlowBlock_Validation_InvalidFormatExpression");
                return new ValidationResult(invalidFormatMessage, [validationContext.MemberName]);
            }

            var placeholderCount = compositeFormat.MinimumArgumentCount;

            var linkedFieldCount = block.FormatParameterDefinitions?.Count ?? 0;

            if (placeholderCount != linkedFieldCount)
            {
                var errorMessage = string.Format(FlowBloxResourceUtil.GetLocalizedString("FormatFlowBlock_Validation_InvalidFieldElementCount"), placeholderCount, linkedFieldCount);
                return new ValidationResult(errorMessage, [validationContext.MemberName]);
            }

            return ValidationResult.Success;
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.TextOperations;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(FormatExpression));
            return properties;
        }

        public override bool Execute(Runtime.BaseRuntime runtime, object data)
        {
            return base.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                object[] parameters = FormatParameterDefinitions.Select(x => x.Field.Value).ToArray();
                var formattedValue = string.Format(FormatExpression, parameters);
                GenerateResult(runtime, formattedValue);
            });
        }
    }
}
