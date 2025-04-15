using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.Core.Interfaces
{
    public interface IRuntimeInterceptor
    {
        FlowBloxProject Project { get; set; }

        BaseRuntime Runtime { get; set; }

        void NotifyWarning(BaseFlowBlock flowBlock, string message);

        void NotifyError(BaseFlowBlock flowBlock, string message, Exception exception = null);

        void NotifyFieldChange(FieldElement fieldElement);
        
        void NotifyRuntimeStarted();
        void NotifyRuntimeFinished();

        void NotifyInvocationStarted(BaseFlowBlock flowBlock);
        void NotifyInvocationFinished(BaseFlowBlock flowBlock);
    }
}
