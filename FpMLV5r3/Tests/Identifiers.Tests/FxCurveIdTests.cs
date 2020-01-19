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
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class FxCurveIdTests
    {
        private readonly DateTime _baseDate = new DateTime(2008, 3, 3);

        [TestMethod]
        public void PricingStructureIdTestWithProperties12()
        {
            var props = new NamedValueSet();
            props.Set(CurveProp.PricingStructureType, "FxCurve");
            props.Set(CurveProp.CurveName, "AUD-USD");
            props.Set("BuildDateTime", _baseDate);
            props.Set(CurveProp.BaseDate, _baseDate);
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
            const string curveId = "AUD-USD";
            var fxCurveId = new FxCurveIdentifier(curveId);
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
