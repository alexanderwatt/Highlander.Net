/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Diagnostics;
using Highlander.EquitiesCalculator.TestData.V5r3;
using Highlander.EquityCalculator.TestData.V5r3;
using Highlander.Utilities.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stock = Highlander.EquityCalculator.TestData.V5r3.Stock;

namespace Highlander.EquitiesCalculator.Tests.V5r3
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
