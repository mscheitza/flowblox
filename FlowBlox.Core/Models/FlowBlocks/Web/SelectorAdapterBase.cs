using FlowBlox.Core.Util.Fields;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    internal abstract class SelectorAdapterBase
    {
        public bool TryGetSelector(out WebEventSelectionMode mode, out string selector)
        {
            mode = default;
            selector = string.Empty;

            if (!string.IsNullOrEmpty(CSSSelector))
            {
                mode = WebEventSelectionMode.CssSelector;
                selector = FlowBloxFieldHelper.ReplaceFieldsInString(CSSSelector);
            }
            else if (!string.IsNullOrEmpty(XPathSelector))
            {
                mode = WebEventSelectionMode.XPath;
                selector = FlowBloxFieldHelper.ReplaceFieldsInString(XPathSelector);
            }
            else
            {
                HandleMissingSelector();
                return false;
            }

            if (string.IsNullOrWhiteSpace(selector))
            {
                HandleMissingSelector();
                return false;
            }

            return true;
        }

        protected abstract string CSSSelector { get; }
        protected abstract string XPathSelector { get; }
        protected abstract void HandleMissingSelector();
    }
}
