using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.DeepCopier;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FlowBlox.Core.Models.FlowBlocks.Xml
{
    [Display(Name = "XmlDocumentNodeWriterFlowBlock_DisplayName", Description = "XmlDocumentNodeWriterFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class XmlDocumentNodeWriterFlowBlock : BaseFlowBlock
    {
        [Display(Name = "XmlDocumentNodeWriterFlowBlock_AssociatedXmlDocument", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [AssociatedFlowBlockResolvable()]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
            SelectionFilterMethod = nameof(GetPossibleXmlDocumentFlowBlocks),
            SelectionDisplayMember = nameof(Name))]
        public XmlDocumentFlowBlock AssociatedXmlDocument { get; set; }

        public List<XmlDocumentFlowBlock> GetPossibleXmlDocumentFlowBlocks()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFlowBlocks<XmlDocumentFlowBlock>()
                .ToList();
        }

        [Display(Name = "XmlDocumentNodeWriterFlowBlock_AssociatedNodeWriter", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
           SelectionFilterMethod = nameof(GetPossibleSourceNodes),
           SelectionDisplayMember = nameof(BaseFlowBlock.Name))]
        public XmlDocumentNodeWriterFlowBlock AssociatedNodeWriter { get; set; }

        public List<XmlDocumentNodeWriterFlowBlock> GetPossibleSourceNodes()
        {
            var registry = FlowBloxRegistryProvider.GetRegistry();
            var flowBlocks = registry.GetFlowBlocks<XmlDocumentNodeWriterFlowBlock>();
            return flowBlocks.ToList();
        }

        [Display(Name = "XmlDocumentNodeWriterFlowBlock_XPath", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(UiOptions = UIOptions.EnableFieldSelection)]
        public string XPath { get; set; }

        [Display(Name = "XmlDocumentNodeWriterFlowBlock_NodeName", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [Required()]
        public string NodeName { get; set; }

        [Display(Name = "XmlDocumentNodeWriterFlowBlock_Assignments", ResourceType = typeof(FlowBloxTexts), Order = 4)]
        [FlowBlockUI(Factory = UIFactory.GridView, Operations = UIOperations.Create | UIOperations.Edit | UIOperations.Delete)]
        public ObservableCollection<XmlAssignment> Assignments { get; set; } = new();

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.code_tags, 16, SKColors.Sienna);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.code_tags, 32, SKColors.Sienna);

        protected void ApplyAssignments(XmlNode baseNode)
        {
            foreach (var assignment in Assignments)
            {
                var xpath = FlowBloxFieldHelper.ReplaceFieldsInString(assignment.XPath);
                var value = assignment.FieldValue != null
                    ? assignment.FieldValue.Value?.ToString()
                    : FlowBloxFieldHelper.ReplaceFieldsInString(assignment.Value);

                XPathEnsurer.EnsureXPathExists(baseNode, xpath);

                var target = baseNode.SelectSingleNode(xpath);

                if (target is XmlAttribute attr)
                {
                    attr.Value = value;
                }
                else if (target is XmlElement el)
                {
                    el.InnerText = value;
                }
            }
        }

        protected override void OnReferencedFieldNameChanged(FieldElement field, string oldFQFieldName, string newFQFieldName)
        {
            foreach (var a in Assignments)
            {
                a.XPath = FlowBloxFieldHelper.ReplaceFQName(a.XPath, oldFQFieldName, newFQFieldName);
                a.Value = FlowBloxFieldHelper.ReplaceFQName(a.Value, oldFQFieldName, newFQFieldName);
            }
            XPath = FlowBloxFieldHelper.ReplaceFQName(XPath, oldFQFieldName, newFQFieldName);
            base.OnReferencedFieldNameChanged(field, oldFQFieldName, newFQFieldName);
        }

        [JsonIgnore]
        [DeepCopierIgnore]
        public XmlNode CreatedOrUpdatedNode { get; private set; }

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Xml;

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(AssociatedXmlDocument));
            properties.Add(nameof(AssociatedNodeWriter));
            properties.Add(nameof(XPath));
            properties.Add(nameof(Assignments));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);

                var associatedXmlDocument = this.AssociatedXmlDocument ?? GetPreviousFlowBlockOnPath<XmlDocumentFlowBlock>(this);
                if (associatedXmlDocument == null)
                    throw new InvalidOperationException("No XML document source is assigned to the flow block.");

                if (associatedXmlDocument.InternalXmlDocument == null)
                    throw new InvalidOperationException("The XML document in the source flow block is null or has not been initialized.");

                XmlNode rootNode = null;
                XmlNode parent = null;

                if (AssociatedNodeWriter != null)
                {
                    if (AssociatedNodeWriter.CreatedOrUpdatedNode == null)
                        throw new InvalidOperationException("NodeSource was specified, but no CreatedNode exists.");

                    rootNode = AssociatedNodeWriter.CreatedOrUpdatedNode;
                    parent = string.IsNullOrEmpty(XPath)
                        ? rootNode
                        : rootNode.SelectSingleNode(XPath);
                }
                else if (associatedXmlDocument?.InternalXmlDocument != null)
                {
                    rootNode = associatedXmlDocument.InternalXmlDocument.DocumentElement;
                    parent = string.IsNullOrEmpty(XPath)
                        ? rootNode
                        : associatedXmlDocument.InternalXmlDocument.SelectSingleNode(XPath);
                }

                if (parent == null)
                {
                    if (!string.IsNullOrEmpty(XPath))
                        throw new InvalidOperationException($"The target node could not be found using the specified XPath: '{XPath}'.");
                    else
                        throw new InvalidOperationException("The target node could not be found.");
                }


                XmlNode newOrUpdatedNode = parent.SelectSingleNode(NodeName);
                if (newOrUpdatedNode == null)
                {
                    var baseDoc = associatedXmlDocument.InternalXmlDocument;
                    var newNode = baseDoc!.CreateElement(NodeName);
                    parent.AppendChild(newNode);
                    newOrUpdatedNode = newNode;
                }

                ApplyAssignments(newOrUpdatedNode);
                ExecuteNextFlowBlocks(runtime);
            });
        }
    }
}
