using System;
using System.Management;

namespace NVUpdateManager.Core
{
    public class DriverInfo
    {
        public string DeviceName { get; }
        public string DriverVersion { get; }

        public DriverInfo(ManagementBaseObject driver)
        {    
            DeviceName = driver.Properties[nameof(DeviceName)].Value.ToString();
            DriverVersion = ParseVersion(driver.Properties[nameof(DriverVersion)].Value.ToString());
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
                versionAsANumber = decimal.Parse(valueArr[2].Substring(valueArr[2].Length - 1, 1) + valueArr[3]) / 100;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Falied to parse driver version number with exception {ex.Message}", ex);
            }

            return versionAsANumber.ToString();
        }
    }
}
