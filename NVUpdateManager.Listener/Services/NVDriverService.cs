using NVUpdateManager.Core;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Listener.Data;

namespace NVUpdateManager.Listener.Services
{
    public class NVDriverService : IDriverService
    {
        private readonly IDriverManager _driverManager;

        public NVDriverService(IDriverManager driverManager)
        {
            _driverManager = driverManager;
        }

        public async Task<DriverInfo> GetDriverInfoAsync()
        {
            return await _driverManager.GetInstalledDriverInfo();
        }
    }
}
