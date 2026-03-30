using System;
using System.Collections.Generic;
using FlowBlox.Services;

namespace FlowBlox.Interfaces
{
    public interface IAppNotificationService
    {
        event EventHandler NotificationsChanged;

        AppNotification GetCurrentNotification();

        IReadOnlyCollection<AppNotification> GetActiveNotifications();

        void Publish(AppNotification notification);

        void Dismiss(string notificationId);

        void RemoveExpiredNotifications();
    }
}
