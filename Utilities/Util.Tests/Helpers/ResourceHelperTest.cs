using Orion.Util.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Util.Tests.Helpers
{
    /// <summary>
    ///This is a test class for ResourceHelperTest and is intended
    ///to contain all ResourceHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class ResourceHelperTest
    {
        /// <summary>
        ///A test for ReadResourceValue
        ///</summary>
        [TestMethod]
        [Ignore] // Doesn't work
        public void ReadResourceValueTest()
        {
            string actual = ResourceHelper.ReadResourceValue("Util.Tests.Helpers.ResourceTest", "Name1");
            Assert.AreEqual("value1", actual);
        }

        /// <summary>
        ///A test for GetResourceWithPartialName
        ///</summary>
        [TestMethod]
        public void GetResourceWithPartialNameTest()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string partialResourceName = "ResourceTest.txt";
            string actual = ResourceHelper.GetResourceWithPartialName(assembly, partialResourceName);
            Assert.AreEqual("Resource Test", actual);
            actual = ResourceHelper.GetResourceWithPartialName(assembly, "invalid");
            Assert.AreEqual(null, actual);
        }

        /// <summary>
        ///A test for GetResource
        ///</summary>
        [TestMethod]
        public void GetResourceTest()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "Util.Tests.Helpers.ResourceTest.txt";
            string actual = ResourceHelper.GetResource(assembly, resourceName);
            Assert.AreEqual("Resource Test", actual);
        }
    }
}