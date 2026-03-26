using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Models.Runtime.WorkItems
{
    internal sealed class IterationEndWorkItem : IRuntimeWorkItem
    {
        private readonly BaseFlowBlock _current;

        public IterationEndWorkItem(BaseFlowBlock current) => _current = current;

        public void Run(BaseRuntime runtime)
        {
            _current.RaiseIterationEnd(runtime);
            runtime.NotifyIterationFinished(_current);
        }
    }
}
