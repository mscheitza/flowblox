using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.Core.Models.FlowBlocks.WebBrowser
{
    internal sealed class WebActionFlowBlockSelectorAdapter : SelectorAdapterBase
    {
        private readonly WebActionFlowblockBase _flowBlock;
        private readonly BaseRuntime _runtime;
        private readonly Action<BaseRuntime> _onMissingSelector;

        public WebActionFlowBlockSelectorAdapter(
            WebActionFlowblockBase flowBlock,
            BaseRuntime runtime,
            Action<BaseRuntime> onMissingSelector)
        {
            _flowBlock = flowBlock ?? throw new ArgumentNullException(nameof(flowBlock));
            _runtime = runtime;
            _onMissingSelector = onMissingSelector ?? throw new ArgumentNullException(nameof(onMissingSelector));
        }

        protected override string CSSSelector => _flowBlock.CSSSelector;
        protected override string XPathSelector => _flowBlock.XPath;

        protected override void HandleMissingSelector()
        {
            _onMissingSelector(_runtime);
        }
    }
}
