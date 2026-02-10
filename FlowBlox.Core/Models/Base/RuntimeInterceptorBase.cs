using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.Core.Models.Base
{
    public class RuntimeInterceptorBase : IRuntimeInterceptor
    {
        public FlowBloxProject Project { get; set; }

        public BaseRuntime Runtime { get; set; }

        public virtual void NotifyError(BaseFlowBlock baseFlowBlock, string message, Exception exception = null)
        {
            
        }

        public virtual void NotifyFieldChange(FieldElement fieldElement)
        {
            
        }

        public virtual void NotifyRuntimeFinished()
        {
            
        }

        public virtual void NotifyRuntimeStarted()
        {
            
        }

        public virtual void NotifyInvocationStarted(BaseFlowBlock flowBlock)
        {

        }

        public virtual void NotifyInvocationFinished(BaseFlowBlock flowBlock)
        {

        }

        public virtual void NotifyWarning(BaseFlowBlock baseFlowBlock, string message)
        {
            
        }
    }
}
