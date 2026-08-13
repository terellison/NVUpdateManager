using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Notifications.Tests
{
    internal sealed class FakeNotificationChannel : INotificationChannel
    {
        public FakeNotificationChannel(string name, bool isConfigured = true, Exception? failure = null)
        {
            Name = name;
            IsConfigured = isConfigured;
            Failure = failure;
        }

        public string Name { get; }

        public bool IsConfigured { get; }

        public Exception? Failure { get; }

        public List<NotificationMessage> Sent { get; } = new();

        public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            if (Failure != null)
            {
                throw Failure;
            }

            Sent.Add(message);

            return Task.CompletedTask;
        }
    }
}
