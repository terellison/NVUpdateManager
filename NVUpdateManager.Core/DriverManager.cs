using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Threading.Tasks;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Core
{
    internal sealed class DriverManager : IDriverManager
    {
        private readonly HttpClient _httpClient;
        public DriverManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

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

                    if (nvDriver == null)
                    {
                        throw new InvalidOperationException("Could not find NVIDIA Game Ready Driver. Ensure that the driver is installed correctly");
                    }

                    return new DriverInfo(nvDriver, IsMobileSystem());
                }
            });
        }

        /// <summary>
        /// Reports whether this machine is a laptop. NVIDIA lists desktop and notebook GPUs as
        /// separate products, and before the Ampere generation both variants carried the same
        /// name, so the chassis is what tells them apart.
        /// </summary>
        private static bool IsMobileSystem()
        {
            const string wmiQuery = "SELECT PCSystemType FROM Win32_ComputerSystem";

            // 2 is "Mobile" in the Win32_ComputerSystem.PCSystemType enumeration.
            const ushort mobileSystem = 2;

            try
            {
                using (var systems = new ManagementObjectSearcher(wmiQuery).Get())
                {
                    foreach (ManagementBaseObject system in systems)
                    {
                        var value = system.Properties["PCSystemType"].Value;

                        if (value != null && Convert.ToUInt16(value) == mobileSystem)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (ManagementException)
            {
                // Not worth failing an update check over; desktop is the safer assumption.
            }

            return false;
        }

        private string ExtractUpdate(string updatePath)
        {
            throw new NotImplementedException();
        }

        private async Task<string> DownloadDriverAsync(string downloadLink)
        {
            var downloadPath = Path.GetRandomFileName();


            using (var response = await _httpClient.GetAsync(downloadPath))
            {
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
