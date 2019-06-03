
using Orion.Analytics.Stochastics.SABR;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.Volatilities
{
    /// <summary>
    ///This is a test class for SABRKeyTest and is intended
    ///to contain all SABRKeyTest Unit Tests
    ///</summary>
    [TestClass]
    public class SABRKeyTest
    {
        private static string _expiry = "6m";
        private static string _tenor = "3yr";

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //    _expiry = "3m";
        //    _tenor = "2yr";
        //}
        
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for TenorAsDouble
        ///</summary>
        [TestMethod]
        public void TenorAsDoubleTest()
        {
            SABRKey target = new SABRKey(_expiry, _tenor);

            decimal actual = target.TenorAsDecimal;
            Assert.AreEqual(3m, actual);
        }

        /// <summary>
        ///A test for Tenor
        ///</summary>
        [TestMethod]
        public void TenorTest()
        {
            SABRKey target = new SABRKey(_expiry, _tenor);
            string actual;

            actual = target.Tenor;
            Assert.AreEqual("3yr", actual);
        }

        /// <summary>
        ///A test for ExpiryAsDouble
        ///</summary>
        [TestMethod]
        public void ExpiryAsDoubleTest()
        {
            SABRKey target = new SABRKey(_expiry, _tenor);

            decimal actual = target.ExpiryAsDecimal;
            Assert.AreEqual(0.5m, actual);
        }

        /// <summary>
        ///A test for Expiry
        ///</summary>
        [TestMethod]
        public void ExpiryTest()
        {
            SABRKey target = new SABRKey(_expiry, _tenor);
            string actual;

            actual = target.Expiry;
            Assert.AreEqual("6m", actual);
        }

        ///// <summary>
        /////A test for LabelSplitter
        /////</summary>
        //[TestMethod()]
        //[DeploymentItem("SABRData.dll")]
        //public void LabelSplitterTest()
        //{
        //    string label = "6mth";
        //    string alpha = string.Empty;
        //    string alphaExpected = "mth";
        //    decimal num = 0.0;
        //    decimal numExpected = 6.0;

        //    SABRKey_Accessor.LabelSplitter(label, ref alpha, ref num);
        //    Assert.AreEqual(alphaExpected, alpha);
        //    Assert.AreEqual(numExpected, num);
        //}

        /// <summary>
        ///A test for GetHashCode. Let us just attempt to prove that hashcodes are consistent.
        ///</summary>
        [TestMethod]
        public void GetHashCodeTest()
        {
            SABRKey target = new SABRKey();
            SABRKey obj = target;
            int expected = obj.GetHashCode();
            int actual;

            actual = target.GetHashCode(obj);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A complete test for Equals
        ///
        ///</summary>
        [TestMethod]
        public void EqualsTest()
        {
            string expiry = "2y";
            string tenor = "5y";
            string expiry1 = "6m";
            string tenor1 = "1y";
            SABRKey target = new SABRKey(); // TODO: Initialize to an appropriate value

            bool expected = true;
            bool actual;

            // test ATM keys
            SABRKey x = new SABRKey(expiry);
            SABRKey y = new SABRKey(expiry);
            actual = target.Equals(x, y);
            Assert.AreEqual(expected, actual);

            // x will remain constant
            x = new SABRKey(expiry, tenor);

            // test full keys
            y = new SABRKey(expiry, tenor);
            actual = target.Equals(x, y);
            Assert.AreEqual(expected, actual);

            // Each equality test should return false
            expected = false;

            // test keys with differing tenors
            y = new SABRKey(expiry, tenor1);
            actual = target.Equals(x, y);
            Assert.AreEqual(expected, actual);

            // test keys with differing expiries
            y = new SABRKey(expiry1, tenor);
            actual = target.Equals(x, y);
            Assert.AreEqual(expected, actual);

            // test keys with differing tenors/expiries
            y = new SABRKey(expiry1, tenor1);
            actual = target.Equals(x, y);
            Assert.AreEqual(expected, actual);

            // test ATM keys with differing expiry
            x = new SABRKey(expiry);
            y = new SABRKey(expiry1);
            actual = target.Equals(x, y);
            Assert.AreEqual(expected, actual);
        }

        ///// <summary>
        /////A test for ConvertToDouble
        /////</summary>
        //[TestMethod()]
        //[DeploymentItem("SABRData.dll")]
        //public void ConvertToDoubleTest()
        //{
        //    string term = _expiry;
        //    decimal expected = 0.5;
        //    decimal actual;
        //    actual = SABRKey_Accessor.ConvertToDouble(term);
        //    Assert.AreEqual(expected, actual);
        //}

        ///// <summary>
        /////A test for ConversionFactor
        /////</summary>
        //[TestMethod()]
        //[DeploymentItem("SABRData.dll")]
        //public void ConversionFactorTest()
        //{
        //    string period = "Mth";
        //    decimal expected = 1.0 / 12.0;
        //    decimal actual;
        //    actual = SABRKey_Accessor.ConversionFactor(period);
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        ///A full test for Compare (x is the target to test against)
        ///</summary>
        [TestMethod]
        public void CompareTest()
        {
            string expiryLess = "1yr";
            string expiry = "3yr";
            string expiryGreater = "5yr";
            string tenorLess = "6mth";
            string tenor = "1yr";
            string tenorGreater = "3yr";

            SABRKey target = new SABRKey(); // TODO: Initialize to an appropriate value
            SABRKey x = new SABRKey(expiry, tenor);

            // Equal tests
            int expected = 0;

            // Test that the Expiry/tenor pairs are equal
            SABRKey y = new SABRKey(expiry, tenor);
            int actual = target.Compare(x, y);
            Assert.AreEqual(expected, actual);

            // Test that the Expiry values (x is the target)
            expected = -1;
            y = new SABRKey(expiryGreater, tenor);
            actual = target.Compare(x, y);
            Assert.AreEqual(expected, actual);

            expected = 1;
            y = new SABRKey(expiryLess, tenor);
            actual = target.Compare(x, y);
            Assert.AreEqual(expected, actual);

            // Test that the Tenor values (x is the target)
            expected = -1;
            y = new SABRKey(expiry, tenorGreater);
            actual = target.Compare(x, y);
            Assert.AreEqual(expected, actual);

            expected = 1;
            y = new SABRKey(expiry, tenorLess);
            actual = target.Compare(x, y);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SABRKey Constructor
        ///</summary>
        [TestMethod]
        public void SABRKeyConstructorTest2()
        {
            SABRKey target = new SABRKey();
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for SABRKey Constructor
        ///</summary>
        [TestMethod]
        public void SABRKeyConstructorTest1()
        {
            string expiry = _expiry;
            SABRKey target = new SABRKey(expiry);
            string expected = _expiry;

            Assert.AreEqual(expected, target.Expiry);
            Assert.AreEqual("ATM", target.Tenor);
        }

        /// <summary>
        ///A test for SABRKey Constructor
        ///</summary>
        [TestMethod]
        public void SABRKeyConstructorTest()
        {
            string expiry = _expiry;
            string tenor = _tenor;
            SABRKey target = new SABRKey(expiry, tenor);

            Assert.AreEqual(expiry, target.Expiry);
            Assert.AreEqual(tenor, target.Tenor);
        }
    }
}