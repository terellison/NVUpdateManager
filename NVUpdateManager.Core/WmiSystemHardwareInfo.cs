using System;
using System.Collections.Generic;
using System.Management;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Core
{
    /// <summary>
    /// Reads hardware facts from WMI.
    ///
    /// This is deliberately the only type that touches System.Management, and it deliberately
    /// contains no decisions - it copies values out of WMI and hands them back. Keeping it free
    /// of branching is what makes it acceptable that no unit test can reach it: there is nothing
    /// here to get wrong beyond the query text and the property names. Everything that judges
    /// these values lives in <see cref="DriverManager"/>, where tests can substitute this out.
    /// </summary>
    internal sealed class WmiSystemHardwareInfo : ISystemHardwareInfo
    {
        public IReadOnlyList<PnpDriverRecord> GetSignedDrivers()
        {
            const string wmiQuery = "SELECT * FROM Win32_PnPSignedDriver";

            var drivers = new List<PnpDriverRecord>();

            using (var results = new ManagementObjectSearcher(wmiQuery).Get())
            {
                foreach (ManagementBaseObject result in results)
                {
                    drivers.Add(new PnpDriverRecord(
                        result.Properties[nameof(PnpDriverRecord.DeviceName)].Value?.ToString(),
                        result.Properties[nameof(PnpDriverRecord.DriverVersion)].Value?.ToString()));
                }
            }

            return drivers;
        }

        public ushort? GetPcSystemType()
        {
            const string wmiQuery = "SELECT PCSystemType FROM Win32_ComputerSystem";

            using (var results = new ManagementObjectSearcher(wmiQuery).Get())
            {
                foreach (ManagementBaseObject result in results)
                {
                    var value = result.Properties["PCSystemType"].Value;

                    if (value != null)
                    {
                        return Convert.ToUInt16(value);
                    }
                }
            }

            return null;
        }
    }
}
