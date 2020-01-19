using System.Diagnostics;
using HLV5r3.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.ExcelAPI.Tests.Helpers
{
    [TestClass]
    public class ApplicationHelperTest
    {
        [TestMethod]
        public void DiagnosticsTest()
        {
            object[,] actual = ApplicationHelper.Diagnostics();
            const int expectedItems = 8;
            Assert.AreEqual(expectedItems - 1, actual.GetUpperBound(0));
            Assert.AreEqual(1, actual.GetUpperBound(1));
            for (int i = 0; i < expectedItems - 1; i++)
            {
                Debug.Print("{0}: {1}", actual[i, 0], actual[i, 1]);
                Assert.IsFalse(string.IsNullOrEmpty(actual[i, 0].ToString()));
                Assert.IsFalse(string.IsNullOrEmpty(actual[i, 1].ToString()));
            }
            //When there is a Public Key Token
            Assert.IsFalse(string.IsNullOrEmpty(actual[expectedItems - 1, 1].ToString()));
            //When Public Key Token is empty, this is true.
            //Assert.IsTrue(string.IsNullOrEmpty(actual[expectedItems - 1, 1].ToString()));
        }
    }
}
