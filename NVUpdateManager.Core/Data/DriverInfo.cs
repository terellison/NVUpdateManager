using System;
using System.Globalization;

namespace NVUpdateManager.Core
{
    public class DriverInfo
    {
        public DriverInfo(string deviceName, string driverVersion, bool isMobileSystem = false)
        {
            DeviceName = deviceName;
            DriverVersion = driverVersion;
            IsMobileSystem = isMobileSystem;
        }

        public string DeviceName { get; }

        /// <summary>
        /// The installed version in the form NVIDIA publishes, for example "527.27".
        /// </summary>
        public string DriverVersion { get; }

        /// <summary>
        /// True when the GPU is installed in a mobile system. NVIDIA catalogues desktop and
        /// notebook parts separately, and older generations gave both the same name, so this is
        /// what decides which of the two a device name refers to.
        /// </summary>
        public bool IsMobileSystem { get; }

        /// <summary>
        /// Creates driver information from the values Windows reports, translating the driver
        /// version into the form NVIDIA publishes.
        /// </summary>
        public static DriverInfo FromWmi(string deviceName, string wmiDriverVersion, bool isMobileSystem = false)
        {
            return new DriverInfo(deviceName, ParseVersion(wmiDriverVersion), isMobileSystem);
        }

        private static string ParseVersion(string value)
        {

            /* Version number from WMI looks like this: 31.0.15.2727
             * Friendly version from Geforce Experience looks like this: 527.27
             * We need the second one...
             */

            decimal versionAsANumber;

            try
            {
                var valueArr = value.Split('.');

                versionAsANumber = decimal.Parse(
                    valueArr[2].Substring(valueArr[2].Length - 1, 1) + valueArr[3],
                    CultureInfo.InvariantCulture) / 100;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Falied to parse driver version number with exception {ex.Message}", ex);
            }

            return versionAsANumber.ToString(CultureInfo.InvariantCulture);
        }
    }
}
