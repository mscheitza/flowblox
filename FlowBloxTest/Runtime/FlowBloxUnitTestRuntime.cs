using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Project;
using System.Collections.Generic;
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

            this.StartFlowBlock.Execute(this, null);

            this.Running = false;
            this.OnAfterRuntimeFinished();
            this.NotifyRuntimeFinished();
        }
    }
}