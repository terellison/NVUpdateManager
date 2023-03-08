using NVUpdateManager.Core;
using NVUpdateManager.Listener.Data;
using static NVUpdateManager.Core.DriverManager;

namespace NVUpdateManager.Listener.Services
{
    public class NVDriverService : IDriverService
    {
        public async Task<DriverInfo> GetDriverInfoAsync()
        {
            return await GetInstalledDriverInfo();
        }
    }
}
