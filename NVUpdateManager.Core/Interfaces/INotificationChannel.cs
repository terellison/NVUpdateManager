using System.Threading;
using System.Threading.Tasks;
using NVUpdateManager.Core.Data;

namespace NVUpdateManager.Core.Interfaces
{
    /// <summary>
    /// One way of telling somebody an update is available.
    /// </summary>
    public interface INotificationChannel
    {
        /// <summary>
        /// How this channel is named in configuration and in logs.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Whether this channel has everything it needs to deliver. A channel that requires
        /// credentials reports false until they are supplied, which is what lets the application
        /// fall back to one that needs no setup.
        /// </summary>
        bool IsConfigured { get; }

        Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    }
}
