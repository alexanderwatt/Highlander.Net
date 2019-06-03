using System.IO;
using Orion.Util.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Util.Tests.Helpers
{
    /// <summary>
    ///This is a test class for DisposeHelperTest and is intended
    ///to contain all DisposeHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class DisposeHelperTest
    {
        [TestMethod]
        public void SafeDisposeTest()
        {
            StreamReader a = new StreamReader(@"C:\windows\explorer.exe");
            DisposeHelper.SafeDispose(ref a);
            Assert.AreEqual(null, a);
        }
    }
}