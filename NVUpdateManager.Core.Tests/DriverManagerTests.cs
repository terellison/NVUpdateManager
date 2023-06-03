using Microsoft.VisualStudio.TestTools.UnitTesting;
using NVUpdateManager.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace NVUpdateManager.Core.Tests
{
    [TestClass()]
    public class DriverManagerTests
    {
        [TestMethod()]
        public async Task GetInstalledDriverInfoTest()
        {
            DriverInfo? driverData = null;

            try
            {
                driverData = await DriverManager.GetInstalledDriverInfo();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NotSupportedException));
            }

            Assert.IsNotNull(driverData);

            var properties = driverData.GetType().GetProperties();

            foreach( var property in properties )
            {
                Assert.IsNotNull(property.GetValue(driverData));
            }
        }
    }
}