using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Web.Data;

namespace NVUpdateManager.Web
{
    /// <summary>
    /// Builds the GPU catalog from NVIDIA's own lookup service.
    ///
    /// The driver search on nvidia.com is driven by numeric identifiers: a product series
    /// ("psid") and a product family ("pfid"). Those numbers are not guessable and change as
    /// products launch, which is why they used to be transcribed into a dictionary by hand.
    /// They are, however, published: the same endpoint the website's own dropdowns call will
    /// enumerate every product type, series, and product, along with the identifier for each.
    ///
    /// Walking that endpoint gives the whole mapping for free. The result is cached on disk so
    /// the walk happens roughly once a week rather than on every driver check.
    /// </summary>
    internal sealed class NvidiaProductCatalog : IProductCatalog
    {
        internal const string HttpClientName = nameof(NvidiaProductCatalog);

        private const string LookupUrl = "https://www.nvidia.com/Download/API/lookupValueSearch.aspx";

        // The lookup endpoint is a single table keyed by "TypeID"; these are the four views of it.
        private const int ProductTypeView = 1;
        private const int ProductSeriesView = 2;
        private const int ProductView = 3;
        private const int OperatingSystemView = 4;

        private static readonly Regex WhitespaceRun = new Regex(@"\s+", RegexOptions.Compiled);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NvidiaCatalogOptions _options;
        private readonly SemaphoreSlim _buildLock = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<int, IReadOnlyList<LookupValue>> _operatingSystemsBySeries =
            new ConcurrentDictionary<int, IReadOnlyList<LookupValue>>();

        private IReadOnlyList<GpuProduct> _products;

        public NvidiaProductCatalog(IHttpClientFactory httpClientFactory, NvidiaCatalogOptions options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options ?? new NvidiaCatalogOptions();
        }

        public async Task<IReadOnlyList<GpuProduct>> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            var cached = _products;
            if (cached != null)
            {
                return cached;
            }

            await _buildLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_products != null)
                {
                    return _products;
                }

                _products = ReadCacheFile() ?? await BuildCatalogAsync(cancellationToken).ConfigureAwait(false);

