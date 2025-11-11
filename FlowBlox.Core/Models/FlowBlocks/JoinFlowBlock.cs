using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Util;
using System.Drawing;
using FlowBlox.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.Resources;
using System.Collections.ObjectModel;
using FlowBlox.Core.Util.FlowBlocks;
using SkiaSharp;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [Display(Name = "JoinFlowBlock_DisplayName", Description = "JoinFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class JoinFlowBlock : BaseSingleResultFlowBlock
    {
        [Display(Name = "JoinFlowBlock_Separator", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [CustomValidation(typeof(JoinFlowBlock), nameof(ValidateSeparators))]
        public string Separator { get; set; }

        [Display(Name = "JoinFlowBlock_SpecialSeparator", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        [CustomValidation(typeof(JoinFlowBlock), nameof(ValidateSeparators))]
        public SpecialSeparator? SpecialSeparator { get; set; }

        public static ValidationResult ValidateSeparators(object separator, ValidationContext validationContext)
        {
            var joinFlowBlock = (JoinFlowBlock)validationContext.ObjectInstance;

            if (string.IsNullOrEmpty(joinFlowBlock.Separator) && !joinFlowBlock.SpecialSeparator.HasValue)
                return new ValidationResult(FlowBloxResourceUtil.GetLocalizedString("JoinFlowBlock_Validation_NoSeparatorDefined"), [validationContext.MemberName]);
            if (!string.IsNullOrEmpty(joinFlowBlock.Separator) && joinFlowBlock.SpecialSeparator.HasValue)
                return new ValidationResult(FlowBloxResourceUtil.GetLocalizedString("JoinFlowBlock_Validation_ManySeparatorsDefined"), [validationContext.MemberName]);
            return ValidationResult.Success;
        }

        [Display(Name = "JoinFlowBlock_JoinedParameters", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.ListView, Operations = UIOperations.Link | UIOperations.Unlink,
            UiOptions = UIOptions.FieldSelectionDefaultNotRequired,
            SelectionFilterMethod = nameof(GetPossibleFieldElements))]
        [FlowBlockListView(LVColumnMemberNames = new[] { nameof(FieldElement.FlowBlockName), nameof(FieldElement.Name) })]
        public ObservableCollection<FieldElement> JoinedParameters { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.set_merge, 16, SKColors.DarkOliveGreen);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.set_merge, 32, SKColors.DarkOliveGreen);

        public JoinFlowBlock() : base()
        {
            this.JoinedParameters = new ObservableCollection<FieldElement>();
        }

        public override void OnAfterCreate()
        {
            this.Separator = FlowBloxOptions.GetOptionInstance().OptionCollection["JoinFlowBlock.DefaultSeparator"].Value.ToString();
            base.OnAfterCreate();
        }

        public override BaseFlowBlock InputReference
        {
            get
            {
                if (this.ReferencedFlowBlocks.OfType<BaseResultFlowBlock>().Count() == 1)
                {
                    var referencedFlowBlock = this.ReferencedFlowBlocks.Single();
                    if (referencedFlowBlock.ReferencedFlowBlocks.Count == 1)
                        return referencedFlowBlock.ReferencedFlowBlocks.Single();
                    else
                        return CommonFlowBlockResolver.FindCommonFlowBlock(referencedFlowBlock);
                }
                else
                {
                    return base.InputReference;
                }
            }
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.TextOperations;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(Separator));
            properties.Add(nameof(SpecialSeparator));
            return properties;
        }

        private readonly List<string> _values = new List<string>();

        private void JoinFlowBlock_OnBeforeInputProcessing()
        {
            _values.Clear();
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                string separator = this.Separator;
                if (SpecialSeparator == Enums.SpecialSeparator.Tab)
                    separator = "\t";
                else if (this.SpecialSeparator == Enums.SpecialSeparator.NewLine)
                    separator = Environment.NewLine;

                if (string.IsNullOrEmpty(separator))
                    throw new InvalidOperationException("No separator was defined.");

                var values = this.JoinedParameters
                    .Select(x => x.StringValue)
                    .ExceptNullOrEmpty();

                _values.AddRange(values);

                if (this.InputDatasets_CurrentIndex == InputDatasets_Count - 1)
                    GenerateResult(runtime, string.Join(separator, _values));
            });
        }

        public override void RuntimeStarted(BaseRuntime runtime)
        {
            OnBeforeInputProcessing -= JoinFlowBlock_OnBeforeInputProcessing;
            OnBeforeInputProcessing += JoinFlowBlock_OnBeforeInputProcessing;

            base.RuntimeStarted(runtime);
        }

        public override void RuntimeFinished(BaseRuntime runtime)
        {
            OnBeforeInputProcessing -= JoinFlowBlock_OnBeforeInputProcessing;

            base.RuntimeFinished(runtime);
        }

        public override void OptionsInit(List<OptionElement> defaults)
        {
            defaults.Add(new OptionElement("JoinFlowBlock.DefaultSeparator", ",", "Dies ist das Standard-Trennzeichen des Join-Elements.", OptionElement.OptionType.Text));
            base.OptionsInit(defaults);
        }
    }
}
