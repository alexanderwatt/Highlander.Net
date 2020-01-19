using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.EquitiesVolCalc.TestData;
using ForwardExpiry = FpML.V5r10.EquityVolatilityCalculator.ForwardExpiry;


namespace FpML.V5r10.EquitiesVolCalcTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ForwardCalc
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestForward()
        {       
            var stock = TestDataHelper.GetStock("AGK");
            var stockObject = TestHelper.CreateStock(stock);      
            var fwdExp = stockObject.VolatilitySurface.Expiries[0];
            var expiry = new DateTime(2011, 11, 9);
            double fwd = stockObject.GetForward(stockObject.Date, expiry);
            Assert.AreEqual(1379.26, Math.Round(fwd,2));                      
        }

        [TestMethod]
        public void TestForward2()
        {
            var stock = TestDataHelper.GetStock("ANZ");
            var stockObject = TestHelper.CreateStock(stock);
            ForwardExpiry fwdExp = stockObject.VolatilitySurface.Expiries[0];
            var expiry = new DateTime(2011, 05, 9);
            double fwd = stockObject.GetForward(stockObject.Date, expiry);
            Assert.AreEqual(2185.27, Math.Round(fwd, 2));
        }      
    }
}
