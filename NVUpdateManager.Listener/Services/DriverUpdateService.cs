using NVUpdateManager.Core;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Listener.Data;

namespace NVUpdateManager.Listener.Services
{
    public class DriverUpdateService : IUpdateService
    {
        public async Task<UpdateResult> InstallUpdate(string downloadLink)
        {
            var result = await DriverManager.InstallUpdate(downloadLink);
            return result;
            
        }
    }
}
