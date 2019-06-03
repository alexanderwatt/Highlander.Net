using System;
using Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.V34.Tests
{
    [TestClass]
    public class ServiceHelperTest
    {
        [TestMethod]
        public void ParseBuildLabelTest()
        {
            ServiceHelper.ParseBuildLabel("3.4.1208.3302", out var major, out var minor, out var buildDate);
            Assert.AreEqual(3, major);
            Assert.AreEqual(4, minor);
            Assert.AreEqual(new DateTime(2018,12,08), buildDate);
            ServiceHelper.ParseBuildLabel("3.4.1407.3302", out major, out minor, out buildDate);
            Assert.AreEqual(3, major);
            Assert.AreEqual(4, minor);
            Assert.AreEqual(new DateTime(2019, 02, 07), buildDate);
        }
    }
}
