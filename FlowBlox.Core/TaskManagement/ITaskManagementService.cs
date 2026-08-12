namespace FlowBlox.Core.TaskManagement
{
    public interface ITaskManagementService
    {
        Task<IReadOnlyList<FlowBloxScheduledTask>> GetTasksAsync(CancellationToken cancellationToken = default);
        Task CreateTaskAsync(FlowBloxScheduledTask task, CancellationToken cancellationToken = default);
        Task UpdateTaskAsync(FlowBloxScheduledTask task, CancellationToken cancellationToken = default);
        Task DeleteTaskAsync(string taskName, CancellationToken cancellationToken = default);
        Task RunTaskAsync(string taskName, CancellationToken cancellationToken = default);
        Task StopTaskAsync(string taskName, CancellationToken cancellationToken = default);
        Task<bool> IsTaskRunningAsync(string taskName, CancellationToken cancellationToken = default);
        Task EnableTaskAsync(string taskName, CancellationToken cancellationToken = default);
        Task DisableTaskAsync(string taskName, CancellationToken cancellationToken = default);
    }
}
