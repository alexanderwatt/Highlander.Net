using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.PublisherWebService;
using Orion.PublisherWebService.Lpm;
using Orion.UnitTestEnv;
using PublisherWebService.Tests.Properties;
using nab.QDS.FpML.V47;
using nab.QDS.Util.NamedValues;
using nab.QDS.Util.Serialisation;

namespace PublisherWebService.Tests
{
    [TestClass]
    public class LpmTest
    {
        private readonly RuntimeMock _runtimeMock = new RuntimeMock();

        #region Conversion tests

        [TestMethod]
        public void BondFuturesPayTest()
        {
            Asserts(Resources.BondFuturesPay_Input, Resources.BondFuturesPay_Input_Properties,
                Resources.BondFuturesPay_Expected, Resources.BondFuturesPay_Expected_Properties,
                "Live.AUDBondFuturesPay.310Y.Official.SydSwapDesk.14/07/2010");
        }

        [TestMethod]
        public void BondFuturesReceiveTest()
        {
            Asserts(Resources.BondFuturesRec_Input, Resources.BondFuturesRec_Input_Properties,
                Resources.BondFuturesRec_Expected, Resources.BondFuturesRec_Expected_Properties,
                "Live.AUDBondFuturesRec.310Y.Official.SydSwapDesk.14/07/2010");
        }

        [TestMethod]
        public void AudSwap6MTest()
        {
            Asserts(Resources.AUDSwap6M_Input, Resources.AUDSwap6M_Input_Properties,
                Resources.AUDSwap6M_Expected, Resources.AUDSwap6M_Expected_Properties,
                "Live.AUDSwap.6M.Official.SydSwapDesk.15/07/2010");
        }

        //[TestMethod]
        //public void CapFloorTest()
        //{
        //    Asserts(Resources.CapFloor_Input, Resources.CapFloor_Input_Properties,
        //        Resources.CapFloor_Expected, Resources.CapFloor_Expected_Properties,
        //        "LPM.Volatility.CapFloor.AUD.SydSPU.2010.Week29");
        //}

        //[TestMethod]
        //public void SwaptionTest()
        //{
        //    Asserts(Resources.Swaption_Input, Resources.Swaption_Input_Properties,
        //        Resources.Swaption_Expected, Resources.Swaption_Expected_Properties,
        //        "LPM.Volatility.Swaption.AUD.SydSPU.2010.Week29");
        //}

        private void Asserts(string inputMarket, string inputProperties, string expectedMarket,
            string expectedProperties, string expectedName)
        {
            // Runtime 3.x values
            var timeSpan = new TimeSpan(7, 0, 0, 0);
            var market = XmlSerializerHelper.DeserializeFromString<Market>(inputMarket);
            var namedValueSet = new NamedValueSet(inputProperties);
            // Do the publishing, including the translations
            var lpm = new LpmPublisher(General.PricingStructures.Logger, _runtimeMock);
            //lpm.
            lpm.Publish(market, namedValueSet, timeSpan);
            // Expected Runtime 2.5 values
            var expectedNamedValueSet = new NamedValueSet(expectedProperties);
            const int expectedOffsetDays = 7;
            // Check
            string expectedData = _runtimeMock.PublishedData.Replace("utf-8", "utf-16"); // the encoding is done in the test
            Assert.AreEqual(expectedMarket, expectedData);
            Assert.AreEqual(expectedName, _runtimeMock.PublishedName);
            //Assert.AreEqual(expectedOffsetDays, _runtimeMock.PublishedTimeSpan.Subtract(timeSpan).TotalDays, 0.1);
            foreach (var pair in expectedNamedValueSet.ToDictionary())
            {
                Assert.AreEqual(pair.Value.GetType(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.GetType(), pair.Key);
                Assert.AreEqual(pair.Value.ToString(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.ToString(), pair.Key);
                Debug.Print("{0}:{1}", pair.Value, _runtimeMock.PublishedProperties.Get(pair.Key, true).Value);
            }
        }
        #endregion

        #region Validate from inputs through to runtime 2.5

        [TestMethod]
        public void ValidateAudSwap6M()
        {
            var ps = new PricingStructures(General.PricingStructures.Logger, _runtimeMock);

            var properties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateCurve"},
                          new object[] {"IndexTenor", "6M"},
                          new object[] {"Currency", "AUD"},
                          new object[] {"Index", "Swap"},
                          new object[] {"Algorithm", "Base algorithm"},
                          new object[] {"MarketName", "LiveTest"},
                          new object[] {"IndexName", "AUDSwap"},
                          new object[] {"Validity", "Official"},
                          new object[] {"Tolerance", 1e-8},
                          //Extra property to keep the date constant
                          new object[] {"BaseDate", new DateTime(2010, 7, 20)},
                          new object[] {"BuildDateTime", new DateTime(2010, 7, 20, 14, 20, 30)}
                      };

            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 60},
                          new object[] {"MarketName", "LiveTest"},
                          new object[] {"LPMCopy", true}
                      };

