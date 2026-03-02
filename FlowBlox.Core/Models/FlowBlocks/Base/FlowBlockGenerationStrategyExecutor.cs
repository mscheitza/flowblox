using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Testing;

namespace FlowBlox.Core.Models.FlowBlocks.Base
{
    public class LogCreatedEventArgs : EventArgs
    {
        public string Message { get; }
        public FlowBloxLogLevel LogLevel { get; }

        public LogCreatedEventArgs(string message, FlowBloxLogLevel logLevel)
        {
            Message = message;
            LogLevel = logLevel;
        }
    }

    public enum TestScope
    {
        All,
        RequiredForExecution
    }

    public class FlowBlockGenerationStrategyExecutor
    {
        private BaseFlowBlock _flowBlock;
        private TestScope _scope;

        public FlowBlockGenerationStrategyExecutor(BaseFlowBlock flowBlock, TestScope scope = TestScope.RequiredForExecution)
        {
            _flowBlock = flowBlock;
            _scope = scope;
        }

        public event EventHandler<LogCreatedEventArgs> LogCreated;

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
                     _flowBlock.TestDefinitions :
                     _flowBlock.TestDefinitions.Where(x => x.RequiredForExecution))
                {
                    var testExecutor = new FlowBloxTestExecutor();
                    testExecutor.Initialize(testDefinition, _flowBlock);
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
                    foreach (var flowBloxGenerationStrategy in _flowBlock.GenerationStrategies)
                    {
                        object result;
                        try
                        {
                            result = flowBloxGenerationStrategy.Execute(generationRuntime, testResults);
                        }
                        catch(Exception e)
                        {
                            OnLogCreated(new LogCreatedEventArgs($"Generation strategy \"{flowBloxGenerationStrategy.Name}\" failed unexpectedly.", FlowBloxLogLevel.Error));
                            OnLogCreated(new LogCreatedEventArgs(e.ToString(), FlowBloxLogLevel.Error));
                            return false;
                        }

                        if (result == null)
                        {
                            OnLogCreated(new LogCreatedEventArgs($"Generation strategy \"{flowBloxGenerationStrategy.Name}\" failed. Please make sure all generation parameters are correct.", FlowBloxLogLevel.Error));
                            return false;
                        }
                        else
                        {
                            OnLogCreated(new LogCreatedEventArgs($"Generation strategy \"{flowBloxGenerationStrategy.Name}\" successful.", FlowBloxLogLevel.Success));
                            flowBloxGenerationStrategy.Assign(result);
                        }
                    }

                    foreach (var testDefinition in _flowBlock.TestDefinitions)
                    {
                        var testExecutor = new FlowBloxTestExecutor();
                        testExecutor.Initialize(testDefinition, _flowBlock);

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

        protected virtual void OnLogCreated(LogCreatedEventArgs e)
        {
            LogCreated?.Invoke(this, e);
        }
    }
}
