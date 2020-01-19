using System;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.UnitTestEnv;
using Orion.Util.NamedValues;

namespace Orion.CurveEngine.Tests
{
    [TestClass]
    public class FactoryTest
    {
        #region Setup

        private const string Market = "FactoryTest";
        private const string Market1 = "FactoryTest1";
        private const string Market2 = "FactoryTest2";

        private static readonly string[] AudBondFinancingBasisInstruments =
        {
            "AUD-RepoSpread-1D",
            "AUD-RepoSpread-1M",
            "AUD-RepoSpread-2M",
            "AUD-RepoSpread-3M"
        };

        private static readonly decimal[] AudBondFinancingBasisValues =
        {
            -.002m,
            -.002m,
            -.002m,
            -.002m,
            -.002m
        };

        private static readonly string[] AudBondFinancingInstruments =
        {
            "AUD-Repo-1D",
            "AUD-Repo-1M",
            "AUD-Repo-2M",
            "AUD-Repo-3M"
        };

        private static readonly decimal[] AudBondFinancingValues =
        {
            .05m,
            .05m,
            .05m,
            .05m,
            .05m
        };

        private static readonly string[] AudBondForwardInstruments =
        {
            "AUD-BondForward-AGB-0D",
            "AUD-BondSpot-AGB",
            "AUD-BondForward-AGB-1M",
            "AUD-BondForward-AGB-2M"
        };

        private static readonly decimal[] AudBondForwardValues =
        {
            .05m,
            .05m,
            .05m,
            .05m,
            .05m
        };

        private static readonly string[] AudBondInstruments =
        {
            "AUD-Bond-Govt.WATC.Fixed.5,5.17/07/2012",
            "AUD-Bond-Govt.WATC.Fixed.7.15/04/2015",
            "AUD-Bond-Govt.WATC.Fixed.8.15/07/2017",
            "AUD-Bond-Govt.WATC.Fixed.7.15/10/2019",
            "AUD-Bond-Govt.WATC.Fixed.7.15/07/2021"
        };

        private static readonly decimal[] AudBondValues =
        {
            .025m,
            .025m,
            .025m,
            .025m,
            .025m
        };

        private static readonly string[] AudUsdInstruments =
            {
                "AUDUSD-FxForward-ON",
                "AUDUSD-FxForward-TN",
                "AUDUSD-FxSpot-SP",
                "AUDUSD-FxForward-1W",
                "AUDUSD-FxForward-2W",
                "AUDUSD-FxForward-3W",
                "AUDUSD-FxForward-1M",
                "AUDUSD-FxForward-2M",
                "AUDUSD-FxForward-3M",
                "AUDUSD-FxForward-4M",
                "AUDUSD-FxForward-5M",
                "AUDUSD-FxForward-6M",
                "AUDUSD-FxForward-9M",
                "AUDUSD-FxForward-1Y",
                "AUDUSD-FxForward-15M",
                "AUDUSD-FxForward-18M",
                "AUDUSD-FxForward-21M",
                "AUDUSD-FxForward-2Y",
                "AUDUSD-FxForward-3Y",
                "AUDUSD-FxForward-4Y",
                "AUDUSD-FxForward-5Y",
                "AUDUSD-FxForward-7Y",
                "AUDUSD-FxForward-10Y",
                "AUDUSD-FxForward-15Y",
            };

        private static readonly decimal[] AudUsdValues =
            {
                0.942972m,
                0.943191m,
                0.9433m,
                0.942535m,
                0.94175m,
                0.9409725m,
                0.939947m,
                0.936285m,
                0.932935m,
                0.9293625m,
                0.925565m,
                0.92236m,
                0.9116775m,
                0.901085m,
                0.890844m,
                0.881032m,
                0.8715095m,
                0.862399m,
                0.8293605m,
                0.798949m,
                0.775842m,
                0.7403m,
                0.701206m,
                0.660938m,
            };

