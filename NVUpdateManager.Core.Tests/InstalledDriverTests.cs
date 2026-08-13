using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NVUpdateManager.Core.Extensions;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Core.Tests
{
    /// <summary>
    /// Covers how the installed GPU is identified from what Windows reports. These run anywhere,
    /// because the WMI query itself sits behind <see cref="ISystemHardwareInfo"/>.
    /// </summary>
    [TestClass]
    public class InstalledDriverTests
    {
        private static IDriverManager DriverManagerFor(FakeSystemHardwareInfo hardware)
        {
            var services = new ServiceCollection();

            // Registered first so AddDriverManager's TryAdd leaves it in place.
            services.AddSingleton<ISystemHardwareInfo>(hardware);
            services.AddDriverManager();

            return services.BuildServiceProvider().GetRequiredService<IDriverManager>();
        }

        [TestMethod]
        public async Task PicksTheNvidiaAdapterFromAmongEveryInstalledDriver()
        {
            var hardware = new FakeSystemHardwareInfo()
                .WithDriver("Intel(R) UHD Graphics 770")
                .WithDriver("Realtek High Definition Audio")
                .WithDriver("NVIDIA GeForce RTX 3080");

            var driver = await DriverManagerFor(hardware).GetInstalledDriverInfo();

            Assert.AreEqual("NVIDIA GeForce RTX 3080", driver.DeviceName);
        }

        [TestMethod]
        [DataRow("NVIDIA GeForce RTX 4090")]
        [DataRow("NVIDIA GeForce GTX 1660 SUPER")]
        [DataRow("NVIDIA RTX A4000")]
        public async Task RecognisesEachNvidiaAdapterNamingStyle(string deviceName)
        {
            var hardware = new FakeSystemHardwareInfo().WithDriver(deviceName);

            var driver = await DriverManagerFor(hardware).GetInstalledDriverInfo();

            Assert.AreEqual(deviceName, driver.DeviceName);
        }

        [TestMethod]
        public async Task SkipsDriversWindowsReportsWithoutAName()
        {
            var hardware = new FakeSystemHardwareInfo()
                .WithDriver(null!)
                .WithDriver("   ")
                .WithDriver("NVIDIA GeForce RTX 3080");

            var driver = await DriverManagerFor(hardware).GetInstalledDriverInfo();

            Assert.AreEqual("NVIDIA GeForce RTX 3080", driver.DeviceName);
        }

        [TestMethod]
        public async Task ReportsAMissingNvidiaAdapterClearly()
        {
            var hardware = new FakeSystemHardwareInfo().WithDriver("Intel(R) UHD Graphics 770");

            var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => DriverManagerFor(hardware).GetInstalledDriverInfo());

            StringAssert.Contains(ex.Message, "Could not find NVIDIA Game Ready Driver");
        }

        [TestMethod]
        public async Task WrapsAFailureToQueryTheSystem()
        {
            var hardware = new FakeSystemHardwareInfo { DriverQueryFailure = new InvalidCastException("WMI is unwell") };

            var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => DriverManagerFor(hardware).GetInstalledDriverInfo());

            Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidCastException));
        }

        [TestMethod]
        public async Task TranslatesTheWindowsDriverVersionToNvidiasFormat()
        {
            // Windows reports 31.0.15.2727 for what NVIDIA calls 527.27.
            var hardware = new FakeSystemHardwareInfo().WithDriver("NVIDIA GeForce RTX 3080", "31.0.15.2727");

            var driver = await DriverManagerFor(hardware).GetInstalledDriverInfo();

            Assert.AreEqual("527.27", driver.DriverVersion);
        }

        [TestMethod]
        public async Task TranslatesTheDriverVersionIndependentlyOfTheMachinesCulture()
        {
            /* A culture with a comma decimal separator used to turn "552.44" into 55244, which
             * then compared as newer than every real update.
             */

            var original = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                var hardware = new FakeSystemHardwareInfo().WithDriver("NVIDIA GeForce RTX 3080", "31.0.15.5244");

                var driver = await DriverManagerFor(hardware).GetInstalledDriverInfo();

                Assert.AreEqual("552.44", driver.DriverVersion);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [TestMethod]
        public async Task ReportsAnUnreadableDriverVersionClearly()
        {
            var hardware = new FakeSystemHardwareInfo().WithDriver("NVIDIA GeForce RTX 3080", "not-a-version");

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => DriverManagerFor(hardware).GetInstalledDriverInfo());
        }

        [TestMethod]
        [DataRow((ushort)2, true, DisplayName = "Mobile chassis")]
        [DataRow((ushort)1, false, DisplayName = "Desktop chassis")]
        [DataRow((ushort)3, false, DisplayName = "Workstation chassis")]
        public async Task DetectsWhetherTheMachineIsALaptop(ushort pcSystemType, bool expected)
        {
            var hardware = new FakeSystemHardwareInfo { PcSystemType = pcSystemType }
                .WithDriver("NVIDIA GeForce GTX 1080");

            var driver = await DriverManagerFor(hardware).GetInstalledDriverInfo();

            Assert.AreEqual(expected, driver.IsMobileSystem);
        }

        [TestMethod]
        public async Task TreatsAnUnreportedChassisAsDesktop()
        {
            var hardware = new FakeSystemHardwareInfo { PcSystemType = null }
                .WithDriver("NVIDIA GeForce GTX 1080");

            var driver = await DriverManagerFor(hardware).GetInstalledDriverInfo();

            Assert.IsFalse(driver.IsMobileSystem);
        }

        [TestMethod]
        public async Task TreatsAFailedChassisQueryAsDesktopRatherThanFailingTheCheck()
        {
            var hardware = new FakeSystemHardwareInfo { ChassisQueryFailure = new InvalidOperationException("no such class") }
                .WithDriver("NVIDIA GeForce GTX 1080");

            var driver = await DriverManagerFor(hardware).GetInstalledDriverInfo();

            Assert.IsFalse(driver.IsMobileSystem);
        }
    }
}
