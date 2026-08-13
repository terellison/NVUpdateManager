using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NVUpdateManager.Notifications.Data;

namespace NVUpdateManager.Notifications.Channels
{
    /// <summary>
    /// Talks SMTP.
    ///
    /// Deliberately the only type here that knows about MailKit, and deliberately free of
    /// decisions - it hands an already composed message to a mail server. Anything worth
    /// asserting on happens before it gets here.
    /// </summary>
    internal sealed class MailKitSmtpTransport : ISmtpTransport
    {
        public async Task SendAsync(SmtpEnvelope envelope, SmtpOptions options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(options.Host))
            {
                throw new InvalidOperationException("No SMTP host is configured.");
            }

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(envelope.FromName, envelope.FromAddress));
            message.To.Add(MailboxAddress.Parse(envelope.ToAddress));
            message.Subject = envelope.Subject;
            message.Body = new BodyBuilder { HtmlBody = envelope.HtmlBody }.ToMessageBody();

            using (var client = new SmtpClient())
            {
                var security = options.UseStartTls
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.SslOnConnect;

                await client.ConnectAsync(options.Host, options.Port, security, cancellationToken)
                    .ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(options.Username))
                {
                    await client.AuthenticateAsync(options.Username, options.Password ?? string.Empty, cancellationToken)
                        .ConfigureAwait(false);
                }

                await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
                await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
