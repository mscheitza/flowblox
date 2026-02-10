using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;

namespace FlowBlox.Core.Models.Runtime
{
    public class TransientRuntime : BaseRuntime
    {
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
    }
}
