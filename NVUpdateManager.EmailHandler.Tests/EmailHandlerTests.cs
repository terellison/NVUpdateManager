using Microsoft.VisualStudio.TestTools.UnitTesting;
using static NVUpdateManager.EmailHandler.EmailHandler;

namespace NVUpdateManager.EmailHandler.Tests
{
    [TestClass()]
    public class EmailHandlerTests
    {
        [TestMethod()]
        public void EncodeLogicAppEndpointTest()
        {
            var writer = new StringWriter();
            Console.SetOut(writer);
            EncodeLogicAppEndpoint("foo");
            var output = writer.ToString();

            Assert.IsTrue(output.Contains("entropy"));
            Assert.IsTrue(output.Contains("encrypted endpoint"));

            var data = output.Split('\n');

            var entropyLength = data[0].LastIndexOf('\"') - data[0].IndexOf('\"');
            var encryptedEndpointLength = data[1].LastIndexOf('\"') - data[1].IndexOf('\"');

            string entropy = data[0].Substring(data[0].IndexOf('\"') + 1, entropyLength - 1);
            string encryptedEndpoint = data[1].Substring(data[1].IndexOf('\"') + 1, encryptedEndpointLength - 1);

            var result = DecodeSecureEndpoint(encryptedEndpoint, entropy);

            Assert.IsTrue(result.Equals("foo"));
        }
    }
}