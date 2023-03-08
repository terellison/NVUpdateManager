using NVUpdateManager.Core;

namespace NVUpdateManager.Listener.Data
{
    public interface IDriverService
    {
        Task<DriverInfo> GetDriverInfoAsync();
    }
}
