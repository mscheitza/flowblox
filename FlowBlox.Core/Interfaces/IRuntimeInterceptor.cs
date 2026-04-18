using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Models.Runtime.Debugging;

namespace FlowBlox.Core.Interfaces
{
    public interface IRuntimeInterceptor
    {
        FlowBloxProject Project { get; set; }

        BaseRuntime Runtime { get; set; }

        void NotifyWarning(BaseFlowBlock flowBlock, string message);

        void NotifyError(BaseFlowBlock flowBlock, string message, Exception exception = null);

        void NotifyFieldChange(FieldElement fieldElement);
        void NotifyFieldChange(FieldElement fieldElement, string oldValue, string newValue);

        void NotifyBeforeRuntimeStarted();
        void NotifyRuntimeStarted();
        void NotifyRuntimeFinished();
        void NotifyRuntimeCancelled(RuntimeCancellationContext cancellationContext);
        void NotifyRuntimeAborted(Exception exception);

        void NotifyInvocationStarted(BaseFlowBlock flowBlock);
        void NotifyInvocationFinished(BaseFlowBlock flowBlock);
        void NotifyBeforeFlowBlockValidation(BaseFlowBlock flowBlock);
        bool ShouldCancelValidation(BaseFlowBlock flowBlock, bool validationFinished);
        void NotifyPreconditionsNotMet(BaseFlowBlock flowBlock, IReadOnlyList<string> messages);
        void NotifyIterationStarted(BaseFlowBlock flowBlock);
        void NotifyIterationFinished(BaseFlowBlock flowBlock);
        void NotifyResultDatasetsGenerated(RuntimeResultDatasetSummary resultDatasetSummary);
    }
}
