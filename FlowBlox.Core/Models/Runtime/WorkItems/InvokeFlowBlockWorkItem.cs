using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Models.Runtime.WorkItems
{
    internal sealed class InvokeFlowBlockWorkItem : IRuntimeWorkItem
    {
        private readonly BaseFlowBlock _block;
        private readonly object _data;

        public InvokeFlowBlockWorkItem(BaseFlowBlock block, object data)
        {
            _block = block;
            _data = data;
        }

        public void Run(BaseRuntime runtime)
        {
            _block.Execute(runtime, _data);
        }
    }
}
