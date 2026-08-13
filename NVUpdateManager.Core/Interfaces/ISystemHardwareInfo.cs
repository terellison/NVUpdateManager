using System.Collections.Generic;
using NVUpdateManager.Core.Data;

namespace NVUpdateManager.Core.Interfaces
{
    /// <summary>
    /// Reads hardware facts from the operating system.
    ///
    /// Implementations return raw values and make no decisions. Everything that interprets those
    /// values lives above this interface, which is what allows the interpretation to be tested
    /// without a Windows machine or a particular GPU installed.
    /// </summary>
    public interface ISystemHardwareInfo
    {
        /// <summary>
        /// Every signed PnP driver the operating system knows about.
        /// </summary>
        IReadOnlyList<PnpDriverRecord> GetSignedDrivers();

        /// <summary>
        /// The raw Win32_ComputerSystem.PCSystemType value, or null when none is reported.
        /// Deliberately not phrased as "is this a laptop?" so that the comparison deciding it
        /// stays in testable code.
        /// </summary>
        ushort? GetPcSystemType();
    }
}
