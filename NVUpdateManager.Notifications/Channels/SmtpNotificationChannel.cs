using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Notifications.Data;

namespace NVUpdateManager.Notifications.Channels
{
    /// <summary>
    /// Sends the notification as ordinary email through any mail account.
    ///
    /// This is what replaces the Azure Logic App: the same result, needing a host, a username and
    /// an app password in configuration rather than a provisioned cloud resource.
    /// </summary>
    internal sealed class SmtpNotificationChannel : INotificationChannel
    {
        private readonly IOptions<NotificationOptions> _options;
        private readonly ISmtpTransport _transport;

        public SmtpNotificationChannel(IOptions<NotificationOptions> options, ISmtpTransport transport)
        {
            _options = options;
            _transport = transport;
        }

        public string Name => "Smtp";

        private SmtpOptions Smtp => _options.Value.Smtp;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Smtp.Host)
            && !string.IsNullOrWhiteSpace(Smtp.ResolvedFrom)
            && !string.IsNullOrWhiteSpace(Smtp.ResolvedTo);

        public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            return _transport.SendAsync(BuildEnvelope(message), Smtp, cancellationToken);
        }

        /// <summary>
        /// Everything about the message that does not depend on being able to reach a mail server,
        /// separated so it can be asserted on directly.
        /// </summary>
        internal SmtpEnvelope BuildEnvelope(NotificationMessage message)
        {
            return new SmtpEnvelope(
                fromAddress: Smtp.ResolvedFrom,
                fromName: Smtp.FromName,
                toAddress: Smtp.ResolvedTo,
                subject: message.Subject,
                htmlBody: message.HtmlBody);
        }
    }
}
