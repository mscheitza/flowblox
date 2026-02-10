using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace FlowBlox.Core.Models.FlowBlocks.Xml
{
    [Display(Name = "XmlDocumentFlowBlock_DisplayName", Description = "XmlDocumentFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class XmlDocumentFlowBlock : BaseFlowBlock
    {
        [JsonIgnore]
        [DeepCopierIgnore]
        public XmlDocument InternalXmlDocument { get; protected set; }

        [Display(Name = "XmlDocumentFlowBlock_XmlContent", ResourceType = typeof(FlowBloxTexts))]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(IsCodingMode = true, MultiLine = true, SyntaxHighlighting = "XML")]
        [Required]
        public string XmlContent { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.xml, 16, SKColors.DarkOrange);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.xml, 32, SKColors.DarkOrange);

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Xml;

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);

                string content = FlowBloxFieldHelper.ReplaceFieldsInString(XmlContent);
                InternalXmlDocument = new XmlDocument();
                InternalXmlDocument.LoadXml(content);

                ExecuteNextFlowBlocks(runtime);
            });
        }
    }
}
