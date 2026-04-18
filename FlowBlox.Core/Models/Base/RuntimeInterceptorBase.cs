using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Runtime.Debugging;

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

        public virtual void NotifyFieldChange(FieldElement fieldElement, string oldValue, string newValue)
        {

        }

        public virtual void NotifyRuntimeFinished()
        {
            
        }

        public virtual void NotifyRuntimeCancelled(RuntimeCancellationContext cancellationContext)
        {

        }

        public virtual void NotifyRuntimeAborted(Exception exception)
        {

        }

        public virtual void NotifyBeforeRuntimeStarted()
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

        public virtual void NotifyBeforeFlowBlockValidation(BaseFlowBlock flowBlock)
        {

        }

        public virtual bool ShouldCancelValidation(BaseFlowBlock flowBlock, bool validationFinished)
        {
            return false;
        }

        public virtual void NotifyPreconditionsNotMet(BaseFlowBlock flowBlock, IReadOnlyList<string> messages)
        {

        }

        public virtual void NotifyIterationStarted(BaseFlowBlock flowBlock)
        {

        }

        public virtual void NotifyIterationFinished(BaseFlowBlock flowBlock)
        {

        }

        public virtual void NotifyResultDatasetsGenerated(RuntimeResultDatasetSummary resultDatasetSummary)
        {

        }

        public virtual void NotifyWarning(BaseFlowBlock baseFlowBlock, string message)
        {
            
        }
    }
}
