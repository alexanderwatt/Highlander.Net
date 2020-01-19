using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.CurveEngine.Extensions;
using Orion.ModelFramework;


namespace Orion.ExcelAPI.Tests.ExcelApi
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
            IPricingStructure pricingStructure = Engine.CreatePricingStructure(structurePropertiesRange.ToNamedValueSet(), valuesRange);
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
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