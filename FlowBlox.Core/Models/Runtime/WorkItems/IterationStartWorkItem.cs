using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Core.Models.Runtime.WorkItems
{
    internal sealed class IterationStartWorkItem : IRuntimeWorkItem
    {
        private readonly BaseFlowBlock _current;

        public IterationStartWorkItem(BaseFlowBlock current) => _current = current;

        public void Run(BaseRuntime runtime)
        {
            runtime.NotifyIterationStarted(_current);
            _current.RaiseIterationStart(runtime);
        }
    }
}
