using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Notifications.Data;
using static NVUpdateManager.EmailHandler.EmailHandler;

namespace NVUpdateManager.Notifications.Channels
{
    /// <summary>
    /// Relays through an Azure Logic App.
    ///
    /// This was the only way to send mail before the SMTP channel existed. It is kept so that
    /// installations already pointing at a Logic App keep working, but it is no longer needed
    /// and no longer the default.
    /// </summary>
    internal sealed class LogicAppNotificationChannel : INotificationChannel
    {
        private readonly IOptions<NotificationOptions> _options;

        public LogicAppNotificationChannel(IOptions<NotificationOptions> options)
        {
            _options = options;
        }

        public string Name => "LogicApp";

        private LogicAppOptions LogicApp => _options.Value.LogicApp;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(LogicApp.EncryptedAzLogicAppEndpoint)
            && !string.IsNullOrWhiteSpace(LogicApp.Entropy)
            && !string.IsNullOrWhiteSpace(LogicApp.NotificationAddress);

        public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            ConfigureLogicAppEndpoint(LogicApp.Entropy, LogicApp.EncryptedAzLogicAppEndpoint);
            ConfigureAddresses(LogicApp.NotificationAddress);

            SendNotificationEmail(message.Subject, message.HtmlBody);

            return Task.CompletedTask;
        }
    }
}
