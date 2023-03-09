using static NVUpdateManager.Core.DriverManager;
using static NVUpdateManager.WebScraper.UpdateFinder;
//using static EmailHandler.EmailHandler;
using NVUpdateManager.WebScraper.Data;
using NVUpdateManager.Core;
using Microsoft.Extensions.Options;
using NVUpdateManager.NotificationService.Data;
using System.Diagnostics.Contracts;

namespace NVUpdateManager.NotificationService
{
    public class NotificationWorker : BackgroundService
    {
        private readonly ILogger<NotificationWorker> _logger;
        private readonly IOptions<EmailConfiguration> _options;
        private readonly IEnumerable<SupportedDriver> _supportedDrivers;

        private const double ITERATION_TIME_IN_HOURS = 24; // Time between checks for updates
        private const string _verifySamplesBusyFile = "D:\\Temp\\VerifySamplesPCLocalRunServer\\ServerBusy.txt";
        
        public NotificationWorker(ILogger<NotificationWorker> logger, IOptions<EmailConfiguration> options, IEnumerable<SupportedDriver> supportedDrivers)
        {
            _logger = logger;
            _options = options;
            _supportedDrivers = supportedDrivers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!File.Exists(_verifySamplesBusyFile))
                {
                    _logger.LogInformation("Checking for new driver update at {Now}", DateTime.Now);

                    var currentDriverInfo = await GetInstalledDriverInfo();
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
                else
                {
                    _logger.LogInformation($"Found {_verifySamplesBusyFile}, waiting 1 hour");

                    await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
                }
            }
        }

        private UpdateInfo? CheckForNewUpdate(DriverInfo currentDriver)
        {
            GetGPUSearchParams(currentDriver, out string gpuSeries, out string gpuName, out string driverType); // Let this throw normally

            var updateInfo = FindLatestUpdate(gpuSeries, gpuName, driverType);

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
            var supportedDriver = _supportedDrivers.FirstOrDefault(x => x.WmiName.Equals(currentDriver.DeviceName));

            if(supportedDriver != null)
            {
                gpuName = supportedDriver.SearchName;
                gpuSeries = supportedDriver.DriverSeries;
                driverType = supportedDriver.DriverType;
            }

            else
            {
                throw new NotSupportedException($"GPU {currentDriver.DeviceName} not supported");
            }
        }

        private void SendUpdateNotification(UpdateInfo info)
        {
            throw new NotImplementedException();
        }

    }
}
