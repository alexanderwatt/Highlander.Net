using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using National.QRSC.Credit.Analytics.Credit;
using National.QRSC.Engine.Factory;
using National.QRSC.Engine.Helpers;
using National.QRSC.Engine.Tests.Helpers;
using National.QRSC.ModelFramework;
using National.QRSC.ObjectCache;
using National.QRSC.Products.Helpers;
using National.QRSC.Products.InterestRates;

namespace National.QRSC.QRLib.Tests.Rates
{
    /// <summary>
    /// Summary description for InterestRateSwapTest
    /// </summary>
    [TestClass]
    public class InterestRateCapFloorTest
    {
        private const double CapBreakEvenRate = 0.02407748;
        private const double _pv = -16.6986;

        private object[,] _instruments
            = {
                  {"AUD-Deposit-1D", 0.0285, 0},
                  {"AUD-Deposit-1M", 0.0285, 0},
                  {"AUD-Deposit-2M", 0.0285, 0},
                  {"AUD-Deposit-3M", 0.0285, 0},
                  {"AUD-IRFuture-IR-U9", 0.0200, 0},
                  {"AUD-IRFuture-IR-Z9", 0.0200, 0},
                  {"AUD-IRFuture-IR-H0", 0.0200, 0},
                  {"AUD-IRFuture-IR-M0", 0.0200, 0},
                  {"AUD-IRFuture-IR-U0", 0.0200, 0},
                  {"AUD-IRFuture-IR-Z0", 0.0200, 0},
                  {"AUD-IRFuture-IR-H1", 0.0200, 0},
                  {"AUD-IRSwap-3Y", 0.0400, 0},
                  {"AUD-IRSwap-4Y", 0.0400, 0},
                  {"AUD-IRSwap-5Y", 0.0400, 0},
                  {"AUD-IRSwap-7Y", 0.0400, 0},
                  {"AUD-IRSwap-10Y", 0.0400, 0},
                  {"AUD-IRSwap-12Y", 0.0400, 0},
                  {"AUD-IRSwap-15Y", 0.0400, 0},
                  {"AUD-IRSwap-20Y", 0.0400, 0},
                  {"AUD-IRSwap-25Y", 0.0400, 0},
                  {"AUD-IRSwap-30Y", 0.0400, 0}
              };


        const string _marketEnvironmentId = "CapFloorTest";
        const string _volatilityStructureId = "VolatilityCube";
        DateTime _baseDate = new DateTime(2009, 09, 04);

        private string BuildMarket(DateTime _baseDate)
        {
            const string rateCurveId = "RateCurve.AUD-LIBOR-BBA-6M";

            object[,] structurePropertiesRange
                = {
                      {"PricingStructureType", "RateCurve"},
                      {"IndexTenor", "6M"},
                      {"Currency", "AUD"},
                      {"Index", "LIBOR-BBA"},
                      {"Algorithm", "FastLinearZero"},
                      {"MarketName", _marketEnvironmentId},
                      {"BuildDateTime", _baseDate},
                      {"IndexName", "AUD-LIBOR-BBA"},
                      {"CurveName", "AUD-LIBOR-BBA-6M"},
                      {"Identifier", rateCurveId},
                      {"Tolerance", 0.000001},
                      {"CompoundingFrequency", "Continuous"}
                  };

            QRLib.PricingStructures.CreatePricingStructure(structurePropertiesRange, _instruments);

            double[] strikes = { 0.02, 0.03 };
            object[,] vols
                = {
                      {"3m", "1y", 0.10, 0.20},
                      {"6m", "1y", 0.11, 0.21},
                      {"3m", "2y", 0.12, 0.22},
                      {"6m", "2y", 0.13, 0.23}
                  };

            object[,] volPropertiesRange
                = {
                      {"PricingStructureType", "RateVolatilityCube"},
                      {"Identifier", _volatilityStructureId},
                      {"CurveName", "AUD-Sydney-03/09/2009"},
                      {"BuildDateTime", _baseDate},
                      {"MarketName", _marketEnvironmentId},
                      {"Instrument", "AUD"},
                      {"Source", "SydneySwapDesk"},
                      {"Algorithm", "Linear"},
                  };

            QRLib.PricingStructures.CreateVolatilityCube(volPropertiesRange, strikes, vols);
            return rateCurveId;
        }

