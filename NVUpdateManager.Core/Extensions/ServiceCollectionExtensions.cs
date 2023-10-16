using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDriverManager(this IServiceCollection services)
        {
            services.TryAddSingleton<IDriverManager, DriverManager>();
            return services;
        }
    }
}
