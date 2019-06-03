using Orion.Util.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Util.Tests.Helpers
{
    /// <summary>
    ///This is a test class for ReflectionHelperTest and is intended
    ///to contain all ReflectionHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class ReflectionHelperTest
    {
        /// <summary>
        ///A test for GetAssemblyCodeBaseLocation
        ///</summary>
        [TestMethod]
        public void GetAssemblyCodeBaseLocationTest()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string actual = ReflectionHelper.GetAssemblyCodeBaseLocation(assembly);

            Assert.IsTrue(!string.IsNullOrEmpty(actual));
        }

        /// <summary>
        ///A test for ChangeType
        ///</summary>
        [TestMethod]
        public void ChangeTypeTest()
        {
            object actual = ReflectionHelper.ChangeType((int)3, typeof(double));
            Assert.AreEqual(3d, actual);

            actual = ReflectionHelper.ChangeType('f', typeof(string));
            Assert.AreEqual("f", actual);

            const EnumHelperTest.TestEnum Value = EnumHelperTest.TestEnum.Value2;
            actual = ReflectionHelper.ChangeType(Value, typeof(decimal));
            Assert.AreEqual((decimal)Value, actual);

            const decimal Value2 = (decimal)EnumHelperTest.TestEnum.Value2;
            actual = ReflectionHelper.ChangeType(Value2, typeof(EnumHelperTest.TestEnum));
            Assert.AreEqual((EnumHelperTest.TestEnum)Value2, actual);
        }
    }
}