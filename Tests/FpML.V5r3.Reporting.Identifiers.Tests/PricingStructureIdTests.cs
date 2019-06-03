using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.Util.NamedValues;

namespace Orion.Identifiers.Tests
{
    [TestClass]
    public class PricingStructureIdTests
    {
        private readonly DateTime baseDate = new DateTime(2008, 3, 3);
        private const string _liborIndexName = "AUD-LIBOR-BBA-3M";

        [TestMethod]
        public void PricingStructureIdTest()
        {
            var rateCurveId = new PricingStructureIdentifier(PricingStructureTypeEnum.RateCurve, _liborIndexName, baseDate);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithdodgyCurveName()
        {
            var rateCurveId = new PricingStructureIdentifier(PricingStructureTypeEnum.FxCurve, "FxCurve.1", baseDate);
            Debug.Print("CurveId : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties1()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "RateCurve");
            props.Set(CurveProp.CurveName, _liborIndexName);
            props.Set("BuildDateTime", baseDate);
            props.Set(CurveProp.BaseDate, baseDate);
            props.Set("Identifier", "RateCurve.Live." + _liborIndexName);
            props.Set("Algorithm", "Default");
            var rateCurveId = new PricingStructureIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties2()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "RateCurve");
            props.Set(CurveProp.CurveName, _liborIndexName);
            props.Set("BuildDateTime", baseDate);
            props.Set("Identifier", "Alex");
            props.Set("Algorithm", "Default");
            props.Set(CurveProp.BaseDate, baseDate);
            var rateCurveId = new PricingStructureIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties3()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "RateCurve");
            props.Set(CurveProp.CurveName, _liborIndexName);
            props.Set("BuildDateTime", baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            props.Set(CurveProp.BaseDate, baseDate);
            var rateCurveId = new PricingStructureIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties4()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "RateCurve");
            props.Set("Identifier", "RateCurve." + _liborIndexName + "." + baseDate);
            props.Set("BuildDateTime", baseDate);
            props.Set(CurveProp.BaseDate, baseDate);
            props.Set(CurveProp.CurveName, _liborIndexName);
            props.Set("Algorithm", "Default");
            var rateCurveId = new PricingStructureIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties5()
        {
            var rateCurveId = new PricingStructureIdentifier("RateCurve." + _liborIndexName + "." + baseDate);
            Assert.IsNotNull(rateCurveId);
            Assert.IsNotNull(rateCurveId.Currency);

            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties6()
        {
            var rateCurveId = new PricingStructureIdentifier("Alex");
            Assert.AreNotEqual(new DateTime(), rateCurveId.BuildDateTime);

        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties7()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "DiscountCurve");
            props.Set(CurveProp.CurveName, "AUD-NAB-SEN");
            props.Set("BuildDateTime", baseDate);
            props.Set(CurveProp.BaseDate, baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            props.Set("CreditInstrumentId", "NAB");
            props.Set("CreditSeniority", "SENIOR");
            var rateCurveId = new PricingStructureIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties8()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "InflationCurve");
            props.Set(CurveProp.CurveName, _liborIndexName);
            props.Set("BuildDateTime", baseDate);
            props.Set(CurveProp.BaseDate, baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            props.Set(CurveProp.IndexName, "AUD-LIBOR-BBA");
            props.Set(CurveProp.IndexTenor, "3M");
            props.Set("InflationLag", "3M");
            var rateCurveId = new PricingStructureIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties9()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "RateCurve");
            props.Set(CurveProp.CurveName, _liborIndexName);
            props.Set("BuildDateTime", baseDate);
            props.Set(CurveProp.BaseDate, baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            var rateCurveId = new PricingStructureIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

    }
}
