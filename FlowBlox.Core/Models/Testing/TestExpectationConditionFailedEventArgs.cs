using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Additions;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.Core.Models.Testing
{
    public sealed class TestExpectationConditionFailedEventArgs : EventArgs
    {
        public FlowBloxTestDefinition TestDefinition { get; }
        public BaseFlowBlock FlowBlock { get; }
        public FieldElement FieldElement { get; }
        public FlowBloxFieldTestConfiguration TestConfiguration { get; }
        public ExpectationCondition FailedCondition { get; }
        public BaseRuntime Runtime { get; }
        public FlowBloxTestResult CurrentResult { get; }

        public bool RepeatLatestExecution { get; set; }

        public TestExpectationConditionFailedEventArgs(
            FlowBloxTestDefinition testDefinition,
            BaseFlowBlock flowBlock,
            FieldElement fieldElement,
            FlowBloxFieldTestConfiguration testConfiguration,
            ExpectationCondition failedCondition,
            BaseRuntime runtime,
            FlowBloxTestResult currentResult)
        {
            TestDefinition = testDefinition;
            FlowBlock = flowBlock;
            FieldElement = fieldElement;
            TestConfiguration = testConfiguration;
            FailedCondition = failedCondition;
            Runtime = runtime;
            CurrentResult = currentResult;
        }
    }
}

