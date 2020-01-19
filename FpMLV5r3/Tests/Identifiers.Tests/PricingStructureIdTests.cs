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
using Highlander.Constants;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Utilities.NamedValues;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Identifiers.Tests.V5r3
{
    [TestClass]
    public class PricingStructureIdTests
    {
        private readonly DateTime _baseDate = new DateTime(2008, 3, 3);
        private const string LiborIndexName = "AUD-LIBOR-BBA-3M";

        [TestMethod]
        public void PricingStructureIdTest()
        {
            var rateCurveId = new PricingStructureIdentifier(PricingStructureTypeEnum.RateCurve, LiborIndexName, _baseDate);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithDodgyCurveName()
        {
            var rateCurveId = new PricingStructureIdentifier(PricingStructureTypeEnum.FxCurve, "FxCurve.1", _baseDate);
            Debug.Print("CurveId : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties1()
        {
            var props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "RateCurve");
            props.Set(CurveProp.CurveName, LiborIndexName);
            props.Set("BuildDateTime", _baseDate);
            props.Set(CurveProp.BaseDate, _baseDate);
            props.Set("Identifier", "RateCurve.Live." + LiborIndexName);
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
            props.Set(CurveProp.CurveName, LiborIndexName);
            props.Set("BuildDateTime", _baseDate);
            props.Set("Identifier", "Alex");
            props.Set("Algorithm", "Default");
            props.Set(CurveProp.BaseDate, _baseDate);
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
            props.Set(CurveProp.CurveName, LiborIndexName);
            props.Set("BuildDateTime", _baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            props.Set(CurveProp.BaseDate, _baseDate);
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
            props.Set("Identifier", "RateCurve." + LiborIndexName + "." + _baseDate);
            props.Set("BuildDateTime", _baseDate);
            props.Set(CurveProp.BaseDate, _baseDate);
            props.Set(CurveProp.CurveName, LiborIndexName);
            props.Set("Algorithm", "Default");
            var rateCurveId = new PricingStructureIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

        [TestMethod]
        public void PricingStructureIdTestWithProperties5()
        {
            var rateCurveId = new PricingStructureIdentifier("RateCurve." + LiborIndexName + "." + _baseDate);
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
            props.Set("BuildDateTime", _baseDate);
            props.Set(CurveProp.BaseDate, _baseDate);
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
            props.Set(CurveProp.CurveName, LiborIndexName);
            props.Set("BuildDateTime", _baseDate);
            props.Set(CurveProp.BaseDate, _baseDate);
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
            props.Set(CurveProp.CurveName, LiborIndexName);
            props.Set("BuildDateTime", _baseDate);
            props.Set(CurveProp.BaseDate, _baseDate);
            props.Set("Algorithm", "Default");
            props.Set("Identifier", "Alex");
            var rateCurveId = new PricingStructureIdentifier(props);
            Debug.Print("RateCurveIdentifier : {0} BuildDateTime : {1} CurveName : {2} PricingStructureType : {3} Algorithm : {4}Currency : {5} BaseDate : {6}",
                        rateCurveId.Id, rateCurveId.BuildDateTime, rateCurveId.CurveName, rateCurveId.PricingStructureType,
                        rateCurveId.Algorithm, rateCurveId.Currency.Value, rateCurveId.BaseDate);
        }

    }
}
