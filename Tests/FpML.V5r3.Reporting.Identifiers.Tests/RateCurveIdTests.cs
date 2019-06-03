using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.Util.NamedValues;

namespace Orion.Identifiers.Tests
{
    [TestClass]
    public class RateCurveIdTests
    {
        private readonly DateTime _baseDate = new DateTime(2008, 3, 3);
        private const string LiborIndexName = "AUD-LIBOR-BBA-3M";

        [TestMethod]
        public void PricingStructureIdTestWithProperties10()
        {
            var props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "DiscountCurve");
            props.Set(CurveProp.CurveName, "AUD-NAB-SENIOR");
            props.Set("BuildDateTime", _baseDate);
            props.Set(CurveProp.BaseDate, _baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            var rateCurveId = new RateCurveIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4} Currency : {5} BaseDate : {6} CreditInstrumentId : {7} CreditSeniority : {8}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate, rateCurveId.CreditInstrumentId.Value, rateCurveId.CreditSeniority.Value);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties11()
        {
            var props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "InflationCurve");
            props.Set(CurveProp.CurveName, LiborIndexName);
            props.Set("BuildDateTime", _baseDate);
            props.Set(CurveProp.BaseDate, _baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            props.Set(CurveProp.IndexName, "AUD-LIBOR-BBA");
            props.Set(CurveProp.IndexTenor, "3M");
            props.Set("InflationLag", "3M");
            var rateCurveId = new RateCurveIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6} InflationLag : {7}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate, rateCurveId.InflationLag);
        }

        [TestMethod]
        public void RateCurveIdConstructorWithCurveIdTest()
        {
            var rateCurve = new RateCurveIdentifier("RateCurve.AUD-ZERO-BANK-3M", DateTime.Today);
            Assert.IsNotNull(rateCurve);
            Assert.AreEqual(DateTime.Today, rateCurve.BaseDate);
            Assert.AreEqual("AUD-ZERO-BANK-3M", rateCurve.CurveName);
            Assert.AreEqual("AUD", rateCurve.Currency.Value);
            Assert.AreEqual("ZERO-BANK", rateCurve.Index);
            Assert.AreEqual("3M", rateCurve.IndexTenor);
        }

    }
}
