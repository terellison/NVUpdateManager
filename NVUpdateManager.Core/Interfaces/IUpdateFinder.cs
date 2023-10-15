using NVUpdateManager.Core.Data;
using System.Threading.Tasks;

namespace NVUpdateManager.Core.Interfaces
{
    public interface IUpdateFinder
    {
        string DownloadUpdate(string updateLink);
        Task<UpdateInfo> FindLatestUpdate(string gpuSeries, string gpuName, string driverType);
    }
}