        [TestMethod]
        public void CreateAmortisedCapTest()
        {
            #region Initialise

            QRLib.PricingStructures.ClearCache();
            string rateCurveId = BuildMarket(_baseDate);
            IMarketEnvironment marketEnvironment = ObjectCacheHelper.GetMarket(_marketEnvironmentId);

            #endregion

            #region Define the Swap

            const string id = "TestAmortisedCap";

            object[,] capFloorTerms
                = {
                      {"DealReferenceIdentifier", id},
                      {"BasePartyBuySellsInd", "Sells"},
                      {"CounterpartyName", "ANZ"},
                      {"DealType", "Cap"},
                      {"EffectiveDate", _baseDate},
                      {"TerminationDate", _baseDate.AddYears(2)},
                      {"AdjustCalculationDates", true},
                  };

            object[,] streamTerms
                = {
                      {"NotionalAmount", "1000000"},
                      {"Currency", "AUD"},
                      {"ScheduleGeneration", "Forward"},
                      {"BusinessCenters", "Sydney"},
                      {"CouponPeriod", "6m"},
                      {"RollDay", "1"},
                      {"CouponDateAdjustment", "FOLLOWING"},
                      {"DayCountConvention", "ACT/365.FIXED"},
                      {"DiscountingType", "Standard"},
                      {"DiscountCurveReference", rateCurveId},
                      {"FixedOrObservedRate", "0.07"},
                      {"ObservedRateSpecified", "FALSE"},
                      {"ForwardCurveReference", rateCurveId},
                      {"FixingDateBusinessCenters", "Sydney"},
                      {"FixingDateResetInterval", "0D"},
                      {"FixingDateAdjustmentConvention", "NONE"},
                      {"RateIndexName", "AUD-LIBOR-BBA"},
                      {"RateIndexTenor", "6m"},
                      {"Spread", "0"},
                      {"VolatilityCurveReference", _volatilityStructureId},
                      {"StrikePrice", "0.07"},
                  };

            #endregion

            #region Define the Valuation Criteria

            string[] requiredMetrics = { "NPV", "BreakEvenRate" };

            #endregion

            #region Create & Value the Swap

            string returnedId = QRLib.Rates.InterestRateCapFloor.CreateAmortised(capFloorTerms, streamTerms, null);
            Assert.AreEqual(id, returnedId);
            InterestRateCapFloor capFloor = ProductHelper.Get<InterestRateCapFloor>(id);
            Assert.IsNotNull(capFloor);

            CreditSettings creditSettings = new CreditSettings(ServerStore.Client);
            IDictionary<string, object> valuation = capFloor.BasicValuation(_baseDate, marketEnvironment, requiredMetrics, creditSettings);

            #endregion

            #region Validate the Results

            Assert.AreEqual(CapBreakEvenRate, Convert.ToDouble(valuation["BreakEvenRate"]), 0.0000001);
            Assert.AreEqual(_pv, Convert.ToDouble(valuation["NPV"]), 0.001);

            #endregion

            #region Reset

            QRLib.PricingStructures.ClearCache();

            #endregion
        }

