using System;

namespace NVUpdateManager.Web.Data
{
    internal static class WindowsRelease
    {
        // Windows 11 kept the major version at 10 and is only distinguishable by build number.
        private const int FirstWindows11Build = 22000;

        /// <summary>
        /// The name of the running operating system as NVIDIA's driver search spells it, so it
        /// can be matched against the list that search offers for a product series.
        /// </summary>
        internal static string GetNvidiaOperatingSystemName()
        {
            var version = Environment.OSVersion.Version;

            return version.Major > 10 || (version.Major == 10 && version.Build >= FirstWindows11Build)
                ? "Windows 11"
                : "Windows 10 64-bit";
        }
    }
}
