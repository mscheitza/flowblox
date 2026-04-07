using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;

namespace FlowBlox.Core.Models.Runtime
{
    public class TransientRuntime : BaseRuntime
    {
        public string? TargetFlowBlockName { get; set; }

        public TransientRuntime(FlowBloxProject project) : base(project)
        {
            this.ExecutionFlowEnabled = false;
            this.DisableInterceptors = true;
        }

        public void InitializeRuntime(List<BaseFlowBlock> capturedFlowBlocks)
        {
            base.OnBeforeRuntimeStarted(capturedFlowBlocks);
        }

        public void ShutdownRuntime(List<BaseFlowBlock> capturedFlowBlocks)
        {
            base.OnAfterRuntimeFinished(capturedFlowBlocks);
        }

        protected override bool ShouldCancelValidation(BaseFlowBlock flowBlock, bool validationFinished)
        {
            if (!validationFinished
                && !string.IsNullOrWhiteSpace(TargetFlowBlockName)
                && flowBlock != null
                && string.Equals(flowBlock.Name, TargetFlowBlockName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return base.ShouldCancelValidation(flowBlock, validationFinished);
        }

        protected override bool ShouldExecuteInputTemplateStartupCommands() => false;
    }
}
