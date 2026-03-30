using System;
using System.Collections.Generic;
using System.Linq;
using FlowBlox.Interfaces;

namespace FlowBlox.Services
{
    public class AppNotificationService : IAppNotificationService
    {
        private readonly object _lock = new object();
        private readonly List<AppNotification> _notifications = [];

        public event EventHandler NotificationsChanged;

        public AppNotification GetCurrentNotification()
        {
            lock (_lock)
            {
                RemoveExpiredNotificationsInternal();
                return _notifications
                    .OrderByDescending(x => x.CreatedUtc)
                    .FirstOrDefault();
            }
        }

        public IReadOnlyCollection<AppNotification> GetActiveNotifications()
        {
            lock (_lock)
            {
                RemoveExpiredNotificationsInternal();
                return _notifications
                    .OrderByDescending(x => x.CreatedUtc)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public void Publish(AppNotification notification)
        {
            if (notification == null)
                return;

            lock (_lock)
            {
                RemoveExpiredNotificationsInternal();
                _notifications.RemoveAll(x => x.Id == notification.Id);
                _notifications.Add(notification);
            }

            NotificationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dismiss(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                return;

            var removed = false;

            lock (_lock)
            {
                removed = _notifications.RemoveAll(x => x.Id == notificationId) > 0;
            }

            if (removed)
                NotificationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveExpiredNotifications()
        {
            var removed = false;

            lock (_lock)
            {
                removed = RemoveExpiredNotificationsInternal();
            }

            if (removed)
                NotificationsChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool RemoveExpiredNotificationsInternal()
        {
            return _notifications.RemoveAll(x => x.IsExpired) > 0;
        }
    }
}
