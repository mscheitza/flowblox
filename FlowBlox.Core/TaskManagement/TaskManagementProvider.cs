using FlowBlox.Core.DependencyInjection;

namespace FlowBlox.Core.TaskManagement
{
    public static class TaskManagementProvider
    {
        public static ITaskManagementService GetService()
        {
            var service = FlowBloxServiceLocator.Instance.GetService<ITaskManagementService>();
            if (service != null)
                return service;

            if (!OperatingSystem.IsWindows())
                throw new NotSupportedException("FlowBlox task management is currently only supported on Windows.");

            throw new NotSupportedException("No Windows task management service is registered.");
        }
    }
}
