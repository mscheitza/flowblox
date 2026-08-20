namespace FlowBlox.Core.Models.Runtime.WorkItems
{
    internal sealed class ExecutionLayerWorkItem : IRuntimeWorkItem
    {
        private readonly int _change;

        private ExecutionLayerWorkItem(int change)
        {
            _change = change;
        }

        public static ExecutionLayerWorkItem Increase() => new(1);

        public static ExecutionLayerWorkItem Decrease() => new(-1);

        public void Run(BaseRuntime runtime)
        {
            if (_change > 0)
                runtime.IncreaseExecutionLayer();
            else
                runtime.DecreaseExecutionLayer();
        }
    }
}
