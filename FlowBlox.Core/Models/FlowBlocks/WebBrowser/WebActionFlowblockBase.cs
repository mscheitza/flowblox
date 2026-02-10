using FlowBlox.Core.Attributes;
using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Util.Fields;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    public abstract class WebActionFlowblockBase : BaseSingleResultFlowBlock
    {
        [Display(Name = "WebActionFlowblockBase_AssociatedWebBrowser", ResourceType = typeof(FlowBloxTexts), Order = 0)]
        [AssociatedFlowBlockResolvable()]
        [FlowBlockUI(Factory = UIFactory.Association, Operations = UIOperations.Link | UIOperations.Unlink,
            SelectionFilterMethod = nameof(GetPossibleWebBrowserFlowBlocks),
            SelectionDisplayMember = nameof(Name))]
        public WebBrowserFlowBlock AssociatedWebBrowser { get; set; }

        public List<WebBrowserFlowBlock> GetPossibleWebBrowserFlowBlocks()
        {
            return FlowBloxRegistryProvider.GetRegistry()
                .GetFlowBlocks<WebBrowserFlowBlock>()
                .ToList();
        }

        public abstract string CSSSelector { get; set; }

        public abstract string XPath { get; set; }

        public override ObservableCollection<BaseFlowBlock> ReferencedFlowBlocks
        {
            get
            {
                return base.ReferencedFlowBlocks;
            }
            set
            {
                UpdateReferencedWebExecutionRequirement(false);
                base.ReferencedFlowBlocks = value;
                UpdateReferencedWebExecutionRequirement(true);
            }
        }

        private void UpdateReferencedWebExecutionRequirement(bool required)
        {
            var webExecutionFlowBlock = GetPreviousFlowBlockOnPath(this, [typeof(WebBrowserFlowBlock), typeof(WebEventFlowBlock)]);
            if (webExecutionFlowBlock is BaseSingleResultFlowBlock singleResultFlowBlock && 
                singleResultFlowBlock?.ResultField != null)
            {
                SetFieldRequirement(singleResultFlowBlock.ResultField, required);
            }
        }

        protected virtual void RequireAndGetSelector(out WebEventSelectionMode mode, out string selector)
        {
            if (!string.IsNullOrEmpty(CSSSelector))
            {
                mode = WebEventSelectionMode.CssSelector;
                selector = FlowBloxFieldHelper.ReplaceFieldsInString(CSSSelector);
            }
            else if (!string.IsNullOrEmpty(XPath))
            {
                mode = WebEventSelectionMode.XPath;
                selector = FlowBloxFieldHelper.ReplaceFieldsInString(XPath);
            }
            else
            {
                throw new InvalidOperationException("Both XPath and CSSSelector are empty.");
            }
        }

        public override List<Type> NotificationTypes
        {
            get
            {
                var notificationTypes = base.NotificationTypes;
                notificationTypes.Add(typeof(WebBrowserActionStatus));
                return notificationTypes;
            }
        }

        protected virtual void HandleFailure(BaseRuntime runtime, WebBrowserActionResult result)
        {
            if (result.Exception != null)
                runtime.Report("A problem occurred while executing the web action.", FlowBloxLogLevel.Error, result.Exception);

            HandleFailure(runtime, result.Status);
        }

        protected virtual void HandleFailure(BaseRuntime runtime, Enum? enumValue)
        {
            if (enumValue != null)
                CreateNotification(runtime, enumValue);

            var webBrowserFlowBlock = AssociatedWebBrowser ?? GetPreviousFlowBlockOnPath<WebBrowserFlowBlock>(this);
            var webBrowser = webBrowserFlowBlock.InternalWebBrowser;
            GenerateResult(runtime, webBrowser.DOMContent);
        }
    }
}
