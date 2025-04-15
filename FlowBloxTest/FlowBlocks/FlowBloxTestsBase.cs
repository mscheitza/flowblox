using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Test.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowBloxTest.FlowBlocks
{
    public class FlowBloxTestsBase
    {
        protected T CreateFlowBlock<T>(FlowBloxRegistry registry, BaseFlowBlock referencedFlowBlock = null) where T : BaseFlowBlock
        {
            var createdFlowBlock = registry.CreateFlowBlockUnregistered<T>();
            registry.PostProcessFlowBlockCreated(createdFlowBlock);
            registry.Register(createdFlowBlock);

            if (referencedFlowBlock != null)
                createdFlowBlock.ReferencedFlowBlocks.Add(referencedFlowBlock);

            return createdFlowBlock;
        }

        protected FieldElement CreateUserField(FlowBloxRegistry registry, string fieldName) => registry.CreateUserField(UserFieldTypes.Memory, fieldName);

        protected T CreateManagedObject<T>(FlowBloxRegistry registry) where T : ManagedObject
        {
            var managedObject = Activator.CreateInstance<T>();
            registry.PostProcessManagedObjectCreated(managedObject);
            registry.Register(managedObject);
            return managedObject;
        }

        protected void CreateRuntimeAndExecute(FlowBloxProject project)
        {
            var runtime = new FlowBloxUnitTestRuntime(project);
            runtime.LogMessageCreated += Runtime_LogMessageCreated;
            runtime.Execute();
        }

        private void Runtime_LogMessageCreated(BaseRuntime runtime, string message, FlowBloxLogLevel logLevel)
        {
            switch (logLevel)
            {
                case FlowBloxLogLevel.Info:
                case FlowBloxLogLevel.Success:
                    Trace.TraceInformation(message);
                    break;
                case FlowBloxLogLevel.Warning:
                    Trace.TraceWarning(message);
                    break;
                case FlowBloxLogLevel.Error:
                    Trace.TraceError(message);
                    break;
                default:
                    Trace.WriteLine(message);
                    break;
            }
        }
    }
}
