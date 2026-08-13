using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Core
{
    internal sealed class DriverManager : IDriverManager
    {
        // 2 is "Mobile" in the Win32_ComputerSystem.PCSystemType enumeration.
        private const ushort MobileSystem = 2;

        private static readonly string[] NvidiaAdapterNames =
        {
            nameof(DriverType.GeForce),
            nameof(DriverType.RTX),
            nameof(DriverType.GTX)
        };

        private const string DriverNotFound =
            "Could not find NVIDIA Game Ready Driver. Ensure that the driver is installed correctly";

        private readonly HttpClient _httpClient;
        private readonly ISystemHardwareInfo _systemHardwareInfo;

        public DriverManager(HttpClient httpClient, ISystemHardwareInfo systemHardwareInfo)
        {
            _httpClient = httpClient;
            _systemHardwareInfo = systemHardwareInfo;
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
                IReadOnlyList<PnpDriverRecord> drivers;

                try
                {
                    drivers = _systemHardwareInfo.GetSignedDrivers();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(DriverNotFound, ex);
                }

                var nvDriver = drivers.FirstOrDefault(IsNvidiaAdapter);

                if (nvDriver == null)
                {
                    throw new InvalidOperationException(DriverNotFound);
                }

                return DriverInfo.FromWmi(nvDriver.DeviceName, nvDriver.DriverVersion, IsMobileSystem());
            });
        }

        /// <summary>
        /// Recognises an NVIDIA display adapter among every signed driver on the machine, which
        /// on a laptop includes the integrated graphics sitting alongside it.
        /// </summary>
        private static bool IsNvidiaAdapter(PnpDriverRecord driver)
        {
            var deviceName = driver?.DeviceName;

            return !string.IsNullOrWhiteSpace(deviceName)
                && NvidiaAdapterNames.Any(name => deviceName.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Reports whether this machine is a laptop. NVIDIA lists desktop and notebook GPUs as
        /// separate products, and before the Ampere generation both variants carried the same
        /// name, so the chassis is what tells them apart.
        /// </summary>
        private bool IsMobileSystem()
        {
            try
            {
                return _systemHardwareInfo.GetPcSystemType() == MobileSystem;
            }
            catch (Exception)
            {
                // Not worth failing an update check over; desktop is the safer assumption.
                return false;
            }
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
