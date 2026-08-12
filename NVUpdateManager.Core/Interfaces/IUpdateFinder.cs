using NVUpdateManager.Core.Data;
using System.Threading;
using System.Threading.Tasks;

namespace NVUpdateManager.Core.Interfaces
{
    public interface IUpdateFinder
    {
        Task<string> DownloadUpdate(string updateLink);

        /// <summary>
        /// Finds the newest driver NVIDIA publishes for a GPU on the current operating system.
        /// </summary>
        Task<UpdateInfo> FindLatestUpdate(GpuProduct product, DriverBranch branch, CancellationToken cancellationToken = default);
    }
}
