using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime.WorkItems;

namespace FlowBlox.Core.Models.Runtime
{
    public sealed class RuntimeTaskRunner
    {
        private readonly BaseRuntime _runtime;
        private readonly Stack<IRuntimeWorkItem> _stack = new();

        public void Run(BaseFlowBlock startFlowBlock)
        {
            EnqueueFlowBlock(startFlowBlock, null);

            while (_stack.Count > 0)
            {
                _runtime.HandlePause();

                var item = _stack.Pop();
                item.Run(_runtime);
            }
        }

        public RuntimeTaskRunner(BaseRuntime runtime)
        {
            _runtime = runtime;
        }

        public void Enqueue(IRuntimeWorkItem item)
        {
            if (item == null) 
                return;
            
            _stack.Push(item);
        }

        public void EnqueueFlowBlock(BaseFlowBlock block, object data)
        {
            Enqueue(new InvokeFlowBlockWorkItem(block, data));
        }

        public void EnqueueBatchInExecutionOrder(IList<IRuntimeWorkItem> items)
        {
            if (items == null)
                return;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item != null)
                    _stack.Push(item);
            }
        }

        /// <summary>
        /// Schedules the successor flow blocks of the specified <paramref name="current"/> block
        /// via the <see cref="RuntimeTaskRunner"/> without using recursive calls.
        /// 
        /// This method replaces the former direct invocation of <c>ExecuteNextFlowBlocks</c>
        /// and ensures a stack-safe, depth-first execution model based on work items.
        /// 
        /// Execution semantics:
        /// - Input-reference blocks are executed immediately to register their inputs
        ///   (no actual processing is performed at this stage).
        /// - Non-input-reference blocks are enqueued as work items and executed later
        ///   by the runner.
        /// - <c>IterationStart</c> and <c>IterationEnd</c> are also scheduled as dedicated
        ///   work items to ensure correct execution order and isolation.
        /// 
        /// The runner processes all scheduled items in LIFO order to emulate the
        /// original recursive depth-first behavior while keeping the call stack flat.
        /// </summary>
        /// <param name="current">
        /// The flow block whose successor blocks should be scheduled.
        /// </param>
        public void ScheduleNext(BaseFlowBlock current)
        {
            if (!_runtime.ExecutionFlowEnabled)
                return;

            var nextBlocks = current.GetNextFlowBlocks();
            foreach (var next in nextBlocks)
            {
                if (next.HasInputReference == true)
                    next.Execute(_runtime, current);
            }

            var items = new List<IRuntimeWorkItem>()
            {
                new IterationStartWorkItem(current)
            };

            foreach (var next in nextBlocks)
            {
                if (next == null) 
                    continue;

                if (next.HasInputReference) 
                    continue;

                items.Add(new InvokeFlowBlockWorkItem(next, current));
            }

            items.Add(new IterationEndWorkItem(current));

            EnqueueBatchInExecutionOrder(items);
        }
    }
}