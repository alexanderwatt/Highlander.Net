using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.CurveEngine.Tests.Helpers;

namespace National.QRSC.PortfolioValuer.Regression.Tests
{
    [TestClass]
    public class General
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            ServerStoreHelper.Initialize();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            ServerStoreHelper.TidyUp();
        }
    }
}