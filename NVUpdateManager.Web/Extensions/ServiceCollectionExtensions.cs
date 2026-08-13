using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Web.Data;

namespace NVUpdateManager.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUpdateFinder(this IServiceCollection services)
        {
            return services.AddUpdateFinder(new NvidiaCatalogOptions());
        }

        public static IServiceCollection AddUpdateFinder(this IServiceCollection services, NvidiaCatalogOptions catalogOptions)
        {
            services.TryAddSingleton(catalogOptions ?? new NvidiaCatalogOptions());

            // The catalog is a singleton so the GPU list is fetched once per process, not per lookup.
            services.AddHttpClient(NvidiaProductCatalog.HttpClientName);
            services.TryAddSingleton<IProductCatalog, NvidiaProductCatalog>();

            services.AddHttpClient<IUpdateFinder, UpdateFinder>();

            return services;
        }
    }
}