        private static readonly string[] GbpUsdInstruments =
            {
                "GBPUSD-FxForward-ON",
                "GBPUSD-FxForward-TN",
                "GBPUSD-FxSpot-SP",
                "GBPUSD-FxForward-1W",
                "GBPUSD-FxForward-2W",
                "GBPUSD-FxForward-3W",
                "GBPUSD-FxForward-1M",
                "GBPUSD-FxForward-2M",
                "GBPUSD-FxForward-3M",
                "GBPUSD-FxForward-4M",
                "GBPUSD-FxForward-5M",
                "GBPUSD-FxForward-6M",
                "GBPUSD-FxForward-9M",
                "GBPUSD-FxForward-1Y",
                "GBPUSD-FxForward-2Y",
                "GBPUSD-FxForward-3Y",
                "GBPUSD-FxForward-4Y",
                "GBPUSD-FxForward-5Y",
                "GBPUSD-FxForward-6Y",
                "GBPUSD-FxForward-7Y",
                "GBPUSD-FxForward-8Y",
                "GBPUSD-FxForward-9Y",
                "GBPUSD-FxForward-10Y",
            };

        private static readonly decimal[] GbpUsdValues =
            {
                1.570161m,
                1.570188m,
                1.5702m,
                1.570118m,
                1.570035m,
                1.569955m,
                1.5698565m,
                1.569505m,
                1.56918m,
                1.56883m,
                1.56846m,
                1.56815m,
                1.5669m,
                1.565482m,
                1.5587m,
                1.54293m,
                1.537245m,
                1.53499m,
                1.53442m,
                1.533065m,
                1.53062m,
                1.52801m,
                1.5254m,
            };

        private static readonly string[] UsdJpyInstruments =
            {
                "USDJPY-FxForward-ON",
                "USDJPY-FxForward-TN",
                "USDJPY-FxSpot-SP",
                "USDJPY-FxForward-1W",
                "USDJPY-FxForward-2W",
                "USDJPY-FxForward-3W",
                "USDJPY-FxForward-1M",
                "USDJPY-FxForward-2M",
                "USDJPY-FxForward-3M",
                "USDJPY-FxForward-4M",
                "USDJPY-FxForward-5M",
                "USDJPY-FxForward-6M",
                "USDJPY-FxForward-9M",
                "USDJPY-FxForward-1Y",
                "USDJPY-FxForward-2Y",
                "USDJPY-FxForward-3Y",
                "USDJPY-FxForward-4Y",
                "USDJPY-FxForward-5Y",
                "USDJPY-FxForward-7Y",
                "USDJPY-FxForward-10Y",
                "USDJPY-FxForward-12Y",
                "USDJPY-FxForward-15Y",
            };

        private static readonly decimal[] UsdJpyValues =
            {
                85.689981m,
                85.6899955m,
                85.69m,
                85.6899535m,
                85.689859m,
                85.6897805m,
                85.6896855m,
                85.689356m,
                85.6890345m,
                85.688565m,
                85.68821m,
                85.687808m,
                85.68641m,
                85.684765m,
                85.6749m,
                85.6637205m,
                85.644566m,
                85.621772m,
                85.578121m,
                85.5300545m,
                85.504859m,
                85.473701m,
            };

        private static readonly string[] IrInstruments =
            {
                "AUD-DEPOSIT-ON",
                "AUD-DEPOSIT-1M",
                "AUD-DEPOSIT-2M",
                "AUD-DEPOSIT-3M",
                "AUD-IRFUTURE-IR-Z0",
                "AUD-IRFUTURE-IR-H1",
                "AUD-IRFUTURE-IR-M1",
                "AUD-IRFUTURE-IR-U1",
                "AUD-IRFUTURE-IR-Z1",
                "AUD-IRFUTURE-IR-H2",
                "AUD-IRFUTURE-IR-M2",
                "AUD-IRFUTURE-IR-U2",
                "AUD-IRSWAP-3Y",
                "AUD-IRSWAP-4Y",
                "AUD-IRSWAP-5Y",
                "AUD-IRSWAP-6Y",
                "AUD-IRSWAP-7Y",
                "AUD-IRSWAP-8Y",
                "AUD-IRSWAP-9Y",
                "AUD-IRSWAP-10Y",
                "AUD-IRSWAP-12Y",
                "AUD-IRSWAP-15Y",
                "AUD-IRSWAP-20Y",
                "AUD-IRSWAP-25Y",
                "AUD-IRSWAP-30Y",
                "AUD-IRSWAP-40Y",
            };

        private static readonly decimal[] IrValues =
            {
                0.045m,
                0.0466m,
                0.04805m,
                0.0494m,
                0.050994892m,
                0.052084922m,
                0.052870841m,
                0.053352742m,
                0.053625546m,
                0.053788795m,
                0.053941944m,
                0.054078661m,
                0.0533585m,
                0.0541835m,
                0.0548335m,
                0.055271m,
                0.0556335m,
                0.055775m,
                0.0558665m,
                0.055975m,
                0.0562665m,
                0.05665m,
                0.0564585m,
                0.0552165m,
                0.0537585m,
                0.051649m,
            };