        [TestMethod]
        public void CreateStructuredCapTest()
        {
            #region Initialise

            QRLib.PricingStructures.ClearCache();
            string rateCurveId = BuildMarket(_baseDate);
            IMarketEnvironment marketEnvironment = ObjectCacheHelper.GetMarket(_marketEnvironmentId);

            #endregion

            #region Define the Swap

            const string id = "TestStructuredCap";

            object[,] capFloorTerms
                = {
                      {"DealReferenceIdentifier", id},
                      {"BasePartyBuySellsInd", "Sells"},
                      {"CounterpartyName", "ANZ"},
                      {"DealType", "Cap"},
                  };

            object[,] streamTerms
                = {
                      {"NotionalAmount", "1000000"},
                      {"Currency", "AUD"},
                      {"DayCountConvention", "ACT/365.FIXED"},
                      {"DiscountingType", "Standard"},
                      {"DiscountCurveReference", rateCurveId},
                      {"ForwardCurveReference", rateCurveId},
                      {"VolatilityCurveReference", _volatilityStructureId},
                      {"RateIndexName", "AUD-LIBOR-BBA"},
                      {"RateIndexTenor", "6m"},
                      {"StrikePrice", "0.07"},
                  };

            const double notionalAmount = -1000000;
            object[,] couponMatrix
                = {
                      {"StartDate", "EndDate", "NotionalAmount", "StrikePrice"},
                      { new DateTime(2009, 09, 04), new DateTime(2010, 03, 01), notionalAmount, "0.07" },
                      { new DateTime(2010, 03, 01), new DateTime(2010, 09, 01), notionalAmount, "0.07" },
                      { new DateTime(2010, 09, 01), new DateTime(2011, 03, 01), notionalAmount, "0.07" },
                      { new DateTime(2011, 03, 01), new DateTime(2011, 09, 05), notionalAmount, "0.07" },
                  };

            #endregion

            #region Define the Valuation Criteria

            string[] requiredMetrics = { "NPV", "BreakEvenRate" };

            #endregion

            #region Create & Value the Swap

            string returnedId = QRLib.Rates.InterestRateCapFloor.CreateStructured(capFloorTerms, streamTerms, couponMatrix);
            Assert.AreEqual(id, returnedId);
            InterestRateCapFloor capFloor = ProductHelper.Get<InterestRateCapFloor>(id);
            Assert.IsNotNull(capFloor);

            CreditSettings creditSettings = new CreditSettings(ServerStore.Client);
            IDictionary<string, object> valuation = capFloor.BasicValuation(_baseDate, marketEnvironment, requiredMetrics, creditSettings);

            #endregion

            #region Validate the Results
            Assert.AreEqual(CapBreakEvenRate, Convert.ToDouble(valuation["BreakEvenRate"]), 0.0000001);
            Assert.AreEqual(_pv, Convert.ToDouble(valuation["NPV"]), 0.001);

            #endregion

            #region Reset

            QRLib.PricingStructures.ClearCache();

            #endregion
        }

        [TestMethod]
        public void CreateAmortisedFloorTest()
        {
            #region Initialise

            QRLib.PricingStructures.ClearCache();
            string rateCurveId = BuildMarket(_baseDate);
            IMarketEnvironment marketEnvironment = ObjectCacheHelper.GetMarket(_marketEnvironmentId);

            #endregion

            #region Define the Swap

            const string id = "TestAmortisedFloor";

            object[,] capFloorTerms
                = {
                      {"DealReferenceIdentifier", id},
                      {"BasePartyBuySellsInd", "Sells"},
                      {"CounterpartyName", "ANZ"},
                      {"DealType", "Floor"},
                      {"EffectiveDate", _baseDate},
                      {"TerminationDate", _baseDate.AddYears(2)},
                      {"AdjustCalculationDates", true},
                  };

            object[,] streamTerms
                = {
                      {"NotionalAmount", "1000000"},
                      {"Currency", "AUD"},
                      {"ScheduleGeneration", "Forward"},
                      {"BusinessCenters", "Sydney"},
                      {"CouponPeriod", "6m"},
                      {"RollDay", "1"},
                      {"CouponDateAdjustment", "FOLLOWING"},
                      {"DayCountConvention", "ACT/365.FIXED"},
                      {"DiscountingType", "Standard"},
                      {"DiscountCurveReference", rateCurveId},
                      {"FixedOrObservedRate", "0.07"},
                      {"ObservedRateSpecified", "FALSE"},
                      {"ForwardCurveReference", rateCurveId},
                      {"FixingDateBusinessCenters", "Sydney"},
                      {"FixingDateResetInterval", "0D"},
                      {"FixingDateAdjustmentConvention", "NONE"},
                      {"RateIndexName", "AUD-LIBOR-BBA"},
                      {"RateIndexTenor", "6m"},
                      {"Spread", "0"},
                      {"VolatilityCurveReference", _volatilityStructureId},
                      {"StrikePrice", "0.07"},
                  };

            #endregion

            #region Define the Valuation Criteria

            string[] requiredMetrics = { "NPV", "BreakEvenRate" };

            #endregion

            #region Create & Value the Swap

            string returnedId = QRLib.Rates.InterestRateCapFloor.CreateAmortised(capFloorTerms, streamTerms, null);
            Assert.AreEqual(id, returnedId);
            InterestRateCapFloor capFloor = ProductHelper.Get<InterestRateCapFloor>(id);
            Assert.IsNotNull(capFloor);

            CreditSettings creditSettings = new CreditSettings(ServerStore.Client);
            IDictionary<string, object> valuation = capFloor.BasicValuation(_baseDate, marketEnvironment, requiredMetrics, creditSettings);

            #endregion

            #region Validate the Results

            Assert.AreEqual(CapBreakEvenRate, Convert.ToDouble(valuation["BreakEvenRate"]), 0.0000001);
            Assert.AreEqual(_pv, Convert.ToDouble(valuation["NPV"]), 3);

            #endregion

            #region Reset

            QRLib.PricingStructures.ClearCache();

            #endregion
        }

