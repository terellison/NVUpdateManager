using NVUpdateManager.Core.Data;

namespace NVUpdateManager.Listener.Data
{
    public interface IUpdateService
    {
        public Task<UpdateResult> InstallUpdate(string downloadLink);
    }
}