        private static readonly string[] IrVolInstruments =
        {
            "AUD-IRFUTUREOPTION-IR-1",
            "AUD-IRFUTUREOPTION-IR-2",
            "AUD-IRFUTUREOPTION-IR-3",
            "AUD-CAPLET-12M-3M",
            "AUD-IRCAP-2Y",//Removed the frequency: -3M
            "AUD-IRCAP-3Y-3M"
        };

        private static readonly decimal[] IrStrikes =
        {
            0.025m,
            0.025m,
            0.025m,
            0.025m,
            0.025m,
            0.025m
        };

        private static readonly decimal[] IrVolValues =
        {
            0.15m,
            0.17m,
            0.19m,
            0.21m,
            0.205m,
            0.22m
        };

        private static readonly string[] IrVolInstruments2 =
        {
            "AUD-CAPLET-1M-3M",
            "AUD-IRFUTUREOPTION-IR-1",
            "AUD-IRCAP-12M-3M",
            "AUD-CAPLET-12M-3M",
            "AUD-IRCAP-2Y",//Removed the frequency: -3M
            "AUD-IRCAP-3Y-3M"
        };

        private static readonly decimal[] IrStrikes2 =
        {
            0.025m,
            0.025m,
            0.025m,
            0.025m,
            0.025m,
            0.025m
        };

        private static readonly decimal[] IrVolValues2 =
        {
            0.15m,
            0.17m,
            0.19m,
            0.21m,
            0.205m,
            0.22m
        };

        private static object[,] ProcessValues(string[] instruments, decimal[] values)
        {
            var result = new object[instruments.Length, 2];
            for (var i = 0; i < instruments.Length; i++)
            {
                result[i, 0] = instruments[i];
                result[i, 1] = values[i];
            }
            return result;
        }

        private static object[,] ProcessValues(string[] instruments, decimal[] forwards, decimal[] volatilities)
        {
            var result = new object[instruments.Length, 3];
            for (var i = 0; i < instruments.Length; i++)
            {
                result[i, 0] = instruments[i];
                if (forwards != null)
                {
                    result[i, 1] = forwards[i];
                }
                else
                {
                    result[i, 1] = 0.0;
                }
                if (volatilities != null)
                {
                    result[i, 2] = volatilities[i];
                }
                else
                {
                    result[i, 2] = 0.0;
                }
            }
            return result;
        }

        private static void CreateBondFinancingBasisCurve(string currency1, string[] instruments, decimal[] values)
        {
            NamedValueSet properties = new NamedValueSet();
            properties.Set("PricingStructureType", "BondFinancingBasisCurve");
            properties.Set("ReferenceBond", "Govt.WATC.Fixed.7.15/10/2019");
            properties.Set("ReferenceCurveUniqueId", "Market." + Market +".DiscountCurve.GBP-LIBOR-SENIOR");
            properties.Set("CurveName", "AUD-GC-SECURED");
            properties.Set("Currency", currency1);
            properties.Set("BondType", "AGB");
            properties.Set("CreditSeniority", "SECURED");
            properties.Set("CreditInstrumentId", "GC");
            properties.Set("MarketName", Market);
            properties.Set("Algorithm", "LinearZero");
            properties.Set("MarketQuote", "YieldToMaturity");
            properties.Set("BaseDate", new DateTime(2010, 3, 1));
            var bondCurve = CurveEngine.CreatePricingStructure(properties, ProcessValues(instruments, values));
            string name = bondCurve.GetPricingStructureId().UniqueIdentifier;
            Market market = bondCurve.GetMarket();
            CurveEngine.SaveCurve(market, name, properties, new TimeSpan(0, 1, 0));
        }

        private static void CreateBondFinancingCurve(string currency1, string[] instruments, decimal[] values)
        {
            NamedValueSet properties = new NamedValueSet();
            properties.Set("PricingStructureType", "BondFinancingCurve");
            properties.Set("ReferenceBond", "Govt.WATC.Fixed.7.15/10/2019");
            properties.Set("Currency", currency1);
            properties.Set("BondType", "AGB");
            properties.Set("CreditSeniority", "SENIOR");
            properties.Set("CreditInstrumentId", "WATC");
            properties.Set("MarketName", Market2);
            properties.Set("Algorithm", "LinearZero");
            properties.Set("MarketQuote", "YieldToMaturity");
            properties.Set("BaseDate", new DateTime(2010, 3, 1));
            var bondCurve = CurveEngine.CreatePricingStructure(properties, ProcessValues(instruments, values));
            string name = bondCurve.GetPricingStructureId().UniqueIdentifier;
            Market market = bondCurve.GetMarket();
            CurveEngine.SaveCurve(market, name, properties, new TimeSpan(0, 1, 0));
        }

