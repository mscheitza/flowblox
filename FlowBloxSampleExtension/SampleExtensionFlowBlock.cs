using System.ComponentModel.DataAnnotations;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Util;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Util.Fields;
using FlowBloxSampleExtension.Model;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;

namespace FlowBloxSampleExtension
{
    [Display(Name = "SampleExtensionFlowBlock_DisplayName", Description = "SampleExtensionFlowBlock_Description", ResourceType = typeof(SampleExtensionResources))]
    public class SampleExtensionFlowBlock : BaseSingleResultFlowBlock
    {
        [Display(Name = "SampleExtensionFlowBlock_OutputText", ResourceType = typeof(SampleExtensionResources), Order = 0)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        [Required()]
        public string OutputText { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(SampleExtensionResources.cube, 16, SKColors.DarkSlateBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(SampleExtensionResources.cube, 32, SKColors.DarkSlateBlue);
    
        public override void OnAfterCreate()
        {
            this.OutputText = FlowBloxOptions.GetOptionInstance().OptionCollection["SampleExtensionFlowBlock.DefaultOutputText"].Value.ToString();
            base.OnAfterCreate();
        }

        public void Test()
        {

        }
    
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;
    
        public override FlowBlockCategory GetCategory() => FlowBloxSampleCategories.Sample;
    
        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(OutputText));
            return properties;
        }
    
        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);
                var outputText = FlowBloxFieldHelper.ReplaceFieldsInString(this.OutputText);
                GenerateResult(runtime, outputText);
            });
        }
    
        public override void OptionsInit(List<OptionElement> defaults)
        {
            defaults.Add(new OptionElement("SampleExtensionFlowBlock.DefaultOutputText", "This is a default output text.", "Defines the default output text for the sample FlowBlock.", OptionElement.OptionType.Text));
            base.OptionsInit(defaults);
        }
    }
}