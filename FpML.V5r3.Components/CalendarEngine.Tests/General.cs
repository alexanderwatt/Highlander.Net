using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.CalendarEngine.Tests
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