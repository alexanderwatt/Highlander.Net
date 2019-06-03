using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.CurveEngine;
using Orion.UnitTestEnv;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.V5r3.PublisherWebService;

namespace PublisherWebService.Tests
{
    [TestClass]
    public class General
    {
        private static UnitTestEnvironment UTE { get; set; }
        public static PricingStructures PricingStructures { get; set; }
        //public static LpmPublisher LpmPublisher { get; set; }
        public static CurveEngine CurveEngine { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            UTE = new UnitTestEnvironment();
            var logger = Reference<ILogger>.Create(UTE.Logger);
            PricingStructures = new PricingStructures(logger, UTE.Cache, UTE.NameSpace);
            //LpmPublisher = new LpmPublisher(logger, UTE.Cache);
            CurveEngine = new CurveEngine(logger.Target, UTE.Cache);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            UTE.Dispose();
        }
    }
}
