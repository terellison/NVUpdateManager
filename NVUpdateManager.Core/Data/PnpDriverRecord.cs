namespace NVUpdateManager.Core.Data
{
    /// <summary>
    /// A signed PnP driver as Windows reports it, reduced to the fields this application reads.
    /// Plain data, so code that interprets it can be tested without a Windows machine.
    /// </summary>
    public sealed class PnpDriverRecord
    {
        public PnpDriverRecord(string deviceName, string driverVersion)
        {
            DeviceName = deviceName;
            DriverVersion = driverVersion;
        }

        /// <summary>
        /// The device name, for example "NVIDIA GeForce RTX 3080".
        /// </summary>
        public string DeviceName { get; }

        /// <summary>
        /// The driver version in the four part form Windows uses, for example "31.0.15.2727".
        /// </summary>
        public string DriverVersion { get; }

        public override string ToString()
        {
            return $"{DeviceName} ({DriverVersion})";
        }
    }
}