        private static void CreateBondCurve(string currency1, string[] instruments, decimal[] values)
        {
            NamedValueSet properties = new NamedValueSet();
            properties.Set("PricingStructureType", "BondCurve");
            properties.Set("ReferenceBond", "Govt.WATC.Fixed.7.15/10/2019");
            properties.Set("Currency", currency1);
            properties.Set("BondType", "AGB");
            properties.Set("CreditSeniority", "SENIOR");
            properties.Set("CreditInstrumentId", "WATC");
            properties.Set("MarketName", Market);
            properties.Set("Algorithm", "LinearZero");
            properties.Set("MarketQuote", "YieldToMaturity");
            properties.Set("BaseDate", new DateTime(2010, 3, 1));
            var bondCurve = CurveEngine.CreatePricingStructure(properties, ProcessValues(instruments, values));
            string name = bondCurve.GetPricingStructureId().UniqueIdentifier;
            Market market = bondCurve.GetMarket();
            CurveEngine.SaveCurve(market, name, properties, new TimeSpan(0, 1, 0));
        }

        private static void CreateBondDiscountCurve(string currency1, string[] instruments, decimal[] values)
        {
            NamedValueSet properties = new NamedValueSet();
            properties.Set("PricingStructureType", "BondDiscountCurve");
            properties.Set("Currency", currency1);
            properties.Set("CreditInstrumentId", "WATC");
            properties.Set("CreditSeniority", "SENIOR");
            properties.Set("MarketName", Market1);
            properties.Set("Algorithm", "LinearZero");
            properties.Set("MarketQuote", "YieldToMaturity");
            properties.Set("BaseDate", new DateTime(2010, 3, 1));
            var bondCurve = CurveEngine.CreatePricingStructure(properties, ProcessValues(instruments, values));
            string name = bondCurve.GetPricingStructureId().UniqueIdentifier;
            Market market = bondCurve.GetMarket();
            CurveEngine.SaveCurve(market, name, properties, new TimeSpan(0, 1, 0));
        }

        private static void CreateFxCurve(string currency1, string currency2, string[] instruments, decimal[] values)
        {
            NamedValueSet properties = new NamedValueSet();
            properties.Set("CurveName", "FxCurve" + currency1 + "-" + currency2);
            properties.Set("PricingStructureType", "FxCurve");
            properties.Set("Currency", currency1);
            properties.Set("Currency2", currency2);
            properties.Set("CurrencyPair", currency1 + "-" + currency2);
            properties.Set("QuoteBasis", "Currency2PerCurrency1");
            properties.Set("MarketName", Market);
            properties.Set("BaseDate", DateTime.Today);
            var fxCurve = CurveEngine.CreatePricingStructure(properties, ProcessValues(instruments, values));
            string name = fxCurve.GetPricingStructureId().UniqueIdentifier;
            Market market = fxCurve.GetMarket();
            CurveEngine.SaveCurve(market, name, properties, new TimeSpan(0, 1, 0));
        }

        private static void CreateRateCurve(string currency1, string indexTenor, string[] instruments, decimal[] values)
        {
            NamedValueSet properties = new NamedValueSet();
            properties.Set("CurveName", "AUD-BBR-BBSW-" + indexTenor);
            properties.Set("PricingStructureType", "RateCurve");
            properties.Set("Currency", currency1);
            properties.Set("IndexTenor", indexTenor);
            properties.Set("IndexName", "AUD-BBR-BBSW");
            properties.Set("MarketName", Market);
            properties.Set("BaseDate", DateTime.Today);
            var rateCurve = CurveEngine.CreatePricingStructure(properties, ProcessValues(instruments, values));
            string name = rateCurve.GetPricingStructureId().UniqueIdentifier;
            Market market = rateCurve.GetMarket();
            CurveEngine.SaveCurve(market, name, properties, new TimeSpan(0, 1, 0));
        }

