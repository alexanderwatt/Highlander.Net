using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.EquitiesVolCalc.TestData;
using Orion.EquityVolCalcTestData;
using Orion.Util.Serialisation;
using Stock = Orion.EquityVolCalcTestData.Stock;

namespace Orion.EquitiesVolCalc.Tests
{
    /// <summary>
    /// Summary description for LoadTestData
    /// </summary>
    [TestClass]
    public class LoadTestData
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestSaveSampleFile()
        {
            var stock1 = new Stock
                {
                    Name = "XXX",
                    AssetId = "XXX",
                    VolatilitySurface = new StockVolatilitySurface
                        {
                            StockId = "XXX",
                            IsComplete = "false",
                            Expiries = new StockVolatilitySurfaceForwardExpiry[1]
                        }
                };
            stock1.VolatilitySurface.Expiries[0] = new StockVolatilitySurfaceForwardExpiry
                {
                    ExpiryDate = DateTime.Now.ToShortDateString()
                };
            XmlSerializerHelper.SerializeToFile(stock1, "sample.xml");
            Debug.Print("Sample.xml save OK.");
            var stock2 = XmlSerializerHelper.DeserializeFromFile<Stock>("sample.xml");
            Assert.AreEqual("XXX",stock2.Name);
            Debug.Print("Sample.xml load OK.");
        }

        [TestMethod]
        public void TestLoadAllSdVols()
        {
            var stock = TestDataHelper.GetStock("AGK");
            Assert.AreEqual("AGK", stock.Name);
        }
    }
}
