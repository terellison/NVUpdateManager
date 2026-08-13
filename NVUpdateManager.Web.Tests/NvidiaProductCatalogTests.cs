using Microsoft.VisualStudio.TestTools.UnitTesting;
using NVUpdateManager.Web.Data;

namespace NVUpdateManager.Web.Tests
{
    [TestClass]
    public class NvidiaProductCatalogTests
    {
        private StubLookupHandler _handler = null!;
        private NvidiaProductCatalog _catalog = null!;

        [TestInitialize]
        public void Setup()
        {
            _handler = new StubLookupHandler();

            // An empty cache path keeps the test off the file system.
            var options = new NvidiaCatalogOptions { CacheFilePath = string.Empty };

            _catalog = new NvidiaProductCatalog(new StubHttpClientFactory(_handler), options);
        }

        [TestMethod]
        public async Task ResolvesDesktopGpuFromWindowsDeviceName()
        {
            var product = await _catalog.ResolveProductAsync("NVIDIA GeForce RTX 4090", preferNotebook: false);

            Assert.IsNotNull(product);
            Assert.AreEqual(127, product.ProductSeriesId);
            Assert.AreEqual(995, product.ProductFamilyId);
            Assert.IsFalse(product.IsNotebook);
        }

        [TestMethod]
        public async Task ResolvesEachGpuToItsOwnIdentifiers()
        {
            /* The hardcoded table this replaced mapped the RTX 4060 to 1015, which is actually
             * the RTX 4070. Resolving against NVIDIA's catalogue makes that class of typo
             * impossible.
             */

            var rtx4070 = await _catalog.ResolveProductAsync("NVIDIA GeForce RTX 4070", preferNotebook: false);
            var rtx4060 = await _catalog.ResolveProductAsync("NVIDIA GeForce RTX 4060", preferNotebook: false);

            Assert.IsNotNull(rtx4070);
            Assert.IsNotNull(rtx4060);
            Assert.AreEqual(1015, rtx4070.ProductFamilyId);
            Assert.AreEqual(1023, rtx4060.ProductFamilyId);
        }

        [TestMethod]
        public async Task ResolvesGpuNamesNvidiaListsWithoutTheVendorPrefix()
        {
            // Windows always reports "NVIDIA ..."; NVIDIA only prefixes its newer catalogue entries.
            var product = await _catalog.ResolveProductAsync("NVIDIA GeForce GTX 1080", preferNotebook: false);

            Assert.IsNotNull(product);
            Assert.AreEqual(815, product.ProductFamilyId);
        }

        [TestMethod]
        public async Task ResolvesLaptopGpu()
        {
            var product = await _catalog.ResolveProductAsync("NVIDIA GeForce RTX 4070 Laptop GPU", preferNotebook: true);

            Assert.IsNotNull(product);
            Assert.AreEqual(129, product.ProductSeriesId);
            Assert.AreEqual(1006, product.ProductFamilyId);
            Assert.IsTrue(product.IsNotebook);
        }

        [TestMethod]
        public async Task ChassisDecidesBetweenIdenticallyNamedDesktopAndNotebookParts()
        {
            // Pre-Ampere notebook GPUs carry no distinguishing suffix.
            var desktop = await _catalog.ResolveProductAsync("NVIDIA GeForce GTX 1080", preferNotebook: false);
            var notebook = await _catalog.ResolveProductAsync("NVIDIA GeForce GTX 1080", preferNotebook: true);

            Assert.IsNotNull(desktop);
            Assert.IsNotNull(notebook);
            Assert.AreEqual(815, desktop.ProductFamilyId);
            Assert.AreEqual(819, notebook.ProductFamilyId);
        }

        [TestMethod]
        [DataRow("nvidia geforce rtx 4090")]
        [DataRow("NVIDIA  GeForce   RTX 4090")]
        [DataRow("  NVIDIA GeForce RTX 4090  ")]
        public async Task IgnoresCasingAndSpacingDifferences(string deviceName)
        {
            var product = await _catalog.ResolveProductAsync(deviceName, preferNotebook: false);

            Assert.IsNotNull(product);
            Assert.AreEqual(995, product.ProductFamilyId);
        }

        [TestMethod]
        public async Task ReturnsNullForGpusNvidiaDoesNotPublish()
        {
            var product = await _catalog.ResolveProductAsync("NVIDIA GeForce RTX 9090", preferNotebook: false);

            Assert.IsNull(product);
        }

        [TestMethod]
        public async Task BuildsTheCatalogOnlyOnce()
        {
            await _catalog.ResolveProductAsync("NVIDIA GeForce RTX 4090", preferNotebook: false);

            var afterFirstLookup = _handler.RequestCount;

            await _catalog.ResolveProductAsync("NVIDIA GeForce RTX 4060", preferNotebook: false);

            Assert.AreEqual(afterFirstLookup, _handler.RequestCount);
        }

        [TestMethod]
        public async Task ResolvesOperatingSystemIdentifier()
        {
            var osId = await _catalog.ResolveOperatingSystemIdAsync(127, "Windows 11");

            Assert.AreEqual(135, osId);
        }

        [TestMethod]
        public async Task FallsBackToTheNewestWindowsAnOlderSeriesOffers()
        {
            // Legacy series predate current Windows releases and must not fail the lookup.
            var osId = await _catalog.ResolveOperatingSystemIdAsync(101, "Windows 14");

            Assert.AreEqual(135, osId);
        }

        [TestMethod]
        public async Task CachesOperatingSystemLookupsPerSeries()
        {
            await _catalog.ResolveOperatingSystemIdAsync(127, "Windows 11");

            var afterFirstLookup = _handler.RequestCount;

            await _catalog.ResolveOperatingSystemIdAsync(127, "Windows 11");

            Assert.AreEqual(afterFirstLookup, _handler.RequestCount);
        }

        [TestMethod]
        public void ParsesLookupValuesFromTheServicesXml()
        {
            // The real service breaks the line between Name and Value.
            const string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><LookupValueSearch><LookupValues>"
                + "<LookupValue RequiresProduct=\"True\" ParentID=\"1\"><Name>GeForce RTX 50 Series</Name>\n<Value>131</Value></LookupValue>"
                + "<LookupValue><Name>Not a number</Name>\n<Value>abc</Value></LookupValue>"
                + "</LookupValues></LookupValueSearch>";

            var values = NvidiaProductCatalog.ParseLookupValues(xml);

            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("GeForce RTX 50 Series", values[0].Name);
            Assert.AreEqual(131, values[0].Value);
        }

        [TestMethod]
        [DataRow("NVIDIA GeForce RTX 3080", "GEFORCE RTX 3080")]
        [DataRow("GeForce RTX 3080", "GEFORCE RTX 3080")]
        [DataRow("  GeForce   GTX 1660   SUPER ", "GEFORCE GTX 1660 SUPER")]
        [DataRow("", "")]
        public void NormalizesNamesForComparison(string input, string expected)
        {
            Assert.AreEqual(expected, NvidiaProductCatalog.NormalizeProductName(input));
        }
    }
}
