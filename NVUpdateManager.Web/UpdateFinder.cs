using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Web.Data;

namespace NVUpdateManager.Web
{
    /// <summary>
    /// Queries NVIDIA for the newest driver published for a GPU.
    ///
    /// This talks to the JSON service behind the driver search rather than scraping the results
    /// page. One request returns the version, release date, release notes, and download URL, so
    /// there is no HTML to parse and nothing that breaks when the site is restyled.
    /// </summary>
    internal sealed class UpdateFinder : IUpdateFinder
    {
        private const string DriverLookupUrl =
            "https://gfwsl.geforce.com/services_toolkit/services/com/nvidia/services/AjaxDriverService.php";

        // LCID for en-US; selects the language the release notes come back in.
        private const int EnglishUnitedStates = 1033;

        private readonly HttpClient _httpClient;
        private readonly IProductCatalog _productCatalog;

        public UpdateFinder(HttpClient httpClient, IProductCatalog productCatalog)
        {
            _httpClient = httpClient;
            _productCatalog = productCatalog;
        }

        public async Task<UpdateInfo> FindLatestUpdate(GpuProduct product, DriverBranch branch, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(product);

            var operatingSystemName = WindowsRelease.GetNvidiaOperatingSystemName();

            var operatingSystemId = await _productCatalog
                .ResolveOperatingSystemIdAsync(product.ProductSeriesId, operatingSystemName, cancellationToken)
                .ConfigureAwait(false);

            if (operatingSystemId == null)
            {
                throw new NotSupportedException(
                    $"NVIDIA publishes no {operatingSystemName} drivers for {product.ProductName}.");
            }

            /* DCH is the driver model every current Windows release uses, but the oldest GPUs in
             * the catalog only ever shipped standard packages. Ask for DCH first and fall back
             * rather than deciding up front which era a GPU belongs to.
             */

            var downloadInfo = await LookupDriverAsync(product, operatingSystemId.Value, branch, useDchDriver: true, cancellationToken)
                .ConfigureAwait(false)
                ?? await LookupDriverAsync(product, operatingSystemId.Value, branch, useDchDriver: false, cancellationToken)
                .ConfigureAwait(false);

            if (downloadInfo == null)
            {
                throw new InvalidOperationException(
                    $"NVIDIA returned no {DescribeBranch(branch)} for {product.ProductName} on {operatingSystemName}.");
            }

            return ParseUpdateInfo(downloadInfo.Value);
        }

        private async Task<JsonElement?> LookupDriverAsync(
            GpuProduct product,
            int operatingSystemId,
            DriverBranch branch,
            bool useDchDriver,
            CancellationToken cancellationToken)
        {
            var url = BuildLookupUrl(product, operatingSystemId, branch, useDchDriver);

            using (var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                using (var document = JsonDocument.Parse(json))
                {
                    var root = document.RootElement;

                    if (!IsSuccessful(root) || !root.TryGetProperty("IDS", out var ids) || ids.GetArrayLength() == 0)
                    {
                        return null;
                    }

                    if (!ids[0].TryGetProperty("downloadInfo", out var downloadInfo) || !IsSuccessful(downloadInfo))
                    {
                        return null;
                    }

                    // Detach from the JsonDocument before it is disposed.
                    return downloadInfo.Clone();
                }
            }
        }

        private static string BuildLookupUrl(GpuProduct product, int operatingSystemId, DriverBranch branch, bool useDchDriver)
        {
            var culture = CultureInfo.InvariantCulture;

            return DriverLookupUrl
                + "?func=DriverManualLookup"
                + $"&psid={product.ProductSeriesId.ToString(culture)}"
                + $"&pfid={product.ProductFamilyId.ToString(culture)}"
                + $"&osID={operatingSystemId.ToString(culture)}"
                + $"&languageCode={EnglishUnitedStates.ToString(culture)}"
                + $"&dch={(useDchDriver ? 1 : 0).ToString(culture)}"
                + $"&upCRD={(branch == DriverBranch.Studio ? 1 : 0).ToString(culture)}"
                + "&numberOfResults=1";
        }

        /// <summary>
        /// The service reports failure as a "Success" of "0" and, confusingly, reports success as
        /// the number of results rather than a fixed value.
        /// </summary>
        private static bool IsSuccessful(JsonElement element)
        {
            return element.TryGetProperty("Success", out var success)
                && success.GetString() != "0";
        }

        private static UpdateInfo ParseUpdateInfo(JsonElement downloadInfo)
        {
            return new UpdateInfo(
                versionNumber: ReadString(downloadInfo, "Version"),
                releaseDate: ReadString(downloadInfo, "ReleaseDateTime"),
                details: ReadString(downloadInfo, "ReleaseNotes"),
                downloadLink: ReadString(downloadInfo, "DownloadURL"),
                name: ReadString(downloadInfo, "Name"));
        }

        /// <summary>
        /// Reads a field, undoing the percent-encoding the service applies to text fields.
        /// </summary>
        private static string ReadString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
            {
                return string.Empty;
            }

            var value = property.GetString();

            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            try
            {
                return Uri.UnescapeDataString(value);
            }
            catch (UriFormatException)
            {
                return value;
            }
        }

        private static string DescribeBranch(DriverBranch branch)
        {
            return branch == DriverBranch.Studio ? "Studio Driver" : "Game Ready Driver";
        }

        public async Task<string> DownloadUpdate(string updateLink)
        {
            var downloadLocation = Path.GetRandomFileName();

            var response = await _httpClient.GetAsync(updateLink).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            using (var fs = new FileStream(downloadLocation, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs).ConfigureAwait(false);
            }

            var newLocation = Path.ChangeExtension(downloadLocation, ".exe");

            File.Move(downloadLocation, newLocation);

            return Path.GetFullPath(newLocation);
        }
    }
}
