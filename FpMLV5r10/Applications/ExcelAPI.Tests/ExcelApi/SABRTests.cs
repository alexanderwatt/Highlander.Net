#region Using directives

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.CurveEngine.PricingStructures.SABR;
using Orion.Util.NamedValues;

#endregion

namespace Orion.ExcelAPI.Tests.ExcelApi
{
    [TestClass]
    public partial class ExcelAPITests
    {
        private static string Handle(string expiry, string tenor)
        {
            return expiry + tenor + " ATM VOLGRID Calibration";
        }

        [TestMethod]
        public void SabrCalibrationSettingsTest()
        {
            const string expiry = "3m";
            const string tenor = "0.25y";
            const string swaption = "VOLGRID " + expiry + tenor;
            string actual = SabrCalibrationSettings(swaption);
            Assert.AreEqual(swaption, actual);
        }

        private static string SabrCalibrationSettings(string swaption)
        {
            const string instrumentType = "Swaption";
            const string currency = "AUD";
            const decimal beta = 1.0m;
            return SABRSwaptionInterface.AddCalibrationSettings(swaption, instrumentType, currency, beta);
        }

        [TestMethod]
        public void SabrAtmCalibrationWithTenorTest()
        {
            const string expiry = "3m";
            const string tenor = "0.25y";
            string actual = SabrAtmCalibrationWithTenor();
            string expected = Handle(expiry, tenor);
            Assert.AreEqual(expected, actual);
        }

        private static string SabrAtmCalibrationWithTenor()
        {
            const string expiry = "3m";
            const string tenor = "0.25y";
            const string swaption = "VOLGRID " + expiry + tenor;
            const decimal atmVolatility = 20m;
            const decimal assetPrice = 3.44m;
            const decimal nu = 0.7561m;
            const decimal rho = -0.3702m;
            string handle = Handle(expiry, tenor);
            SabrCalibrationSettings(swaption);
            return SABRSwaptionInterface.CalibrateSABRATMModelWithTenor(handle, swaption, nu, rho, atmVolatility,
                                                                         assetPrice, expiry, tenor);                                                  
        }

