using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Models.Runtime;
using FlowBlox.Core.Provider;
using FlowBlox.Core.Provider.Registry;
using FlowBlox.Test.Runtime;
using System.Diagnostics;

namespace FlowBloxTest.FlowBlocks
{
    public class FlowBloxTestsBase
    {
        protected FlowBloxRegistry Registry
        {
            get
            {
                var registry = FlowBloxRegistryProvider.GetRegistry();
                if (registry == null)
                    throw new InvalidOperationException(
                        "No active FlowBloxRegistry available. Ensure ActiveProject is initialized.");
                return registry;
            }
        }

        protected T CreateFlowBlock<T>(BaseFlowBlock referencedFlowBlock = null)
            where T : BaseFlowBlock
        {
            var registry = Registry;

            var createdFlowBlock = registry.CreateFlowBlockUnregistered<T>();
            registry.PostProcessFlowBlockCreated(createdFlowBlock);
            registry.Register(createdFlowBlock);

            if (referencedFlowBlock != null)
                createdFlowBlock.ReferencedFlowBlocks.Add(referencedFlowBlock);

            return createdFlowBlock;
        }

        protected FieldElement CreateUserField(string fieldName)
        {
            return Registry.CreateUserField(UserFieldTypes.Memory, fieldName: fieldName);
        }

        protected T CreateManagedObject<T>() where T : ManagedObject
        {
            var registry = Registry;

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