using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NVUpdateManager.Core.Data;
using NVUpdateManager.Core.Extensions;
using NVUpdateManager.Core.Interfaces;
using NVUpdateManager.Web.Data;
using NVUpdateManager.Web.Extensions;

namespace NVUpdateManager.Web.Tests
{
    /// <summary>
    /// Joins the two halves together: what Windows reports about the installed GPU, through to
    /// the identifiers NVIDIA's driver search needs. Both ends are substituted, so this runs
    /// anywhere and does not depend on the machine's own hardware.
    /// </summary>
    [TestClass]
    public class InstalledGpuIdentificationTests
    {
        private sealed class StubSystemHardwareInfo : ISystemHardwareInfo
        {
            private readonly PnpDriverRecord[] _drivers;
            private readonly ushort? _pcSystemType;

            public StubSystemHardwareInfo(string deviceName, string driverVersion, ushort? pcSystemType)
            {
                _drivers = new[] { new PnpDriverRecord(deviceName, driverVersion) };
                _pcSystemType = pcSystemType;
            }

            public IReadOnlyList<PnpDriverRecord> GetSignedDrivers() => _drivers;

            public ushort? GetPcSystemType() => _pcSystemType;
        }

        private static ServiceProvider BuildProvider(StubSystemHardwareInfo hardware, StubLookupHandler handler)
        {
            var services = new ServiceCollection();

            services.AddSingleton<ISystemHardwareInfo>(hardware);
            services.AddSingleton<IHttpClientFactory>(new StubHttpClientFactory(handler));
            services.AddSingleton(new NvidiaCatalogOptions { CacheFilePath = string.Empty });
            services.AddDriverManager();
            services.AddUpdateFinder();

            return services.BuildServiceProvider();
        }

        private static async Task<GpuProduct> IdentifyAsync(string deviceName, ushort? pcSystemType)
        {
            var provider = BuildProvider(
                new StubSystemHardwareInfo(deviceName, "31.0.15.2727", pcSystemType),
                new StubLookupHandler());

            var installed = await provider.GetRequiredService<IDriverManager>().GetInstalledDriverInfo();

            return await provider.GetRequiredService<IProductCatalog>()
                .ResolveProductAsync(installed.DeviceName, installed.IsMobileSystem);
        }

        [TestMethod]
        public async Task ALaptopResolvesToTheNotebookVariantOfASharedName()
        {
            /* Before Ampere the desktop and notebook GTX 1080 were both simply
             * "NVIDIA GeForce GTX 1080"; only the chassis separates them.
             */

            var product = await IdentifyAsync("NVIDIA GeForce GTX 1080", pcSystemType: 2);

            Assert.IsNotNull(product);
            Assert.AreEqual(819, product.ProductFamilyId);
            Assert.IsTrue(product.IsNotebook);
        }

        [TestMethod]
        public async Task ADesktopResolvesToTheDesktopVariantOfTheSameName()
        {
            var product = await IdentifyAsync("NVIDIA GeForce GTX 1080", pcSystemType: 1);

            Assert.IsNotNull(product);
            Assert.AreEqual(815, product.ProductFamilyId);
            Assert.IsFalse(product.IsNotebook);
        }

        [TestMethod]
        public async Task AModernGpuResolvesWithoutNeedingTheChassis()
        {
            // From Ampere onwards NVIDIA suffixes the mobile parts, so the name alone is enough.
            var product = await IdentifyAsync("NVIDIA GeForce RTX 4070 Laptop GPU", pcSystemType: 2);

            Assert.IsNotNull(product);
            Assert.AreEqual(129, product.ProductSeriesId);
            Assert.AreEqual(1006, product.ProductFamilyId);
        }

        [TestMethod]
        public async Task TheInstalledVersionIsReportedInNvidiasFormat()
        {
            var provider = BuildProvider(
                new StubSystemHardwareInfo("NVIDIA GeForce RTX 4090", "31.0.15.2727", 1),
                new StubLookupHandler());

            var installed = await provider.GetRequiredService<IDriverManager>().GetInstalledDriverInfo();

            Assert.AreEqual("527.27", installed.DriverVersion);
        }
    }
}
