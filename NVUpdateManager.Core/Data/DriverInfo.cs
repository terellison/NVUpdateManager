using System;
using System.Globalization;
using System.Management;

namespace NVUpdateManager.Core
{
    public class DriverInfo
    {
        public string DeviceName { get; }
        public string DriverVersion { get; }

        /// <summary>
        /// True when the GPU is installed in a mobile system. NVIDIA catalogues desktop and
        /// notebook parts separately, and older generations gave both the same name, so this is
        /// what decides which of the two a device name refers to.
        /// </summary>
        public bool IsMobileSystem { get; }

        public DriverInfo(ManagementBaseObject driver, bool isMobileSystem = false)
        {
            DeviceName = driver.Properties[nameof(DeviceName)].Value.ToString();
            DriverVersion = ParseVersion(driver.Properties[nameof(DriverVersion)].Value.ToString());
            IsMobileSystem = isMobileSystem;
        }

        /// <summary>
        /// Creates driver information from already-parsed values.
        /// </summary>
        public DriverInfo(string deviceName, string driverVersion, bool isMobileSystem = false)
        {
            DeviceName = deviceName;
            DriverVersion = driverVersion;
            IsMobileSystem = isMobileSystem;
        }

        private string ParseVersion(string value)
        {

            /* Version number from WMI looks like this: 31.0.15.2727
             * Friendly version from Geforce Experience looks like this: 527.27
             * We need the second one...
             */

            var valueArr = value.Split('.');

            decimal versionAsANumber;

            try
            {
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