                return _products;
            }
            finally
            {
                _buildLock.Release();
            }
        }

        public async Task<GpuProduct> ResolveProductAsync(string deviceName, bool preferNotebook, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                return null;
            }

            var products = await GetProductsAsync(cancellationToken).ConfigureAwait(false);

            var wanted = NormalizeProductName(deviceName);

            var matches = products
                .Where(p => string.Equals(NormalizeProductName(p.ProductName), wanted, StringComparison.Ordinal))
                .ToList();

            if (matches.Count == 0)
            {
                return null;
            }

            if (matches.Count == 1)
            {
                return matches[0];
            }

            /* Desktop and notebook parts shared a name until the Ampere generation; from then on
             * NVIDIA suffixes the mobile entries with "Laptop GPU". For the older overlapping
             * names the chassis is what breaks the tie.
             */

            return matches.FirstOrDefault(p => p.IsNotebook == preferNotebook) ?? matches[0];
        }

        public async Task<int?> ResolveOperatingSystemIdAsync(int productSeriesId, string operatingSystemName, CancellationToken cancellationToken = default)
        {
            if (!_operatingSystemsBySeries.TryGetValue(productSeriesId, out var operatingSystems))
            {
                operatingSystems = await GetLookupValuesAsync(OperatingSystemView, productSeriesId, cancellationToken).ConfigureAwait(false);
                _operatingSystemsBySeries[productSeriesId] = operatingSystems;
            }

            var match = operatingSystems.FirstOrDefault(
                o => string.Equals(o.Name, operatingSystemName, StringComparison.OrdinalIgnoreCase));

            /* Older series predate the current Windows releases and only list what they shipped
             * for. Falling back to the newest Windows entry NVIDIA offers keeps those GPUs working
             * instead of failing the lookup outright.
             */

            match ??= operatingSystems.LastOrDefault(o => o.Name.StartsWith("Windows", StringComparison.OrdinalIgnoreCase));

            return match?.Value;
        }

        /// <summary>
        /// Reduces a GPU name to a form that compares reliably between what Windows reports and
        /// what NVIDIA publishes. The two disagree on the "NVIDIA" prefix (Windows always
        /// includes it, NVIDIA's catalog only does for recent products) and on capitalisation
        /// of suffixes such as SUPER.
        /// </summary>
        internal static string NormalizeProductName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var normalized = WhitespaceRun.Replace(name, " ").Trim().ToUpperInvariant();

            const string vendorPrefix = "NVIDIA ";

            if (normalized.StartsWith(vendorPrefix, StringComparison.Ordinal))
            {
                normalized = normalized.Substring(vendorPrefix.Length);
            }

            return normalized;
        }

        private async Task<IReadOnlyList<GpuProduct>> BuildCatalogAsync(CancellationToken cancellationToken)
        {
            var productTypes = await GetLookupValuesAsync(ProductTypeView, null, cancellationToken).ConfigureAwait(false);

            var seriesLists = await RunThrottledAsync(
                productTypes,
                type => GetLookupValuesAsync(ProductSeriesView, type.Value, cancellationToken),
                cancellationToken).ConfigureAwait(false);

            // A series can be reachable from more than one product type; keep the first sighting.
            var series = seriesLists
                .SelectMany(s => s)
                .GroupBy(s => s.Value)
                .Select(g => g.First())
                .ToList();

            var productLists = await RunThrottledAsync(
                series,
                s => GetLookupValuesAsync(ProductView, s.Value, cancellationToken),
                cancellationToken).ConfigureAwait(false);

            var products = new List<GpuProduct>();

            for (var i = 0; i < series.Count; i++)
            {
                var seriesName = series[i].Name;
                var isNotebookSeries = seriesName.IndexOf("Notebook", StringComparison.OrdinalIgnoreCase) >= 0;

                foreach (var product in productLists[i])
                {
                    var isNotebook = isNotebookSeries
                        || product.Name.IndexOf("Laptop", StringComparison.OrdinalIgnoreCase) >= 0;

                    products.Add(new GpuProduct(product.Name, seriesName, series[i].Value, product.Value, isNotebook));
                }
            }

            if (products.Count == 0)
            {
                throw new InvalidOperationException(
                    "NVIDIA's lookup service returned an empty GPU catalog. Refusing to cache it.");
            }

            WriteCacheFile(products);

            return products;
        }

        /// <summary>
        /// Runs a request per item with a ceiling on how many run at once, preserving input order.
        /// </summary>
        private async Task<IReadOnlyList<TResult>> RunThrottledAsync<TSource, TResult>(
            IReadOnlyList<TSource> source,
            Func<TSource, Task<TResult>> request,
            CancellationToken cancellationToken)
        {
            var concurrency = Math.Max(1, _options.MaxConcurrentRequests);

            using (var throttle = new SemaphoreSlim(concurrency, concurrency))
            {
                var tasks = source.Select(async item =>
                {
                    await throttle.WaitAsync(cancellationToken).ConfigureAwait(false);

                    try
                    {
                        return await request(item).ConfigureAwait(false);
                    }
                    finally
                    {
                        throttle.Release();
                    }
                });

                return await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task<IReadOnlyList<LookupValue>> GetLookupValuesAsync(int typeId, int? parentId, CancellationToken cancellationToken)
        {
            var url = $"{LookupUrl}?TypeID={typeId.ToString(CultureInfo.InvariantCulture)}";

            if (parentId.HasValue)
            {
                url += $"&ParentID={parentId.Value.ToString(CultureInfo.InvariantCulture)}";
            }

            var attempts = Math.Max(1, _options.MaxRetries);

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    using (var client = _httpClientFactory.CreateClient(HttpClientName))
                    using (var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();

                        var xml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                        return ParseLookupValues(xml);
                    }
                }
                catch (Exception ex) when (attempt < attempts && ex is not OperationCanceledException)
                {
                    // The lookup service throttles bursts; back off and try again.
                    await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken).ConfigureAwait(false);
                }
            }
        }

        internal static IReadOnlyList<LookupValue> ParseLookupValues(string xml)
        {
            var values = new List<LookupValue>();

            if (string.IsNullOrWhiteSpace(xml))
            {
                return values;
            }

            var document = XDocument.Parse(xml);

            foreach (var element in document.Descendants("LookupValue"))
            {
                var name = (string)element.Element("Name");
                var rawValue = (string)element.Element("Value");

                if (string.IsNullOrWhiteSpace(name)
                    || !int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                {
                    continue;
                }

                values.Add(new LookupValue(name.Trim(), value));
            }

            return values;
        }

        private IReadOnlyList<GpuProduct> ReadCacheFile()
        {
            try
            {
                var path = _options.CacheFilePath;

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    return null;
                }

                var cache = JsonSerializer.Deserialize<CatalogCacheFile>(File.ReadAllText(path));

                if (cache?.Products == null || cache.Products.Count == 0)
                {
                    return null;
                }

                if (DateTimeOffset.UtcNow - cache.RetrievedAt > _options.CacheLifetime)
                {
                    return null;
                }

                return cache.Products
                    .Select(p => new GpuProduct(p.ProductName, p.SeriesName, p.ProductSeriesId, p.ProductFamilyId, p.IsNotebook))
                    .ToList();
            }
            catch (Exception ex) when (ex is IOException || ex is JsonException || ex is UnauthorizedAccessException)
            {
                // An unreadable cache is not fatal; fall through and rebuild it.
                return null;
            }
        }

        private void WriteCacheFile(IReadOnlyList<GpuProduct> products)
        {
            try
            {
                var path = _options.CacheFilePath;

                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                var directory = Path.GetDirectoryName(path);

                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var cache = new CatalogCacheFile
                {
                    RetrievedAt = DateTimeOffset.UtcNow,
                    Products = products
                        .Select(p => new CatalogCacheEntry
                        {
                            ProductName = p.ProductName,
                            SeriesName = p.SeriesName,
                            ProductSeriesId = p.ProductSeriesId,
                            ProductFamilyId = p.ProductFamilyId,
                            IsNotebook = p.IsNotebook
                        })
                        .ToList()
                };

                File.WriteAllText(path, JsonSerializer.Serialize(cache));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // Caching is an optimisation; losing it only costs time on the next run.
            }
        }

        internal sealed class LookupValue
        {
            public LookupValue(string name, int value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public int Value { get; }
        }

        private sealed class CatalogCacheFile
        {
            public DateTimeOffset RetrievedAt { get; set; }
            public List<CatalogCacheEntry> Products { get; set; }
        }

        private sealed class CatalogCacheEntry
        {
            public string ProductName { get; set; }
            public string SeriesName { get; set; }
            public int ProductSeriesId { get; set; }
            public int ProductFamilyId { get; set; }
            public bool IsNotebook { get; set; }
        }
    }
}
