using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Core.Tests
{
    /// <summary>
    /// Stands in for WMI so the code that interprets what Windows reports can be tested on any
    /// platform, without depending on which GPU happens to be installed.
    /// </summary>
    internal sealed class FakeSystemHardwareInfo : ISystemHardwareInfo
    {
        public List<PnpDriverRecord> Drivers { get; } = new();

        public ushort? PcSystemType { get; set; }

        /// <summary>When set, <see cref="GetSignedDrivers"/> throws it instead of returning.</summary>
        public Exception? DriverQueryFailure { get; set; }

        /// <summary>When set, <see cref="GetPcSystemType"/> throws it instead of returning.</summary>
        public Exception? ChassisQueryFailure { get; set; }

        public FakeSystemHardwareInfo WithDriver(string deviceName, string driverVersion = "31.0.15.2727")
        {
            Drivers.Add(new PnpDriverRecord(deviceName, driverVersion));
            return this;
        }

        public IReadOnlyList<PnpDriverRecord> GetSignedDrivers()
        {
            return DriverQueryFailure == null ? Drivers : throw DriverQueryFailure;
        }

        public ushort? GetPcSystemType()
        {
            return ChassisQueryFailure == null ? PcSystemType : throw ChassisQueryFailure;
        }
    }
}