            var values
                = new[]
                      {
                          // ="new object[] {"""&A1&""", "&B1&"d, 0},"
                          new object[] {"AUD-Deposit-1D", 0.045d, 0},
                          new object[] {"AUD-Deposit-1M", 0.0474d, 0},
                          new object[] {"AUD-Deposit-2M", 0.0482d, 0},
                          new object[] {"AUD-Deposit-88D", 0.0483d, 0},
                          new object[] {"AUD-IRFuture-IR-U0", 0.04815d, 0},
                          new object[] {"AUD-IRFuture-IR-Z0", 0.04765d, 0},
                          new object[] {"AUD-IRFuture-IR-H1", 0.04765d, 0},
                          new object[] {"AUD-IRFuture-IR-M1", 0.0481d, 0},
                          new object[] {"AUD-IRFuture-IR-U1", 0.0487d, 0},
                          new object[] {"AUD-IRFuture-IR-Z1", 0.04945d, 0},
                          new object[] {"AUD-IRFuture-IR-H2", 0.0498d, 0},
                          new object[] {"AUD-IRFuture-IR-M2", 0.0499d, 0},
                          new object[] {"AUD-IRSwap-3Y", 0.049075d, 0},
                          new object[] {"AUD-IRSwap-4Y", 0.051225d, 0},
                          new object[] {"AUD-IRSwap-5Y", 0.05235d, 0},
                          new object[] {"AUD-IRSwap-7Y", 0.054475d, 0},
                          new object[] {"AUD-IRSwap-10Y", 0.05595d, 0},
                          new object[] {"AUD-IRSwap-15Y", 0.05715d, 0},
                          new object[] {"AUD-IRSwap-20Y", 0.05665d, 0},
                          new object[] {"AUD-IRSwap-25Y", 0.0554d, 0},
                          new object[] {"AUD-IRSwap-26Y", 0.0551d, 0},
                          new object[] {"AUD-IRSwap-27Y", 0.0548d, 0},
                          new object[] {"AUD-IRSwap-28Y", 0.0545d, 0},
                          new object[] {"AUD-IRSwap-29Y", 0.0542d, 0},
                          new object[] {"AUD-IRSwap-30Y", 0.0539d, 0},
                          new object[] {null, null, null}
                      };

            string result = ps.PublishCurve(properties, publishProperties, values);

            Assert.IsNotNull(result);
            Assert.AreEqual("LiveTest.AUDSwap.6M.Official.SydSwapDesk.20/07/2010", _runtimeMock.PublishedName);
            var expectedNamedValueSet = new NamedValueSet(Resources.ValidateAudSwap6m_Properties);
            foreach (var pair in expectedNamedValueSet.ToDictionary())
            {
                Assert.AreEqual(pair.Value.GetType(),
                                _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.GetType(), pair.Key);
                Assert.AreEqual(pair.Value.ToString(),
                                _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.ToString(), pair.Key);
                Debug.Print("{0}:{1}", pair.Value, _runtimeMock.PublishedProperties.Get(pair.Key, true).Value);
            }

            // the encoding is done in the test, don't worry about it
            string actualData = _runtimeMock.PublishedData.Replace("utf-8", "utf-16");

            Assert.AreEqual(Resources.ValidateAudSwap6m_Data, actualData);
        }

