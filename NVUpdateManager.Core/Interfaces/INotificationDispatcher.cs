using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NVUpdateManager.Core.Data;

namespace NVUpdateManager.Core.Interfaces
{
    public interface INotificationDispatcher
    {
        /// <summary>
        /// Delivers a notification through every selected channel.
        /// </summary>
        /// <returns>The names of the channels that delivered it.</returns>
        Task<IReadOnlyList<string>> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    }
}
