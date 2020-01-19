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
using Highlander.CurveEngine.V5r3.Extensions;
using Highlander.Reporting.ModelFramework.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Excel.Tests.V5r3.ExcelApi
{
    /// <summary>
    /// Tests for CommodityCurves.xls
    /// </summary>
    public partial class ExcelAPITests
    {
        ///<summary>
        /// Creates the specified curve type.
        ///</summary>
        ///<param name="structurePropertiesRange">The properties range. This must include all mandatory properties.</param>
        ///<param name="valuesRange">The values to be used for bootstrapping.</param>
        ///<returns>A handle to a bootstrapped pricing structure.</returns>
        ///<exception cref="NotImplementedException"></exception>
        private string CreatePricingStructure(object[,] structurePropertiesRange, object[,] valuesRange)
        {
            IPricingStructure pricingStructure = Highlander.Excel.Tests.V5r3.ExcelApi.ExcelAPITests.Engine.CreatePricingStructure(structurePropertiesRange.ToNamedValueSet(), valuesRange);
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Highlander.Excel.Tests.V5r3.ExcelApi.ExcelAPITests.Engine.SaveCurve(pricingStructure);
            return structureId;
        }

        private string CreateCommodityCurve()
        {
            DateTime baseDate = DateTime.Today.AddDays(-1);

            var properties
                = new object[,]
                      {
                          {"PricingStructureType", "CommodityCurve"},
                          {"BaseDate", baseDate},
                          {"MarketName", "TEST"},
                          {"CurveName", "USD-Wheat"},
                          {"Algorithm", "LinearForward"}
                      };

            var values
                = new object[,]
                      {
                          {"USD-CommodityForward-CME.W-1D", 100, 0, 0},
                          {"USD-CommoditySpot-CME.W", 110, 0, 0},
                          {"USD-CommodityForward-CME.W-1W", 120, 0, 0},
                          {"USD-CommodityForward-CME.W-1M", 130, 0, 0},
                          {"USD-CommodityForward-CME.W-1Y", 140, 0, 0},
                          {"USD-CommodityForward-CME.W-10Y", 150, 0, 0}
                      };

            return CreatePricingStructure(properties, values);
        }

        [TestMethod]
        public void CreateCommodityCurveTest()
        {
            string actual = CreateCommodityCurve();
            //Failed	CreateCommodityCurveTest	ExcelAPI.Tests	Assert.AreEqual failed. Expected:<CommodityCurve.USD-Wheat>. Actual:<Orion.Market.TEST.CommodityCurve.USD-Wheat>. 	
            string expected = "Market.TEST.CommodityCurve.USD-Wheat";
            Assert.AreEqual(expected, actual);
        }
    }
}