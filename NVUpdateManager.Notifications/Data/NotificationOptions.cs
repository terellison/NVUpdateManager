using System.Collections.Generic;

namespace NVUpdateManager.Notifications.Data
{
    /// <summary>
    /// How an SMTP connection is secured.
    /// </summary>
    public enum SmtpSecurity
    {
        /// <summary>Upgrade to TLS after connecting. Port 587, and what most providers want.</summary>
        StartTls,

        /// <summary>TLS from the first byte. Port 465.</summary>
        SslOnConnect,

        /// <summary>No encryption. For a relay on localhost, or a mail catcher while testing.</summary>
        None
    }

    /// <summary>
    /// How update notifications are delivered. Everything here is optional: with no configuration
    /// at all the application falls back to whichever channels need no setup, which on Windows
    /// means a desktop notification.
    /// </summary>
    public sealed class NotificationOptions
    {
        /// <summary>
        /// Which channels to use, by name ("Toast", "Smtp", "LogicApp"). Leave empty to use every
        /// channel that has what it needs.
        /// </summary>
        public List<string> Channels { get; set; } = new List<string>();

        public SmtpOptions Smtp { get; set; } = new SmtpOptions();

        public LogicAppOptions LogicApp { get; set; } = new LogicAppOptions();
    }

    /// <summary>
    /// An ordinary mail account to send through. Any provider works; for Gmail or Outlook this
    /// needs an app password rather than the account password.
    /// </summary>
    public sealed class SmtpOptions
    {
        public string? Host { get; set; }

        public int Port { get; set; } = 587;

        /// <summary>
        /// How the connection is secured. STARTTLS on port 587 is what nearly every provider
        /// expects; implicit TLS is port 465; None exists for a local relay or a mail catcher
        /// used while testing.
        /// </summary>
        public SmtpSecurity Security { get; set; } = SmtpSecurity.StartTls;

        public string? Username { get; set; }

        public string? Password { get; set; }

        /// <summary>The sender address. Defaults to <see cref="Username"/> when left unset.</summary>
        public string? From { get; set; }

        public string FromName { get; set; } = "NVUpdateManager";

        /// <summary>Where to send. Defaults to <see cref="From"/> when left unset.</summary>
        public string? To { get; set; }

        public string ResolvedFrom => string.IsNullOrWhiteSpace(From) ? Username ?? string.Empty : From;

        public string ResolvedTo => string.IsNullOrWhiteSpace(To) ? ResolvedFrom : To;
    }

    /// <summary>
    /// The original Azure Logic App relay, kept for installations already using it.
    /// </summary>
    public sealed class LogicAppOptions
    {
        public string? EncryptedAzLogicAppEndpoint { get; set; }

        public string? Entropy { get; set; }

        public string? NotificationAddress { get; set; }
    }
}
