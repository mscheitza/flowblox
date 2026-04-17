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

    public abstract class FlowBlockGenerationStrategyExecutorBase
    {
        protected readonly BaseFlowBlock FlowBlock;

        protected FlowBlockGenerationStrategyExecutorBase(BaseFlowBlock flowBlock)
        {
            FlowBlock = flowBlock;
        }

        public event EventHandler<LogCreatedEventArgs> LogCreated;

        protected bool ExecuteGenerationStrategies(
            BaseRuntime generationRuntime,
            IReadOnlyDictionary<FlowBloxTestDefinition, FlowBloxTestResult> testResults)
        {
            foreach (var flowBloxGenerationStrategy in FlowBlock.GenerationStrategies)
            {
                object result;
                try
                {
                    result = flowBloxGenerationStrategy.Execute(generationRuntime, new Dictionary<FlowBloxTestDefinition, FlowBloxTestResult>(testResults));
                }
                catch (Exception e)
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

                OnLogCreated(new LogCreatedEventArgs($"Generation strategy \"{flowBloxGenerationStrategy.Name}\" successful.", FlowBloxLogLevel.Success));
                flowBloxGenerationStrategy.Assign(result);
            }

            return true;
        }

        protected virtual void OnLogCreated(LogCreatedEventArgs e)
        {
            LogCreated?.Invoke(this, e);
        }
    }
}

