using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NVUpdateManager.Core.Data;

namespace NVUpdateManager.Core.Interfaces
{
    /// <summary>
    /// Supplies the product identifiers NVIDIA's driver search expects. Implementations are
    /// expected to read them from NVIDIA at runtime, so that newly released GPUs resolve
    /// without a code change.
    /// </summary>
    public interface IProductCatalog
    {
        /// <summary>
        /// Every GPU NVIDIA currently publishes drivers for.
        /// </summary>
        Task<IReadOnlyList<GpuProduct>> GetProductsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds the catalog entry matching a device name as reported by the operating system.
        /// </summary>
        /// <param name="deviceName">The adapter name, for example "NVIDIA GeForce RTX 3080".</param>
        /// <param name="preferNotebook">
        /// Which variant to pick when a desktop and a notebook GPU share a name. Pass true on
        /// mobile systems.
        /// </param>
        /// <returns>The matching product, or null when the name is not in NVIDIA's catalog.</returns>
        Task<GpuProduct> ResolveProductAsync(string deviceName, bool preferNotebook, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves the "osID" query string value for an operating system, as offered for a
        /// given product series. NVIDIA lists a different set of operating systems per series.
        /// </summary>
        /// <returns>The operating system identifier, or null when the series offers no match.</returns>
        Task<int?> ResolveOperatingSystemIdAsync(int productSeriesId, string operatingSystemName, CancellationToken cancellationToken = default);
    }
}
