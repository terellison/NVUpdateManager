using Microsoft.VisualStudio.TestTools.UnitTesting;
using NVUpdateManager.Core.Interfaces;

namespace NVUpdateManager.Core.Tests
{
    [TestClass()]
    public class DriverManagerTests
    {

        private readonly IDriverManager _driverManager;

        public DriverManagerTests(IDriverManager driverManager)
        {
            _driverManager = driverManager;
        }

        [TestMethod()]
        public async Task GetInstalledDriverInfoTest()
        {
            DriverInfo? driverData = null;

            try
            {
                driverData = await _driverManager.GetInstalledDriverInfo();
            }
            catch (Exception ex)
            {
                Assert.IsNotInstanceOfType(ex, typeof(NotSupportedException));
            }

            if (driverData != null)
            {
                var properties = driverData.GetType().GetProperties();

                foreach (var property in properties)
                {
                    Assert.IsNotNull(property.GetValue(driverData));
                } 
            }
        }
    }
}