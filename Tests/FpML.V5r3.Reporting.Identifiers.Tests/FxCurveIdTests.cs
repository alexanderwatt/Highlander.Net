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
    public class FxCurveIdTests
    {
        private readonly DateTime baseDate = new DateTime(2008, 3, 3);

        [TestMethod]
        public void PricingStructureIdTestWithProperties12()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "FxCurve");
            props.Set(CurveProp.CurveName, "AUD-USD");
            props.Set("BuildDateTime", baseDate);
            props.Set(CurveProp.BaseDate, baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            props.Set("CurrencyPair", "AUD-USD");
            props.Set("QuoteBasis", "Currency2PerCurrency1");
            var curveId = new FxCurveIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6} QuotedCurrencyPair1 : {7} QuotedCurrencyPair2 : {8}",
                        curveId.Id, curveId.BuildDateTime, curveId.CurveName, curveId.PricingStructureType,
                        curveId.Algorithm, curveId.Currency.Value, curveId.BaseDate, curveId.QuotedCurrencyPair.currency1.Value, curveId.QuotedCurrencyPair.currency2.Value);
        }

        [TestMethod]
        public void FxCurveIdConstructByIdTest()
        {
            const string CurveId = "AUD-USD";
            var fxCurveId = new FxCurveIdentifier(CurveId);
            Assert.IsNotNull(fxCurveId);
            Assert.IsNotNull(fxCurveId.Currency);
            Assert.AreEqual("AUD", fxCurveId.Currency.Value);
            Assert.AreEqual("USD", fxCurveId.QuoteCurrency.Value);
            Assert.AreEqual("USD", fxCurveId.QuotedCurrencyPair.currency2.Value);

            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        fxCurveId.Id, fxCurveId.BuildDateTime, fxCurveId.CurveName, fxCurveId.PricingStructureType,
                        fxCurveId.Algorithm, fxCurveId.Currency.Value, fxCurveId.BaseDate);
        }

    }
}