        private static void CreateDiscountCurve(string currency1, string[] instruments, decimal[] values)
        {
            NamedValueSet properties = new NamedValueSet();
            properties.Set("PricingStructureType", "DiscountCurve");
            properties.Set("CreditInstrumentId", "LIBOR");
            properties.Set("CreditSeniority", "SENIOR");
            properties.Set("CurveName", currency1 + "-LIBOR-SENIOR");
            properties.Set("Currency", currency1);
            properties.Set("MarketName", Market);
            properties.Set("BaseDate", DateTime.Today);
            var rateCurve = CurveEngine.CreatePricingStructure(properties, ProcessValues(instruments, values));
            string name = rateCurve.GetPricingStructureId().UniqueIdentifier;
            Market market = rateCurve.GetMarket();
            CurveEngine.SaveCurve(market, name, properties, new TimeSpan(0, 1, 0));
        }

        private static void CreateATMRateVolCurve(string currency1, string indexTenor, string[] instruments, decimal[] marketvalues, decimal[] additionalforwards)
        {
            NamedValueSet properties = new NamedValueSet();
            properties.Set("Instrument", "AUD-BBR-BBSW-" + indexTenor);
            properties.Set("PricingStructureType", "CapVolatilityCurve");
            properties.Set("ReferenceCurveUniqueId", "Market." + Market + ".DiscountCurve.GBP-LIBOR-SENIOR");
            properties.Set("ReferenceCurrency2CurveId", "Market." + Market + ".DiscountCurve.GBP-LIBOR-SENIOR");
            properties.Set("Currency", currency1);
            properties.Set("IndexTenor", indexTenor);
            properties.Set("IndexName", "AUD-BBR-BBSW");
            properties.Set("MarketName", Market);
            properties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
            properties.Set("MeasureType", MeasureTypesEnum.Volatility.ToString());
            properties.Set("QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString());
            properties.Set("Algorithm", "Default");
            properties.Set("QuotationSide", QuotationSideEnum.Mid.ToString());
            properties.Set("Timing", QuoteTimingEnum.Close.ToString());
            properties.Set("ValuationDate", DateTime.Today);
            properties.Set("BusinessCenter","Sydney");
            //properties.Set("Strike", 0.025);//ATMFlatForward = 1.0
            properties.Set("BaseDate", DateTime.Today);
            var volCurve = CurveEngine.CreatePricingStructure(properties, ProcessValues(instruments, marketvalues, additionalforwards));
            string name = volCurve.GetPricingStructureId().UniqueIdentifier;
            Market market = volCurve.GetMarket();
            CurveEngine.SaveCurve(market, name, properties, new TimeSpan(0, 1, 0));
        }

        private static void CreateStrikeRateVolCurve(string currency1, string indexTenor, string[] instruments, decimal[] marketvalues, decimal[] additionalforwards)
        {
            NamedValueSet properties = new NamedValueSet();
            properties.Set("Instrument", "AUD-BBR-BBSW-" + indexTenor);
            properties.Set("PricingStructureType", "CapVolatilityCurve");
            properties.Set("ReferenceCurveUniqueId", "Market." + Market + ".DiscountCurve.GBP-LIBOR-SENIOR");
            properties.Set("ReferenceCurrency2CurveId", "Market." + Market + ".DiscountCurve.GBP-LIBOR-SENIOR");
            properties.Set("Currency", currency1);
            properties.Set("IndexTenor", indexTenor);
            properties.Set("IndexName", "AUD-BBR-BBSW");
            properties.Set("MarketName", Market);
            properties.Set("StrikeQuoteUnits", StrikeQuoteUnitsEnum.DecimalRate.ToString());
            properties.Set("MeasureType", MeasureTypesEnum.Volatility.ToString());
            properties.Set("QuoteUnits", QuoteUnitsEnum.LogNormalVolatility.ToString());
            properties.Set("Algorithm", "Linear");
            properties.Set("QuotationSide", QuotationSideEnum.Mid.ToString());
            properties.Set("Timing", QuoteTimingEnum.Close.ToString());
            properties.Set("ValuationDate", DateTime.Today);
            properties.Set("BusinessCenter", "Sydney");
            properties.Set("Strike", 0.025m);
            properties.Set("BaseDate", DateTime.Today);
            var volCurve = CurveEngine.CreatePricingStructure(properties, ProcessValues(instruments, marketvalues, additionalforwards));
            string name = volCurve.GetPricingStructureId().UniqueIdentifier;
            Market market = volCurve.GetMarket();
            CurveEngine.SaveCurve(market, name, properties, new TimeSpan(0, 1, 0));
        }

        #endregion

        #region Properties

        private static CurveEngine CurveEngine { get; set; }
        private static CurveUnitTestEnvironment UTE { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            UTE = new CurveUnitTestEnvironment();
            CurveEngine = new CurveEngine(UTE.Logger, UTE.Cache);
            CreateFxCurve("AUD", "USD", AudUsdInstruments, AudUsdValues);
            CreateFxCurve("GBP", "USD", GbpUsdInstruments, GbpUsdValues);
            CreateFxCurve("USD", "JPY", UsdJpyInstruments, UsdJpyValues);
            CreateRateCurve("AUD", "1M", IrInstruments, IrValues);
            CreateRateCurve("AUD", "3M", IrInstruments, IrValues);
            CreateDiscountCurve("GBP", IrInstruments, IrValues);
            CreateBondDiscountCurve("AUD", AudBondInstruments, AudBondValues);
            CreateBondCurve("AUD", AudBondForwardInstruments, AudBondForwardValues);
            CreateBondFinancingCurve("AUD", AudBondFinancingInstruments, AudBondFinancingValues);
            CreateBondFinancingBasisCurve("AUD", AudBondFinancingBasisInstruments, AudBondFinancingBasisValues);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
            UTE.Dispose();
        }

        #endregion

        #region FX Curve Spot tests - used to check that the curves have been created the right way round

        [TestMethod]
        public void SpotGbpAudTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "GBP", "AUD", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(1.67d, (double)curve.GetSpotRate(), 0.01);
        }

