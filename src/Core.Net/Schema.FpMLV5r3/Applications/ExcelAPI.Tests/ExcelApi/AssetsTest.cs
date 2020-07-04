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
using System.Linq;
using Highlander.Constants;
using Highlander.Metadata.Common;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.UnitTestEnv.V5r3;
using Highlander.Utilities.NamedValues;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Excel.Tests.V5r3.ExcelApi
{
    /// <summary>
    ///This is a test class for AssetsTest and is intended
    ///to contain all AssetsTest Unit Tests
    ///</summary>
    public partial class ExcelAPITests
    {
        #region Properties

        //private static ILogger Logger_obs { get; set; }
        private static UnitTestEnvironment UTE { get; set; }
        public static CurveEngine.V5r3.CurveEngine Engine { get; set; }
        private static IBusinessCalendar FixingCalendar { get; set; }
        private static IBusinessCalendar PaymentCalendar { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            // load regression test data and expected values
            UTE = new UnitTestEnvironment();
            //Set the calendar engine
            Engine = new CurveEngine.V5r3.CurveEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
            UTE.Dispose();
        }

        #endregion

        #region Tests

        [TestMethod]
        public void GetAlgorithmDiscountCurveCalypsoAlgo4360()
        {
            var result = Engine.GetAlgorithm("DiscountCurve", "CalypsoAlgo4_360");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmDiscountCurveCalypsoAlgo4()
        {
            var result = Engine.GetAlgorithm("DiscountCurve", "CalypsoAlgo4");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmDiscountCurveDefault()
        {
            var result = Engine.GetAlgorithm("DiscountCurve", "Default");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmDiscountCurveFastLinearZero()
        {
            var result = Engine.GetAlgorithm("DiscountCurve", "FastLinearZero");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-08);
        }

        [TestMethod]
        public void GetAlgorithmDiscountCurveLinearZero()
        {
            var result = Engine.GetAlgorithm("DiscountCurve", "LinearZero");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmDiscountCurveFlatForward()
        {
            var result = Engine.GetAlgorithm("DiscountCurve", "FlatForward");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmRateCurveCalypsoAlgo4360()
        {
            var result = Engine.GetAlgorithm("RateCurve", "CalypsoAlgo4_360");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmRateCurveCalypsoAlgo4()
        {
            var result = Engine.GetAlgorithm("RateCurve", "CalypsoAlgo4");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmRateCurveDefault()
        {
            var result = Engine.GetAlgorithm("RateCurve", "Default");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmRateCurveFastLinearZero()
        {
            var result = Engine.GetAlgorithm("RateCurve", "FastLinearZero");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-08);
        }

        [TestMethod]
        public void GetAlgorithmRateCurveLinearZero()
        {
            var result = Engine.GetAlgorithm("RateCurve", "LinearZero");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmRateCurveFlatForward()
        {
            var result = Engine.GetAlgorithm("RateCurve", "FlatForward");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetAlgorithmRateCurveSimpleGapStep()
        {
            var result = Engine.GetAlgorithm("RateCurve", "SimpleGapStep");
            var property = result?.Properties.Where(a => a.name.Equals("Tolerance", StringComparison.OrdinalIgnoreCase));
            var properties = property as Property[] ?? property?.ToArray();
            Assert.IsNotNull(properties?.First());
            Assert.AreEqual(Convert.ToDouble(properties.First().Value), 1E-10);
        }

        [TestMethod]
        public void GetExistingAsset()
        {
            object[,] result = RangeExtension.ConvertAssetToRange(Engine.GetAssetConfigurationData("AUD", "FrA", "", ""));
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.periodMultiplier", result[2, 0]);
            Assert.AreEqual("0", result[2, 1]);
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.period", result[3, 0]);
            Assert.AreEqual("D", result[3, 1]);
        }

        [TestMethod]
        public void GetExistingAssetWithTenor()
        {
            // Check one
            object[,] result = RangeExtension.ConvertAssetToRange(Engine.GetAssetConfigurationData("EUR", "XccyDepo", "1D", ""));
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.periodMultiplier", result[3, 0]);
            Assert.AreEqual("0", result[3, 1]);
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.period", result[4, 0]);
            Assert.AreEqual("D", result[4, 1]);

            // Check another
            result = RangeExtension.ConvertAssetToRange(Engine.GetAssetConfigurationData("EUR", "XccyDepo", "2D", ""));
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.periodMultiplier", result[3, 0]);
            Assert.AreEqual("0", result[3, 1]);
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.period", result[4, 0]);
            Assert.AreEqual("D", result[4, 1]);

            // Check without
            result = RangeExtension.ConvertAssetToRange(Engine.GetAssetConfigurationData("EUR", "XccyDepo", "", ""));
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.periodMultiplier", result[2, 0]);
            Assert.AreEqual("2", result[2, 1]);
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.period", result[3, 0]);
            Assert.AreEqual("D", result[3, 1]);

            // Check with invalid tenor
            result = RangeExtension.ConvertAssetToRange(Engine.GetAssetConfigurationData("EUR", "XccyDepo", "TEST", ""));
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.periodMultiplier", result[2, 0]);
            Assert.AreEqual("2", result[2, 1]);
            Assert.AreEqual("Instrument.InstrumentNodeItem.SpotDate.period", result[3, 0]);
            Assert.AreEqual("D", result[3, 1]);
        }

        [TestMethod]
        public void GetAssetNotExisting()
        {
            try
            {
                object[,] result = RangeExtension.ConvertAssetToRange(Engine.GetAssetConfigurationData("XYZ", "FRa", "", ""));
                Assert.Fail();
            }
            catch (Exception)
            {
                // Pass
            }
        }

        /// <summary>
        ///A test for GetStructuredSwapMetric
        ///</summary>
        [TestMethod]
        public void GetStructuredSwapMetricTest()
        {
            const string currency = "AUD";
            DateTime baseDate = new DateTime(2009, 1, 6);
            string curveId = CreateCurve(baseDate);
            const Decimal notional = 1000000;
            List<DateTime> dates = new List<DateTime>
                                    {
                                        new DateTime(2009, 03, 05),
                                        new DateTime(2009, 06, 05),
                                        new DateTime(2009, 09, 05),
                                        new DateTime(2009, 12, 05),
                                    };
            List<Decimal> notionalWeights = new List<Decimal> { 1.0m, 0.8m, 0.7m, 0.6m};
            string fixedLegDayCount = "ACT/365.FIXED";
            Decimal fixedRate = 0.06m;
            string businessDayConvention = "FOLLOWING";
            string businessCentersAsString = "GBLO";
            string metric = "DeltaR";
            Decimal expected = 50.969m;
            Decimal actual = Engine.GetStructuredSwapMetric(curveId, currency, baseDate, notional, dates, notionalWeights, fixedLegDayCount, fixedRate, businessDayConvention, businessCentersAsString, metric);
            Assert.AreEqual(expected, Math.Round(actual, 3));
        }

        /// <summary>
        ///A test for GetStructuredSwapMetric
        ///</summary>
        [TestMethod]
        public void GetSwapImpliedQuoteTest()
        {
            const string currency = "AUD";
            DateTime baseDate = new DateTime(2009, 1, 6);
            DateTime spotDate = new DateTime(2009, 1, 8);
            string curveId = CreateCurve(baseDate);
            string fixedLegDayCount = "ACT/365.FIXED";
            string term = "3y";
            string paymentFrequency = "1M";
            string rollConvention = "EOM";
            Decimal expected = 0.0454m;
            Decimal actual = Engine.GetSwapImpliedQuote(curveId, currency, baseDate, spotDate, fixedLegDayCount, term, paymentFrequency, rollConvention);
            Assert.AreEqual(expected, Math.Round(actual, 4));
        }

        private string CreateCurve(DateTime baseDate)
        {
            NamedValueSet propertySet = new NamedValueSet();
            const string currency = "AUD";
            const string index = "LIBOR-BBA";
            string curveType = "RateCurve";
            const string indexName = "AUDSwap";
            string indexTenor = "6M";
            string curveName = indexName + "-" + indexTenor;
            string identifier = curveType + "." + curveName;
            string marketName = "UnitTest";

            propertySet.Set(CurveProp.PricingStructureType, curveType);
            propertySet.Set(CurveProp.Market, marketName);
            propertySet.Set(CurveProp.IndexTenor, indexTenor);
            propertySet.Set("Currency", currency);
            propertySet.Set("Index", index);
            propertySet.Set("Algorithm", "Default");
            propertySet.Set("BuildDateTime", baseDate);
            propertySet.Set(CurveProp.IndexName, indexName);
            propertySet.Set(CurveProp.CurveName,  curveName);
            propertySet.Set("Identifier",  identifier);

            object[,] values = 
                { 
                    {"AUD-Deposit-1D",0.03,	0},
                    {"AUD-Deposit-1M", 0.0311,	0},
                    {"AUD-Deposit-2M", 0.0311,	0},
                    {"AUD-Deposit-88D", 0.0311,	0},

                    {"AUD-IRFuture-IR-U9", (100.0 - 96.895) / 100.0,	0}, // IR01
                    {"AUD-IRFuture-IR-Z9", (100.0 - 96.825)/100.0,	0}, // IR02
                    {"AUD-IRFuture-IR-H0", (100.0 - 96.545)/100.0,	0}, // IR03
                    {"AUD-IRFuture-IR-M0", (100.0 - 96.115)/100.0,	0}, //IR04
                    {"AUD-IRFuture-IR-U0", (100.0 - 95.665)/100.0,	0}, // IR05
                    {"AUD-IRFuture-IR-Z0", (100.0 - 95.285)/100.0,	0}, // IR06
                    {"AUD-IRFuture-IR-H1", (100.0 - 94.955)/100.0,	0}, // IR07
                    {"AUD-IRFuture-IR-M1", (100.0 - 94.645)/100.0,	0}, //IR08

                    {"AUD-IRSwap-3Y", 0.0455749999999999,	0},
                    {"AUD-IRSwap-4Y", 0.0500999999999999,	0},
                    {"AUD-IRSwap-5Y", 0.0526999999999999,	0},
                    {"AUD-IRSwap-7Y", 0.0558,	0},
                    {"AUD-IRSwap-10Y", 0.057975,	0},
                    {"AUD-IRSwap-15Y", 0.059725,	0},
                    {"AUD-IRSwap-20Y", 0.058825,	0},
                    {"AUD-IRSwap-25Y", 0.057675,	0},
                    {"AUD-IRSwap-26Y", 0.05741,	0},
                    {"AUD-IRSwap-27Y", 0.057145,	0},
                    {"AUD-IRSwap-28Y", 0.05688,	0},
                    {"AUD-IRSwap-29Y", 0.056615,	0},
                    {"AUD-IRSwap-30Y", 0.05635,	0}
                };

            int length = values.GetLength(0);
            string[] instrument = new string[length];
            decimal[] value = new decimal[length];
            decimal[] extra = new decimal[length];
            for (int i = 0; i < length; i++)
            {
                instrument[i] = (string)values[i, 0];
                value[i] = Convert.ToDecimal(values[i, 1]);
                extra[i] = Convert.ToDecimal(values[i, 2]);
            }
            var curve = Engine.CreateCurve(propertySet, instrument, value, extra, null, null);
            Engine.SaveCurve(curve);
            return curve.GetPricingStructureId().UniqueIdentifier;
        }

        #endregion
    }
}