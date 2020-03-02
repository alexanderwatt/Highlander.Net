﻿/*
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
using System.Collections.Generic;
using Highlander.Codes.V5r3;
using Highlander.CurveEngine.V5r3.Extensions;
using Highlander.CurveEngine.V5r3.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Excel.Tests.V5r3.ExcelApi
{
    /// <summary>
    /// Test methods for InflationAssets.xls
    /// </summary>
    public partial class ExcelAPITests
    {
        private string _curveId;
        private object[,] _values;
        private string _fxcurveId;
        private readonly decimal[] _rates = { 0.70m, 0.69m, 0.68m, 0.67m, 0.66m };
        private readonly decimal[] _additional = { 0, 0, 0, 0, 0 };
        private readonly string[] _fxinstruments = {
                                           "AUDUSD-FxForward-1D",
                                           "AUDUSD-FxSpot-SP",
                                           "AUDUSD-FxForward-1M",
                                           "AUDUSD-FxForward-2M",
                                           "AUDUSD-FxForward-3M"
                                       };

        private readonly object[,] _properties2 = {
                           {"PricingStructureType", "FxCurve"},
                           {"BaseDate", DateTime.Today},
                           {"MarketName", "FXAssetsTests"},
                           {"Currency", "AUD"},
                           {"QuoteCurrency", "USD"},
                           {"Algorithm", "LinearForward"},
                           {"QuoteBasis", "Currency1PerCurrency2"}
                       };


        readonly string[] _surfaceExpiries = { "1D", "1W", "1M", "2M", "3M" };
        readonly double[] _surfaceStrikes = { 0.25, 0.50, 0.75, 1.00 };
        readonly double[,] _surfaceVols = {
                                 {.12, .13, .14, .15},
                                 {.22, .23, .24, .25},
                                 {.32, .33, .34, .35},
                                 {.42, .43, .44, .45},
                                 {.52, .53, .54, .55}
        };

        readonly object[,] _surfaceProperties = {
                           {"PricingStructureType", "FxVolatilityMatrix"},
                           {"BaseDate", DateTime.Today},
                           {"MarketName", "FXAssetsTests"},
                           {"Instrument", "AUDUSD-FxForward"},
                           {"Source", "FxOptionsDesk"},
                           {"Algorithm", "Linear"},
                           {"Currency", "AUD"},
                           {"QuoteUnits", "LogNormalVolatility"},
                           {"MeasureType", "Volatility"},
                           {"StrikeQuoteUnits", PriceQuoteUnitsScheme.GetEnumString(PriceQuoteUnitsEnum.Rate) },
                           {"QuotationSide", "Mid"},
                           {"Timing", "Close"},
                           {"BusinessCenter", "Sydney"},
                           {"CurveName", "AUDUSD-FxForward-FxOptionsDesk"}
                       };

        [TestInitialize]
        public void Initialize()
        {
            _values
            = new object[,]
                      {
                          {"AUD-CPIndex-1D", 0.0102, 0},
                          {"AUD-CPIndex-1M", 0.0102, 0},
                          {"AUD-CPIndex-2M", 0.0102, 0},
                          {"AUD-ZCCPISwap-1Y", 0.0102, 0},
                          {"AUD-ZCCPISwap-2Y", 0.0126, 0}
                      };

            var properties = new object[,]
                                       {
                                           {"PricingStructureType", "InflationCurve"},
                                           {"ReferenceDate", DateTime.Today},
                                           {"MarketName", "InflationAssetsTests"},
                                           {"Currency", "AUD"},
                                           {"Index", "CPI"},
                                           {"IndexTenor", "3M"},
                                           {"Algorithm", "FlatForward"},
                                           {"Tolerance","1e-7"}
                                       };         

#pragma warning disable 612,618
            //var curve = Engine.CreateCurve(properties.ToNamedValueSet(), _instruments, _rates, _additional, null, null);
            var curve = Engine.CreatePricingStructure(properties.ToNamedValueSet(), _values);
            _curveId = curve.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(curve);
            var curve2 = Engine.CreateCurve(_properties2.ToNamedValueSet(), _fxinstruments, _rates, _additional, null, null);
            _fxcurveId = curve2.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(curve2);          
#pragma warning restore 612,618
        }

        #region Sheet Curves

        [TestMethod]
        public void PersistToCacheTest()
        {
            //Failed	PersistToCacheTest	ExcelAPI.Tests	Assert.AreEqual failed. Expected:<InflationCurve.AUD-CPI-3M>. Actual:<Orion.Market.InflationAssetsTests.InflationCurve.AUD-CPI-3M>. 	
            Assert.AreEqual("Market.InflationAssetsTests.InflationCurve.AUD-CPI-3M", _curveId);
        }

        #endregion
        
        #region Priceable Inflation Asssets

        [TestMethod]
        public void Evaluate()
        {
            var metrics = new List<string> { "ImpliedQuote", "MarketQuote" };
            var keys = new List<string> { "AUD-CPIndex-1D", "AUD-CPIndex-2M", "AUD-ZCCPISwap-2Y" };
            var valuations = Engine.EvaluateMetricsForAssetSet(metrics, _curveId, keys, DateTime.Today);
            object[,] result = MetricsHelper.BuildEvaluationResults(keys, metrics, valuations.assetQuote);
            Assert.AreEqual(keys[0], result[0, 0]);
            Assert.AreEqual(metrics[0], result[0, 1]);
            Assert.AreEqual(typeof(decimal), result[0, 2].GetType());
            Assert.AreNotEqual(0, result[0, 2]);
            Assert.AreEqual(metrics[1], result[0, 3]);
            Assert.AreEqual(typeof(decimal), result[0, 4].GetType());
            Assert.AreNotEqual(0, result[0, 4]);
        }

        #endregion

        #region Daily Forward Rates

        [TestMethod]
        public void GetAbsoluteValue()
        {
            double result = Engine.GetValue( _curveId, DateTime.Today);
            Assert.AreNotEqual(0, result);
        }

        #endregion
    }
}