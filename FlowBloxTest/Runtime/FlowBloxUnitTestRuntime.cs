using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;

namespace FlowBlox.Test.Runtime
{
    public class FlowBloxUnitTestRuntime : BaseRuntime
    {
        public FlowBloxUnitTestRuntime(FlowBloxProject project) : base(project)
        {
        }

        public void Execute()
        {
            this.Running = true;

            this.OnBeforeRuntimeStarted();
            this.NotifyRuntimeStarted();

            this.TaskRunner.Run(this.StartFlowBlock);

            this.Running = false;
            this.OnAfterRuntimeFinished();
            this.NotifyRuntimeFinished();
        }
    }
}