using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public sealed class FlowBlockRuntimeRegenerationExecutor : FlowBlockGenerationStrategyExecutorBase
    {
        public FlowBlockRuntimeRegenerationExecutor(BaseFlowBlock flowBlock)
            : base(flowBlock)
        {
        }

        public bool ExecuteRegeneration(BaseRuntime runtime, FlowBloxTestDefinition testDefinition, FlowBloxTestResult testResult)
        {
            var testResults = new Dictionary<FlowBloxTestDefinition, FlowBloxTestResult>
            {
                [testDefinition] = testResult
            };

            return ExecuteRegeneration(runtime, testResults);
        }

        public bool ExecuteRegeneration(
            BaseRuntime runtime,
            IReadOnlyDictionary<FlowBloxTestDefinition, FlowBloxTestResult> testResults)
        {
            if (runtime == null)
                throw new ArgumentNullException(nameof(runtime));

            if (testResults == null)
                throw new ArgumentNullException(nameof(testResults));

            if (!FlowBlock.GenerationStrategies.Any())
            {
                OnLogCreated(new LogCreatedEventArgs(
                    $"No generation strategies configured for flow block \"{FlowBlock.Name}\".",
                    FlowBloxLogLevel.Warning));
                return false;
            }

            return ExecuteGenerationStrategies(runtime, testResults);
        }
    }
}

