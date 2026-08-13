namespace NVUpdateManager.Core.Data
{
    /// <summary>
    /// A notification to deliver, in the several shapes the channels need. A toast wants one
    /// short line; an email wants the full release notes.
    /// </summary>
    public sealed class NotificationMessage
    {
        public NotificationMessage(string subject, string summary, string htmlBody, string downloadLink)
        {
            Subject = subject;
            Summary = summary;
            HtmlBody = htmlBody;
            DownloadLink = downloadLink;
        }

        /// <summary>
        /// One line naming what happened, for example "GeForce Game Ready Driver 610.88 available".
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// A short plain text line, for channels with no room for release notes.
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// The full message as HTML, including release notes.
        /// </summary>
        public string HtmlBody { get; }

        /// <summary>
        /// Where the driver can be downloaded.
        /// </summary>
        public string DownloadLink { get; }

        public static NotificationMessage ForUpdate(UpdateInfo update, string deviceName)
        {
            var driverName = string.IsNullOrWhiteSpace(update.Name) ? "driver" : update.Name;

            return new NotificationMessage(
                subject: $"New {driverName} update available",
                summary: $"{driverName} {update.VersionNumber} is available for your {deviceName}.",
                htmlBody: update.ToString(),
                downloadLink: update.DownloadLink);
        }
    }
}
