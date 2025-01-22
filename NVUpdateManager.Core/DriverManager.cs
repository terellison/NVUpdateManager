using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Core
{
    internal sealed class DriverManager : IDriverManager
    {
        public Task<UpdateResult> InstallUpdate(string downloadLink)
        {
            return Task.Run(async () =>
            {
                var updatePath = await DownloadDriverAsync(downloadLink);
                var extractedUpdatePath = ExtractUpdate(updatePath);
                return UpdateResult.Success;
            });
        }

        public Task<DriverInfo> GetInstalledDriverInfo()
        {
            return Task.Run(() =>
            {
                const string wmiQuery = "SELECT * FROM Win32_PnPSignedDriver";

                ManagementBaseObject nvDriver;

                using (var drivers = new ManagementObjectSearcher(wmiQuery).Get())
                {
                    try
                    {
                        nvDriver = (from ManagementBaseObject x in drivers
                                    let deviceName = x.Properties["DeviceName"].Value?.ToString()
                                    where
                                        !string.IsNullOrWhiteSpace(deviceName)
                                        && (deviceName.Contains(nameof(DriverType.GeForce))
                                        || deviceName.Contains(nameof(DriverType.RTX))
                                        || deviceName.Contains(nameof(DriverType.GTX)))
                                    select x)
                                                    .FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Could not find NVIDIA Game Ready Driver. Ensure that the driver is installed correctly", ex);
                    }

                    return new DriverInfo(nvDriver);
                }
            });
        }

        private string ExtractUpdate(string updatePath)
        {
            throw new NotImplementedException();
        }

        private async Task<string> DownloadDriverAsync(string downloadLink)
        {
            var downloadPath = Path.GetRandomFileName();

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(downloadPath);
                response.EnsureSuccessStatusCode();

                var bytes = await response.Content.ReadAsByteArrayAsync();

                await File.WriteAllBytesAsync(downloadPath, bytes);
            }

            var updateFile = Path.ChangeExtension(downloadPath, ".exe");

            File.Move(downloadPath, updateFile);

            return updateFile;
        }
    }
}
