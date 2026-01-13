using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.WebBrowser;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Util.Fields;
using FlowBlox.Core.Util.Resources;
using SkiaSharp;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FlowBlox.Core.Models.FlowBlocks
{
    [FlowBlockUIGroup("WebEventFlowBlock_Groups_Advanced", 0)]
    [Display(Name = "WebEventFlowBlock_DisplayName", Description = "WebEventFlowBlock_Description", ResourceType = typeof(FlowBloxTexts))]
    public class WebEventFlowBlock : WebActionFlowblockBase
    {
        public enum WebBrowserEventTypes
        {
            [Display(Name = "WebBrowserEventTypes_Click", ResourceType = typeof(FlowBloxTexts))]
            Click,
            [Display(Name = "WebBrowserEventTypes_Scroll", ResourceType = typeof(FlowBloxTexts))]
            Scroll,
            [Display(Name = "WebBrowserEventTypes_Enter", ResourceType = typeof(FlowBloxTexts))]
            Enter,
            [Display(Name = "WebBrowserEventTypes_UpdateDOM", ResourceType = typeof(FlowBloxTexts))]
            UpdateDOM,
            [Display(Name = "WebBrowserEventTypes_UploadFile", ResourceType = typeof(FlowBloxTexts))]
            UploadFile,
            [Display(Name = "WebBrowserEventTypes_SwitchToUrl", ResourceType = typeof(FlowBloxTexts))]
            SwitchToUrl,
            [Display(Name = "WebBrowserEventTypes_ClickAll", ResourceType = typeof(FlowBloxTexts))]
            ClickAll,
        }

        [Display(Name = "WebEventFlowBlock_WebBrowserEventType", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [FlowBlockUI(Factory = UIFactory.ComboBox)]
        public WebBrowserEventTypes WebBrowserEventType { get; set; } = WebBrowserEventTypes.Click;

        [Display(Name = "WebEventFlowBlock_XPath", ResourceType = typeof(FlowBloxTexts), Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        public override string XPath { get; set; }

        [Display(Name = "WebEventFlowBlock_CSSSelector", ResourceType = typeof(FlowBloxTexts), Order = 2)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        public override string CSSSelector { get; set; }

        [Display(Name = "WebEventFlowBlock_InputText", ResourceType = typeof(FlowBloxTexts), Order = 3)]
        [FlowBlockUI(Factory = UIFactory.Default, UiOptions = UIOptions.EnableFieldSelection)]
        [FlowBlockTextBox(MultiLine = true)]
        public string InputText { get; set; }

        [Display(Name = "WebEventFlowBlock_WaitingTimeAfterExecution", ResourceType = typeof(FlowBloxTexts), GroupName = "WebEventFlowBlock_Groups_Advanced", Order = 0)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public int WaitingTimeAfterExecution { get; set; }

        [Display(Name = "WebEventFlowBlock_ElementDeterminationRequired", ResourceType = typeof(FlowBloxTexts), GroupName = "WebEventFlowBlock_Groups_Advanced", Order = 1)]
        [FlowBlockUI(Factory = UIFactory.Default)]
        public bool ElementDeterminationRequired { get; set; }

        public int LatestScrollRectangleHeight { get; private set; }

        public override SKImage Icon16 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.lightning_bolt_outline, 16, SKColors.MediumPurple);
        public override SKImage Icon32 => FlowBloxIconUtil.CreateFromSVG(FlowBloxIcons.lightning_bolt_outline, 32, SKColors.MediumPurple);


        public WebEventFlowBlock() : base()
        {
            this.WaitingTimeAfterExecution = WebBrowserConstants.DefaultWaitingTimeAfterExecution;
        }

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.One;

        public override FlowBlockCategory GetCategory() => FlowBlockCategory.Web;

        public override List<string> GetDisplayableProperties()
        {
            var properties = base.GetDisplayableProperties();
            properties.Add(nameof(WebBrowserEventType));
            properties.Add(nameof(XPath));
            properties.Add(nameof(CSSSelector));
            properties.Add(nameof(InputText));
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
                string inputText = FlowBloxFieldHelper.ReplaceFieldsInString(this.InputText);

                WebEventSelectionMode mode;
                string selector;
                
                if (WebBrowserEventType == WebBrowserEventTypes.Enter)
                {
                    RequireAndGetSelector(out mode, out selector);
                    var enterResult = webBrowser.EnterTextForElement(selector, inputText, mode);
                    if (runtime != null)
                    {
                        if (enterResult.Success)
                        {
                            runtime.Report($"Enter action successfully executed for selector \"{selector}\" and text \"{inputText}\".");
                            WaitAfterExecution();
                            GenerateResult(runtime, webBrowser.DOMContent);
                        }
                        else
                        {
                            HandleFailure(runtime, enterResult);
                        }
                    }
                }
                else if (WebBrowserEventType == WebBrowserEventTypes.Click)
                {
                    RequireAndGetSelector(out mode, out selector);
                    var clickResult = webBrowser.ClickOnElement(selector, mode);
                    if (runtime != null)
                    {
                        if (clickResult.Success)
                        {
                            runtime.Report($"Click action successfully executed for selector \"{selector}\".");
                            WaitAfterExecution();
                            GenerateResult(runtime, webBrowser.DOMContent);
                        }
                        else
                        {
                            HandleFailure(runtime, clickResult);
                        }
                    }
                }
                else if (WebBrowserEventType == WebBrowserEventTypes.ClickAll)
                {
                    RequireAndGetSelector(out mode, out selector);
                    var clickResult = webBrowser.ClickOnAllElements(selector, mode);
                    if (runtime != null)
                    {
                        if (clickResult.Success)
                        {
                            runtime.Report($"Click action successfully executed for selector \"{selector}\".");
                            WaitAfterExecution();
                            GenerateResult(runtime, webBrowser.DOMContent);
                        }
                        else
                        {
                            HandleFailure(runtime, clickResult);
                        }
                    }
                }
                else if (WebBrowserEventType == WebBrowserEventTypes.Scroll)
                {
                    if (LatestScrollRectangleHeight < webBrowser.Height)
                    {
                        var scrollResult = webBrowser.ScrollMax();
                        if (scrollResult.Success)
                        {
                            runtime.Report($"Scroll action successfully executed.");
                            WaitAfterExecution();
                            GenerateResult(runtime, webBrowser.DOMContent);
                        }
                        else
                        {
                            HandleFailure(runtime, scrollResult);
                        }
                    }
                    else
                    {
                        HandleFailure(runtime, WebEventNotifications.FurtherScrollActionsNotPossible);
                    }
                }
                else if (WebBrowserEventType == WebBrowserEventTypes.UpdateDOM)
                {
                    RequireAndGetSelector(out mode, out selector);
                    var updateResult = webBrowser.UpdateDOM(selector, inputText, mode);
                    if (runtime != null)
                    {
                        if (updateResult.Success)
                        {
                            runtime.Report($"UpdateDOM action successfully executed for selector \"{selector}\" with content \"{inputText}\".");
                            WaitAfterExecution();
                            GenerateResult(runtime, webBrowser.DOMContent);
                        }
                        else
                        {
                            HandleFailure(runtime, updateResult);
                        }
                    }
                }
                else if (WebBrowserEventType == WebBrowserEventTypes.UploadFile)
                {
                    RequireAndGetSelector(out mode, out selector);
                    var uploadResult = webBrowser.UploadFile(selector, mode, inputText);
                    if (runtime != null)
                    {
                        if (uploadResult.Success)
                        {
                            runtime.Report($"UploadFile action successfully executed for selector \"{selector}\" with file \"{inputText}\".");
                            WaitAfterExecution();
                            GenerateResult(runtime, webBrowser.DOMContent);
                        }
                        else
                        {
                            HandleFailure(runtime, uploadResult);
                        }
                    }
                }
                else if (WebBrowserEventType == WebBrowserEventTypes.SwitchToUrl)
                {
                    var switchResult = webBrowser.SwitchToUrl(inputText);
                    if (runtime != null)
                    {
                        if (switchResult.Success)
                        {
                            runtime.Report($"SwitchToUrl action successfully executed with URL \"{inputText}\".");
                            WaitAfterExecution();
                            GenerateResult(runtime, webBrowser.DOMContent);
                        }
                        else
                        {
                            HandleFailure(runtime, switchResult);
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException("Execution for \"" + WebBrowserEventType.ToString() + "\" is not implemented.");
                }
            });
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(WebEventNotifications));
                return notificationTypes;
            }
        }

        public enum WebEventNotifications
        {
            [FlowBlockNotification(NotificationType = NotificationType.Warning)]
            [Display(Name = "Further scroll actions not possible")]
            FurtherScrollActionsNotPossible
        }

        private void WaitAfterExecution()
        {
            if (this.WaitingTimeAfterExecution > 0)
                Thread.Sleep(this.WaitingTimeAfterExecution);
        }
    }
}
