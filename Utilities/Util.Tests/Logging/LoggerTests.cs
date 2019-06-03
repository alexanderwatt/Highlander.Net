using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Logging;

//using NLog;

namespace Util.Tests.Logging
{
    /// <summary>
    /// Summary description for LoggerTests
    /// </summary>
    [TestClass]
    public class LoggerTests
    {
        public LoggerTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestFileLogger()
        {
            using (FileLogger fileLogger = new FileLogger("c:\\temp\\test.log"))
            {
                fileLogger.LogInfo("testing 1 2 3");
                fileLogger.LogDebug("testing 1 2 3");
            }

        }
    }
}
