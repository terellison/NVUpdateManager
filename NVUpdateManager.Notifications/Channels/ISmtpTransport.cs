using System.Threading;
using System.Threading.Tasks;
using NVUpdateManager.Notifications.Data;

namespace NVUpdateManager.Notifications.Channels
{
    /// <summary>
    /// Puts a composed message on the wire. Kept separate from the channel so that composing the
    /// message can be tested without a mail server.
    /// </summary>
    internal interface ISmtpTransport
    {
        Task SendAsync(SmtpEnvelope envelope, SmtpOptions options, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// A composed message, in the terms a mail server needs and nothing more.
    /// </summary>
    internal sealed class SmtpEnvelope
    {
        public SmtpEnvelope(string fromAddress, string fromName, string toAddress, string subject, string htmlBody)
        {
            FromAddress = fromAddress;
            FromName = fromName;
            ToAddress = toAddress;
            Subject = subject;
            HtmlBody = htmlBody;
        }

        public string FromAddress { get; }
        public string FromName { get; }
        public string ToAddress { get; }
        public string Subject { get; }
        public string HtmlBody { get; }
    }
}
