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
using System.Collections.Generic;
using Highlander.CurveEngine.V5r3.Extensions;
using Highlander.CurveEngine.V5r3.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Excel.Tests.V5r3.ExcelApi
{
    /// <summary>
    /// Test methods for FXAssets.xls
    /// </summary>
    public partial class ExcelAPITests
    {

        #region Sheet FXCurves

        [TestMethod]
        public void FXAssetsPersistToCacheTest()
        {       //Failed	FXAssetsPersistToCacheTest	ExcelAPI.Tests	Assert.AreEqual failed. Expected:<FxCurve.AUD-USD>. Actual:<Orion.Market.FXAssetsTests.FxCurve.AUD-USD>. 	   
            Assert.AreEqual("Market.FXAssetsTests.FxCurve.AUD-USD", _fxcurveId);
        }

        #endregion

        #region Sheet FXVol

        [TestMethod]
        public void GetExpiryTermStrikeValue()
        {
            var curve3 = Engine.CreateVolatilitySurface(_surfaceProperties.ToNamedValueSet(), _surfaceExpiries, _surfaceStrikes, _surfaceVols);
            Engine.SaveCurve(curve3);
            var _surfaceId = curve3.GetPricingStructureId().UniqueIdentifier;
            double response = Engine.GetExpiryTermStrikeValue(_surfaceId, _surfaceExpiries[0], _surfaceStrikes[0]);
            Assert.AreEqual(_surfaceVols[0,0], response);
            response = Engine.GetExpiryTermStrikeValue(_surfaceId,  _surfaceExpiries[1], _surfaceStrikes[1]);
            Assert.AreEqual(_surfaceVols[1, 1], response);
        }

        [TestMethod]
        public void GetExpiryDateStrikeValue()
        {
            var curve3 = Engine.CreateVolatilitySurface(_surfaceProperties.ToNamedValueSet(), _surfaceExpiries, _surfaceStrikes, _surfaceVols);
            Engine.SaveCurve(curve3);
            var _surfaceId = curve3.GetPricingStructureId().UniqueIdentifier;
            DateTime oneDay = DateTime.Today.AddDays(1);
            double response = Engine.GetExpiryDateStrikeValue(_surfaceId, DateTime.Today, oneDay, _surfaceStrikes[0]);
            Assert.AreEqual(_surfaceVols[0, 0], response);
            DateTime oneWeek = DateTime.Today.AddDays(8);
            response = Engine.GetExpiryDateStrikeValue(_surfaceId, DateTime.Today, oneWeek, _surfaceStrikes[2]);
            Assert.AreEqual(_surfaceVols[1, 2], response);
        }
        
        #endregion

        #region PriceableFX Asssets

        [TestMethod]
        public void EvaluateFxAssets()
        {
            List<string> metrics = new List<string> { "ImpliedQuote" };
            List<string> keys = new List<string> { "AUDUSD-FxForward-1D", "AUDUSD-FxSpot-SP", "AUDUSD-FxForward-1M" };
            var valuations = Engine.EvaluateMetricsForAssetSet(metrics, _fxcurveId, keys, DateTime.Today);
            object[,] result = MetricsHelper.BuildEvaluationResults(keys, metrics, valuations.assetQuote);
            Assert.AreEqual(keys[0], result[0, 0]);
            Assert.AreEqual("ImpliedQuote", result[0, 1]);
            Assert.AreEqual(typeof(decimal), result[0, 2].GetType());
            Assert.AreNotEqual(0, result[0, 2]);
        }

        #endregion

        #region Daily Forward Rates

        [TestMethod]
        public void GetFxAbsoluteValue()
        {
            double result = Engine.GetValue(_fxcurveId, DateTime.Today);
            Assert.AreNotEqual(0, result);
        }

        #endregion
    }
}
