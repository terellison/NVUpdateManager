using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Notifications.Data;

namespace NVUpdateManager.Notifications
{
    /// <summary>
    /// Decides which channels deliver a notification, and sees that one failing does not stop
    /// the others.
    ///
    /// With nothing configured it uses every channel that has what it needs, which is what makes
    /// the application work on a fresh install: the desktop notification requires no setup, so
    /// there is always at least one.
    /// </summary>
    internal sealed class NotificationDispatcher : INotificationDispatcher
    {
        private readonly IEnumerable<INotificationChannel> _channels;
        private readonly IOptions<NotificationOptions> _options;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            IEnumerable<INotificationChannel> channels,
            IOptions<NotificationOptions> options,
            ILogger<NotificationDispatcher> logger)
        {
            _channels = channels;
            _options = options;
            _logger = logger;
        }

        public async Task<IReadOnlyList<string>> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            var selected = SelectChannels();

            if (selected.Count == 0)
            {
                _logger.LogWarning(
                    "No notification channel is available, so this update will not be announced. "
                    + "Configure Notifications:Smtp, or run where a desktop notification can be shown.");

                return Array.Empty<string>();
            }

            var delivered = new List<string>();

            foreach (var channel in selected)
            {
                try
                {
                    await channel.SendAsync(message, cancellationToken).ConfigureAwait(false);

                    delivered.Add(channel.Name);

                    _logger.LogInformation("Notified via {Channel}", channel.Name);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    /* One channel failing is not a reason to skip the rest, and certainly not a
                     * reason to fail the update check that found the driver.
                     */

                    _logger.LogError(ex, "The {Channel} notification channel failed", channel.Name);
                }
            }

            return delivered;
        }

        /// <summary>
        /// Works out which channels to use: those named in configuration, or every channel that
        /// is ready when none are named.
        /// </summary>
        internal IReadOnlyList<INotificationChannel> SelectChannels()
        {
            var requested = _options.Value.Channels;

            if (requested == null || requested.Count == 0)
            {
                return _channels.Where(c => c.IsConfigured).ToList();
            }

            var selected = new List<INotificationChannel>();

            foreach (var name in requested)
            {
                var channel = _channels.FirstOrDefault(
                    c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

                if (channel == null)
                {
                    _logger.LogWarning(
                        "Notification channel {Channel} is configured but does not exist. Known channels: {Known}",
                        name,
                        string.Join(", ", _channels.Select(c => c.Name)));

                    continue;
                }

                if (!channel.IsConfigured)
                {
                    _logger.LogWarning(
                        "Notification channel {Channel} is configured but is missing settings it needs, so it is being skipped",
                        channel.Name);

                    continue;
                }

                selected.Add(channel);
            }

            return selected;
        }
    }
}
