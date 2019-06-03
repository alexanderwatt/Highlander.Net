using Orion.Util.NamedValues;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Util.Tests.NamedValues
{
    /// <summary>
    ///This is a test class for NamedValueSetHelperTest and is intended
    ///to contain all NamedValueSetHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NamedValueSetHelperTest
    {
        /// <summary>
        ///A test for RangeToNamedValueSet
        ///</summary>
        [TestMethod]
        public void RangeToNamedValueSetTest()
        {
            object[,] properties = new object[,]
                                       {
                                           {"CurveType", "RateCurve"},
                                           {"ExpiryMins", 1}
                                       };
            NamedValueSet actual = new NamedValueSet(properties);
            Assert.AreEqual("RateCurve", actual.Get("CurveType").ValueString);
            Assert.AreEqual(1, actual.Get("ExpiryMins").Value);
        }

        /// <summary>
        ///A test for DistinctInstances
        ///</summary>
        [TestMethod]
        public void DistinctInstancesTest()
        {
            object[,] properties = new object[,]
                                       {
                                           {"CurveType", "RateCurve"},
                                           {"ExpiryMins", 1},
                                           {"ExpiryMins", 2}
                                       };
            NamedValueSet actual = NamedValueSetHelper.DistinctInstances(properties);
            Assert.AreEqual("RateCurve", actual.Get("CurveType").ValueString);
            Assert.AreEqual(1, ((object[])actual.Get("ExpiryMins").Value)[0]);
            Assert.AreEqual(2, ((object[])actual.Get("ExpiryMins").Value)[1]);
        }

        /// <summary>
        ///A test for Build
        ///</summary>
        [TestMethod]
        public void BuildTest()
        {
            string[] names = { "CurveType", "ExpiryMins" };
            object[] values = { "RateCurve", 1 }; 
            NamedValueSet actual = NamedValueSetHelper.Build(names, values);
            Assert.AreEqual("RateCurve", actual.Get("CurveType").ValueString);
            Assert.AreEqual(1, actual.Get("ExpiryMins").Value);
        }
    }
}
