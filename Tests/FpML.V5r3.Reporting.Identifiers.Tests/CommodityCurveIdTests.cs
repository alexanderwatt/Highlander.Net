using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.Util.NamedValues;

namespace Orion.Identifiers.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class CommodityCurveIdTests
    {
        private readonly DateTime baseDate = new DateTime(2008, 3, 3);

        [TestMethod]
        public void PricingStructureIdTestWithProperties13()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "CommodityCurve");
            props.Set(CurveProp.CurveName, "AUD-USD");
            props.Set("BuildDateTime", baseDate);
            props.Set(CurveProp.BaseDate, baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            props.Set("CommodityAsset", "Wheat");
            var curveId = new CommodityCurveIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6} CommodityAsset : {7} Domain : {8} SourceSystem : {9} UniqueId : {10} Market : {11}",
                        curveId.Id, curveId.BuildDateTime, curveId.CurveName, curveId.PricingStructureType,
                        curveId.Algorithm, curveId.Currency.Value, curveId.BaseDate, curveId.CommodityAsset, curveId.Domain, curveId.SourceSystem, curveId.UniqueIdentifier, curveId.Market);
        }
    }
}
