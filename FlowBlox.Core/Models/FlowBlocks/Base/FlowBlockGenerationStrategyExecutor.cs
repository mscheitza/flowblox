using FlowBlox.Core.Enums;
using FlowBlox.Core.Extensions;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    

    public class FlowBlockGenerationStrategyExecutor : FlowBlockGenerationStrategyExecutorBase
    {
        private readonly TestScope _scope;

        public FlowBlockGenerationStrategyExecutor(BaseFlowBlock flowBlock, TestScope scope = TestScope.RequiredForExecution)
            : base(flowBlock)
        {
            _scope = scope;
        }

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
                foreach (var testDefinition in _scope == TestScope.All ?
                     FlowBlock.TestDefinitions : 
                     FlowBlock.TestDefinitions.Where(x => x.RequiredForExecution))
                {
                    var includedFlowBlocks = testDefinition.Entries
                        .Select(x => x.FlowBlock)
                        .ExceptNull()
                        .Where(x => !ReferenceEquals(x, FlowBlock))
                        .ToList();

                    var testExecutor = new FlowBloxTestExecutor();
                    testExecutor.Initialize(testDefinition, FlowBlock, includedFlowBlocks);
                    var runtime = testExecutor.GetRuntime();
                    runtime.LogMessageCreated += Runtime_LogMessageCreated;

                    var testResult = await testExecutor.ExecuteTestAsync();
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
                        var includedFlowBlocks = testDefinition.Entries
                            .Select(x => x.FlowBlock)
                            .Where(x => x != null && !ReferenceEquals(x, FlowBlock))
                            .Distinct()
                            .ToList();

                        var testExecutor = new FlowBloxTestExecutor();
                        testExecutor.Initialize(testDefinition, FlowBlock, includedFlowBlocks);

                        var testResult1 = await testExecutor.ExecuteTestAsync();
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
