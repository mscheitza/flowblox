using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.WebBrowser;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [Display(Name = "WebSelectorFlowBlock_DisplayName", Description = "WebSelectorFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class WebSelectorFlowBlock : WebActionFlowblockBase
    {
        [Display(Name = "WebSelectorFlowBlock_XPath", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        public override string XPath { get; set; }

        [Display(Name = "WebSelectorFlowBlock_CSSSelector", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        public override string CSSSelector { get; set; }

        [Display(Name = "WebSelectorFlowBlock_InnerContent", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public bool InnerContent { get; set; } = true;

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.cursor_default_click, 16, SKColors.DeepSkyBlue);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.cursor_default_click, 32, SKColors.DeepSkyBlue);


        public WebSelectorFlowBlock() : base()
        {
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Web;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(XPath));
            properties.Add(nameof(CSSSelector));
            properties.Add(nameof(InnerContent));
            return properties;
        }

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return this.Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);

                var webBrowserFlowBlock = AssociatedWebBrowser ?? GetPreviousFlowBlockOnPath<WebBrowserFlowBlock>(this);
                var webBrowser = webBrowserFlowBlock.InternalWebBrowser;

                string content;
                WebEventSelectionMode mode;
                string selector;

                RequireAndGetSelector(out mode, out selector);
                var result = webBrowser.GetContent(selector, mode, InnerContent);

                if (runtime != null)
                {
                    if (result.Success)
                    {
                        runtime.Report($"Content retrieval successfully executed for selector \"{selector}\".");
                        GenerateResult(runtime, result.Content);
                    }
                    else
                    {
                        HandleFailure(runtime, result);
                    }
                }
            });
        }

    }
}
