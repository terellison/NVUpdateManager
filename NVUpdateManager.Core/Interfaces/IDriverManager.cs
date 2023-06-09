using NVUpdateManager.Core.Data;
using System.Threading.Tasks;

namespace NVUpdateManager.Core.Interfaces
{
    internal interface IDriverManager
    {
        Task<DriverInfo> GetInstalledDriverInfo();
        Task<UpdateResult> InstallUpdate(string downloadLink);
    }
}