        [TestMethod]
        public void ValidateAudBondFuturesPay()
        {
            var ps = new PricingStructures(General.PricingStructures.Logger, _runtimeMock);

            #region Inputs
            var properties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateCurve"},
                          new object[] {"IndexTenor", "310Y"},
                          new object[] {"Currency", "AUD"},
                          new object[] {"Index", "BondFuturesPay"},
                          new object[] {"Algorithm", "Base algorithm"},
                          new object[] {"MarketName", "LiveUnitTest"},
                          new object[] {"IndexName", "AUDBondFuturesPay"},
                          new object[] {"Validity", "Official"},
                          new object[] {"Tolerance", 0.000001},
                          //Extra property to keep the date constant
                          new object[] {"BaseDate", new DateTime(2010,7,16)},
                          new object[] {"BuildDateTime", new DateTime(2010,7,16,09,20,36)},
                          new object[] {null, null}
                      };

            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 10},
                          new object[] {"MarketName", "LiveUnitTest"},
                          new object[] {"LPMCopy", "TRUE"},
                          new object[] {null, null}
                      };

            var values
                = new[]
                      {
                          new object[] {"AUD-IRFuture-IR-U0", 0.0447, 0},
                          new object[] {"AUD-IRFuture-IR-Z0", 0.0505, 0},
                          new object[] {null, null, null}
                      };
            const string id = "LiveUnitTest.AUDBondFuturesPay.310Y.Official.SydSwapDesk.16/07/2010";
            #endregion

            // Publish
            string result = ps.PublishCurve(properties, publishProperties, values);

            #region Asserts

            Assert.IsNotNull(result);
            Assert.AreEqual(id, _runtimeMock.PublishedName);
            var expectedNamedValueSet = new NamedValueSet(Resources.ValidateAudBondFuturesPay_Properties);
            foreach (var pair in expectedNamedValueSet.ToDictionary())
            {
                Assert.AreEqual(pair.Value.GetType(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.GetType(), pair.Key);
                Assert.AreEqual(pair.Value.ToString(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.ToString(), pair.Key);
                Debug.Print("{0}:{1}", pair.Value, _runtimeMock.PublishedProperties.Get(pair.Key, true).Value);
            }
            string expectedData = _runtimeMock.PublishedData.Replace("utf-8", "utf-16"); // the encoding is done in the test
            Assert.AreEqual(Resources.ValidateAudBondFuturesPay_Data, expectedData);
            #endregion
        }

        [TestMethod]
        public void ValidateAudBondFuturesReceive()
        {
            var ps = new PricingStructures(General.PricingStructures.Logger, _runtimeMock);

            #region Inputs
            var properties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateCurve"},
                          new object[] {"IndexTenor", "310Y"},
                          new object[] {"Currency", "AUD"},
                          new object[] {"Index", "BondFuturesRec"},
                          new object[] {"Algorithm", "Base algorithm"},
                          new object[] {"MarketName", "LiveUnitTest"},
                          new object[] {"IndexName", "AUD-BondFuturesRec"},
                          new object[] {"Validity", "Official"},
                          new object[] {"Tolerance", 0.000001},
                          //Extra property to keep the date constant
                          new object[] {"BaseDate", new DateTime(2010,7,16)},
                          new object[] {"BuildDateTime", new DateTime(2010,7,16,09,20,36)},
                          new object[] {null, null}
                      };

            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 10},
                          new object[] {"MarketName", "LiveUnitTest"},
                          new object[] {"LPMCopy", "TRUE"},
                          new object[] {null, null}
                      };

            var values
                = new[]
                      {
                          new object[] {"AUD-IRFuture-IR-U0", 0.0445999999999999, 0},
                          new object[] {"AUD-IRFuture-IR-Z0", 0.05045, 0},
                          new object[] {null, null, null}
                      };
            const string id = "LiveUnitTest.AUDBondFuturesRec.310Y.Official.SydSwapDesk.16/07/2010";
            #endregion

            // Publish
            string result = ps.PublishCurve(properties, publishProperties, values);


            #region Asserts

            Assert.IsNotNull(result);
            Assert.AreEqual(id, _runtimeMock.PublishedName);
            var expectedNamedValueSet = new NamedValueSet(Resources.ValidateAudBondFuturesReceive_Properties);
            foreach (var pair in expectedNamedValueSet.ToDictionary())
            {
                Assert.AreEqual(pair.Value.GetType(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.GetType(), pair.Key);
                Assert.AreEqual(pair.Value.ToString(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.ToString(), pair.Key);
                Debug.Print("{0}:{1}", pair.Value, _runtimeMock.PublishedProperties.Get(pair.Key, true).Value);
            }
            string expectedData = _runtimeMock.PublishedData.Replace("utf-8", "utf-16"); // the encoding is done in the test
            Assert.AreEqual(Resources.ValidateAudBondFuturesReceive_Data, expectedData);
            #endregion
        }

        [TestMethod]
        public void ValidateCapFloor()
        {
            ValidateAudSwap6M();
            var ps = new PricingStructures(General.PricingStructures.Logger, _runtimeMock);
            const string marketName = "LpmTest";

            #region Inputs
            var properties
                = new[]
                      {
                          new object[] {"PricingStructureType", "LpmCapFloorCurve"},
                          new object[] {"CapFrequency", "3M"},
                          new object[] {"Currency", "AUD"},
                          new object[] {"Instrument", "CapFloor"},
                          new object[] {"MarketName", marketName},
                          new object[] {"CapStartLag", 1},
                          new object[] {"Source", "SydSPU"},
                          new object[] {"Handle", "AUD SydSPU ATM Bootstrap Settings"},
                          new object[] {"ParVolatilityInterpolation", "CubicHermiteSpline"},
                          new object[] {"RollConvention", "MODFOLLOWING"},
                          new object[] {"IndexName", "Volatility"},
                          new object[] {"Calculation Date", new DateTime(2010,7,20)},
                          //Extra property to keep the date constant
                          new object[] {"BuildDateTime", new DateTime(2010,7,20,14,20,30)},
                          new object[] {null, null}
                      };

            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 60},
                          new object[] {"MarketName", marketName},
                          new object[] {"LPMCopy", true},
                          new object[] {null, null}
                      };

            var values
                = new[]
                      {
                          // ="new object[] {"""&D2&""", "&E2&"d, """&F2&"""},"
                          new object[] {"Expiry", "PPD", "Type"},
                          new object[] {"46d", 6.9d, "ETO"},
                          new object[] {"136d", 8.2d, "ETO"},
                          new object[] {"227d", 8.2d, "ETO"},
                          new object[] {"319d", 8.2d, "ETO"},
                          new object[] {"2Y", 8d, "Cap/Floor"},
                          new object[] {"3Y", 7.9d, "Cap/Floor"},
                          new object[] {"4Y", 7.65d, "Cap/Floor"},
                          new object[] {"5Y", 7.5d, "Cap/Floor"},
                          new object[] {"7Y", 7.1d, "Cap/Floor"},
                          new object[] {"10Y", 6.8d, "Cap/Floor"},
                          new object[] {"15Y", 6.4d, "Cap/Floor"},
                          new object[] {"20Y", 6.4d, "Cap/Floor"},
                          new object[] {"25Y", 6.4d, "Cap/Floor"},
                          new object[] {null, null, null}
                      };

            var rateCurveFiltersRange
                 = new[]
                      {
                          new object[] {"PricingStructureType", "RateCurve"},
                          new object[] {"IndexTenor", "6M"},
                          new object[] {"Currency", "AUD"},
                          new object[] {"Index", "Swap"},
                          new object[] {"Algorithm", "Base algorithm"},
                          new object[] {"MarketName", "LiveTest"},
                          new object[] {"IndexName", "AUDSwap"},
                          new object[] {"Validity", "Official"},
                          new object[] {null, null}
                      };

            const string id = marketName + ".Volatility.CapFloor.AUD.SydSPU.2010.Week30";
            #endregion

            // Publish
            string result = ps.PublishLpmCapFloorVolMatrix(properties, publishProperties, values, rateCurveFiltersRange);

            #region Asserts

            Assert.IsNotNull(result);
            Assert.AreEqual(id, _runtimeMock.PublishedName);
            var expectedNamedValueSet = new NamedValueSet(Resources.ValidateCapFloor_Properties);
            foreach (var pair in expectedNamedValueSet.ToDictionary())
            {
                Assert.AreEqual(pair.Value.GetType(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.GetType(), pair.Key);
                Assert.AreEqual(pair.Value.ToString(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.ToString(), pair.Key);
                Debug.Print("{0}:{1}", pair.Value, _runtimeMock.PublishedProperties.Get(pair.Key, true).Value);
            }
            string expectedData = _runtimeMock.PublishedData.Replace("utf-8", "utf-16"); // the encoding is done in the test

// Slightly different values are generated by Debug or Release builds
#if (DEBUG)
            Assert.AreEqual(Resources.ValidateCapFloor_Data, expectedData);    
#else
            Assert.AreEqual(Resources.ValidateCapFloor_Data_Release, expectedData);
#endif

            #endregion
        }

        [TestMethod]
        public void ValidateSwaption()
        {
            ValidateAudSwap6M();
            var ps = new PricingStructures(General.PricingStructures.Logger, _runtimeMock);
            const string marketName = "LpmTest";

            #region Inputs
            var properties
                = new[]
                      {
                          new object[] {"PricingStructureType", "LPMSwaptionCurve"},
                          new object[] {"Currency", "AUD"},
                          new object[] {"Instrument", "Swaption"},
                          new object[] {"Source", "SydSPU"},
                          new object[] {"MarketName", marketName},
                          new object[] {"IndexName", "Volatility"},
                          new object[] {"Index", "LIBOR-BBA"},
                          //Extra property to keep the date constant
                          new object[] {"BaseDate", new DateTime(2010,7,20)},
                          new object[] {"BuildDateTime", new DateTime(2010,7,20,09,20,36)},
                          new object[] {null, null}
                      };

            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 10},
                          new object[] {"MarketName", marketName},
                          new object[] {"LPMCopy", true},
                          new object[] {null, null}
                      };

            var values
                = new[]
                      {
                          new object[] {"Expiry", "1Y", "2Y", "3Y", "4Y", "5Y", "7Y", "10Y", "15Y", "20Y"},
                          new object[] {"1M", 5.95d, 6.05d, 6.1d, 6.1d, 6.1d, 6.1d, 6.1d, 6.1d, 6.1d},
                          new object[] {"2M", 6.05d, 6.15d, 6.2d, 6.25d, 6.3d, 6.3d, 6.3d, 6.3d, 6.3d},
                          new object[] {"3M", 6.1d, 6.2d, 6.25d, 6.25d, 6.25d, 6.21d, 6.2d, 6.2d, 6.2d},
                          new object[] {"6M", 6.45d, 6.4d, 6.4d, 6.33d, 6.27d, 6.1d, 6d, 6d, 6d},
                          new object[] {"1Y", 6.75d, 6.55d, 6.45d, 6.3d, 6.2d, 6d, 5.91d, 5.91d, 5.91d},
                          new object[] {"2Y", 7d, 6.75d, 6.5d, 6.3d, 6.15d, 5.92d, 5.76d, 5.76d, 5.76d},
                          new object[] {"3Y", 6.85d, 6.59d, 6.35d, 6.2d, 6.05d, 5.83d, 5.64d, 5.64d, 5.64d},
                          new object[] {"5Y", 6.35d, 6.15d, 6d, 5.85d, 5.7d, 5.55d, 5.4d, 5.4d, 5.4d},
                          new object[] {"7Y", 5.95d, 5.85d, 5.75d, 5.64d, 5.55d, 5.4d, 5.25d, 5.25d, 5.25d},
                          new object[] {"10Y", 5.55d, 5.5d, 5.45d, 5.4d, 5.35d, 5.25d, 5.15d, 5.15d, 5.15d},
                          new object[] {null, null, null, null, null, null, null, null, null, null}
                      };

            var rateCurveFiltersRange
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateCurve"},
                          new object[] {"IndexTenor", "6M"},
                          new object[] {"Currency", "AUD"},
                          new object[] {"Index", "Swap"},
                          new object[] {"Algorithm", "Base algorithm"},
                          new object[] {"MarketName", "LiveTest"},
                          new object[] {"IndexName", "AUDSwap"},
                          new object[] {"Validity", "Official"},
                          new object[] {null, null}
                      };

            const string id = marketName + ".Volatility.Swaption.AUD.SydSPU.2010.Week30";
            #endregion

            // Publish
            string result = ps.PublishLpmSwaptionVolMatrix(properties, publishProperties, values, rateCurveFiltersRange);

            #region Asserts

            Assert.IsNotNull(result);
            Assert.AreEqual(id, _runtimeMock.PublishedName);
            var expectedNamedValueSet = new NamedValueSet(Resources.ValidateSwaption_Properties);
            foreach (var pair in expectedNamedValueSet.ToDictionary())
            {
                Assert.AreEqual(pair.Value.GetType(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.GetType(), pair.Key);
                Assert.AreEqual(pair.Value.ToString(), _runtimeMock.PublishedProperties.Get(pair.Key, true).Value.ToString(), pair.Key);
                Debug.Print("{0}:{1}", pair.Value, _runtimeMock.PublishedProperties.Get(pair.Key, true).Value);
            }
            string expectedData = _runtimeMock.PublishedData.Replace("utf-8", "utf-16"); // the encoding is done in the test
            Assert.AreEqual(Resources.ValidateSwaption_Data, expectedData);
            #endregion
        }

        #endregion
    }
}
