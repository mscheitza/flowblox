using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace FlowBlox.Core.Models.FlowBlocks.Xml
{
    [Display(Name = "XPathSelectorFlowBlock_DisplayName", Description = "XPathSelectorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class XPathSelectorFlowBlock : BaseSingleResultFlowBlock
    {
        [Display(Name = "XPathSelectorFlowBlock_XmlContent", Description = "XPathSelectorFlowBlock_XmlContent_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBloxTextBox(IsCodingMode = true, MultiLine = true, SyntaxHighlighting = "XML")]
        [Required]
        public string XmlContent { get; set; }

        [Display(Name = "XPathSelectorFlowBlock_XPath", Description = "XPathSelectorFlowBlock_XPath_Tooltip", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBloxUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection, ToolboxCategory = nameof(FlowBloxToolboxCategory.XPath))]
        [Required]
        public string XPath { get; set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.text_search, 16, SKColors.MediumSeaGreen);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.text_search, 32, SKColors.MediumSeaGreen);

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Xml;
        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(XPathSelectorNotifications));
                return notificationTypes;
            }
        }

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(XPath));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);

                var xmlContent = FlowBloxFieldHelper.ReplaceFieldsInString(XmlContent);
                if (string.IsNullOrWhiteSpace(xmlContent))
                {
                    CreateNotification(runtime, XPathSelectorNotifications.XmlContentIsEmpty);
                    GenerateResult(runtime);
                    return;
                }

                var xpath = FlowBloxFieldHelper.ReplaceFieldsInString(XPath);
                if (string.IsNullOrWhiteSpace(xpath))
                {
                    CreateNotification(runtime, XPathSelectorNotifications.XPathExpressionIsEmpty);
                    GenerateResult(runtime);
                    return;
                }

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlContent);

                var values = XPathSelector.SelectValues(xmlDocument, xpath);
                if (values.Count == 0)
                    CreateNotification(runtime, XPathSelectorNotifications.NoMatchingValuesFound);

                GenerateResult(runtime, values);
            });
        }

        public enum XPathSelectorNotifications
        {
            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "XPathSelectorFlowBlock_Notification_XmlContentIsEmpty", ResourceType = typeof(FlowBloxTexts))]
            XmlContentIsEmpty,

            [FlowBloxNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "XPathSelectorFlowBlock_Notification_XPathExpressionIsEmpty", ResourceType = typeof(FlowBloxTexts))]
            XPathExpressionIsEmpty,

            [FlowBloxNotification(NotificationType = NotificationType.Error)]
            [Display(Name = "XPathSelectorFlowBlock_Notification_NoMatchingValuesFound", ResourceType = typeof(FlowBloxTexts))]
            NoMatchingValuesFound
        }
    }
}