        [TestMethod]
        public void CreateAtmCapFloorBootstrapEngineTest()
        {
            #region inputs

            var settings = new object[,]
            {
                {"Instrument", "AUD-BBR-BBSW-3M"},
                { "PricingStructureType", "CapVolatilityCurve"},
                { "BaseDate", new DateTime(2007,11,29) },
                { "ValuationDate", new DateTime(2007,11,29) },
                { "IndexTenor", "3M" },
                { "IndexName", "AUD-BBR-BBSW" },
                { "MarketName", "Test" },
                { "Currency", "AUD" },
                { "StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()},
                { "MeasureType", MeasureTypesEnum.Volatility.ToString()},
                { "QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString()},
                { "Algorithm", "Default"},
                { "EngineHandle", "AUD ATM Bootstrap Engine" },
                { "SettingsHandle", "AUD ATM Bootstrap Settings" }
            };

            DateTime[] dates
                = {
                      new DateTime(2010, 8, 30),
                      new DateTime(2010, 11, 29),
                      new DateTime(2011, 2, 28),
                      new DateTime(2011, 5, 30),
                      new DateTime(2011, 8, 29),
                      new DateTime(2011, 11, 28),
                      new DateTime(2012, 2, 28),
                      new DateTime(2012, 5, 28),
                      new DateTime(2012, 8, 28),
                      new DateTime(2012, 11, 28),
                      new DateTime(2013, 2, 28),
                      new DateTime(2013, 5, 28),
                      new DateTime(2013, 8, 28),
                      new DateTime(2013, 11, 28),
                      new DateTime(2014, 2, 28),
                      new DateTime(2014, 5, 28),
                      new DateTime(2014, 8, 28),
                      new DateTime(2014, 11, 28),
                      new DateTime(2015, 2, 27),
                      new DateTime(2015, 5, 28),
                      new DateTime(2015, 8, 28),
                      new DateTime(2015, 11, 30),
                      new DateTime(2016, 2, 29),
                      new DateTime(2016, 5, 30),
                      new DateTime(2016, 8, 29),
                      new DateTime(2016, 11, 28),
                      new DateTime(2017, 2, 28),
                      new DateTime(2017, 5, 29),
                      new DateTime(2017, 8, 28),
                      new DateTime(2017, 11, 28),
                      new DateTime(2018, 2, 28),
                      new DateTime(2018, 5, 28),
                      new DateTime(2018, 8, 28),
                      new DateTime(2018, 11, 28),
                      new DateTime(2019, 2, 28),
                      new DateTime(2019, 5, 28),
                      new DateTime(2019, 8, 28),
                      new DateTime(2019, 11, 28),
                      new DateTime(2020, 2, 28),
                      new DateTime(2020, 5, 28),
                      new DateTime(2020, 8, 28),
                      new DateTime(2020, 11, 30),
                      new DateTime(2021, 2, 26),
                      new DateTime(2021, 5, 28),
                      new DateTime(2021, 8, 30),
                      new DateTime(2021, 11, 29),
                      new DateTime(2022, 2, 28),
                      new DateTime(2022, 5, 30),
                      new DateTime(2022, 8, 29),
                      new DateTime(2022, 11, 28),
                      new DateTime(2023, 2, 28),
                      new DateTime(2023, 5, 29),
                      new DateTime(2023, 8, 28),
                      new DateTime(2023, 11, 28),
                      new DateTime(2024, 2, 28),
                      new DateTime(2024, 5, 28),
                      new DateTime(2024, 8, 28),
                      new DateTime(2024, 11, 28),
                      new DateTime(2025, 2, 28),
                      new DateTime(2025, 5, 28),
                      new DateTime(2025, 8, 28),
                      new DateTime(2025, 11, 28),
                      new DateTime(2026, 2, 27),
                      new DateTime(2026, 5, 28),
                      new DateTime(2026, 8, 28),
                      new DateTime(2026, 11, 30),
                      new DateTime(2027, 2, 26),
                      new DateTime(2027, 5, 28),
                      new DateTime(2027, 8, 30),
                      new DateTime(2027, 11, 29),
                      new DateTime(2028, 2, 28),
                      new DateTime(2028, 5, 29),
                      new DateTime(2028, 8, 28),
                      new DateTime(2028, 11, 28),
                      new DateTime(2029, 2, 28),
                      new DateTime(2029, 5, 28),
                      new DateTime(2029, 8, 28),
                      new DateTime(2029, 11, 28),
                      new DateTime(2030, 2, 28),
                      new DateTime(2030, 5, 28),
                      new DateTime(2030, 8, 28),
                      new DateTime(2030, 11, 28),
                      new DateTime(2031, 2, 28),
                      new DateTime(2031, 5, 28),
                      new DateTime(2031, 8, 28),
                      new DateTime(2031, 11, 28),
                      new DateTime(2032, 2, 27),
                      new DateTime(2032, 5, 28),
                      new DateTime(2032, 8, 30),
                      new DateTime(2032, 11, 29),
                      new DateTime(2033, 2, 28),
                      new DateTime(2033, 5, 30),
                      new DateTime(2033, 8, 29),
                      new DateTime(2033, 11, 28),
                      new DateTime(2034, 2, 28),
                      new DateTime(2034, 5, 29),
                      new DateTime(2034, 8, 28),
                      new DateTime(2034, 11, 28),
                      new DateTime(2035, 2, 28),
                      new DateTime(2035, 5, 28),
                      new DateTime(2035, 8, 28),
                      new DateTime(2035, 11, 28),
                      new DateTime(2036, 2, 28),
                      new DateTime(2036, 5, 28),
                      new DateTime(2036, 8, 28),
                      new DateTime(2036, 11, 28),
                      new DateTime(2037, 2, 27),
                      new DateTime(2037, 5, 28),
                      new DateTime(2037, 8, 28),
                      new DateTime(2037, 11, 30),
                      new DateTime(2038, 2, 26),
                      new DateTime(2038, 5, 28),
                      new DateTime(2038, 8, 30),
                      new DateTime(2038, 11, 29),
                      new DateTime(2039, 2, 28),
                      new DateTime(2039, 5, 30),
                      new DateTime(2039, 8, 29),
                      new DateTime(2039, 11, 28),
                      new DateTime(2040, 2, 28),
                      new DateTime(2040, 5, 28),
                      new DateTime(2040, 8, 28),
                      new DateTime(2040, 11, 28),
                      new DateTime(2041, 2, 28),
                      new DateTime(2041, 5, 28),
                      new DateTime(2041, 8, 28),
                      new DateTime(2041, 11, 28),
                      new DateTime(2042, 2, 28),
                      new DateTime(2042, 5, 28),
                      new DateTime(2042, 8, 28),
                      new DateTime(2042, 11, 28),
                      new DateTime(2043, 2, 27),
                      new DateTime(2043, 5, 28),
                      new DateTime(2043, 8, 28),
                      new DateTime(2043, 11, 30),
                      new DateTime(2044, 2, 29),
                      new DateTime(2044, 5, 30),
                      new DateTime(2044, 8, 29),
                      new DateTime(2044, 11, 28),
                      new DateTime(2045, 2, 28),
                      new DateTime(2045, 5, 29),
                      new DateTime(2045, 8, 28),
                      new DateTime(2045, 11, 28),
                      new DateTime(2046, 2, 28),
                      new DateTime(2046, 5, 28),
                      new DateTime(2046, 8, 28),
                      new DateTime(2046, 11, 28),
                      new DateTime(2047, 2, 28),
                      new DateTime(2047, 5, 28),
                      new DateTime(2047, 8, 28),
                      new DateTime(2047, 11, 28),
                      new DateTime(2048, 2, 28),
                      new DateTime(2048, 5, 28),
                      new DateTime(2048, 8, 28),
                      new DateTime(2048, 11, 30),
                      new DateTime(2049, 2, 26),
                      new DateTime(2049, 5, 28),
                      new DateTime(2049, 8, 30),
                      new DateTime(2049, 11, 29),
                      new DateTime(2050, 2, 28),
                      new DateTime(2050, 5, 30)
                  };

            decimal[] discountFactors
                = {
                      0.987518712m,
                      0.976295684m,
                      0.9649125m,
                      0.953388204m,
                      0.94166812m,
                      0.929795825m,
                      0.917715973m,
                      0.905898714m,
                      0.893874697m,
                      0.881835588m,
                      0.86977551m,
                      0.858089024m,
                      0.845537741m,
                      0.832923844m,
                      0.820255666m,
                      0.807964428m,
                      0.795958233m,
                      0.783985532m,
                      0.772179728m,
                      0.760545449m,
                      0.748904536m,
                      0.737074286m,
                      0.725685628m,
                      0.714372418m,
                      0.703425614m,
                      0.692563644m,
                      0.681669846m,
                      0.67109924m,
                      0.660724404m,
                      0.650334924m,
                      0.640045807m,
                      0.630188111m,
                      0.620402678m,
                      0.610727398m,
                      0.601161867m,
                      0.592015048m,
                      0.582920044m,
                      0.573939201m,
                      0.565071554m,
                      0.556507012m,
                      0.548020869m,
                      0.539467077m,
                      0.531565141m,
                      0.523500336m,
                      0.515282143m,
                      0.507433934m,
                      0.499690512m,
                      0.49205554m,
                      0.484664917m,
                      0.477377081m,
                      0.470112325m,
                      0.463104602m,
                      0.456117408m,
                      0.44915273m,
                      0.442286616m,
                      0.435663923m,
                      0.428989077m,
                      0.422409064m,
                      0.415922659m,
                      0.409747646m,
                      0.404543017m,
                      0.399433413m,
                      0.394470931m,
                      0.38965077m,
                      0.384811919m,
                      0.379958343m,
                      0.375495784m,
                      0.370962047m,
                      0.366363562m,
                      0.361992208m,
                      0.357698346m,
                      0.353480452m,
                      0.349337036m,
                      0.345222313m,
                      0.341180754m,
                      0.337339255m,
                      0.333437471m,
                      0.329604676m,
                      0.325839526m,
                      0.322260433m,
                      0.318637998m,
                      0.315079421m,
                      0.311583469m,
                      0.308259975m,
                      0.304883738m,
                      0.301566625m,
                      0.298342628m,
                      0.295174306m,
                      0.291958885m,
                      0.288900556m,
                      0.285894805m,
                      0.282940657m,
                      0.280037154m,
                      0.277183363m,
                      0.274347813m,
                      0.271621277m,
                      0.268911211m,
                      0.266218294m,
                      0.263571702m,
                      0.261054696m,
                      0.258617538m,
                      0.256224336m,
                      0.25387429m,
                      0.251616338m,
                      0.249349374m,
                      0.247123286m,
                      0.244960887m,
                      0.242860203m,
                      0.240751162m,
                      0.238635584m,
                      0.236690427m,
                      0.234714322m,
                      0.232710175m,
                      0.230805289m,
                      0.228934531m,
                      0.227097324m,
                      0.225293101m,
                      0.223521309m,
                      0.221762463m,
                      0.220072868m,
                      0.218102234m,
                      0.216162604m,
                      0.214253465m,
                      0.212435122m,
                      0.210584508m,
                      0.208762918m,
                      0.206969876m,
                      0.20526203m,
                      0.203523802m,
                      0.201812766m,
                      0.200146649m,
                      0.198524177m,
                      0.196891279m,
                      0.195249217m,
                      0.193684513m,
                      0.192143967m,
                      0.1906272m,
                      0.189133843m,
                      0.187647501m,
                      0.186215911m,
                      0.184790629m,
                      0.183372046m,
                      0.181975599m,
                      0.180645434m,
                      0.17929155m,
                      0.177958812m,
                      0.176646897m,
                      0.175397282m,
                      0.174125422m,
                      0.172873469m,
                      0.171641125m,
                      0.170454265m,
                      0.169259859m,
                      0.16805886m,
                      0.166952006m,
                      0.165824947m,
                      0.164679175m,
                      0.163587568m,
                      0.162513022m,
                      0.161455298m
                  };

            string[] volTypes =
                {
                    "AUD-CAPLET-9d-90d",
                    "AUD-CAPLET-100d-90d",
                    "AUD-CAPLET-191d-90d",
                    "AUD-CAPLET-282d-90d",
                    "AUD-IRCAP-2Y",
                    "AUD-IRCAP-3Y",
                    "AUD-IRCAP-4Y",
                    "AUD-IRCAP-5Y",
                    "AUD-IRCAP-7Y",
                    "AUD-IRCAP-10Y",
                    "AUD-IRCAP-15Y",
                    "AUD-IRCAP-20Y",
                    "AUD-IRCAP-25Y",
                    "AUD-IRCAP-30Y"
                };

            decimal[] vols =

                {
                    0.1638m,
                    0.1865m,
                    0.1911m,
                    0.2033m,
                    0.2171m,
                    0.2059m,
                    0.1961m,
                    0.1851m,
                    0.1680m,
                    0.1532m,
                    0.1440m,
                    0.1440m,
                    0.1440m,
                    0.1440m
                };

            #endregion
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.NewInstance();
            var properties = cfInterface.CreateCapFloorProperties(settings);
            Assert.AreEqual("AUD ATM Bootstrap Settings", properties.GetValue<string>("SettingsHandle", true));
            const string engineHandle = "AUD ATM Bootstrap Engine";
            string result = cfInterface.CreateCapFloorATMCurve(Engine.Logger, Engine.Cache, Engine.NameSpace, properties, volTypes,
                                                vols, dates, discountFactors);
            Assert.AreEqual(engineHandle, result);
        }

        [TestMethod]
        public void CreateSabrCapFloorBootstrapSettingsTest()
        {
            var actual = CreateSabrCapFloorBootstrapSettings();
            Assert.AreEqual("ATM Bootstrap Settings", actual.GetValue<string>("SettingsHandle", true));
        }

        private NamedValueSet CreateSabrCapFloorBootstrapSettings()
        {
            var settings = new object[,]
            {
                {"Instrument", "AUD-BBR-BBSW-3M"},
                { "PricingStructureType", "CapVolatilityCurve"},
                { "BaseDate", new DateTime(2007,11,29) },
                { "ValuationDate", new DateTime(2007,11,29) },
                { "IndexTenor", "3M" },
                { "IndexName", "AUD-BBR-BBSW" },
                { "MarketName", "Test" },
                { "Currency", "AUD" },
                { "StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()},
                { "MeasureType", MeasureTypesEnum.Volatility.ToString()},
                { "QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString()},
                { "Algorithm", "Default"},
                { "EngineHandle", "AUD ATM Bootstrap Engine" },
                { "SettingsHandle", "ATM Bootstrap Settings" }
            };
            SABRCapFloorInterface cfInterface = SABRCapFloorInterface.Instance();
            var properties = cfInterface.CreateCapFloorProperties(settings);
            return properties;
        }

        [TestMethod]
        public void CreateSabrCapFloorEngineTest()
        {
            const string engineHandle = "ATM Bootstrap Engine";
            var settings = CreateSabrCapFloorBootstrapSettings();
            settings.Set("EngineHandle", engineHandle);
            string[] types = {"AUD-IRCAP-1Y", "AUD-IRCAP-2Y", "AUD-IRCAP-3Y", "AUD-IRCAP-4Y", "AUD-IRCAP-5Y" };
            decimal[,] vols = { { .1966m }, { .1909m }, { .1965m }, { .1941m }, { .1882m } };
            DateTime[] dates = {
                                   new DateTime(2010, 09, 10),
                                   new DateTime(2011, 09, 10),
                                   new DateTime(2012, 09, 10),
                                   new DateTime(2013, 09, 10),
                                   new DateTime(2014, 09, 10)
                               };
            decimal[] discounts = {.999877m, 0.951649m, 0.905211m, 0.860439m, 0.816905m};
            string result = SABRCapFloorInterface.Instance().CreateCapFloorCurve(Engine.Logger, Engine.Cache, 
                Engine.NameSpace, settings, types, vols, dates, discounts);
            Assert.AreEqual(engineHandle, result);
        }
    }
}
