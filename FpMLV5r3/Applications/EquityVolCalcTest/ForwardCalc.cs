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
using Highlander.EquitiesCalculator.TestData.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Highlander.EquitiesCalculator.Tests.V5r3
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
            var fwdExp = stockObject.VolatilitySurface.Expiry[0];
            var expiry = new DateTime(2011, 11, 9);
            double fwd = stockObject.GetForward(stockObject.Date, expiry);
            Assert.AreEqual(1379.26, Math.Round(fwd,2));                      
        }

        [TestMethod]
        public void TestForward2()
        {
            var stock = TestDataHelper.GetStock("ANZ");
            var stockObject = TestHelper.CreateStock(stock);
            var fwdExp = stockObject.VolatilitySurface.Expiry[0];
            var expiry = new DateTime(2011, 05, 9);
            double fwd = stockObject.GetForward(stockObject.Date, expiry);
            Assert.AreEqual(2185.27, Math.Round(fwd, 2));
        }      
    }
}
