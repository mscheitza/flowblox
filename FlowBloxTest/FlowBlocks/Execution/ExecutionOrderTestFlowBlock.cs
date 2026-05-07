using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Runtime;
using FlowBloxTest.Constants;

namespace FlowBloxTest.FlowBlocks.Execution
{
    public class ExecutionOrderTestFlowBlock : BaseSingleResultFlowBlock
    {
        public override FlowBlockCategory GetCategory() => FlowBlockCategory.ControlFlow;

        public override FlowBlockCardinalities GetInputCardinality() => FlowBlockCardinalities.Many;

        public override bool Execute(BaseRuntime runtime, object data)
        {
            return Invoke(runtime, data, () =>
            {
                runtime.Focus(this);
                Wait(runtime);
                SetParentElement(data);
                GenerateResult(runtime, FlowBloxTestsConstants.MockFieldValue);
            });
        }
    }
}
