using NVUpdateManager.Core.Interfaces;
using static NVUpdateManager.Web.UpdateFinder;
using static NVUpdateManager.EmailHandler.EmailHandler;
using NVUpdateManager.Core;
using Microsoft.Extensions.Options;
using NVUpdateManager.NotificationService.Data;
using NVUpdateManager.Core.Data;

namespace NVUpdateManager.NotificationService
{
    public class NotificationWorker : BackgroundService
    {
        private readonly ILogger<NotificationWorker> _logger;
        private readonly IOptions<EmailConfiguration> _options;
        private readonly IEnumerable<SupportedDriver> _supportedDrivers;
        private readonly IDriverManager _driverManager;
        private const double ITERATION_TIME_IN_HOURS = 24; // Time between checks for updates
        
        public NotificationWorker(ILogger<NotificationWorker> logger, IOptions<EmailConfiguration> options, IEnumerable<SupportedDriver> supportedDrivers, IDriverManager driverManager)
        {
            _logger = logger;
            _options = options;
            _supportedDrivers = supportedDrivers;
            _driverManager = driverManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                
                _logger.LogInformation("Checking for new driver update at {Now}", DateTime.Now);

                var currentDriverInfo = await _driverManager.GetInstalledDriverInfo();
                var newUpdateInfo = CheckForNewUpdate(currentDriverInfo);

                if (newUpdateInfo != null)
                {
                    _logger.LogInformation(
                        "Found new Game Ready Driver update" +
                        $"\nDetails: \n{newUpdateInfo}\n" +
                        $"Sending notification to {_options.Value.NotificationAddress}");

                    try
                    {
                        SendUpdateNotification(newUpdateInfo);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Failed to send update email to maintenance", ex);
                    }
                }

                await Task.Delay(TimeSpan.FromHours(ITERATION_TIME_IN_HOURS), stoppingToken);
                
            }
        }

        private UpdateInfo? CheckForNewUpdate(DriverInfo currentDriver)
        {
            GetGPUSearchParams(currentDriver, out string gpuSeries, out string gpuName, out string driverType); // Let this throw normally

            var updateInfo = FindLatestUpdate(gpuSeries, gpuName, driverType).Result;

            try
            {
                if (decimal.Parse(updateInfo.VersionNumber) > decimal.Parse(currentDriver.DriverVersion))
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

        private void GetGPUSearchParams(DriverInfo currentDriver, out string gpuSeries, out string gpuName, out string driverType)
        {
            /* 
             * Null reference warnings are disabled here because dependency injection required 
             * that SupportedDriver's properties be nullable; They will never be null
             */

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

            var supportedDriver = _supportedDrivers.FirstOrDefault(x => x.WmiName.Equals(currentDriver.DeviceName));
            if (supportedDriver != null)
            {

                gpuName = supportedDriver.SearchName;
                gpuSeries = supportedDriver.DriverSeries;
                driverType = supportedDriver.DriverType;
            }

#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            else
            {
                throw new NotSupportedException($"GPU {currentDriver.DeviceName} not supported");
            }
        }

        private void SendUpdateNotification(UpdateInfo info)
        {
            if(!IsConfigured)
            {
                ConfigureLogicAppEndpoint(_options.Value.Entropy, _options.Value.EncryptedAzLogicAppEndpoint);
                ConfigureAddresses(_options.Value.NotificationAddress);
            }

            SendNotificationEmail($"New Game Ready Driver update available",
                info.ToString());
        }

    }
}