        [TestMethod]
        public void CreateStructuredFloorTest()
        {
            #region Initialise

            QRLib.PricingStructures.ClearCache();
            string rateCurveId = BuildMarket(_baseDate);
            IMarketEnvironment marketEnvironment = ObjectCacheHelper.GetMarket(_marketEnvironmentId);

            #endregion

            #region Define the Swap

            const string id = "TestStructuredFloor";

            object[,] capFloorTerms
                = {
                      {"DealReferenceIdentifier", id},
                      {"BasePartyBuySellsInd", "Sells"},
                      {"CounterpartyName", "ANZ"},
                      {"DealType", "Floor"},
                  };

            object[,] streamTerms
                = {
                      {"NotionalAmount", "1000000"},
                      {"Currency", "AUD"},
                      {"DayCountConvention", "ACT/365.FIXED"},
                      {"DiscountingType", "Standard"},
                      {"DiscountCurveReference", rateCurveId},
                      {"ForwardCurveReference", rateCurveId},
                      {"VolatilityCurveReference", _volatilityStructureId},
                      {"RateIndexName", "AUD-LIBOR-BBA"},
                      {"RateIndexTenor", "6m"},
                      {"StrikePrice", "0.07"},
                  };

            const double notionalAmount = -1000000;
            object[,] couponMatrix
                = {
                      {"StartDate", "EndDate", "NotionalAmount", "StrikePrice"},
                      { new DateTime(2009, 09, 04), new DateTime(2010, 03, 01), notionalAmount, "0.07" },
                      { new DateTime(2010, 03, 01), new DateTime(2010, 09, 01), notionalAmount, "0.07" },
                      { new DateTime(2010, 09, 01), new DateTime(2011, 03, 01), notionalAmount, "0.07" },
                      { new DateTime(2011, 03, 01), new DateTime(2011, 09, 05), notionalAmount, "0.07" },
                  };

            #endregion

            #region Define the Valuation Criteria

            string[] requiredMetrics = { "NPV", "BreakEvenRate" };

            #endregion

            #region Create & Value the Swap

            string returnedId = QRLib.Rates.InterestRateCapFloor.CreateStructured(capFloorTerms, streamTerms, couponMatrix);
            Assert.AreEqual(id, returnedId);
            InterestRateCapFloor capFloor = ProductHelper.Get<InterestRateCapFloor>(id);
            Assert.IsNotNull(capFloor);

            CreditSettings creditSettings = new CreditSettings(ServerStore.Client);
            IDictionary<string, object> valuation = capFloor.BasicValuation(_baseDate, marketEnvironment, requiredMetrics, creditSettings);

            #endregion

            #region Validate the Results

            Assert.AreEqual(CapBreakEvenRate, Convert.ToDouble(valuation["BreakEvenRate"]), 0.0000001);
            Assert.AreEqual(_pv, Convert.ToDouble(valuation["NPV"]), 3);

            #endregion

            #region Reset

            QRLib.PricingStructures.ClearCache();

            #endregion
        }
    }
}
