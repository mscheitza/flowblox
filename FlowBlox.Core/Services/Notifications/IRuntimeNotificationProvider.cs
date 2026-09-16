using FlowBlox.Core.Models.Notifications;

namespace FlowBlox.Core.Services.Notifications
{
    public interface IRuntimeNotificationProvider
    {
        string Name { get; }

        void Send(RuntimeNotificationConfiguration configuration, RuntimeNotificationContext context);
    }
}
