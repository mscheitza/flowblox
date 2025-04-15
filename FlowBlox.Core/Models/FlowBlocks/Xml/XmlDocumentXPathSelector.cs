using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace FlowBlox.Core.Models.FlowBlocks.Xml
{
    [Display(Name = "XmlDocumentXPathSelector_DisplayName", Description = "XmlDocumentXPathSelector_Description", ResourceType = typeof(FlowBloxTexts))]
    public class XmlDocumentXPathSelector : BaseSingleResultFlowBlock
    {
        [Display(Name = "XmlDocumentXPathSelector_AssociatedXmlDocument", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [AssociatedFlowBlockResolvable()]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
            SelectionFilterMethod = nameof(GetPossibleXmlDocumentFlowBlocks),
            SelectionDisplayMember = nameof(Name))]
        public XmlDocumentFlowBlock AssociatedXmlDocument { get; set; }

        private List<XmlDocumentFlowBlock> GetPossibleXmlDocumentFlowBlocks()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFlowBlocks<XmlDocumentFlowBlock>()
                .ToList();
        }

        [Display(Name = "XmlDocumentXPathSelector_XPath", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(ToolboxCategory = FlowBloxToolboxCategory.XPath)]
        [Required]
        public string XPath { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.magnify_scan, 16, SKColors.MediumSeaGreen);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.magnify_scan, 32, SKColors.MediumSeaGreen);
        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Xml;
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

        public override List<string> GetDisplayableProperties()
        {
            var props = base.GetDisplayableProperties();
            props.Add(nameof(AssociatedXmlDocument));
            props.Add(nameof(XPath));
            return props;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);

                var associatedXmlDocument = this.AssociatedXmlDocument ?? GetPreviousFlowBlockOnPath<XmlDocumentFlowBlock>(this);
                if (associatedXmlDocument == null)
                    throw new InvalidOperationException("No XML document source is assigned.");

                var xmlDoc = associatedXmlDocument.InternalXmlDocument;
                if (xmlDoc == null)
                    throw new InvalidOperationException("The XML document in the source flow block is null or has not been initialized.");

                if (string.IsNullOrEmpty(XPath))
                    throw new InvalidOperationException("No XPath expression provided.");

                var nodes = xmlDoc.SelectNodes(XPath);
                if (nodes == null || nodes.Count == 0)
                    throw new InvalidOperationException($"No matching nodes found for XPath: '{XPath}'.");

                var contents = new List<string>();

                foreach (XmlNode node in nodes)
                {
                    string text = node.InnerXml?.Trim();
                    if (!string.IsNullOrEmpty(text))
                        contents.Add(text);
                }

                GenerateResult(runtime, contents);
            });
        }
    }
}