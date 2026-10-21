using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public class FlowBlockGenerationStrategyExecutor : FlowBlockGenerationStrategyExecutorBase
    {
        public FlowBlockGenerationStrategyExecutor(BaseFlowBlock flowBlock)
            : base(flowBlock)
        {

        }

        /// <summary>Reports completed tests before shutdown or the next execution overwrites result datasets.</summary>
        public event Action<FlowBloxTestDefinition, FlowBloxTestResult, string>? TestExecuted;

        public bool ExecuteGeneration()
        {
            return Task.Run(async() => await ExecuteGenerationAsync())
                .GetAwaiter()
                .GetResult();
        }

        public async Task<bool> ExecuteGenerationAsync()
        {
            var testResults = new Dictionary<FlowBloxTestDefinition, FlowBloxTestResult>();
            FlowBloxTestExecutor runtimeOwnerTestExecutor = null;
            BaseRuntime generationRuntime = null;

            var success = true;
            try
            {
                foreach (var testDefinition in FlowBlock.TestDefinitions)
                {
                    var includedFlowBlocks = testDefinition.Entries?
                        .Where(x => x.Execute)
                        .Select(x => x.FlowBlock)
                        .ExceptNull()
                        .Where(x => !ReferenceEquals(x, FlowBlock))
                        .ToList();

                    var testExecutor = new FlowBloxTestExecutor();
                    testExecutor.Initialize(testDefinition, FlowBlock, includedFlowBlocks);
                    var runtime = testExecutor.GetRuntime();
                    runtime.LogMessageCreated += Runtime_LogMessageCreated;

                    var testResult = await testExecutor.ExecuteTestAsync();
                    TestExecuted?.Invoke(testDefinition, testResult, "BeforeGeneration");
                    if (!testResult.Success)
                    {
                        OnLogCreated(new LogCreatedEventArgs($"Test case \"{testDefinition.Name}\" failed, so the generation strategies are executed.", FlowBloxLogLevel.Info));
                        success = false;
                    }

                    if (runtimeOwnerTestExecutor == null)
                    {
                        runtimeOwnerTestExecutor = testExecutor;
                        generationRuntime = runtime;
                    }
                    else
                    {
                        testExecutor.Shutdown();
                    }

                    testResults[testDefinition] = testResult;
                }

                if (!success)
                {
                    if (!ExecuteGenerationStrategies(generationRuntime, testResults))
                        return false;

                    foreach (var testDefinition in FlowBlock.TestDefinitions)
                    {
                        var includedFlowBlocks = testDefinition.Entries?
                            .Where(x => x.Execute)
                            .Select(x => x.FlowBlock)
                            .ExceptNull()
                            .Where(x => !ReferenceEquals(x, FlowBlock))
                            .Distinct()
                            .ToList();

                        var testExecutor = new FlowBloxTestExecutor();
                        testExecutor.Initialize(testDefinition, FlowBlock, includedFlowBlocks);

                        var testResult1 = await testExecutor.ExecuteTestAsync();
                        TestExecuted?.Invoke(testDefinition, testResult1, "AfterGeneration");
                        testExecutor.Shutdown();

                        if (!testResult1.Success)
                        {
                            OnLogCreated(new LogCreatedEventArgs($"Test case \"{testDefinition.Name}\" failed after regeneration.", FlowBloxLogLevel.Error));
                            return false;
                        }
                        else
                        {
                            OnLogCreated(new LogCreatedEventArgs($"Test case \"{testDefinition.Name}\" successful after regeneration.", FlowBloxLogLevel.Success));
                        }
                    }
                }

                return true;
            }
            finally
            {
                runtimeOwnerTestExecutor?.Shutdown();
            }
        }

        private void Runtime_LogMessageCreated(BaseRuntime runtime, string message, FlowBloxLogLevel logLevel)
        {
            OnLogCreated(new LogCreatedEventArgs(message, logLevel));
        }
    }
}
