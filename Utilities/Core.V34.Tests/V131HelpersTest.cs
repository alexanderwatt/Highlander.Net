using Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Logging;

namespace Core.V34.Tests
{
    [TestClass]
    public class V131HelpersTest
    {
        [TestMethod]
        public void CheckRequiredFileVersionTest()
        {
            ILogger logger = new TraceLogger(false);
            const string RequiredVersion = "3.4.1222.0";

            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, RequiredVersion, "3.3.1011.6602"));
            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, RequiredVersion, "3.3.1208.6602"));
            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, RequiredVersion, "3.3.1222.6602"));

            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, RequiredVersion, "3.4.1011.6602"));
            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, RequiredVersion, "3.4.1208.6602"));
            Assert.IsTrue(V131Helpers.CheckRequiredFileVersion(logger, RequiredVersion, "3.4.1222.6602"));

            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, RequiredVersion, "3.5.1208.6602"));
            Assert.IsTrue(V131Helpers.CheckRequiredFileVersion(logger, RequiredVersion, "3.5.1222.6602"));
            Assert.IsTrue(V131Helpers.CheckRequiredFileVersion(logger, RequiredVersion, "3.5.1407.6602"));
        }
    }
}
