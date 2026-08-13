using System;
using System.IO;

namespace NVUpdateManager.Web.Data
{
    /// <summary>
    /// Tuning for how the GPU catalog is fetched from NVIDIA and cached locally.
    /// </summary>
    public sealed class NvidiaCatalogOptions
    {
        /// <summary>
        /// How long a cached catalog is trusted before it is fetched again. NVIDIA only adds
        /// entries when a product launches, so this can be generous.
        /// </summary>
        public TimeSpan CacheLifetime { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// Where the cached catalog is written. Defaults to a file under the machine-wide
        /// application data folder, which a service running as LocalSystem can write to.
        /// </summary>
        public string CacheFilePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "NVUpdateManager",
            "gpu-catalog.json");

        /// <summary>
        /// How many catalog requests may be in flight at once. Building the catalog needs one
        /// request per product series, so this bounds the burst without hammering NVIDIA.
        /// </summary>
        public int MaxConcurrentRequests { get; set; } = 4;

        /// <summary>
        /// How many times a single catalog request is retried before the build is abandoned.
        /// A partial catalog is never cached, so a failed request fails the whole build.
        /// </summary>
        public int MaxRetries { get; set; } = 3;
    }
}
