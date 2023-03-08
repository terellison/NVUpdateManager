using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading.Tasks;
using NVUpdateManager.Core.Data;

namespace NVUpdateManager.Core
{
    public static class DriverManager
    {
        public static Task<UpdateResult> InstallUpdate(string downloadLink)
        {
            return Task.Run(async () =>
            {
                var updatePath = await DownloadDriver(downloadLink);
                var extractedUpdatePath = ExtractUpdate(updatePath);
                return UpdateResult.Success;
            });
        }

        public static Task<DriverInfo> GetInstalledDriverInfo()
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

        private static string ExtractUpdate(string updatePath)
        {
            throw new NotImplementedException();
        }

        private static Task<string> DownloadDriver(string downloadLink)
        {
            return Task.Run(() =>
            {
                var downloadPath = Path.GetRandomFileName();

                using (var client = new WebClient())
                {
                    client.DownloadFile(new Uri(downloadLink), downloadPath);
                }

                var updateFile = Path.ChangeExtension(downloadPath, ".exe");

                File.Move(downloadPath, updateFile);

                return updateFile;
            });
        }
    }
}
