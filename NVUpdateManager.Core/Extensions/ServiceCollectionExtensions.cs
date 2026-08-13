using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDriverManager(this IServiceCollection services)
        {
            // TryAdd so a test (or a future non-Windows host) can substitute its own source.
            services.TryAddSingleton<ISystemHardwareInfo, WmiSystemHardwareInfo>();
            services.AddHttpClient<IDriverManager, DriverManager>();
            return services;
        }
    }
}
