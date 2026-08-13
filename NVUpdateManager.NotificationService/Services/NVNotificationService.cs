using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Core;
using Microsoft.Extensions.Options;
using NVUpdateManager.NotificationService.Data;
using NVUpdateManager.Core.Data;
using System.Globalization;

namespace NVUpdateManager.NotificationService.Services
{
    public class NVNotificationService : INotificationService
    {
        private readonly ILogger<NVNotificationService> _logger;
        private readonly IOptions<DriverSearchConfiguration> _driverSearch;
        private readonly IDriverManager _driverManager;
        private readonly IUpdateFinder _updateFinder;
        private readonly IProductCatalog _productCatalog;
        private readonly INotificationDispatcher _notifications;

        public NVNotificationService(
            ILogger<NVNotificationService> logger,
            IOptions<DriverSearchConfiguration> driverSearch,
            IDriverManager driverManager,
            IUpdateFinder updateFinder,
            IProductCatalog productCatalog,
            INotificationDispatcher notifications)
        {
            _logger = logger;
            _driverSearch = driverSearch;
            _driverManager = driverManager;
            _updateFinder = updateFinder;
            _productCatalog = productCatalog;
            _notifications = notifications;
        }

        public async Task Run()
        {

            _logger.LogInformation("Checking for new driver update at {Now}", DateTime.Now);

            var currentDriverInfo = await _driverManager.GetInstalledDriverInfo();
            var newUpdateInfo = await CheckForNewUpdate(currentDriverInfo);

            if (newUpdateInfo != null)
            {
                _logger.LogInformation("Found new driver update\nDetails: \n{Details}\n", newUpdateInfo);

                var message = NotificationMessage.ForUpdate(newUpdateInfo, currentDriverInfo.DeviceName);

                var delivered = await _notifications.SendAsync(message);

                if (delivered.Count == 0)
                {
                    _logger.LogWarning("Found an update but could not announce it through any channel");
                }
            }

        }

        private async Task<UpdateInfo?> CheckForNewUpdate(DriverInfo currentDriver)
        {
            var product = await ResolveProduct(currentDriver); // Let this throw normally

            _logger.LogInformation("Identified GPU as {Product}", product);

            var updateInfo = await _updateFinder.FindLatestUpdate(product, _driverSearch.Value.Branch);

            try
            {
                var available = decimal.Parse(updateInfo.VersionNumber, CultureInfo.InvariantCulture);
                var installed = decimal.Parse(currentDriver.DriverVersion, CultureInfo.InvariantCulture);

                if (available > installed)
                {
                    return updateInfo;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to compare current version number to update with exception {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Works out which NVIDIA product the installed GPU is.
        ///
        /// The identifiers NVIDIA's driver search needs used to be transcribed into the source
        /// along with a list of GPUs the application claimed to support. Both are now read from
        /// NVIDIA's own catalogue at runtime, so a newly released GPU works without a release.
        /// </summary>
        private async Task<GpuProduct> ResolveProduct(DriverInfo currentDriver)
        {
            var search = _driverSearch.Value;

            var deviceName = string.IsNullOrWhiteSpace(search.ProductNameOverride)
                ? currentDriver.DeviceName
                : search.ProductNameOverride;

            if (search.ProductSeriesId.HasValue && search.ProductFamilyId.HasValue)
            {
                _logger.LogInformation(
                    "Using configured NVIDIA identifiers psid={ProductSeriesId}, pfid={ProductFamilyId} for {DeviceName}",
                    search.ProductSeriesId.Value,
                    search.ProductFamilyId.Value,
                    deviceName);

                return new GpuProduct(
                    deviceName,
                    seriesName: "configured",
                    search.ProductSeriesId.Value,
                    search.ProductFamilyId.Value,
                    currentDriver.IsMobileSystem);
            }

            var product = await _productCatalog.ResolveProductAsync(deviceName, currentDriver.IsMobileSystem);

            if (product == null)
            {
                throw new NotSupportedException(
                    $"NVIDIA publishes no drivers under the name \"{deviceName}\". "
                    + $"Set {nameof(DriverSearchConfiguration)}:{nameof(DriverSearchConfiguration.ProductNameOverride)} "
                    + "in appsettings.json to the name NVIDIA lists this GPU under.");
            }

            return product;
        }

    }
}