        [TestMethod]
        public void SpotAudGbpTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "AUD", "GBP", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(0.60d, (double)curve.GetSpotRate(), 0.01);
        }

        [TestMethod]
        public void SpotGbpUsdTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "GBP", "USD", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(1.57d, (double)curve.GetSpotRate(), 0.01);
        }

        [TestMethod]
        public void SpotUsdGbpTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "USD", "GBP", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(0.64d, (double)curve.GetSpotRate(), 0.01);
        }

        [TestMethod]
        public void SpotGbpJpyTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "GBP", "JPY", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(134.55d, (double)curve.GetSpotRate(), 0.01);
        }

        [TestMethod]
        public void SpotJpyGbpTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "JPY", "GBP", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(0.0075, (double)curve.GetSpotRate(), 0.0001);
        }

        [TestMethod]
        public void SpotAudUsdTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "AUD", "USD", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(0.94d, (double)curve.GetSpotRate(), 0.01);
        }

        [TestMethod]
        public void SpotUsdAudTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "USD", "AUD", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(1.06d, (double)curve.GetSpotRate(), 0.01);
        }

        [TestMethod]
        public void SpotAudJpyTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "AUD", "JPY", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(80.83d, (double)curve.GetSpotRate(), 0.01);
        }

        [TestMethod]
        public void SpotJpyAudTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "JPY", "AUD", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(0.0124d, (double)curve.GetSpotRate(), 0.0001);
        }

        [TestMethod]
        public void SpotUsdJpyTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "USD", "JPY", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(85.69d, (double)curve.GetSpotRate(), 0.01);
        }

        [TestMethod]
        public void SpotJpyUsdTest()
        {
            var curve = CurveEngine.LoadFxCurve(Market, "JPY", "USD", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual(0.0117d, (double)curve.GetSpotRate(), 0.0001);
        }

        #endregion

        #region Bond Curve Tests

        [TestMethod]
        public void LoadBondFinancingBasisCurveTest()
        {
            var curve = CurveEngine.LoadDiscountCurve(Market, "AUD", "GC", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual("GC", curve.GetPricingStructureId().Properties.GetString("CreditInstrumentId", true));
        }

        [TestMethod]
        public void LoadBondFinancingCurveTest()
        {
            var curve = CurveEngine.LoadDiscountCurve(Market2, "AUD", "WATC", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual("WATC", curve.GetPricingStructureId().Properties.GetString("CreditInstrumentId", true));
        }

        [TestMethod]
        public void LoadBondDiscountCurveTest()
        {
            var curve = CurveEngine.LoadDiscountCurve(Market1, "AUD", "WATC", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual("WATC", curve.GetPricingStructureId().Properties.GetString("CreditInstrumentId", true));
        }

        [TestMethod]
        public void LoadBondCurveTest()
        {
            var curve = CurveEngine.LoadDiscountCurve(Market, "AUD", "WATC", null);
            Assert.IsNotNull(curve);
            Assert.AreEqual("WATC", curve.GetPricingStructureId().Properties.GetString("CreditInstrumentId", true));
        }

        #endregion

        #region IR Curve tests

        [TestMethod]
        public void LoadInterestRateCurveAud3MTest()
        {
            var curve = CurveEngine.LoadInterestRateCurve(Market, "AUD", "3M", null);
            Assert.IsNotNull(curve);
            // 3M does exist
            Assert.AreEqual("3M", curve.GetPricingStructureId().Properties.GetString("IndexTenor", true));
        }

        [TestMethod]
        public void LoadInterestRateCurveAud1MTest()
        {
            var curve = CurveEngine.LoadInterestRateCurve(Market, "AUD", "1M", null);
            Assert.IsNotNull(curve);
            // 1M does exist
            Assert.AreEqual("1M", curve.GetPricingStructureId().Properties.GetString("IndexTenor", true));
        }

        [TestMethod]
        public void LoadInterestRateCurveAud6MTest()
        {
            var curve = CurveEngine.LoadInterestRateCurve(Market, "AUD", "6M", null);
            Assert.IsNotNull(curve);
            // 6M does not exist, 3M loaded instead
            Assert.AreEqual("3M", curve.GetPricingStructureId().Properties.GetString("IndexTenor", true));
        }

        [TestMethod]
        public void LoadInterestRateCurveGbpNullTenorTest()
        {
            var curve = CurveEngine.LoadInterestRateCurve(Market, "GBP", null, null);
            Assert.IsNotNull(curve);
            // curve without tenor exists
            Assert.AreEqual("NotFound", curve.GetPricingStructureId().Properties.GetString("IndexTenor", "NotFound"));
        }

        [TestMethod]
        public void LoadInterestRateCurveFailTest()
        {
            var curve = CurveEngine.LoadInterestRateCurve(Market, "NZD", "3M", null);
            Assert.IsNull(curve);
        }

        #endregion

        #region IR Cap Tests

        [TestMethod]
        public void LoadInterestRateVolCurveTest()
        {
            CreateATMRateVolCurve("AUD", "3M", IrVolInstruments, IrVolValues, IrStrikes);
            var curve = (CapVolatilityCurve)CurveEngine.LoadVolatilityCurve(Market, "AUD", "3M", null);
            Assert.IsNotNull(curve);
            // 3M does exist
            Assert.AreEqual("3M", curve.GetPricingStructureId().Properties.GetString("IndexTenor", true));
            //var baseDate = curve.GetBaseDate();
            //var expiration = VolatilitySurfaceHelper.GetDimensionValues(curve.GetVolatilityMatrix().dataPoints?.point,
            //CubeDimension.Expiration);
            //var expiryArray = new List<TimeDimension>();
            //var valArray = new List<Decimal>();
            //Debug.Print(baseDate.ToString(CultureInfo.InvariantCulture));
            //foreach (var expiry in expiration)
            //{
            //    //Temporary
            //    if (expiry is TimeDimension timeDimension)
            //    {
            //        expiryArray.Add(timeDimension);
            //    }
            //    //var date = expiry as DateTime? ?? baseDate.AddDays(1);
            //    //expiryArray.Add(date);
            //    Debug.Print(expiry.ToString());
            //}         
            ////var term = VolatilitySurfaceHelper.GetDimensionValues(curve.GetVolatilityMatrix().dataPoints?.point,
            ////CubeDimension.Strike);
            //var termPoints = TermPointsFactory.Create(expiryArray, valArray);
            //var termCurve = new TermCurve { point = termPoints };
            //var output = XmlSerializerHelper.SerializeToString(termCurve);
            //Debug.Print(output);
        }

        [TestMethod]
        public void LoadInterestRateVolCurveWithStrikeTest()
        {
            CreateStrikeRateVolCurve("AUD", "3M", IrVolInstruments2, IrVolValues2, IrStrikes2);
            var curve = CurveEngine.LoadVolatilityCurve(Market, "AUD", "3M", null);
            Assert.IsNotNull(curve);
            // 3M does exist
            Assert.AreEqual("3M", curve.GetPricingStructureId().Properties.GetString("IndexTenor", true));
        }

        #endregion
    }
}
