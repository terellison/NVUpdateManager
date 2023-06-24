using NVUpdateManager.Core;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Listener.Data;

namespace NVUpdateManager.Listener.Services
{
    public class DriverUpdateService : IUpdateService
    {

        private readonly IDriverManager _driverManager;

        public DriverUpdateService(IDriverManager driverManager)
        {
            _driverManager = driverManager;
        }

        public async Task<UpdateResult> InstallUpdate(string downloadLink)
        {
            var result = await _driverManager.InstallUpdate(downloadLink);
            return result;
            
        }
    }
}
