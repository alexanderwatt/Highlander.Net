#region Using

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.UnitTestEnv;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using FpML.V5r10.Reporting;
using Orion.Constants;
using FpML.V5r10.Reporting.Identifiers;
using Orion.Analytics.Helpers;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Helpers;
using Orion.CalendarEngine.Schedulers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.CurveEngine.Markets;
using Orion.CurveEngine.PricingStructures;
using Orion.ValuationEngine.Generators;
using Orion.ValuationEngine.Valuations;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Instruments;
using Orion.ValuationEngine.Pricers.Products;
using Orion.ValuationEngine.Tests.Helpers;
using XsdClassesFieldResolver = FpML.V5r10.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Tests 
{
    [TestClass]
    public class ValuationEngineTests1
    {
        #region Properties

        //private static ILogger Logger_obs { get; set; }
        private static UnitTestEnvironment UTE { get; set; }
        private static CurveEngine.CurveEngine CurveEngine { get; set; }
        private static CalendarEngine.CalendarEngine CalendarEngine { get; set; }
        //private static TimeSpan Retention { get; set; }
        private static IBusinessCalendar FixingCalendar { get; set; }
        private static IBusinessCalendar PaymentCalendar { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
            //Set the calendar engine
            CurveEngine = new CurveEngine.CurveEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
            CalendarEngine = new CalendarEngine.CalendarEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
            // Set the Retention
            //Retention = TimeSpan.FromHours(1);
            const string paymentCenter = "AUSY";
            const string fixingCenter = "AUSY-GBLO";
            FixingCalendar = BusinessCenterHelper.ToBusinessCalendar(UTE.Cache, BusinessCentersHelper.Parse(fixingCenter), UTE.NameSpace);
            PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(UTE.Cache, BusinessCentersHelper.Parse(paymentCenter), UTE.NameSpace);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
            UTE.Dispose();
        }

        #endregion

        #region Schedulers

        [TestMethod]
        public void TestGetNotionalScheduleImpl()
        {

            var inputAndOutput = new Dictionary<double[], List<double>>
                {
                    {
                        new[] {50000000.0, -5000000.0, 1.0, 12.0}, new List<double>(new[]
                            {
                                50000000.0,
                                50000000.0 + (-5000000.0)*1,
                                50000000.0 + (-5000000.0)*2,
                                50000000.0 + (-5000000.0)*3,
                                50000000.0 + (-5000000.0)*4,
                                50000000.0 + (-5000000.0)*5,
                                50000000.0 + (-5000000.0)*6,
                                50000000.0 + (-5000000.0)*7,
                                50000000.0 + (-5000000.0)*8,
                                50000000.0 + (-5000000.0)*9,
                                50000000.0 + (-5000000.0)*10,
                                50000000.0 + (-5000000.0)*11
                            })
                    },
                    {
                        new[] {50000000.0, +10000000.0, 2.0, 8.0}, new List<double>(new[]
                            {
                                50000000.0,
                                50000000.0,
                                50000000.0 + (10000000.0)*1,
                                50000000.0 + (10000000.0)*1,
                                50000000.0 + (10000000.0)*2,
                                50000000.0 + (10000000.0)*2,
                                50000000.0 + (10000000.0)*3,
                                50000000.0 + (10000000.0)*3
                            })
                    },
                    {
                        new[] {50000000.0, +10000000.0, 3.0, 8.0}, new List<double>(new[]
                            {
                                50000000.0,
                                50000000.0,
                                50000000.0,
                                50000000.0 + (10000000.0)*1,
                                50000000.0 + (10000000.0)*1,
                                50000000.0 + (10000000.0)*1,
                                50000000.0 + (10000000.0)*2,
                                50000000.0 + (10000000.0)*2
                            })
                    }
                };


            foreach (double[] input in inputAndOutput.Keys)
            {
                double initialValue = input[0];
                double step = input[1];
                double applyStepToEachNthCashflow = input[2];
                double totalNumberOfCashflows = input[3];

                List<double> notionalSchedule = NotionalScheduleGenerator.GetNotionalScheduleImpl(initialValue, step, (int)applyStepToEachNthCashflow, (int)totalNumberOfCashflows);
                Assert.AreEqual(totalNumberOfCashflows, notionalSchedule.Count);

                List<double> expectedOutput = inputAndOutput[input];
                Assert.AreEqual(expectedOutput.Count, notionalSchedule.Count);

                for (int i = 0; i < expectedOutput.Count; ++i)
                {
                    Assert.AreEqual(expectedOutput[i], notionalSchedule[i]);
                }
            }
        }

        [TestMethod]
        public void TestGetSpreadScheduleImpl()
        {

            var inputAndOutput = new Dictionary<double[], List<double>>
                {
                    {
                        new[] {0.05, 0.0025, 1.0, 12.0}, new List<double>(new[]
                            {
                                0.05,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*2,
                                0.05 + (0.0025)*3,
                                0.05 + (0.0025)*4,
                                0.05 + (0.0025)*5,
                                0.05 + (0.0025)*6,
                                0.05 + (0.0025)*7,
                                0.05 + (0.0025)*8,
                                0.05 + (0.0025)*9,
                                0.05 + (0.0025)*10,
                                0.05 + (0.0025)*11
                            })
                    },
                    {
                        new[] {0.05, +0.0025, 2.0, 8.0}, new List<double>(new[]
                            {
                                0.05,
                                0.05,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*2,
                                0.05 + (0.0025)*2,
                                0.05 + (0.0025)*3,
                                0.05 + (0.0025)*3
                            })
                    },
                    {
                        new[] {0.05, +0.0025, 3.0, 8.0}, new List<double>(new[]
                            {
                                0.05,
                                0.05,
                                0.05,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*2,
                                0.05 + (0.0025)*2
                            })
                    }
                };


            foreach (double[] input in inputAndOutput.Keys)
            {
                double initialValue = input[0];
                double step = input[1];
                double applyStepToEachNthCashflow = input[2];
                double totalNumberOfCashflows = input[3];

                List<double> notionalSchedule = StrikeScheduleGenerator.GetStrikeScheduleImpl(initialValue, step, (int)applyStepToEachNthCashflow, (int)totalNumberOfCashflows);
                Assert.AreEqual(totalNumberOfCashflows, notionalSchedule.Count);

                List<double> expectedOutput = inputAndOutput[input];
                Assert.AreEqual(expectedOutput.Count, notionalSchedule.Count);

                for (int i = 0; i < expectedOutput.Count; ++i)
                {
                    Assert.AreEqual(expectedOutput[i], notionalSchedule[i], 0.0000001);
                }
            }
        }

        [TestMethod]
        public void TestGetStrikeScheduleImpl()
        {

            var inputAndOutput = new Dictionary<double[], List<double>>
                {
                    {
                        new[] {0.05, 0.0025, 1.0, 12.0}, new List<double>(new[]
                            {
                                0.05,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*2,
                                0.05 + (0.0025)*3,
                                0.05 + (0.0025)*4,
                                0.05 + (0.0025)*5,
                                0.05 + (0.0025)*6,
                                0.05 + (0.0025)*7,
                                0.05 + (0.0025)*8,
                                0.05 + (0.0025)*9,
                                0.05 + (0.0025)*10,
                                0.05 + (0.0025)*11
                            })
                    },
                    {
                        new[] {0.05, +0.0025, 2.0, 8.0}, new List<double>(new[]
                            {
                                0.05,
                                0.05,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*2,
                                0.05 + (0.0025)*2,
                                0.05 + (0.0025)*3,
                                0.05 + (0.0025)*3
                            })
                    },
                    {
                        new[] {0.05, +0.0025, 3.0, 8.0}, new List<double>(new[]
                            {
                                0.05,
                                0.05,
                                0.05,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*1,
                                0.05 + (0.0025)*2,
                                0.05 + (0.0025)*2
                            })
                    }
                };


            foreach (double[] input in inputAndOutput.Keys)
            {
                double initialValue = input[0];
                double step = input[1];
                double applyStepToEachNthCashflow = input[2];
                double totalNumberOfCashflows = input[3];

                List<double> notionalSchedule = StrikeScheduleGenerator.GetStrikeScheduleImpl(initialValue, step, (int)applyStepToEachNthCashflow, (int)totalNumberOfCashflows);
                Assert.AreEqual(totalNumberOfCashflows, notionalSchedule.Count);

                List<double> expectedOutput = inputAndOutput[input];
                Assert.AreEqual(expectedOutput.Count, notionalSchedule.Count);

                for (int i = 0; i < expectedOutput.Count; ++i)
                {
                    Assert.AreEqual(expectedOutput[i], notionalSchedule[i], 0.0000001);
                }
            }
        }

        #endregion

        #region Test Coupons and FxLegs

        #region Data

        const string CDayCountFraction = "ACT/365.FIXED";
        private readonly DateTime[] _dates = new[] { new DateTime(2009, 6, 15), new DateTime(2009, 7, 15), new DateTime(2009, 8, 15)};
        private readonly DateTime _baseDate = new DateTime(2009, 5, 20);
        private readonly string[] _metrics = GetCouponMetrics();
        private readonly static DateTime BaseDate = new DateTime(2008, 2, 20);

        #endregion

        #region Methods

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private FxSingleLegPricer CreateFxLeg(string party1, FxLegParametersRange legParametersRange)
        {
            var quoteBasis = EnumHelper.Parse<QuoteBasisEnum>(legParametersRange.QuoteBasis);
            var leg = FxSingleLeg.CreateSpot(party1, "party2", legParametersRange.ExchangeCurrency1Amount, legParametersRange.Currency1,
                legParametersRange.Currency2, quoteBasis, legParametersRange.ExchangeDate, legParametersRange.FxRate);
            var controllers = new FxSingleLegPricer(leg, party1, ProductTypeSimpleEnum.FxForward);
            return controllers;
        }

        public static string[] GetCouponMetrics()
        {
            var metrics = Enum.GetNames(typeof(InstrumentMetrics));
            var result = new List<string>(metrics) { "BreakEvenRate" };
            return result.ToArray();
        }

        public FxLegParametersRange GetFxLegs()
        {
            return new FxLegParametersRange
                                      {
                                          ExchangeCurrency1Amount = 1000000.0m,
                                            ExchangeCurrency2Amount = 1480000.0m,
                                            ExchangeDate = BaseDate.AddDays(2),
                                            Currency1 = "GBP",
                                            Currency2 = "USD",
                                            QuoteBasis = "Currency2PerCurrency1",
                                            FxRate = 1.48m,
                                            Currency1PayParty = "party1",
                                            Currency2PayParty = "party2"
                                      };
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceableRateCoupon> CreateFloatingRateCoupon(string id, DateTime baseDate, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceableRateCoupon>();
            foreach (var date in dates)
            {
                var money = MoneyHelper.GetAmount(10000000.0m, "AUD");
                var dt = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var priceableAsset =
                    new PriceableFloatingRateCoupon
                        (id + date.ToShortDateString()
                         , false
                         , baseDate
                         , date
                         , false
                         , BusinessCentersHelper.Parse("AUSY")
                         , BusinessDayConventionEnum.FOLLOWING
                         , DayCountFractionHelper.Parse(CDayCountFraction)
                         , ResetRelativeToEnum.CalculationPeriodStartDate
                         , RelativeDateOffsetHelper.Create("2D", DayTypeEnum.Business, BusinessDayConventionEnum.FOLLOWING.ToString(), "AUSY", "CaclulationPeriodStartDate")
                         , 0.0m
                         , 0.07m
                         , money
                         , dt
                         , ForecastRateIndexHelper.Parse("AUD-BBA-BBSW", "3M")
                         , null
                         , null
                         , null
                         , FixingCalendar
                         , PaymentCalendar);
                //var priceableAsset = InterestRateCouponFactory.CreateFixedCoupon(id + date.ToShortDateString(), id, 10000000.0m, "AUD",
                //    baseDate, date, false, date, new[] { "AUSY" }, BusinessDayConventionEnum.FOLLOWING, cDayCountFraction, DiscountingTypeEnum.Standard, 0.07m);
                controllers.Add(priceableAsset);
            }
            return controllers;
        }


        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceableRateCoupon> CreateFloatingRateCoupon2(string id, DateTime baseDate, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceableRateCoupon>();
            foreach (var date in dates)
            {
                var money = MoneyHelper.GetAmount(10000000.0m, "AUD");
                var dt = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var priceableAsset =
                    new PriceableFloatingRateCoupon
                        (id + date.ToShortDateString()
                        , false
                         , baseDate
                         , date
                         , false
                         , BusinessCentersHelper.Parse("AUSY")
                         , BusinessDayConventionEnum.FOLLOWING
                         , DayCountFractionHelper.Parse(CDayCountFraction)
                         , ResetRelativeToEnum.CalculationPeriodStartDate
                         , RelativeDateOffsetHelper.Create("2D", DayTypeEnum.Business, BusinessDayConventionEnum.FOLLOWING.ToString(), "AUSY", "CaclulationPeriodStartDate")
                         , 0.0m
                         , 0.07m
                         , money
                         , dt
                         , ForecastRateIndexHelper.Parse("AUD-BBA-BBSW", "3M")
                         , DiscountingTypeEnum.Standard
                         , 0.07m
                         , null
                         , FixingCalendar
                         , PaymentCalendar);
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceableRateCoupon> CreateFloatingRateCoupon3(string id, DateTime baseDate, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceableRateCoupon>();
            foreach (var date in dates)
            {
                var money = MoneyHelper.GetAmount(10000000.0m, "AUD");
                var dt = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var priceableAsset =
                    new PriceableFloatingRateCoupon
                        (id + date.ToShortDateString()
                        , false
                         , baseDate
                         , date
                         , false
                         , BusinessCentersHelper.Parse("AUSY")
                         , BusinessDayConventionEnum.FOLLOWING
                         , DayCountFractionHelper.Parse(CDayCountFraction)
                         , ResetRelativeToEnum.CalculationPeriodStartDate
                         , RelativeDateOffsetHelper.Create("2D", DayTypeEnum.Business, BusinessDayConventionEnum.FOLLOWING.ToString(), "AUSY", "CaclulationPeriodStartDate")
                         , 0.0m
                         , 0.07m
                         , money
                         , dt
                         , ForecastRateIndexHelper.Parse("AUD-BBA-BBSW", "3M")
                         , DiscountingTypeEnum.Standard
                         , 0.07m
                         , null
                         , FixingCalendar
                         , PaymentCalendar)
                    {
                        PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward
                    };
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceableRateCoupon> CreateFixedRateCoupon(string id, DateTime baseDate, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceableRateCoupon>();
            foreach (var date in dates)
            {
                var money = MoneyHelper.GetAmount(10000000.0m, "AUD");
                var bc = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.FOLLOWING,
                                                             BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var dt = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var calc = CalculationFactory.CreateFixed(0.07m, money, DayCountFractionHelper.Parse(CDayCountFraction),
                                                          DiscountingTypeEnum.Standard);
                var priceableAsset =
                    new PriceableFixedRateCoupon
                        (id + date.ToShortDateString()
                        , false
                         , baseDate
                         , date
                         , false
                         , bc
                         , dt
                         , calc
                         , money
                         , PaymentCalendar);
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceableRateCoupon> CreateFixedRateCoupon2(string id, DateTime baseDate, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceableRateCoupon>();
            foreach (var date in dates)
            {
                var money = MoneyHelper.GetAmount(10000000.0m, "AUD");
                var bc = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.FOLLOWING,
                                                             BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var dt = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var calc = CalculationFactory.CreateFixed(0.07m, money, DayCountFractionHelper.Parse(CDayCountFraction),
                                                          DiscountingTypeEnum.Standard);
                var priceableAsset =
                    new PriceableFixedRateCoupon
                        (id + date.ToShortDateString()
                        , false
                         , baseDate
                         , date
                         , false
                         , bc
                         , dt
                         , calc
                         , money
                         , PaymentCalendar);
                //var priceableAsset = InterestRateCouponFactory.CreateFixedCoupon(id + date.ToShortDateString(), id, 10000000.0m, "AUD",
                //    baseDate, date, false, date, new[] { "AUSY" }, BusinessDayConventionEnum.FOLLOWING, cDayCountFraction, DiscountingTypeEnum.Standard, 0.07m);
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceableRateCoupon> CreateFixedRateCoupon3(string id, DateTime baseDate, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceableRateCoupon>();
            foreach (var date in dates)
            {
                var money = MoneyHelper.GetAmount(10000000.0m, "AUD");
                var bc = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.FOLLOWING,
                                                             BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var dt = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var calc = CalculationFactory.CreateFixed(0.07m, money, DayCountFractionHelper.Parse(CDayCountFraction),
                                                          DiscountingTypeEnum.Standard);
                var priceableAsset =
                    new PriceableFixedRateCoupon
                        (id + date.ToShortDateString()
                        , false
                         , baseDate
                         , date
                         , false
                         , bc
                         , dt
                         , calc
                         , money
                         , PaymentCalendar)
                    {
                        PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward
                    };
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceableRateCoupon> CreateFixedRateCoupon(string id, DateTime baseDate, IEnumerable<DateTime> dates, decimal rate)
        {
            var controllers = new List<PriceableRateCoupon>();
            foreach (var date in dates)
            {
                var money = MoneyHelper.GetAmount(10000000.0m, "AUD");
                var dt = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var priceableAsset =
                    new PriceableFixedRateCoupon
                        (id + date.ToShortDateString()
                        , false
                         , baseDate
                         , date
                         , false
                         , BusinessCentersHelper.Parse("AUSY")
                         , BusinessDayConventionEnum.FOLLOWING
                         , DayCountFractionHelper.Parse(CDayCountFraction)
                         , rate
                         , money
                         , dt
                         , null
                         , null
                         , null
                         , PaymentCalendar);
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        static public IAssetControllerData CreateModelData(string[] metrics, DateTime baseDate, IMarketEnvironment market)
        {
            var bav = new BasicAssetValuation();
            var quotes = new BasicQuotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                quotes[index] = BasicQuotationHelper.Create(0.0m, metric);
                index++;
            }
            bav.quote = quotes;
            return new AssetControllerData(bav, baseDate, market);
        }

        static internal IInstrumentControllerData CreateInstrumentModelData(string[] metrics, DateTime baseDate, IMarketEnvironment market)
        {
            var bav = new AssetValuation();
            var quotes = new Quotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                quotes[index] = QuotationHelper.Create(0.0m, metric);
                index++;
            }
            bav.quote = quotes;
            return new InstrumentControllerData(bav, market, baseDate);
        }

        static internal IInstrumentControllerData CreateInstrumentModelData(string[] metrics, DateTime baseDate, IMarketEnvironment market, string reportingCurrency)
        {
            var bav = new AssetValuation();
            var curreny = CurrencyHelper.Parse(reportingCurrency);
            var quotes = new Quotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                quotes[index] = QuotationHelper.Create(0.0m, metric, "DecimalValue");
                index++;
            }
            bav.quote = quotes;
            return new InstrumentControllerData(bav, market, baseDate, curreny);
        }

        static internal IInstrumentControllerData CreateInstrumentModelData(string[] metrics, DateTime baseDate, IMarketEnvironment market, string reportingCurrency, string baseParty)
        {
            var bav = new AssetValuation();
            var curreny = CurrencyHelper.Parse(reportingCurrency);
            var quotes = new Quotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                quotes[index] = QuotationHelper.Create(0.0m, metric, "DecimalValue");
                index++;
            }
            bav.quote = quotes;
            return new InstrumentControllerData(bav, market, baseDate, curreny, new PartyIdentifier(baseParty));
        }

        public static IFxCurve TestFxCurve(DateTime baseDate)
        {
            const string curveName = "AUD-USD";
            const string algorithm = "LinearForward";
            var fxProperties = new NamedValueSet();
            fxProperties.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties.Set("BuildDateTime", baseDate);
            fxProperties.Set(CurveProp.BaseDate, baseDate);
            fxProperties.Set(CurveProp.Market, "LIVE");
            fxProperties.Set("Identifier", "FxCurve.AUD-USD");
            fxProperties.Set(CurveProp.Currency1, "AUD");
            fxProperties.Set(CurveProp.CurrencyPair, curveName);
            fxProperties.Set(CurveProp.CurveName, curveName);
            fxProperties.Set("Algorithm", algorithm);
            fxProperties.Set(CurveProp.OptimizeBuild, false);
            string[] instruments =  
                {   "AUDUSD-FxForward-0D", "AUDUSD-FxSpot-SP", "AUDUSD-FxForward-1M", "AUDUSD-FxForward-2M", "AUDUSD-FxForward-3M"
                };
            decimal[] rates = { 0.90m, 0.90m, 0.90m, 0.90m, 0.90m };
            var curve = CurveEngine.CreateCurve(fxProperties, instruments, rates, null, null, null) as IFxCurve;
            return curve;
        }

        public static IFxCurve TestFxCurve(DateTime baseDate, string currency1, string currency2)
        {
            string curveName = currency1 +"-" + currency2;
            string currencyPair = currency1 + currency2;
            const string algorithm = "LinearForward";
            var fxProperties = new NamedValueSet();
            fxProperties.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties.Set("BuildDateTime", baseDate);
            fxProperties.Set(CurveProp.BaseDate, baseDate);
            fxProperties.Set(CurveProp.Market, "LIVE");
            fxProperties.Set("Identifier", "FxCurve." + curveName);
            fxProperties.Set(CurveProp.Currency1, currency1);
            fxProperties.Set(CurveProp.Currency2, currency2);
            fxProperties.Set(CurveProp.CurrencyPair, curveName);
            fxProperties.Set(CurveProp.CurveName, curveName);
            fxProperties.Set("Algorithm", algorithm);
            fxProperties.Set(CurveProp.OptimizeBuild, false);
            string[] instruments =
            {   currencyPair +"-FxForward-0D", currencyPair +"-FxSpot-SP", currencyPair +"-FxForward-1M", currencyPair +"-FxForward-2M", currencyPair +"-FxForward-3M"
            };
            decimal[] rates = { 0.90m, 0.90m, 0.90m, 0.90m, 0.90m };
            var curve = CurveEngine.CreateCurve(fxProperties, instruments, rates, null, null, null) as IFxCurve;
            return curve;
        }

        public static IRateCurve TestRateCurve(DateTime baseDate)
        {
            const string curveName = "AUD-LIBOR-BBA-3M";
            const string indexTenor = "3M";
            const string algorithm = "FastLinearZero";
            const string marketEnvironment = "Bob";
            const double tolerance = 0.00000001;
            var props = new object[11, 2];
            props[0, 0] = CurveProp.CurveName;
            props[0, 1] = curveName;
            props[1, 0] = "Algorithm";
            props[1, 1] = algorithm;
            props[2, 0] = CurveProp.PricingStructureType;
            props[2, 1] = "RateCurve";
            props[3, 0] = "BuildDateTime";
            props[3, 1] = baseDate;
            props[4, 0] = CurveProp.IndexName;
            props[4, 1] = "AUD-LIBOR-BBA";
            props[5, 0] = CurveProp.IndexTenor;
            props[5, 1] = indexTenor;
            props[6, 0] = "Identifier";
            props[6, 1] = "Alex";
            props[7, 0] = CurveProp.Market;
            props[7, 1] = marketEnvironment;
            props[8, 0] = "BaseDate";
            props[8, 1] = baseDate;
            props[9, 0] = "Tolerance";
            props[9, 1] = tolerance;
            props[10, 0] = CurveProp.OptimizeBuild;
            props[10, 1] = false;
            var namevalues = new NamedValueSet(props);
            string[] instruments =  
                {   "AUD-Deposit-1D", "AUD-Deposit-1M", "AUD-Deposit-2M", "AUD-Deposit-3M",
                    "AUD-IRFuture-IR-0", "AUD-IRFuture-IR-1", "AUD-IRFuture-IR-2", "AUD-IRFuture-IR-3", 
                    "AUD-IRFuture-IR-4", "AUD-IRFuture-IR-5", "AUD-IRFuture-IR-6", "AUD-IRFuture-IR-7",
                    "AUD-IRSwap-3Y", "AUD-IRSwap-4Y", "AUD-IRSwap-5Y", "AUD-IRSwap-6Y", 
                    "AUD-IRSwap-7Y", "AUD-IRSwap-8Y", "AUD-IRSwap-9Y", "AUD-IRSwap-10Y", 
                    "AUD-IRSwap-12Y", "AUD-IRSwap-15Y", "AUD-IRSwap-20Y", "AUD-IRSwap-25Y", "AUD-IRSwap-30Y"
                };
            decimal[] rates =      {0.0725m,    0.0755m,    0.0766m,    0.07755m, 
                                    0.0781m,    0.07865m,   0.0794m,    0.07862m, 
                                    0.07808m,   0.07745m,   0.07752m,   0.0764m,
                                    0.06915m,   0.06745m,   0.06745m,   0.0785m,
                                    0.0786m,    0.0795m,    0.0725m,    0.0785m,
                                    0.0785m,    0.0785m,    0.0786m,    0.0787m, 0.0788m
                                   };
            decimal[] additional = {      
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m
                                   };
            var curve = CurveEngine.CreateCurve(namevalues, instruments, rates, additional, null, null);
            return curve as IRateCurve;
        }

        public string BuildAndCacheRateCurve(DateTime baseDate)
        {
            var curve = TestRateCurve(baseDate);
            string curveId = curve.GetPricingStructureId().UniqueIdentifier;
            CurveEngine.SaveCurve(curve);
            return curveId;
        }

        static public ISwapLegEnvironment CreateInterestRateStreamEnvironment(DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new SwapLegEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            var curve = new SimpleDiscountFactorCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure("DiscountCurve", curve);
            market.AddPricingStructure("ForecastCurve", curve);
            return market;
        }

        static public IMarketEnvironment CreateFxLegTestEnvironment(DateTime baseDate, string reportingCurrency)
        {
            string currency1 = "GBP";
            string currency2 = "USD";
            var marketEnv = new MarketEnvironment("temp");
            var discurve1 = TestRateCurve(baseDate);
            var discurve1Name = CurveNameHelpers.GetDiscountCurveName(currency1, true);
            var discurve2Name = CurveNameHelpers.GetDiscountCurveName(currency2, true);
            marketEnv.AddPricingStructure(discurve1Name, discurve1);
            marketEnv.AddPricingStructure(discurve2Name, discurve1);
            if (reportingCurrency == "USD")
            {
                var fx1Curve = TestFxCurve(baseDate, currency1, reportingCurrency);
                var fx3Curve = TestFxCurve(baseDate, currency1, currency2);
                marketEnv.AddPricingStructure("FxCurve." + currency1 + "-" + reportingCurrency, fx1Curve);
                marketEnv.AddPricingStructure("FxCurve." + currency1 + "-" + currency2, fx3Curve);
            }
            else
            {
                var fx1Curve = TestFxCurve(baseDate, currency1, reportingCurrency);
                var fx2Curve = TestFxCurve(baseDate, currency2, reportingCurrency);
                var fx3Curve = TestFxCurve(baseDate, currency1, currency2);
                marketEnv.AddPricingStructure("FxCurve." + currency1 + "-" + reportingCurrency, fx1Curve);
                marketEnv.AddPricingStructure("FxCurve." + currency2 + "-" + reportingCurrency, fx2Curve);
                marketEnv.AddPricingStructure("FxCurve." + currency1 + "-" + currency2, fx3Curve);
            }
            return marketEnv;
        }

        static public ISwapLegEnvironment CreateInterestRateStreamTestEnvironment(DateTime baseDate)
        {
            var market = new SwapLegEnvironment();
            var curve = TestRateCurve(baseDate);
            var fxcurve = TestFxCurve(baseDate);
            market.AddPricingStructure("DiscountCurve", curve);
            market.AddPricingStructure("ForecastCurve", curve);
            market.AddPricingStructure("ReportingCurrencyFxCurve", fxcurve);
            return market;
        }

        static public ISimpleRateMarketEnvironment CreateSimpleRateMarketEnvironment(DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new SimpleRateMarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            var curve = new SimpleDiscountFactorCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure("DiscountCurve", curve);
            market.PricingStructureIdentifier = "DiscountCurve";
            return market;
        }

        static public void ProcessInstrumentControllerResults(InstrumentControllerBase instrumentController, string[] metrics, DateTime baseDate)
        {
            Assert.IsNotNull(instrumentController);           
            ISwapLegEnvironment market = CreateInterestRateStreamTestEnvironment(baseDate);
            IInstrumentControllerData controllerData = CreateInstrumentModelData(metrics, baseDate, market);
            Assert.IsNotNull(controllerData);
            var results = instrumentController.Calculate(controllerData);
            Debug.Print("Id : {0}", instrumentController.Id);
            foreach (var metric in results.quote)
            {
                Debug.Print("Id : {0} Metric Name : {1} Metric Value : {2}", instrumentController.Id, metric.measureType.Value, metric.value);
            }
        }

        static public void ProcessInstrumentControllerResultsWithCurrency(InstrumentControllerBase instrumentController, string[] metrics, DateTime baseDate, string currency)
        {
            Assert.IsNotNull(instrumentController);
            ISwapLegEnvironment market = CreateInterestRateStreamTestEnvironment(baseDate);
            IInstrumentControllerData controllerData = CreateInstrumentModelData(metrics, baseDate, market, currency);
            Assert.IsNotNull(controllerData);
            var results = instrumentController.Calculate(controllerData);
            Debug.Print("Id : {0}", instrumentController.Id);
            foreach (var metric in results.quote)
            {
                Debug.Print("Id : {0} Metric Name : {1} Metric Value : {2}", instrumentController.Id, metric.measureType.Value, metric.value);
            }
        }

        static public void ProcessInstrumentControllerResultsEvolve(InstrumentControllerBase instrumentController, string[] metrics, DateTime baseDate, DateTime valuationDate)
        {
            Assert.IsNotNull(instrumentController);
            ISwapLegEnvironment market = CreateInterestRateStreamTestEnvironment(baseDate);
            IInstrumentControllerData controllerData = CreateInstrumentModelData(metrics, valuationDate, market);
            Assert.IsNotNull(controllerData);
            var results = instrumentController.Calculate(controllerData);
            Debug.Print("Id : {0}", instrumentController.Id);
            foreach (var metric in results.quote)
            {
                Debug.Print("Id : {0} Metric Name : {1} Metric Value : {2}", instrumentController.Id, metric.measureType.Value, metric.value);
            }
        }

        static public void ProcessFxLegResultsWithCurrency(InstrumentControllerBase instrumentController,
            string[] metrics, DateTime baseDate, string currency)
        {
            Assert.IsNotNull(instrumentController);
            IMarketEnvironment market = CreateFxLegTestEnvironment(baseDate, currency);
            IInstrumentControllerData controllerData = CreateInstrumentModelData(metrics, baseDate, market, currency);
            Assert.IsNotNull(controllerData);
            var results = instrumentController.Calculate(controllerData);
            Debug.Print("Id : {0}", instrumentController.Id);
            foreach (var metric in results.quote)
            {
                Debug.Print("Id : {0} Metric Name : {1} Metric Value : {2}", instrumentController.Id, metric.measureType.Value, metric.value);
            }
        }

        #endregion

        #region Tests

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void FxLegPricerWithReportingCurrency()
        {
            var priceableController = CreateFxLeg("party1", GetFxLegs());
            ProcessFxLegResultsWithCurrency(priceableController,
                _metrics, BaseDate, "AUD");
        }

        [TestMethod]
        public void FxLegPricerWithReportingCurrencyUSD()
        {
            var priceableControllers = CreateFxLeg("party1", GetFxLegs());
            ProcessFxLegResultsWithCurrency(priceableControllers, _metrics, BaseDate, "USD");
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFixedRateCouponWithReportingCurrency()
        {
            var priceableControllers = CreateFixedRateCoupon("FixedRateCoupon", _baseDate, _dates);
            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
                var coupon = priceableController.Build();
                Debug.Print(XmlSerializerHelper.SerializeToString<PaymentBase>(coupon));
            }
        }

        [TestMethod]
        public void PriceableFixedRateCouponWithReportingCurrencyUSD()
        {
            var priceableControllers = CreateFixedRateCoupon("FixedRateCoupon", _baseDate, _dates);
            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "USD");
                var coupon = priceableController.Build();
                Debug.Print(XmlSerializerHelper.SerializeToString<PaymentBase>(coupon));
            }
        }

        [TestMethod]
        public void PriceableFixedRateCoupon1()
        {
            var priceableControllers = CreateFixedRateCoupon("FixedRateCoupon", _baseDate, _dates);

            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                var coupon = priceableController.Build();
                Debug.Print(XmlSerializerHelper.SerializeToString<PaymentBase>(coupon));
            }
        }

        [TestMethod]
        public void PriceableFixedRateCoupon2()
        {
            var priceableControllers = CreateFixedRateCoupon("FixedRateCoupon", _baseDate, _dates, 0.07m);

            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                var coupon = priceableController.Build();
                Debug.Print(XmlSerializerHelper.SerializeToString<PaymentBase>(coupon));
            }
        }

        [TestMethod]
        public void PriceableFixedRateCoupon3()
        {
            var priceableControllers = CreateFixedRateCoupon2("FixedRateCoupon", _baseDate, _dates);

            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                var coupon = priceableController.Build();
                Debug.Print(XmlSerializerHelper.SerializeToString<PaymentBase>(coupon));
            }
        }

        [TestMethod]
        public void PriceableFixedRateCouponEvolve()
        {
            var priceableControllers = CreateFixedRateCoupon3("FixedRateCoupon", _baseDate.AddDays(4), _dates);

            foreach (var priceableController in priceableControllers)
            {
                for (int i = 1; i < 10; i++)
                {
                    var date = _baseDate.AddDays(i);
                    //var metrics = new[] {"MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV"};
                    ProcessInstrumentControllerResultsEvolve(priceableController, _metrics, _baseDate, date);
                    var coupon = priceableController.Build();
                    Debug.Print(XmlSerializerHelper.SerializeToString<PaymentBase>(coupon));
                }
            }
        }

        [TestMethod]
        public void PriceableFloatingRateCoupon1()
        {
            var priceableControllers = CreateFloatingRateCoupon("FloatingRateCoupon", _baseDate, _dates);

            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                var coupon = priceableController.Build();
                Debug.Print(XmlSerializerHelper.SerializeToString<PaymentBase>(coupon));
            }
        }

        [TestMethod]
        public void PriceableFloatingRateCoupon2()
        {
            var priceableControllers = CreateFloatingRateCoupon2("FloatingRateCoupon", _baseDate, _dates);

            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                var coupon = priceableController.Build();
                Debug.Print(XmlSerializerHelper.SerializeToString<PaymentBase>(coupon));
            }
        }

        [TestMethod]
        public void PriceableFloatingRateCouponEvolve()
        {
            var priceableControllers = CreateFloatingRateCoupon3("FloatingRateCoupon", _baseDate.AddDays(4), _dates);
            //var metric = RateInstrumentResults.

            foreach (var priceableController in priceableControllers)
            {
                for (int i = 0; i < 10; i++)
                {
                    var date = _baseDate.AddDays(7*i);
                    //var metrics = new[] { 
                    //    "MarketQuote", "ImpliedQuote",
                    //    "ExpectedValue", "HistoricalValue",
                    //    "NPV", "FloatingNPV", "NFV",
                    //    "Delta1", "HistoricalDelta1",
                    //    "AccrualFactor", "HistoricalAccrualFactor", 
                    //    "Delta0", "HistoricalDelta0",
                    //    "DeltaR", "HistoricalDeltaR"};//BucketedDeltaVector, BucketedDeltaVector2, BucketedDelta1
                    ProcessInstrumentControllerResultsEvolve(priceableController, _metrics, _baseDate, date);
                    var coupon = priceableController.Build();
                    Debug.Print(XmlSerializerHelper.SerializeToString<PaymentBase>(coupon));
                }
            }
        }

        #endregion

        #endregion

        #region Test IRStreams

        #region Data

        private readonly SwapLegParametersRange_Old _legParametersRange= new SwapLegParametersRange_Old
                                      {
                                          LegType = LegType.Fixed,
                                          FirstRegularPeriodStartDate = BaseDate.AddMonths(6),
                                          MaturityDate = BaseDate.AddYears(5),
                                          NotionalAmount = 10000000.0m,
                                          Payer = "NAB",
                                          Receiver = "UBS",
                                          PaymentCalendar = "AUSY",
                                          PaymentFrequency = "6M",
                                          PaymentBusinessDayAdjustments = "FOLLOWING",
                                          EffectiveDate = BaseDate,
                                          AdjustedType = AdjustedType.Adjusted,
                                          Currency = "AUD",
                                          DayCount = CDayCountFraction,
                                          CouponOrLastResetRate = 0.07m,
                                          DiscountingType = "None",
                                          RollConvention = "20",
                                          InitialStubType = "ShortInitial",
                                          FinalStubType = "LongFinal",
                                          FixingCalendar = "AUSY",
                                          FixingBusinessDayAdjustments = "FOLLOWING"
                                      };
        private readonly SwapLegParametersRange_Old _legParametersRange2= new SwapLegParametersRange_Old
                                       {
                                           LegType = LegType.Floating,
                                           FirstRegularPeriodStartDate = BaseDate.AddMonths(6),
                                           MaturityDate = BaseDate.AddYears(5),
                                           NotionalAmount = 10000000.0m,
                                           Payer = "NAB",
                                           Receiver = "UBS",
                                           PaymentCalendar = "AUSY",
                                           PaymentFrequency = "6M",
                                           PaymentBusinessDayAdjustments = "FOLLOWING",
                                           EffectiveDate = BaseDate,
                                           AdjustedType = AdjustedType.Adjusted,
                                           Currency = "AUD",
                                           DayCount = CDayCountFraction,
                                           CouponOrLastResetRate = 0.07m,
                                           DiscountingType = "None",
                                           RollConvention = "20",
                                           InitialStubType = "ShortInitial",
                                           FinalStubType = "LongFinal",
                                           FixingCalendar = "AUSY",
                                           FixingBusinessDayAdjustments = "FOLLOWING"
                                       };

        #endregion

        #region Methods

        public static InterestRateStream GetCashflowsSchedule(SwapLegParametersRange_Old legParametersRange)
        {
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);
            stream.cashflows = cashflows;
            return stream;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFixedRateStream(string id, bool pay, SwapLegParametersRange_Old legParametersRange)
        {
            var dayCountFraction = DayCountFractionHelper.Parse(CDayCountFraction);
            var stream = GetCashflowsSchedule(legParametersRange);
            var calculation = CalculationFactory.CreateFixed(0.07m, MoneyHelper.GetAmount(10000000.0m, "AUD"), dayCountFraction, null);
            var principalExchanges = new PrincipalExchanges();
            var irstream = new InterestRateStream
            {
                principalExchanges = principalExchanges,
                calculationPeriodAmount = new CalculationPeriodAmount { Item = calculation },
                cashflows = stream.cashflows,
                id = id,
                payerPartyReference = new PartyReference { href = "NAB" },
                receiverPartyReference = new PartyReference { href = "UBS" }
            };
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, irstream, FixingCalendar, PaymentCalendar);
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFloatingRateStream(string id, bool pay, SwapLegParametersRange_Old legParametersRange)
        {
            var dayCountFraction = DayCountFractionHelper.Parse(CDayCountFraction);
            var stream = GetCashflowsSchedule(legParametersRange);
            var floatingRateIndex = FloatingRateIndexHelper.Parse("AUD-BBA-BBSW");
            var interval = PeriodHelper.Parse("6M");
            var calculation = CalculationFactory.CreateFloating(MoneyHelper.GetAmount(10000000.0m, "AUD"), floatingRateIndex, interval, dayCountFraction, null);
            var principalExchanges = new PrincipalExchanges();
            var irstream = new InterestRateStream
            {
                principalExchanges = principalExchanges,
                calculationPeriodAmount = new CalculationPeriodAmount { Item = calculation },
                cashflows = stream.cashflows,
                id = id,
                payerPartyReference = new PartyReference { href = "NAB" },
                receiverPartyReference = new PartyReference { href = "UBS" }
            };
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, irstream, FixingCalendar, PaymentCalendar);
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFixedRateStreamEx06(string id, bool pay)
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap06ExampleObject();
            swap.id = id;
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, swap.swapStream[1], FixingCalendar, PaymentCalendar);
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFloatingRateStreamEx06(string id, bool pay)
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap06ExampleObject();
            swap.id = id;
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, swap.swapStream[0], FixingCalendar, PaymentCalendar);
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFixedRateStreamEx01(string id, bool pay)
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            swap.id = id;
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, swap.swapStream[1], FixingCalendar, PaymentCalendar);
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFloatingRateStreamEx01(string id, bool pay)
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            swap.id = id;
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, swap.swapStream[0], FixingCalendar, PaymentCalendar);
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFixedRateStreamEx04(string id, bool pay)
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap04ExampleObject();
            swap.id = id;
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, swap.swapStream[1], FixingCalendar, PaymentCalendar);
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFloatingRateStreamEx04(string id, bool pay)
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap04ExampleObject();
            swap.id = id;
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, swap.swapStream[0], FixingCalendar, PaymentCalendar);
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFixedRateStreamEx05(string id, bool pay)
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap05ExampleObject();
            swap.id = id;
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, swap.swapStream[1], FixingCalendar, PaymentCalendar);
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private PriceableInterestRateStream CreateFloatingRateStreamEx05(string id, bool pay)
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap05ExampleObject();
            swap.id = id;
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var controllers = new PriceableInterestRateStream(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, pay, swap.swapStream[0], FixingCalendar, PaymentCalendar);
            return controllers;
        }
       
        #endregion

        #region Tests

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFixedRateStreamWithReportingCurrency()
        {
            var priceableController = CreateFixedRateStream("001", true, _legParametersRange);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
        }

        [TestMethod]
        public void PriceableFixedRateStreamWithReportingCurrencyUSD()
        {
            var priceableControllers = CreateFixedRateStream("001", true, _legParametersRange);
            ProcessInstrumentControllerResultsWithCurrency(priceableControllers, _metrics, _baseDate, "USD");
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFloatingRateStreamWithReportingCurrency()
        {
            var priceableController = CreateFloatingRateStream("001", true, _legParametersRange);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
        }

        [TestMethod]
        public void PriceableFloatingRateStreamWithReportingCurrencyUSD()
        {
            var priceableControllers = CreateFloatingRateStream("001", true, _legParametersRange);
            ProcessInstrumentControllerResultsWithCurrency(priceableControllers, _metrics, _baseDate, "USD");
        }

        [TestMethod]
        public void PriceableFixedRateStream1()
        {
            var priceableControllers = CreateFixedRateStream("001", true, _legParametersRange);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFixedRateStream2()
        {
            var priceableControllers = CreateFixedRateStream("001", false, _legParametersRange);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFixedRateStreamEvolve()
        {
            var priceableControllers = CreateFixedRateStream("001", false, _legParametersRange);
            priceableControllers.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;

            for (int i = 0; i < 6; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableControllers, _metrics, _baseDate, _baseDate.AddMonths(5 * i));
                foreach (var priceableController in priceableControllers.GetChildren())
                {
                    ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                }
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStream1()
        {
            var priceableControllers = CreateFloatingRateStream("001", true, _legParametersRange2);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStream2()
        {
            var priceableControllers = CreateFloatingRateStream("001", false, _legParametersRange2);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStreamEvolve()
        {
            var priceableControllers = CreateFloatingRateStream("001", false, _legParametersRange2);
            priceableControllers.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;
            for (int i = 0; i < 6; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableControllers, _metrics, _baseDate, _baseDate.AddMonths(5 * i));
                foreach (var priceableController in priceableControllers.GetChildren())
                {
                    ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                }
            }
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFixedRateCouponWithReportingCurrencyEx06()
        {
            var priceableController = CreateFixedRateStreamEx06("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
            var result = priceableController.Build();
            //var irstream = XmlSerializerHelper.SerializeToString<InterestRateStream>(result);
            Debug.Print(result.ToString());
        }

        [TestMethod]
        public void PriceableFixedRateCouponWithReportingCurrencyUSDEx06()
        {
            var priceableControllers = CreateFixedRateStreamEx06("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableControllers, _metrics, _baseDate, "USD");
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFloatingRateStreamWithReportingCurrencyEx06()
        {
            var priceableController = CreateFloatingRateStreamEx06("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");

        }

        [TestMethod]
        public void PriceableFloatingRateStreamWithReportingCurrencyUSDEx06()
        {
            var priceableControllers = CreateFloatingRateStreamEx06("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableControllers, _metrics, _baseDate, "USD");
        }

        [TestMethod]
        public void PriceableFixedRateStream1Ex06()
        {
            var priceableControllers = CreateFixedRateStreamEx06("001", true);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFixedRateStream2Ex06()
        {
            var priceableControllers = CreateFixedRateStreamEx06("001", false);

            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);

            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

            }

            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFixedRateStreamEvolveEx06()
        {
            var priceableControllers = CreateFixedRateStreamEx06("001", false);
            priceableControllers.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;

            for (int i = 0; i < 6; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableControllers, _metrics, _baseDate, _baseDate.AddMonths(5 * i));

                foreach (var priceableController in priceableControllers.GetChildren())
                {
                    ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

                }
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStream1Ex01()
        {
            var priceableControllers = CreateFloatingRateStreamEx01("001", true);

            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);

            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

            }

            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStream2Ex01()
        {
            var priceableControllers = CreateFloatingRateStreamEx01("001", false);

            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);

            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

            }

            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStreamEvolveEx01()
        {
            var priceableControllers = CreateFloatingRateStreamEx01("001", false);
            priceableControllers.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;

            for (int i = 0; i < 6; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableControllers, _metrics, _baseDate, _baseDate.AddMonths(5 * i));

                foreach (var priceableController in priceableControllers.GetChildren())
                {
                    ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

                }
            }
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFixedRateCouponWithReportingCurrencyEx01()
        {
            var priceableController = CreateFixedRateStreamEx01("001", true);

            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString<InterestRateStream>(irstream));
        }

        [TestMethod]
        public void PriceableFixedRateCouponWithReportingCurrencyUSDEx01()
        {
            var priceableControllers = CreateFixedRateStreamEx01("001", true);

            ProcessInstrumentControllerResultsWithCurrency(priceableControllers, _metrics, _baseDate, "USD");
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFloatingRateStreamWithReportingCurrencyEx01()
        {
            var priceableController = CreateFloatingRateStreamEx01("001", true);

            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString<InterestRateStream>(irstream));
        }

        [TestMethod]
        public void PriceableFloatingRateStreamWithReportingCurrencyUSDEx01()
        {
            var priceableController = CreateFloatingRateStreamEx01("001", true);

            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "USD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString<InterestRateStream>(irstream));
        }

        [TestMethod]
        public void PriceableFixedRateStream1Ex01()
        {
            var priceableControllers = CreateFixedRateStreamEx01("001", true);

            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);

            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }

            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }

        }

        [TestMethod]
        public void PriceableFixedRateStream2Ex01()
        {
            var priceableControllers = CreateFixedRateStreamEx01("001", false);

            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);

            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

            }

            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFixedRateStreamEvolveEx01()
        {
            var priceableControllers = CreateFixedRateStreamEx01("001", false);
            priceableControllers.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;

            for (int i = 0; i < 6; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableControllers, _metrics, _baseDate, _baseDate.AddMonths(5 * i));

                foreach (var priceableController in priceableControllers.GetChildren())
                {
                    ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

                }
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStream1Ex04()
        {
            var priceableControllers = CreateFloatingRateStreamEx04("001", true);//TODO

            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);

            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

            }

            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStream2Ex04()
        {
            var priceableControllers = CreateFloatingRateStreamEx04("001", false);

            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);

            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

            }

            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStreamEvolveEx04()
        {
            var priceableControllers = CreateFloatingRateStreamEx04("001", false);
            priceableControllers.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;

            for (int i = 0; i < 6; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableControllers, _metrics, _baseDate, _baseDate.AddMonths(5 * i));

                foreach (var priceableController in priceableControllers.GetChildren())
                {
                    ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);

                }
            }
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFixedRateCouponWithReportingCurrencyEx04()
        {
            var priceableController = CreateFixedRateStreamEx04("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString<InterestRateStream>(irstream));
        }

        [TestMethod]
        public void PriceableFixedRateCouponWithReportingCurrencyUSDEx04()
        {
            var priceableController = CreateFixedRateStreamEx04("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "USD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString<InterestRateStream>(irstream));
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFloatingRateStreamWithReportingCurrencyEx04()
        {
            var priceableController = CreateFloatingRateStreamEx04("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString<InterestRateStream>(irstream));
        }

        [TestMethod]
        public void PriceableFloatingRateStreamWithReportingCurrencyUSDEx04()
        {
            var priceableController = CreateFloatingRateStreamEx04("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "USD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString<InterestRateStream>(irstream));
        }

        [TestMethod]
        public void PriceableFixedRateStream1Ex04()
        {
            var priceableControllers = CreateFixedRateStreamEx04("001", true);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }

        }

        [TestMethod]
        public void PriceableFixedRateStream2Ex04()
        {
            var priceableControllers = CreateFixedRateStreamEx04("001", false);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFixedRateStreamEvolveEx04()
        {
            var priceableControllers = CreateFixedRateStreamEx04("001", false);
            priceableControllers.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;
            for (int i = 0; i < 6; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableControllers, _metrics, _baseDate, _baseDate.AddMonths(5 * i));
                foreach (var priceableController in priceableControllers.GetChildren())
                {
                    ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                }
            }
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFixedRateCouponWithReportingCurrencyEx05()
        {
            var priceableController = CreateFixedRateStreamEx05("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString(irstream));
        }

        [TestMethod]
        public void PriceableFixedRateCouponWithReportingCurrencyUSDEx05()
        {
            var priceableController = CreateFixedRateStreamEx05("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "USD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString(irstream));
        }

        [TestMethod]
        //TODO need to add the vector of dates for bucketting risk.
        public void PriceableFloatingRateStreamWithReportingCurrencyEx05()
        {
            var priceableController = CreateFloatingRateStreamEx05("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "AUD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString(irstream));
        }

        [TestMethod]
        public void PriceableFloatingRateStreamWithReportingCurrencyUSDEx05()
        {
            var priceableController = CreateFloatingRateStreamEx05("001", true);
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseDate, "USD");
            var irstream = priceableController.Build();
            Debug.Print(XmlSerializerHelper.SerializeToString(irstream));
        }

        [TestMethod]
        public void PriceableFixedRateStream1Ex05()
        {
            var priceableControllers = CreateFixedRateStreamEx05("001", true);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFixedRateStream2Ex05()
        {
            var priceableControllers = CreateFixedRateStreamEx05("001", false);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFixedRateStreamEvolveEx05()
        {
            var priceableControllers = CreateFixedRateStreamEx05("001", false);
            priceableControllers.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;
            for (int i = 0; i < 6; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableControllers, _metrics, _baseDate, _baseDate.AddMonths(5 * i));
                foreach (var priceableController in priceableControllers.GetChildren())
                {
                    ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                }
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStream1Ex05()
        {
            var priceableControllers = CreateFloatingRateStreamEx05("001", true);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStream2Ex05()
        {
            var priceableControllers = CreateFloatingRateStreamEx05("001", false);
            ProcessInstrumentControllerResults(priceableControllers, _metrics, _baseDate);
            foreach (var priceableController in priceableControllers.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
            }
            foreach (var metric in _metrics)
            {
                var result = priceableControllers.AggregateMetric(metric, _baseDate);
                Debug.Print("Metric Name : {0} Metric Value : {1}", metric, result);
            }
        }

        [TestMethod]
        public void PriceableFloatingRateStreamEvolveEx05()
        {
            var priceableControllers = CreateFloatingRateStreamEx05("001", false);
            priceableControllers.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;
            for (int i = 0; i < 6; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableControllers, _metrics, _baseDate, _baseDate.AddMonths(5 * i));
                foreach (var priceableController in priceableControllers.GetChildren())
                {
                    ProcessInstrumentControllerResults(priceableController, _metrics, _baseDate);
                }
            }
        }

        #endregion

        #endregion

        #region Test Payments

        #region Methods

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceablePayment> CreatePayment(string id, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceablePayment>();
            foreach (var date in dates)
            {
                var money = MoneyHelper.GetAmount(10000000.0m, "AUD");
                var dt = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var priceableAsset =
                    new PriceablePayment
                        (id + date.ToShortDateString()
                         , "xxx"
                         , "yyy"
                         , false
                         , money
                         , dt
                         , PaymentCalendar);
                //var priceableAsset = InterestRateCouponFactory.CreateFixedCoupon(id + date.ToShortDateString(), id, 10000000.0m, "AUD",
                //    baseDate, date, false, date, new[] { "AUSY" }, BusinessDayConventionEnum.FOLLOWING, cDayCountFraction, DiscountingTypeEnum.Standard, 0.07m);
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceablePayment> CreatePayment2(string id, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceablePayment>();
            foreach (var date in dates)
            {
                var money = MoneyHelper.GetAmount(10000000.0m, "AUD");
                var dt = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var priceableAsset =
                    new PriceablePayment
                        (id + date.ToShortDateString()
                         , "xxx"
                         , "yyy"
                         , false
                         , money
                         , dt
                         , PaymentCalendar)
                    {
                        PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward
                    };
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        #endregion

        #region Tests

        [TestMethod]
        public void PriceablePaymentWithReportingCurrency()
        {
            var priceableControllers = CreatePayment("Payment", _dates);

            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, BaseDate, "AUD");
            }
        }

        [TestMethod]
        public void PriceablePaymentWithReportingUSDCurrency()
        {
            var priceableControllers = CreatePayment("Payment", _dates);

            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, BaseDate, "USD");
            }
        }

        [TestMethod]
        public void PriceablePayment()
        {
            var priceableControllers = CreatePayment("Payment", _dates);

            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResults(priceableController, _metrics, BaseDate);
            }
        }

        [TestMethod]
        public void PriceablePaymentEvolve()
        {
            var priceableControllers = CreatePayment2("Payment", _dates);

            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                for (int i = 1; i < 10; i++)
                {
                    ProcessInstrumentControllerResultsEvolve(priceableController, _metrics, BaseDate, BaseDate.AddDays(3 * i));
                }
            }
        }

        #endregion

        #endregion

        #region Test Principal Exchanges

        #region Methods

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceableCashflow> CreatePrincipalExchange(string id, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceableCashflow>();
            foreach (var date in dates)
            {
                const decimal money = 10000000.0m;
                const string currency = "AUD";
                var priceableAsset =
                    new PriceablePrincipalExchange
                        (id + date.ToShortDateString()
                         , false
                         , money
                         , currency
                         , date
                         , PaymentCalendar);
                //var priceableAsset = InterestRateCouponFactory.CreateFixedCoupon(id + date.ToShortDateString(), id, 10000000.0m, "AUD",
                //    baseDate, date, false, date, new[] { "AUSY" }, BusinessDayConventionEnum.FOLLOWING, cDayCountFraction, DiscountingTypeEnum.Standard, 0.07m);
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        static private IEnumerable<PriceableCashflow> CreatePrincipalExchange2(string id, IEnumerable<DateTime> dates)
        {
            var controllers = new List<PriceableCashflow>();
            foreach (var date in dates)
            {
                const decimal money = 10000000.0m;
                const string currency = "AUD";
                //var dt = DateTypesHelper.ToAdjustableDate(date, BusinessDayConventionEnum.FOLLOWING.ToString(),
                //                                          BusinessCentersHelper.BusinessCentersString(new[] { "AUSY" }));
                var priceableAsset =
                    new PriceablePrincipalExchange
                        (id + date.ToShortDateString()
                         , false
                         , money
                         , currency
                         , date
                         , PaymentCalendar)
                        {
                            PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward
                        };
                controllers.Add(priceableAsset);
            }
            return controllers;
        }

        #endregion

        #region Tests

        [TestMethod]
        public void PriceablePrincipalExchangeWithReportingCurrency()
        {
            var priceableControllers = CreatePrincipalExchange("PrincipalExchange", _dates);
            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, BaseDate, "AUD");
            }
        }

        [TestMethod]
        public void PriceablePrincipalExchangeWithReportingUSDCurrency()
        {
            var priceableControllers = CreatePrincipalExchange("PrincipalExchange", _dates);
            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, BaseDate, "USD");
            }
        }

        [TestMethod]
        public void PriceablePrincipalExchange()
        {
            var priceableControllers = CreatePrincipalExchange("PrincipalExchange", _dates);
            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                ProcessInstrumentControllerResults(priceableController, _metrics, BaseDate);
            }
        }

        [TestMethod]
        public void PriceablePrincipalExchangeEvolve()
        {
            var priceableControllers = CreatePrincipalExchange2("PrincipalExchange", _dates);
            foreach (var priceableController in priceableControllers)
            {
                //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
                for (int i = 1; i < 10; i++)
                {
                    ProcessInstrumentControllerResultsEvolve(priceableController, _metrics, BaseDate, BaseDate.AddDays(3 * i));
                }
            }
        }

        #endregion

        #endregion

        #region Test Bullet Payment pricer

        private readonly DateTime _baseBulletDate = new DateTime(1998, 2, 20);

        [TestMethod]
        public void BulletPaymentPricer()
        {
            BulletPayment bullet = FpMLTestsSwapHelper.GetBulletPaymentExampleObject();
            bullet.id = "TestTrade001";
            var payerParty = bullet.payment.payerPartyReference.href;
            var priceableController = new BulletPaymentPricer(bullet, payerParty, PaymentCalendar);
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseBulletDate);
        }

        [TestMethod]
        public void BulletPaymentPricerEvolve()
        {
            BulletPayment bullet = FpMLTestsSwapHelper.GetBulletPaymentExampleObject();
            bullet.id = "TestTrade001";
            var payerParty = bullet.payment.payerPartyReference.href;
            var metrics2 = new[] { "NPV" };
            var priceableController = new BulletPaymentPricer(bullet, payerParty, PaymentCalendar)
            {
                PricingStructureEvolutionType =
                    PricingStructureEvolutionType.SpotToForward
            };
            priceableController.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;
            var index = 0;
            for (int i = 1; i < 10; i++)
            {
                var valuationDate = _baseDate.AddMonths(index);

                ProcessInstrumentControllerResultsEvolve(priceableController, metrics2, _baseBulletDate, valuationDate);
                index++;
            }
        }

        #endregion

        #region Test Fra Pricer

        private readonly DateTime _baseFraDate = new DateTime(2007, 6, 15);
        private const string fpmlFRAAsString = @"<fra xmlns='http://www.fpml.org/FpML-5/reporting'>" +
            //@"<fra>" +
                                               @"	<buyerPartyReference href='NAB'/>                                       " +
                                               @"	<sellerPartyReference href=""ABNAMRO""/>                                      " +
                                               @"	<adjustedEffectiveDate id=""resetDate"">2007-06-15</adjustedEffectiveDate>    " +
                                               "	<adjustedTerminationDate>2008-01-15</adjustedTerminationDate>               " +
                                               "	<paymentDate>                                                               " +
                                               "		<unadjustedDate>2007-06-15</unadjustedDate>                             " +
                                               "		<dateAdjustments>                                                       " +
                                               "			<businessDayConvention>FOLLOWING</businessDayConvention>            " +
                                               "			<businessCenters>                                                   " +
                                               "				<businessCenter>AUSY</businessCenter>                           " +
                                               "			</businessCenters>                                                  " +
                                               "		</dateAdjustments>                                                      " +
                                               "	</paymentDate>                                                              " +
                                               "	<fixingDateOffset>                                                          " +
                                               "		<periodMultiplier>-2</periodMultiplier>                                 " +
                                               "		<period>D</period>                                                      " +
                                               "		<dayType>Business</dayType>                                             " +
                                               "		<businessDayConvention>NONE</businessDayConvention>                     " +
                                               "		<businessCenters>                                                       " +
                                               "			<businessCenter>GBLO</businessCenter>                               " +
                                               "		</businessCenters>                                                      " +
                                               @"		<dateRelativeTo href=""resetDate""/>                                      " +
                                               "	</fixingDateOffset>                                                      " +
                                               "	<dayCountFraction>ACT/360</dayCountFraction>                             " +
                                               "	<calculationPeriodNumberOfDays>184</calculationPeriodNumberOfDays>       " +
                                               "	<notional>                                                               " +
                                               "		<currency>AUD</currency>                                             " +
                                               "		<amount>10000000.00</amount>                                         " +
                                               "	</notional>                                                              " +
                                               "	<fixedRate>0.0667</fixedRate>                                              " +
                                               "	<floatingRateIndex>AUD-BBR-ISDC</floatingRateIndex>                     " +
                                               "	<indexTenor>                                                             " +
                                               "		<periodMultiplier>6</periodMultiplier>                               " +
                                               "		<period>M</period>                                              " +
                                               "	</indexTenor>                                                       " +
                                               "	<fraDiscounting>ISDA</fraDiscounting>                               " +
                                               "</fra>";

        [TestMethod]
        public void TestFraPricer1()
        {
            var parameters = new FraInputRange
            {
                AdjustedEffectiveDate = DateTime.Parse("2007-06-15"),
                AdjustedTerminationDate = DateTime.Parse("2008-01-15"),
                UnadjustedPaymentDate = DateTime.Parse("2007-06-15"),
                PaymentDateBusinessDayConvention = "FOLLOWING",
                PaymentDateBusinessCenters = "AUSY-USNY",
                FixingDayOffsetPeriod = "-2d",
                FixingDayOffsetDayType = "Business",
                FixingDayOffsetBusinessDayConvention = "NONE",
                FixingDayOffsetBusinessCenters = "GBLO",
                FixingDayOffsetDateRelativeTo = "resetDate",
                DayCountFraction = "ACT/360",
                NotionalAmount = 10000000.00,
                NotionalCurrency = "AUD",
                FixedRate = 0.0667,
                FloatingRateIndex = "AUD-BBR-ISDC",
                IndexTenor = "6M",
                FraDiscounting = FraDiscountingEnum.ISDA,
                Sell = "TRUE",
                ValuationDate = new DateTime(2007, 6, 15).AddDays(-10)
            };
            var fra = ProductFactory.GetFpMLFra(parameters);
            var frapricer = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, true, fra, CurveEngine.NameSpace);
            var fraCopy = frapricer.Build();
            var result = XmlSerializerHelper.SerializeToString(fraCopy);
            Debug.WriteLine("--------------------------------------------------");
            Debug.Print(result);
            Debug.WriteLine("--------------------------------------------------");
            var priceableController = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, fraCopy, CurveEngine.NameSpace);
            //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseFraDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseFraDate);
            }
        }

        [TestMethod]
        public void TestFraPricer2()
        {
            var parameters = new FraInputRange
            {
                AdjustedEffectiveDate = DateTime.Parse("2007-06-15"),
                AdjustedTerminationDate = DateTime.Parse("2007-12-15"),
                UnadjustedPaymentDate = DateTime.Parse("2007-06-15"),
                PaymentDateBusinessDayConvention = "MODFOLLOWING",
                PaymentDateBusinessCenters = "AUSY",
                FixingDayOffsetPeriod = "-2d",
                FixingDayOffsetDayType = "Business",
                FixingDayOffsetBusinessDayConvention = "NONE",
                FixingDayOffsetBusinessCenters = "AUSY",
                FixingDayOffsetDateRelativeTo = "resetDate",
                DayCountFraction = "ACT/365.FIXED",
                NotionalAmount = 10000000.00,
                NotionalCurrency = "AUD",
                FixedRate = 0.0667,
                FloatingRateIndex = "AUD-BBR-BBSW",
                IndexTenor = "6M",
                FraDiscounting = FraDiscountingEnum.AFMA,
                Sell = "TRUE",
                ValuationDate = new DateTime(2007, 6, 15).AddDays(-10)
            };
            var fra = ProductFactory.GetFpMLFra(parameters);
            var frapricer = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, true, fra, CurveEngine.NameSpace);
            var fraCopy = frapricer.Build();
            var result = XmlSerializerHelper.SerializeToString(fra);
            Debug.WriteLine("--------------------------------------------------");
            Debug.Print(result);
            Debug.WriteLine("--------------------------------------------------");
            var priceableController = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, fraCopy, CurveEngine.NameSpace);
            //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseFraDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseFraDate);
            }
        }

        [TestMethod]
        public void TestFraInstrument1()
        {
            var document = new XmlDocument();
            document.LoadXml(fpmlFRAAsString);
            XmlNameTable nt = new NameTable();
            var nsMgr = new XmlNamespaceManager(nt);
            nsMgr.AddNamespace("f", "http://www.fpml.org/FpML-5/reporting");
            XmlNode node = document.SelectSingleNode("//f:fra", nsMgr);
            var fraFpmlDes = XmlSerializerHelper.DeserializeNode<Fra>(node);
            var priceableController = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, fraFpmlDes, CurveEngine.NameSpace);
            //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseFraDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseFraDate);
            }

        }


        [TestMethod]
        public void TestFraInstrument2()
        {
            var document = new XmlDocument();
            document.LoadXml(fpmlFRAAsString);
            XmlNameTable nt = new NameTable();
            var nsMgr = new XmlNamespaceManager(nt);
            nsMgr.AddNamespace("f", "http://www.fpml.org/FpML-5/reporting");
            XmlNode node = document.SelectSingleNode("//f:fra", nsMgr);
            var fraFpmlDes = XmlSerializerHelper.DeserializeNode<Fra>(node);
            var priceableController = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, fraFpmlDes, CurveEngine.NameSpace);
            //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseFraDate);
            //double d2 = (double)forwardRateAgreement.Price(new DateTime(2007, 6, 15).AddDays(-10), rateCurve.GetTermStructure()).amount;

            //Debug.WriteLine("--------------------------------------------------");
            //Trace.WriteLine(String.Format("Fixed Rate : {0} ", fraFpmlDes.fixedRate));
            //Trace.WriteLine(String.Format("Notional amount : {0} ", fraFpmlDes.notional.amount));
            //Trace.WriteLine(String.Format("FRA Price : {0} ", d2));
            //Debug.WriteLine("--------------------------------------------------");
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseFraDate);

            }

        }

        [TestMethod]
        public void TestFraInstrument3()
        {
            var document = new XmlDocument();
            document.LoadXml(fpmlFRAAsString);
            XmlNameTable nt = new NameTable();
            var nsMgr = new XmlNamespaceManager(nt);
            nsMgr.AddNamespace("f", "http://www.fpml.org/FpML-5/reporting");
            XmlNode node = document.SelectSingleNode("//f:fra", nsMgr);
            var fraFpmlDes = XmlSerializerHelper.DeserializeNode<Fra>(node);
            var priceableController = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, true, fraFpmlDes, CurveEngine.NameSpace);
            //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseFraDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseFraDate);
            }
        }

        [TestMethod]
        public void TestFraInstrumentWithReportingCurrency()
        {
            var document = new XmlDocument();
            document.LoadXml(fpmlFRAAsString);
            XmlNameTable nt = new NameTable();
            var nsMgr = new XmlNamespaceManager(nt);
            nsMgr.AddNamespace("f", "http://www.fpml.org/FpML-5/reporting");
            XmlNode node = document.SelectSingleNode("//f:fra", nsMgr);
            var fraFpmlDes = XmlSerializerHelper.DeserializeNode<Fra>(node);
            var priceableController = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, true, fraFpmlDes, CurveEngine.NameSpace);
            //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseFraDate, "AUD");
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseFraDate);
            }
        }

        [TestMethod]
        public void TestFraInstrumentWithReportingCurrencyUSD()
        {
            var document = new XmlDocument();
            document.LoadXml(fpmlFRAAsString);
            XmlNameTable nt = new NameTable();
            var nsMgr = new XmlNamespaceManager(nt);
            nsMgr.AddNamespace("f", "http://www.fpml.org/FpML-5/reporting");
            XmlNode node = document.SelectSingleNode("//f:fra", nsMgr);
            var fraFpmlDes = XmlSerializerHelper.DeserializeNode<Fra>(node);
            var priceableController = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, true, fraFpmlDes, CurveEngine.NameSpace);
            //var metrics = new[] { "MarketQuote", "NPV", "ImpliedQuote", "Delta1", "ImpliedQuote", "DeltaR", "NPV" };
            ProcessInstrumentControllerResultsWithCurrency(priceableController, _metrics, _baseFraDate, "USD");
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseFraDate);
            }
        }

        [TestMethod]
        public void TestFraInstrumentEvolve()
        {
            var document = new XmlDocument();
            document.LoadXml(fpmlFRAAsString);
            XmlNameTable nt = new NameTable();
            var nsMgr = new XmlNamespaceManager(nt);
            nsMgr.AddNamespace("f", "http://www.fpml.org/FpML-5/reporting");
            XmlNode node = document.SelectSingleNode("//f:fra", nsMgr);
            var fraFpmlDes = XmlSerializerHelper.DeserializeNode<Fra>(node);
            var priceableController = new FraPricer(CurveEngine.Logger, CurveEngine.Cache, null, null, fraFpmlDes, CurveEngine.NameSpace)
            {
                PricingStructureEvolutionType =
                    PricingStructureEvolutionType.SpotToForward
            };
            var metrics = new[] { "NPV" };
            for (int i = 0; i < 0; i++)
            {
                ProcessInstrumentControllerResultsEvolve(priceableController, metrics, _baseFraDate, _baseFraDate.AddDays(3 * 1));
            }
        }

        #endregion

        #region Test Swap Pricer

        private readonly DateTime _baseSwapDate = new DateTime(1998, 2, 20);

        #region Methods

        private static void SetNotional(PaymentCalculationPeriod paymentCalculationPeriod, decimal value)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            XsdClassesFieldResolver.CalculationPeriodSetNotionalAmount(calculationPeriodArray[0], value);
        }

        private static decimal GetNotional(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            return XsdClassesFieldResolver.CalculationPeriodGetNotionalAmount(calculationPeriodArray[0]);
        }

        private static void SetFixedRate(PaymentCalculationPeriod paymentCalculationPeriod, decimal value)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            XsdClassesFieldResolver.SetCalculationPeriodFixedRate(calculationPeriodArray[0], value);
        }

        private static decimal GetFixedRate(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);

            return XsdClassesFieldResolver.CalculationPeriodGetFixedRate(calculationPeriodArray[0]);
        }



        private static void SetSpread(PaymentCalculationPeriod paymentCalculationPeriod, decimal spreadValue)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriodArray[0]);
            floatingRateDefinition.spread = spreadValue;
        }

        private static decimal GetSpread(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            CalculationPeriod[] calculationPeriodArray = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
            FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriodArray[0]);
            return floatingRateDefinition.spread;
        }

        private static SwapLegParametersRange_Old GetFloatLeg()
        {
            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };


            return receiveLeg;
        }

        private static SwapLegParametersRange_Old GetFixedLeg()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            return payLeg;
        }

        private static DateTime GetCalculationPeriodAdjustedEndDate(DateTime adjustedStartDate, IBusinessCalendar calendar, Period lenOfPeriod, BusinessDayConventionEnum businessDayConvention)
        {
            //offset in calendar months
            return calendar.Advance(adjustedStartDate, OffsetHelper.FromInterval(lenOfPeriod, DayTypeEnum.Calendar), businessDayConvention);
        }

        private static PaymentCalculationPeriod GetPaymentCalculationPeriod(IBusinessCalendar calendar, BusinessDayConventionEnum businessDayConvention,
                                                                            DateTime adjustedEndDate, Period paymentDayOffset)//ofsset in business days
        {
            var paymentCalculationPeriod = new PaymentCalculationPeriod
                {
                    adjustedPaymentDate =
                        calendar.Advance(adjustedEndDate, OffsetHelper.FromInterval(paymentDayOffset, DayTypeEnum.Business),
                                         businessDayConvention),
                    adjustedPaymentDateSpecified = true
                };
            return paymentCalculationPeriod;
        }
        public Swap GenerateSwapParametricWithCashflows()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                MaturityDate = new DateTime(1999, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR",
                DiscountingType = "None"
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                MaturityDate = new DateTime(1999, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR",
                ForecastCurve = "AUD-LIBOR",
                DiscountingType = "None"
            };

            Double[] times = { 0, 1, 2, 3, 4, 5 };
            Double[] dfs = { 1, 0.98, 0.96, 0.91, 0.88, 0.85 };
            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamEnvironment(new DateTime(1994, 12, 14), times, dfs);
            var valuationDT = new DateTime(1994, 12, 20);
            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, null, null, marketEnvironment, valuationDT);
            MarketEnvironmentRegistry.Remove("1234567");
            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            return swap;
        }

        public CalculationPeriodsPrincipalExchangesAndStubs GetCalculationPeriods(InterestRateStream interestRateStream)
        {
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, interestRateStream.calculationPeriodDates.calculationPeriodDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            IBusinessCalendar fixingCalendar = null;
            if (interestRateStream.resetDates!=null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, interestRateStream.resetDates.resetDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            }
            CalculationPeriodsPrincipalExchangesAndStubs calculationPeriodsPrincipalExchangesAndStubs = StreamCashflowsGenerator.GenerateCalculationPeriodsPrincipalExchangesAndStubs(interestRateStream, fixingCalendar, paymentCalendar);
            return calculationPeriodsPrincipalExchangesAndStubs;
        }

        public void TestGetPaymentPeriods()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[0].calculationPeriodDates.calculationPeriodDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            CalculationPeriodsPrincipalExchangesAndStubs listCalculationPeriods = GetCalculationPeriods(swap.swapStream[0]);
            List<PaymentCalculationPeriod> result = StreamCashflowsGenerator.GetPaymentCalculationPeriods(swap.swapStream[0], listCalculationPeriods, paymentCalendar);
            foreach (PaymentCalculationPeriod period in result)
            {
                Trace.WriteLine(String.Format("Payment Period Pay date {0} ", period.adjustedPaymentDate.ToShortDateString()));
            }
        }

        #endregion

        #region Interest Rate Swap Pricer Tests

        [TestMethod]
        public void IRSwapPricer0()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            swap.id = "TestTrade001";
            var payerParty = swap.swapStream[0].payerPartyReference.href;
            var priceableController = new InterestRateSwapPricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, swap, payerParty);
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseSwapDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseSwapDate);
            }
            var result = priceableController.Build();
            Debug.WriteLine(XmlSerializerHelper.SerializeToString<Product>(result));
        }

        [TestMethod]
        public void IRSwapPricer1()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            var payerParty = swap.swapStream[0].payerPartyReference.href;
            var priceableController = new InterestRateSwapPricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, swap, payerParty);
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseSwapDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseSwapDate);
            }
            var result = priceableController.Build();
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(result));
        }

        [TestMethod]
        public void IRSwapPricer3()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap02StubAmort();
            var payerParty = swap.swapStream[0].payerPartyReference.href;
            var priceableController = new InterestRateSwapPricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, swap, payerParty);
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseSwapDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseSwapDate);
            }
            var result = priceableController.Build();
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(result));
        }

        /// <summary>
        /// This is a compound swap with multiple calculationPeriods. Hence is not yet implemented.
        /// </summary>
        //[TestMethod]
        public void IRSwapPricer4()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            var payerParty = swap.swapStream[0].payerPartyReference.href;
            var priceableController = new InterestRateSwapPricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, swap, payerParty);
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseSwapDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseSwapDate);
            }
            var result = priceableController.Build();
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(result));
        }

        [TestMethod]
        public void IRSwapPricer5()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap05ExampleObject();
            var payerParty = swap.swapStream[0].payerPartyReference.href;
            var priceableController = new InterestRateSwapPricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, swap, payerParty);
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseSwapDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseSwapDate);
            }
            var result = priceableController.Build();
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(result));
        }


        [TestMethod]
        public void IRSwapPricer6()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap04ExampleObject();
            var payerParty = swap.swapStream[0].payerPartyReference.href;
            var priceableController = new InterestRateSwapPricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, swap, payerParty);
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseSwapDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseSwapDate);
            }
            var result = priceableController.Build();
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(result));
        }

        [TestMethod]
        public void PriceableXccySwap()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap06ExampleObject();
            var payerParty = swap.swapStream[0].payerPartyReference.href;
            var priceableController = new CrossCurrencySwapPricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, swap, payerParty);
            ProcessInstrumentControllerResults(priceableController, _metrics, _baseSwapDate);
            foreach (var priceableCashflow in priceableController.GetChildren())
            {
                ProcessInstrumentControllerResults(priceableCashflow, _metrics, _baseSwapDate);
            }
            var result = priceableController.Build();
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(result));
        }

        [TestMethod]
        public void IRSwapPricerEvolve()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            var payerParty = swap.swapStream[0].payerPartyReference.href;
            var metrics2 = new[] { "NPV" };
            var priceableController = new InterestRateSwapPricer(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, null, swap, payerParty)
            {
                PricingStructureEvolutionType =
                    PricingStructureEvolutionType.SpotToForward
            };
            priceableController.PricingStructureEvolutionType = PricingStructureEvolutionType.SpotToForward;
            var index = 0;
            for (int i = 1; i < 10; i++)
            {
                var valuationDate = _baseSwapDate.AddMonths(index);
                ProcessInstrumentControllerResultsEvolve(priceableController, metrics2, _baseSwapDate, valuationDate);
                index++;
            }
        }

        #endregion

        #region Parametric Swap Generation Tests

        [TestMethod]
        public void GenerateSwaptionParametricDefininionOnly()
        {
            SwapLegParametersRange_Old payLeg = GetFixedLeg();

            SwapLegParametersRange_Old receiveLeg = GetFloatLeg();

            var swaptionParametersRange = new SwaptionParametersRange
                                              {
                                                  ExpirationDate = receiveLeg.EffectiveDate.AddDays(-2),
                                                  ExpirationDateCalendar = "GBLO-AUSY",
                                                  ExpirationDateBusinessDayAdjustments = "FOLLOWING",
                                                  PaymentDate = receiveLeg.EffectiveDate,
                                                  PaymentDateCalendar = "AUSY",
                                                  PaymentDateBusinessDayAdjustments = "FOLLOWING",
                                                  Premium = 12345,
                                                  StrikeRate = 0.08m,
                                                  Volatility = 0.2m
                                              };

            Swaption swaption = SwaptionGenerator.GenerateSwaptionDefiniton(payLeg, PaymentCalendar, receiveLeg, PaymentCalendar, swaptionParametersRange);

            Assert.AreEqual(swaption.swap.swapStream.Length, 2);
            Assert.IsNull(swaption.swap.swapStream[0].cashflows);
            Assert.IsNull(swaption.swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swaption));
        }  
   
        [TestMethod]
        public void Test1()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            //var cashflows = new Cashflows
            //                    {
            //                        cashflowsMatchParameters = true,
            //                        paymentCalculationPeriod =
            //                            new[] {new PaymentCalculationPeriod()}
            //                    };
            var adjustableEffectiveDate = (AdjustableDate) swap.swapStream[0].calculationPeriodDates.Item;
            DateTime unadjustedEffectiveDate = adjustableEffectiveDate.unadjustedDate.Value;
            var adjustableTerminationDate = (AdjustableDate) swap.swapStream[0].calculationPeriodDates.Item1;
            DateTime unadjustedTerminationDate = adjustableTerminationDate.unadjustedDate.Value;
            IBusinessCalendar calendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, new[] { "GBLO", "USNY" }, CurveEngine.NameSpace);
            // Create a list of calculation periods
            //
            var listCalculationPeriod = new List<CalculationPeriod>();
            const BusinessDayConventionEnum calPeriodBusinessDayConvention = BusinessDayConventionEnum.MODFOLLOWING;
            int periodNumber = 1;
            DateTime calcPeriodStart;

            do
            {
                //DateTime calcPeriodStartWithRollConv = new DateTime(calcPeriodStart.Year, calcPeriodStart.Month, 27);

                DateTime calcPeriodEnd = GetCalculationPeriodAdjustedEndDate(unadjustedEffectiveDate, calendar, IntervalHelper.FromMonths(periodNumber * 3), calPeriodBusinessDayConvention);
                calcPeriodStart = GetCalculationPeriodAdjustedEndDate(unadjustedEffectiveDate, calendar, IntervalHelper.FromMonths((periodNumber - 1) * 3), calPeriodBusinessDayConvention);
                CalculationPeriod calcPeriod = CalculationPeriodFactory.Create(calcPeriodStart, calcPeriodEnd, DateTime.MinValue, DateTime.MinValue);
                Trace.WriteLine(String.Format("Calc Period {0} - {1} ", calcPeriodStart.ToShortDateString(), calcPeriodEnd.ToShortDateString()));
                listCalculationPeriod.Add(calcPeriod);
                //calcPeriodStart = calcPeriodEnd;
                ++periodNumber;

            } while (calcPeriodStart < unadjustedTerminationDate);

            
            var listPaymentCalculationPeriod = new List<PaymentCalculationPeriod>();
            Period paymentCalPeriodLen = IntervalHelper.FromMonths(6);//CALENDAR
            Period paymentCalPayOffset = IntervalHelper.FromDays(5);//BUSINESS
            const BusinessDayConventionEnum modFollowing = BusinessDayConventionEnum.MODFOLLOWING;

            DateTime paymentCalcPeriodStart = unadjustedEffectiveDate;

            do
            {
                // CALENDAR MONTHS
                //
                DateTime paymentCalcPeriodEnd = calendar.Advance(paymentCalcPeriodStart, OffsetHelper.FromInterval(paymentCalPeriodLen, DayTypeEnum.Calendar), modFollowing);
                PaymentCalculationPeriod payCalcPeriod =
                    GetPaymentCalculationPeriod(calendar, modFollowing,
                                                paymentCalcPeriodEnd, paymentCalPayOffset);
                Trace.WriteLine(String.Format("Pay Calc Period {0} - {1}, pay date : {2} ", paymentCalcPeriodStart.ToShortDateString(), paymentCalcPeriodEnd.ToShortDateString(), payCalcPeriod.adjustedPaymentDate.ToShortDateString()));
                listPaymentCalculationPeriod.Add(payCalcPeriod);
                paymentCalcPeriodStart = paymentCalcPeriodEnd;

            } while (paymentCalcPeriodStart < unadjustedTerminationDate);
        }

        [TestMethod]
        public void TestGetFloatingCashflows()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[0].calculationPeriodDates.calculationPeriodDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            IBusinessCalendar fixingCalendar = null;
            if (swap.swapStream[0].resetDates != null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[0].resetDates.resetDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            }
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows( swap.swapStream[0], fixingCalendar, paymentCalendar);
            Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
            Debug.WriteLine("Xml dump:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
        }

        //[TestMethod]
        //public void TestGetFloatingCashflows_ShortInitialStub()
        //{
        //    //RateCurveDatabase rateCurveDatabase = new RateCurveDatabase();
        //    //RateCurve rateCurve = RateCurveTests.CreateAudLiborBba3MonthFlat8PercentCurve(DateTime.Parse("1990-01-25"), DateTime.Parse("1990-01-25"));
        //    //rateCurveDatabase.AddIndexCurve("EUR-LIBOR-BBA", "4m", rateCurve);
        //    //rateCurveDatabase.AddIndexCurve("EUR-LIBOR-BBA", "5m", rateCurve);
        //    //rateCurveDatabase.AddIndexCurve("EUR-LIBOR-BBA", "6m", rateCurve);
        //    //MarketRegistry.AddRateCurveDatabase(rateCurveDatabase);


        //    Swap swap = FpMLTestsSwapHelper.GetSwap02_Initial_Short_Stub();

        //    BusinessCentersResolver.ResolveBusinessCenters(swap);

        //    Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(swap.swapStream[0]);

        //    Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
        //    Debug.WriteLine("Xml dump:");
        //    Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));

        //    Assert.AreEqual(10, cashflows.paymentCalculationPeriod.Length);
        //}


        // NB: currently not supported. DON'T DELETE THIS TEST!
        //
        //[TestMethod]
        //public void TestGetFloatingCashflows_LongInitial_ShortFinal_Stub()
        //{
        //    RateCurve rateCurve = RateCurveTests.CreateAudBkBill3MonthFlat8PercentCurve(DateTime.Parse("2000-01-25"), DateTime.Parse("2000-01-25"));

        //    Swap swap = FpMLTestsSwapHelper.GetSwap05_ExampleObject();

        //    BusinessCentersResolver.ResolveBusinessCenters(swap);

        //    InterestRateStream floatingStream = swap.swapStream[0];


        //    DateTime valDate = DateTime.Parse("2000-01-26");

        //    floatingStream.cashflows = FloatingRateStreamCashflowGenerator.GetCashflows(floatingStream);
        //    FloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatingStream, rateCurve, rateCurve, valDate);

        //    Trace.WriteLine(String.Format("Number of cashflows : {0}", floatingStream.cashflows.paymentCalculationPeriod.Length));
        //    Debug.WriteLine("Xml dump:");
        //    Debug.WriteLine(XmlSerializerHelper.SerializeToString(floatingStream.cashflows));
        //    Assert.AreEqual(10, floatingStream.cashflows.paymentCalculationPeriod.Length);
        //}


        //to CF the Swap05 the firstPeriodStart element should be taken into account. Swap05 is very non-standard FpML swap
        //[TestMethod]
        //public void TestGetFixedCashflows_LongInitial_ShortFinal_Stub()
        //{
        //    Swap swap = FpMLTestsSwapHelper.GetSwap05_ExampleObject();

        //    BusinessCentersResolver.ResolveBusinessCenters(swap);

        //    Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1]);

        //    Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
        //    Debug.WriteLine("Xml dump:");
        //    Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
        //    Assert.AreEqual(6, cashflows.paymentCalculationPeriod.Length);
        //}

        //[TestMethod]
        //public void TestGetFixedCashflows_ShortInitialStub()
        //{
        //    Swap swap = FpMLTestsSwapHelper.GetSwap02_Initial_Short_Stub();

        //    BusinessCentersResolver.ResolveBusinessCenters(swap);

        //    //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1]);
        //    Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1]);

        //    Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
        //    Debug.WriteLine("Xml dump:");
        //    Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));

        //    Assert.AreEqual(5, cashflows.paymentCalculationPeriod.Length);
        //}

        [TestMethod]
        public void TestGetFixedCashflowsVanillaSwap()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1]);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[1].calculationPeriodDates.calculationPeriodDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            IBusinessCalendar fixingCalendar = null;
            if (swap.swapStream[1].resetDates != null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[1].resetDates.resetDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            }
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1], fixingCalendar, paymentCalendar);
            Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
            Debug.WriteLine("Xml dump:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
            Assert.AreEqual(5, cashflows.paymentCalculationPeriod.Length);
        }

        [TestMethod]
        public void TestGetFixedCashflowsVanillaSwapStringInterface()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1]);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[1].calculationPeriodDates.calculationPeriodDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            IBusinessCalendar fixingCalendar = null;
            if (swap.swapStream[1].resetDates != null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[1].resetDates.resetDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            }
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1], fixingCalendar, paymentCalendar);
            Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
            Assert.AreEqual(5, cashflows.paymentCalculationPeriod.Length);
            // output date format:
            //
            //  startdate       enddate     paymentdate     rate    notional 
            //  
            //
            //
            var result = new List<List<string>> {new List<string>()};
            result[0].Add("startdate");
            result[0].Add("enddate");
            result[0].Add("paymentdate");
            result[0].Add("rate");
            result[0].Add("notional");
            result[0].Add("couponAmount");
            result[0].Add("discountedAmount");

            foreach (PaymentCalculationPeriod paymentCalculationPeriod in cashflows.paymentCalculationPeriod)
            {
                CalculationPeriod[] calculationPeriods = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod);
                var row = new List<string>
                    {
                        calculationPeriods[0].adjustedStartDate.ToShortDateString(),
                        calculationPeriods[0].adjustedEndDate.ToShortDateString(),
                        paymentCalculationPeriod.adjustedPaymentDate.ToShortDateString()
                    };
                decimal fixedRate = XsdClassesFieldResolver.CalculationPeriodGetFixedRate(calculationPeriods[0]);
                row.Add(fixedRate.ToString("#.#00"));
                decimal notional = XsdClassesFieldResolver.CalculationPeriodGetNotionalAmount(calculationPeriods[0]);
                row.Add(notional.ToString(CultureInfo.InvariantCulture));
                //FixedRateCashflow cashflow = new FixedRateCashflow(paymentCalculationPeriod);
                //Money couponAmount = cashflow.GetAmount();
                //row.Add(couponAmount.amount.ToString("#.#00"));
                //double discountFactor = discountCurve.GetDiscountFactor(cashflow.PaymentDate);
                //Money discountedAmount = MoneyHelper.Mul(couponAmount, discountFactor);
                //row.Add(discountedAmount.amount.ToString("#.#00"));
                //row.Add(discountFactor.ToString("#.####00"));
                result.Add(row);
            }

            StringMatrix.PrintToDebug(result);
        }

        [TestMethod]
        public void TestGetFloatingCashflowsVanillaSwap()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap01ExampleObject();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[0].calculationPeriodDates.calculationPeriodDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            IBusinessCalendar fixingCalendar = null;
            if (swap.swapStream[0].resetDates != null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[0].resetDates.resetDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            }
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(swap.swapStream[0], fixingCalendar, paymentCalendar);
            Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
            Debug.WriteLine("Xml dump:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
            Assert.AreEqual(10, cashflows.paymentCalculationPeriod.Length);
        }

        [TestMethod]
        public void TestGetFixedCashflowsStubAmortisingNotional()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap02StubAmort();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1]);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[1].calculationPeriodDates.calculationPeriodDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            IBusinessCalendar fixingCalendar = null;
            if (swap.swapStream[1].resetDates != null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[1].resetDates.resetDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            }
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1], fixingCalendar, paymentCalendar);
            Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
            Debug.WriteLine("Xml dump:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
            Assert.AreEqual(5, cashflows.paymentCalculationPeriod.Length);
        }

        [TestMethod]
        public void TestGetFloatingCashflowsStubAmortisingNotional()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap02StubAmort();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[0].calculationPeriodDates.calculationPeriodDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            IBusinessCalendar fixingCalendar = null;
            if (swap.swapStream[0].resetDates != null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[0].resetDates.resetDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            }
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(swap.swapStream[0], fixingCalendar, paymentCalendar);
            Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
            Debug.WriteLine("Xml dump:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
            Assert.AreEqual(10, cashflows.paymentCalculationPeriod.Length);
        }
           
        [TestMethod]
        public void TestGetFixedCashflows()
        {   
            Swap swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            //Cashflows cashflows = FixedRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1]);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[1].calculationPeriodDates.calculationPeriodDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            IBusinessCalendar fixingCalendar = null;
            if (swap.swapStream[1].resetDates != null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, swap.swapStream[1].resetDates.resetDatesAdjustments.businessCenters, CurveEngine.NameSpace);
            }
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(swap.swapStream[1], fixingCalendar, paymentCalendar);
            Trace.WriteLine(String.Format("Number of cashflows : {0}", cashflows.paymentCalculationPeriod.Length));
            Debug.WriteLine("Xml dump:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(cashflows));
        }

        [TestMethod]
        public void TestGetCalculationPeriods()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            CalculationPeriodsPrincipalExchangesAndStubs calculationPeriodsPrincipalExchangesAndStubs = GetCalculationPeriods(swap.swapStream[0]);
            if (null != calculationPeriodsPrincipalExchangesAndStubs.InitialStubCalculationPeriod)
            {
                Trace.WriteLine(String.Format("InitialStub {0} ({2})- {1} ({3}) ", calculationPeriodsPrincipalExchangesAndStubs.InitialStubCalculationPeriod.adjustedStartDate.ToShortDateString(), calculationPeriodsPrincipalExchangesAndStubs.InitialStubCalculationPeriod.adjustedEndDate.ToShortDateString(), calculationPeriodsPrincipalExchangesAndStubs.InitialStubCalculationPeriod.unadjustedStartDate.ToShortDateString(), calculationPeriodsPrincipalExchangesAndStubs.InitialStubCalculationPeriod.unadjustedEndDate.ToShortDateString()));
            }            
            foreach (CalculationPeriod period in calculationPeriodsPrincipalExchangesAndStubs.CalculationPeriods)
            {
                Trace.WriteLine(String.Format("Calc Period {0} ({2})- {1} ({3}) ", period.adjustedStartDate.ToShortDateString(), period.adjustedEndDate.ToShortDateString(), period.unadjustedStartDate.ToShortDateString(), period.unadjustedEndDate.ToShortDateString()));
            }
            if (null != calculationPeriodsPrincipalExchangesAndStubs.FinalStubCalculationPeriod)
            {
                Trace.WriteLine(String.Format("Final Stub {0} ({2})- {1} ({3}) ", calculationPeriodsPrincipalExchangesAndStubs.FinalStubCalculationPeriod.adjustedStartDate.ToShortDateString(), calculationPeriodsPrincipalExchangesAndStubs.FinalStubCalculationPeriod.adjustedEndDate.ToShortDateString(), calculationPeriodsPrincipalExchangesAndStubs.FinalStubCalculationPeriod.unadjustedStartDate.ToShortDateString(), calculationPeriodsPrincipalExchangesAndStubs.FinalStubCalculationPeriod.unadjustedEndDate.ToShortDateString()));
            }
        }

        [TestMethod]
        public void TestGetAdjustedResetDates()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            CalculationPeriodsPrincipalExchangesAndStubs listCalculationPeriods = GetCalculationPeriods(swap.swapStream[0]);
            List<DateTime> result = ResetDatesGenerator.GetAdjustedResetDates(CurveEngine.Cache, swap.swapStream[0], listCalculationPeriods.CalculationPeriods, null, CurveEngine.NameSpace);          
            foreach (DateTime adjustedResetDate in result)
            {
                Trace.WriteLine(String.Format("Adjusted reset date {0} ", adjustedResetDate.ToShortDateString()));
            }
        }
        [TestMethod]
        public void TestGetAdjustedFixingDates()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            BusinessCentersResolver.ResolveBusinessCenters(swap);
            CalculationPeriodsPrincipalExchangesAndStubs listCalculationPeriods = GetCalculationPeriods(swap.swapStream[0]);
            List<DateTime> resetDates = ResetDatesGenerator.GetAdjustedResetDates(CurveEngine.Cache, swap.swapStream[0], listCalculationPeriods.CalculationPeriods, null, CurveEngine.NameSpace);
            List<DateTime> result = FixingDatesGenerator.GetAdjustedFixingDates(CurveEngine.Cache, swap.swapStream[0], resetDates, null, CurveEngine.NameSpace);
            foreach (DateTime adjustedResetDate in result)
            {
                Trace.WriteLine(String.Format("Adjusted fixing date {0} ", adjustedResetDate.ToShortDateString()));
            }
        }

        [TestMethod]
        public void TestFromEnd()
        {
            Swap swap = FpMLTestsSwapHelper.GetSwap03AUDExampleObject();
            //var cashflows = new Cashflows();
            //cashflows.cashflowsMatchParameters = true;
            //cashflows.paymentCalculationPeriod = new[] {new PaymentCalculationPeriod()};
            var adjustableEffectiveDate = (AdjustableDate) swap.swapStream[0].calculationPeriodDates.Item;
            DateTime unadjustedEffectiveDate = adjustableEffectiveDate.unadjustedDate.Value;
            var adjustableTerminationDate = (AdjustableDate) swap.swapStream[0].calculationPeriodDates.Item1;
            DateTime unadjustedTerminationDate = adjustableTerminationDate.unadjustedDate.Value;
            IBusinessCalendar cal = BusinessCenterHelper.ToBusinessCalendar(CurveEngine.Cache, new[] { "GBLO", "USNY" }, CurveEngine.NameSpace);
            DateTime adjustedTerminationDate = cal.Roll(unadjustedTerminationDate, BusinessDayConventionEnum.MODFOLLOWING);
            // Create a list of calculation periods
            //
            var listCalculationPeriod = new List<CalculationPeriod>();
            const BusinessDayConventionEnum calPeriodBusinessDayConvention = BusinessDayConventionEnum.MODFOLLOWING;
            int periodNumber = 1;
            DateTime calcPeriodEnd;
            do
            {
                //DateTime calcPeriodStartWithRollConv = new DateTime(calcPeriodStart.Year, calcPeriodStart.Month, 27);
                calcPeriodEnd = GetCalculationPeriodAdjustedEndDate(adjustedTerminationDate, cal, IntervalHelper.FromMonths((periodNumber - 1) * -3), calPeriodBusinessDayConvention);
                DateTime calcPeriodStart = GetCalculationPeriodAdjustedEndDate(adjustedTerminationDate, cal, IntervalHelper.FromMonths(periodNumber * -3), calPeriodBusinessDayConvention);
                CalculationPeriod calcPeriod = CalculationPeriodFactory.Create(calcPeriodStart, calcPeriodEnd, DateTime.MinValue, DateTime.MinValue);
                Trace.WriteLine(String.Format("Calc Period {0} - {1} ", calcPeriodStart.ToShortDateString(), calcPeriodEnd.ToShortDateString()));
                listCalculationPeriod.Add(calcPeriod);
                //calcPeriodStart = calcPeriodEnd;
                ++periodNumber;

            } while (calcPeriodEnd > unadjustedEffectiveDate);
        }

        [TestMethod]
        public void GenerateSwapParametricDefininionOfSpreadSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };


            var spreadScheduleList = new List<Pair<DateTime, decimal>>
                                         {
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate, 0.002m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*1), 0.002m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*2), 0.003m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*3), 0.0045m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*4), 0.0050m)
                                         };

            Schedule spreadSchedule = ScheduleHelper.Create(spreadScheduleList);

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            Swap swap = SwapGenerator.GenerateDefiniton(payLeg, receiveLeg);

            //  Apply spread schedule to floating stream
            //
            InterestRateStreamParametricDefinitionGenerator.SetSpreadSchedule(swap.swapStream[1], spreadSchedule);
            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNull(swap.swapStream[0].cashflows);
            Assert.IsNull(swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }
        [TestMethod]
        public void GenerateSwapParametricDefininionOfFixedRateSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var fixedRateScheduleList = new List<Pair<DateTime, decimal>>
                                            {
                                                new Pair<DateTime, decimal>(
                                                    payLeg.EffectiveDate, 0.07m),
                                                new Pair<DateTime, decimal>(
                                                    payLeg.EffectiveDate.AddMonths(6*1),
                                                    0.075m),
                                                new Pair<DateTime, decimal>(
                                                    payLeg.EffectiveDate.AddMonths(6*2),
                                                    0.080m),
                                                new Pair<DateTime, decimal>(
                                                    payLeg.EffectiveDate.AddMonths(6*3),
                                                    0.085m),
                                                new Pair<DateTime, decimal>(
                                                    payLeg.EffectiveDate.AddMonths(6*4),
                                                    0.084m)
                                            };

            Schedule fixedRateSchedule = ScheduleHelper.Create(fixedRateScheduleList);

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            Swap swap = SwapGenerator.GenerateDefiniton(payLeg, receiveLeg);

            //  Apply fixed rate schedule to stream
            //
            InterestRateStreamParametricDefinitionGenerator.SetFixedRateSchedule(swap.swapStream[0], fixedRateSchedule);
            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNull(swap.swapStream[0].cashflows);
            Assert.IsNull(swap.swapStream[1].cashflows);
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapParametricDefininionOfNotionalSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var notionalScheduleList = new List<Pair<DateTime, decimal>>
                                           {
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate, 1000000m),
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate.AddMonths(6*1),
                                                   1000000m*0.9m),
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate.AddMonths(6*2),
                                                   1000000m*0.8m),
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate.AddMonths(6*3),
                                                   1000000m*0.7m),
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate.AddMonths(6*4),
                                                   1000000m*0.6m)
                                           };

            NonNegativeSchedule notionalSchedule = NonNegativeScheduleHelper.Create(notionalScheduleList);
            Currency currency = CurrencyHelper.Parse(payLeg.Currency);//assuming we have the same ccy for both legs
            NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalSchedule, currency);

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            Swap swap = SwapGenerator.GenerateDefiniton(payLeg, receiveLeg);

            //  Apply notional schedule to BOTH streams
            //
            InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(swap.swapStream[0], amountSchedule);
            InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(swap.swapStream[1], amountSchedule);
            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNull(swap.swapStream[0].cashflows);
            Assert.IsNull(swap.swapStream[1].cashflows);
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapParametricDefininionOfNotionalScheduleAndSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var notionalScheduleList = new List<Pair<DateTime, decimal>>
                                           {
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate, 1000000m),
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate.AddMonths(6*1),
                                                   1000000m*0.9m),
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate.AddMonths(6*2),
                                                   1000000m*0.8m),
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate.AddMonths(6*3),
                                                   1000000m*0.7m),
                                               new Pair<DateTime, decimal>(
                                                   payLeg.EffectiveDate.AddMonths(6*4),
                                                   1000000m*0.6m)
                                           };

            NonNegativeSchedule notionalSchedule = NonNegativeScheduleHelper.Create(notionalScheduleList);
            Currency currency = CurrencyHelper.Parse(payLeg.Currency);//assuming we have the same ccy for both legs
            NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalSchedule, currency);

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            Swap swap = SwapGenerator.GenerateDefinitionCashflows(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, null, amountSchedule);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapCashflowsSpreadSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var spreadScheduleList = new List<Pair<DateTime, decimal>>
                                         {
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate, 0.002m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*1), 0.002m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*2), 0.003m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*3), 0.0045m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*4), 0.0050m)
                                         };

            Schedule spreadSchedule = ScheduleHelper.Create(spreadScheduleList);

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            Swap swap = SwapGenerator.GenerateDefinitionCashflows(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, spreadSchedule, null);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapAmountsSpreadSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var spreadScheduleList = new List<Pair<DateTime, decimal>>
                                         {
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate, 0.002m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*1), 0.002m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*2), 0.003m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*3), 0.0045m),
                                             new Pair<DateTime, decimal>(
                                                 payLeg.EffectiveDate.AddMonths(6*4), 0.0050m)
                                         };

            Schedule spreadSchedule = ScheduleHelper.Create(spreadScheduleList);

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            string marketEnvironmentId = Guid.NewGuid().ToString();
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 20);
            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, 
                                                                        payLeg, null, receiveLeg, null,
                                                                         null, spreadSchedule, null,
                                                                         marketEnvironment, valuationDT);

            MarketEnvironmentRegistry.Remove(marketEnvironmentId);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapVanilla()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            string marketEnvironmentId = Guid.NewGuid().ToString();
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 20);

            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null,
                                                                         null, null, null,
                                                                         marketEnvironment, valuationDT);

            MarketEnvironmentRegistry.Remove(marketEnvironmentId);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapVanillaWithPrincipalExchanges()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None",
                GeneratePrincipalExchanges = true
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None",
                GeneratePrincipalExchanges = true
            };

            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            string marketEnvironmentId = Guid.NewGuid().ToString();
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 14);

            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null,
                                                                         null, null, null,
                                                                         marketEnvironment, valuationDT);


            //            //  Add 4 PEX-s
            //            //
            //            InterestRateStream stream1 = swap.swapStream[0];
            //
            //            stream1.cashflows.principalExchange = new PrincipalExchange[2];
            //            stream1.cashflows.principalExchange[0] = new PrincipalExchange();
            //            stream1.cashflows.principalExchange[0].adjustedPrincipalExchangeDate = payLeg.EffectiveDate;
            //            stream1.cashflows.principalExchange[0].adjustedPrincipalExchangeDateSpecified = true;
            //            stream1.cashflows.principalExchange[0].discountFactor = 1;
            //            stream1.cashflows.principalExchange[0].discountFactorSpecified = true;
            //            stream1.cashflows.principalExchange[0].principalExchangeAmount = payLeg.NotionalAmount;
            //            stream1.cashflows.principalExchange[0].principalExchangeAmountSpecified = true;
            //            stream1.cashflows.principalExchange[0].presentValuePrincipalExchangeAmount = MoneyHelper.Mul(MoneyHelper.GetAmount(payLeg.NotionalAmount), stream1.cashflows.principalExchange[0].discountFactor);
            //
            //            stream1.cashflows.principalExchange[1] = new PrincipalExchange();
            //            stream1.cashflows.principalExchange[1].adjustedPrincipalExchangeDate = payLeg.MaturityDate;
            //            stream1.cashflows.principalExchange[1].adjustedPrincipalExchangeDateSpecified = true;
            //            stream1.cashflows.principalExchange[1].discountFactor = 0.7m;
            //            stream1.cashflows.principalExchange[1].discountFactorSpecified = true;
            //            stream1.cashflows.principalExchange[1].principalExchangeAmount = payLeg.NotionalAmount;
            //            stream1.cashflows.principalExchange[1].principalExchangeAmountSpecified = true;
            //            stream1.cashflows.principalExchange[1].presentValuePrincipalExchangeAmount = MoneyHelper.Mul(MoneyHelper.GetAmount(payLeg.NotionalAmount), stream1.cashflows.principalExchange[1].discountFactor);
            //
            //
            //            InterestRateStream stream2 = swap.swapStream[1];
            //            stream2.cashflows.principalExchange = new PrincipalExchange[2];
            //            stream2.cashflows.principalExchange[0] = new PrincipalExchange();
            //            stream2.cashflows.principalExchange[0].adjustedPrincipalExchangeDate = receiveLeg.EffectiveDate;
            //            stream2.cashflows.principalExchange[0].adjustedPrincipalExchangeDateSpecified = true;
            //            stream2.cashflows.principalExchange[0].discountFactor = 1;
            //            stream2.cashflows.principalExchange[0].discountFactorSpecified = true;
            //            stream2.cashflows.principalExchange[0].principalExchangeAmount = receiveLeg.NotionalAmount;
            //            stream2.cashflows.principalExchange[0].principalExchangeAmountSpecified = true;
            //            stream2.cashflows.principalExchange[0].presentValuePrincipalExchangeAmount = MoneyHelper.Mul(MoneyHelper.GetAmount(receiveLeg.NotionalAmount), stream2.cashflows.principalExchange[0].discountFactor);
            //
            //            stream2.cashflows.principalExchange[1] = new PrincipalExchange();
            //            stream2.cashflows.principalExchange[1].adjustedPrincipalExchangeDate = receiveLeg.MaturityDate;
            //            stream2.cashflows.principalExchange[1].adjustedPrincipalExchangeDateSpecified = true;
            //            stream2.cashflows.principalExchange[1].discountFactor = 0.7m;
            //            stream2.cashflows.principalExchange[1].discountFactorSpecified = true;
            //            stream2.cashflows.principalExchange[1].principalExchangeAmount = receiveLeg.NotionalAmount;
            //            stream2.cashflows.principalExchange[1].principalExchangeAmountSpecified = true;
            //            stream2.cashflows.principalExchange[1].presentValuePrincipalExchangeAmount = MoneyHelper.Mul(MoneyHelper.GetAmount(receiveLeg.NotionalAmount), stream2.cashflows.principalExchange[1].discountFactor);


            MarketEnvironmentRegistry.Remove(marketEnvironmentId);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapVanillaFixedVsFloatFloatVsFixedFixedVsFixedFloatVsFloat()
        {
            var fixedLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var floatLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            string marketEnvironmentId = Guid.NewGuid().ToString();
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 20);
            // Fixed vs Float
            //
            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, fixedLeg, null, floatLeg, null,
                                                                         null, null, null,
                                                                         marketEnvironment, valuationDT);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            // Float vs Fixed
            //
            swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, floatLeg, null, fixedLeg, null,
                                                                    null, null, null,
                                                                    marketEnvironment, valuationDT);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            // Fixed vs Fixed
            //
            swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, fixedLeg, null, fixedLeg, null,
                                                                    null, null, null,
                                                                    marketEnvironment, valuationDT);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            // Float vs Float
            //
            swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, floatLeg, null, floatLeg, null,
                                                                    null, null, null,
                                                                    marketEnvironment, valuationDT);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            MarketEnvironmentRegistry.Remove(marketEnvironmentId);
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapParametricDefininionOfNotionalScheduleFixedRateScheduleAndSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };


            var notionalScheduleList = new List<Pair<DateTime, decimal>>
                                           {
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate, 1000000m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*1),
                                                                           1000000m*0.9m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*2),
                                                                           1000000m*0.8m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*3),
                                                                           1000000m*0.7m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*4),
                                                                           1000000m*0.6m)
                                           };

            NonNegativeSchedule notionalSchedule = NonNegativeScheduleHelper.Create(notionalScheduleList);
            Currency currency = CurrencyHelper.Parse(payLeg.Currency);//assuming we have the same ccy for both legs
            NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalSchedule, currency);

            var fixedRateScheduleList = new List<Pair<DateTime, decimal>>
                                            {
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate, 0.07m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*1), 0.075m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*2), 0.080m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*3), 0.085m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*4), 0.084m)
                                            };

            Schedule fixedRateSchedule = ScheduleHelper.Create(fixedRateScheduleList);

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            Swap swap = SwapGenerator.GenerateDefinitionCashflows(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, fixedRateSchedule, null, amountSchedule);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapParametricDefininionOfFixedRateScheduleAndSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var fixedRateScheduleList = new List<Pair<DateTime, decimal>>
                                            {
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate, 0.07m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*1), 0.075m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*2), 0.080m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*3), 0.085m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*4), 0.084m)
                                            };

            Schedule fixedRateSchedule = ScheduleHelper.Create(fixedRateScheduleList);

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            //  Apply fixed rate schedule to stream
            //
            Swap swap = SwapGenerator.GenerateDefinitionCashflows(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, fixedRateSchedule, null, null);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

        }

        [TestMethod]
        public void GenerateSwapParametricDefininionOnly()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            Swap swap = SwapGenerator.GenerateDefiniton(payLeg, receiveLeg);


            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNull(swap.swapStream[0].cashflows);
            Assert.IsNull(swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapDefininionAndSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            Swap swap = SwapGenerator.GenerateDefinitionCashflows(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, null, null);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapParametricWithCashflowsStandardDiscounting()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            const string marketEnvironmentId = "1234567";
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 20);
            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache,
                CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, null, null, marketEnvironment, valuationDT);
            MarketEnvironmentRegistry.Remove("1234567");

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapParametricWithCashflowsNotionalAndFixedRateSchedule()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None"
            };

            var notionalScheduleList = new List<Pair<DateTime, decimal>>
                                           {
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate, 1000000m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*1),
                                                                           1000000m*0.9m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*2),
                                                                           1000000m*0.8m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*3),
                                                                           1000000m*0.7m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*4),
                                                                           1000000m*0.6m)
                                           };

            NonNegativeSchedule notionalSchedule = NonNegativeScheduleHelper.Create(notionalScheduleList);

            Currency currency = CurrencyHelper.Parse(payLeg.Currency);//assuming we have the same ccy for both legs
            NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalSchedule, currency);

            var fixedRateScheduleList = new List<Pair<DateTime, decimal>>
                                            {
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate, 0.07m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*1), 0.075m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*2), 0.080m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*3), 0.085m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*4), 0.084m)
                                            };

            Schedule fixedRateSchedule = ScheduleHelper.Create(fixedRateScheduleList);
            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            string marketEnvironmentId = Guid.NewGuid().ToString();
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 20);

            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null,
                                                                         fixedRateSchedule, null, amountSchedule,
                                                                         marketEnvironment, valuationDT);

            MarketEnvironmentRegistry.Remove(marketEnvironmentId);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapParametricWithCashflowsNotionalAndFixedRateSchedulePe()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "None",
                GeneratePrincipalExchanges = true
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "None",
                GeneratePrincipalExchanges = true
            };

            var notionalScheduleList = new List<Pair<DateTime, decimal>>
                                           {
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate, 1000000m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*1),
                                                                           1000000m*0.9m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*2),
                                                                           1000000m*0.8m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*3),
                                                                           1000000m*0.7m),
                                               new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*4),
                                                                           1000000m*0.6m)
                                           };

            NonNegativeSchedule notionalSchedule = NonNegativeScheduleHelper.Create(notionalScheduleList);
            Currency currency = CurrencyHelper.Parse(payLeg.Currency);//assuming we have the same ccy for both legs
            NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalSchedule, currency);

            var fixedRateScheduleList = new List<Pair<DateTime, decimal>>
                                            {
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate, 0.07m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*1), 0.075m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*2), 0.080m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*3), 0.085m),
                                                new Pair<DateTime, decimal>(payLeg.EffectiveDate.AddMonths(6*4), 0.084m)
                                            };

            Schedule fixedRateSchedule = ScheduleHelper.Create(fixedRateScheduleList);
            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            string marketEnvironmentId = Guid.NewGuid().ToString();
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 14);

            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null,
                                                                         fixedRateSchedule, null, amountSchedule,
                                                                         marketEnvironment, valuationDT);

            MarketEnvironmentRegistry.Remove(marketEnvironmentId);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapParametricWithCashflowsFraDiscounting()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                DiscountingType = "Standard"
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR-3M",
                ForecastCurve = "AUD-LIBOR-3M",
                DiscountingType = "Standard"
            };

            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            string marketEnvironmentId = Guid.NewGuid().ToString();
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 14);

            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, null, null, marketEnvironment, valuationDT);
            //Swap swap = SwapGenerator.GenerateDefinitionCashflows(payLeg, receiveLeg);

            MarketEnvironmentRegistry.Remove(marketEnvironmentId);

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        [TestMethod]
        public void GenerateSwapParametricWithCashflowsUpdateSpreadFixedRateNotinalInFirstTwoCFs()
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING"
            };

            string curveId = "AUD-LIBOR-3M" + Guid.NewGuid();
            payLeg.DiscountCurve = curveId;
            payLeg.DiscountingType = "None";

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                MaturityDate = new DateTime(1999, 12, 14),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0.01m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = curveId,
                ForecastCurve = curveId,
                DiscountingType = "None"
            };

            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            string marketEnvironmentId = Guid.NewGuid().ToString();
            marketEnvironment.Id = marketEnvironmentId;
            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 20);
            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, null, null, marketEnvironment, valuationDT);
            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);

            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));


            InterestRateStream fixedStream = swap.swapStream[0];//fixed
            InterestRateStream floatStream = swap.swapStream[1];//floating

            // Check spread
            //
            Assert.AreEqual(GetSpread(floatStream.cashflows.paymentCalculationPeriod[0]), receiveLeg.FloatingRateSpread);
            Assert.AreEqual(GetSpread(floatStream.cashflows.paymentCalculationPeriod[1]), receiveLeg.FloatingRateSpread);

            // Update spread in a first two cashflows
            // 
            SetSpread(floatStream.cashflows.paymentCalculationPeriod[0], 0.0002m);
            SetSpread(floatStream.cashflows.paymentCalculationPeriod[1], 0.0003m);

            // Update notional 
            //
            SetNotional(fixedStream.cashflows.paymentCalculationPeriod[0], 1000000);
            SetNotional(fixedStream.cashflows.paymentCalculationPeriod[1], 900000);
            SetNotional(fixedStream.cashflows.paymentCalculationPeriod[2], 500000);
            SetNotional(fixedStream.cashflows.paymentCalculationPeriod[3], 1500000);

            SetNotional(floatStream.cashflows.paymentCalculationPeriod[0], 1000001);
            SetNotional(floatStream.cashflows.paymentCalculationPeriod[1], 900001);
            SetNotional(floatStream.cashflows.paymentCalculationPeriod[2], 500001);
            SetNotional(floatStream.cashflows.paymentCalculationPeriod[3], 1500001);

            // Update fixed rate
            //
            SetFixedRate(fixedStream.cashflows.paymentCalculationPeriod[0], 0.071m);
            SetFixedRate(fixedStream.cashflows.paymentCalculationPeriod[1], 0.072m);

            // Update amounts - this call should not change the manually update values
            //
            //
            IRateCurve payStreamDiscountingCurve = marketEnvironment.GetDiscountRateCurve();

            // Update fixed
            //
            //FixedRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, payStreamDiscountingCurve, valuationDT);
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(fixedStream, null, payStreamDiscountingCurve, valuationDT);

            IRateCurve receiveStreamForecastCurve = marketEnvironment.GetForecastRateCurve();
            IRateCurve receiveStreamDiscountingCurve = payStreamDiscountingCurve;

            // Update float
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(floatStream, receiveStreamForecastCurve, receiveStreamDiscountingCurve, valuationDT);

            // Check spread
            //
            Assert.AreEqual(GetSpread(floatStream.cashflows.paymentCalculationPeriod[0]), 0.0002m);
            Assert.AreEqual(GetSpread(floatStream.cashflows.paymentCalculationPeriod[1]), 0.0003m);

            // Check notional
            //
            Assert.AreEqual(GetNotional(fixedStream.cashflows.paymentCalculationPeriod[0]), 1000000);
            Assert.AreEqual(GetNotional(fixedStream.cashflows.paymentCalculationPeriod[1]), 900000);
            Assert.AreEqual(GetNotional(fixedStream.cashflows.paymentCalculationPeriod[2]), 500000);
            Assert.AreEqual(GetNotional(fixedStream.cashflows.paymentCalculationPeriod[3]), 1500000);

            Assert.AreEqual(GetNotional(floatStream.cashflows.paymentCalculationPeriod[0]), 1000001);
            Assert.AreEqual(GetNotional(floatStream.cashflows.paymentCalculationPeriod[1]), 900001);
            Assert.AreEqual(GetNotional(floatStream.cashflows.paymentCalculationPeriod[2]), 500001);
            Assert.AreEqual(GetNotional(floatStream.cashflows.paymentCalculationPeriod[3]), 1500001);

            // Check fixed rate
            //
            Assert.AreEqual(GetFixedRate(fixedStream.cashflows.paymentCalculationPeriod[0]), 0.071m);
            Assert.AreEqual(GetFixedRate(fixedStream.cashflows.paymentCalculationPeriod[1]), 0.072m);


            Debug.WriteLine(XmlSerializerHelper.SerializeToString(swap));
        }

        #endregion

        #endregion

        #region Test CapFloor Pricer

        #region Methods

        private static Schedule GetSpreadSchedule(CapFloorLegParametersRange_Old capFloorLegParametersRange)
        {
            var spreadScheduleList = new List<Pair<DateTime, decimal>>
                                         {
                                             new Pair<DateTime, decimal>(capFloorLegParametersRange.EffectiveDate,
                                                                         0.002m),
                                             new Pair<DateTime, decimal>(
                                                 capFloorLegParametersRange.EffectiveDate.AddMonths(6*1), 0.002m),
                                             new Pair<DateTime, decimal>(
                                                 capFloorLegParametersRange.EffectiveDate.AddMonths(6*2), 0.003m),
                                             new Pair<DateTime, decimal>(
                                                 capFloorLegParametersRange.EffectiveDate.AddMonths(6*3), 0.0045m),
                                             new Pair<DateTime, decimal>(
                                                 capFloorLegParametersRange.EffectiveDate.AddMonths(6*4), 0.0050m)
                                         };

            return ScheduleHelper.Create(spreadScheduleList);
        }

        private static Schedule GetStrikeRateSchedule(CapFloorLegParametersRange_Old capFloorLegParametersRange)
        {
            var capRateScheduleList = new List<Pair<DateTime, decimal>>
                                          {
                                              new Pair<DateTime, decimal>(capFloorLegParametersRange.EffectiveDate,
                                                                          0.07m),
                                              new Pair<DateTime, decimal>(
                                                  capFloorLegParametersRange.EffectiveDate.AddMonths(6*1), 0.075m),
                                              new Pair<DateTime, decimal>(
                                                  capFloorLegParametersRange.EffectiveDate.AddMonths(6*2), 0.080m),
                                              new Pair<DateTime, decimal>(
                                                  capFloorLegParametersRange.EffectiveDate.AddMonths(6*3), 0.085m),
                                              new Pair<DateTime, decimal>(
                                                  capFloorLegParametersRange.EffectiveDate.AddMonths(6*4), 0.084m)
                                          };

            return ScheduleHelper.Create(capRateScheduleList);
        }

        private static NonNegativeAmountSchedule GetNotionalSchedule(CapFloorLegParametersRange_Old capFloorLegParametersRange)
        {
            var notionalScheduleList = new List<Pair<DateTime, decimal>>
                                           {
                                               new Pair<DateTime, decimal>(capFloorLegParametersRange.EffectiveDate,
                                                                           1000000m),
                                               new Pair<DateTime, decimal>(
                                                   capFloorLegParametersRange.EffectiveDate.AddMonths(6*1),
                                                   1000000m*0.9m),
                                               new Pair<DateTime, decimal>(
                                                   capFloorLegParametersRange.EffectiveDate.AddMonths(6*2),
                                                   1000000m*0.8m),
                                               new Pair<DateTime, decimal>(
                                                   capFloorLegParametersRange.EffectiveDate.AddMonths(6*3),
                                                   1000000m*0.7m),
                                               new Pair<DateTime, decimal>(
                                                   capFloorLegParametersRange.EffectiveDate.AddMonths(6*4),
                                                   1000000m*0.6m)
                                           };


            NonNegativeSchedule schedule = NonNegativeScheduleHelper.Create(notionalScheduleList);

            Currency currency = CurrencyHelper.Parse(capFloorLegParametersRange.Currency);
            NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(schedule, currency);

            return amountSchedule;
        }

        private const string _nab = "NAB";
        private const string Counterparty = "CounterParty";

        private static CapFloorLegParametersRange_Old GetCapFloorInputParameters(CapFloorType capFloorType, string discountingType)
        {
            var floorLegParametersRange = new CapFloorLegParametersRange_Old
            {
                Payer = _nab,
                Receiver = Counterparty,
                EffectiveDate = new DateTime(1994, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 12, 14),
                MaturityDate = new DateTime(1999, 12, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                NotionalAmount = 50000000,
                Currency = "AUD",
                PaymentFrequency = "6M",
                DayCount = "30E/360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "GBLO",
                FixingBusinessDayAdjustments = "FOLLOWING",
                DiscountCurve = "AUD-LIBOR-BBA.3M",
                ForecastCurve = "AUD-LIBOR-BBA.3M",
                DiscountingType = discountingType,
                CapOrFloor = capFloorType,
                StrikeRate = 0.060m
            };


            //result.LegType = LegType.Fixed;
            //result.CouponOrLastResetRate = 0.058m;

            return floorLegParametersRange;
        }

        #endregion

        #region Tests

        [TestMethod]
        public void CapGenerateDefitinion()
        {
            CapFloorLegParametersRange_Old capFloorLegParametersRange = GetCapFloorInputParameters(CapFloorType.Cap, "None");

            Schedule spreadSchedule = null;
            Schedule capOrFloorSchedule = null;
            NonNegativeSchedule notionalSchedule = null;

            CapFloor capFloor = CapFloorGenerator.GenerateDefiniton(capFloorLegParametersRange, spreadSchedule, capOrFloorSchedule, notionalSchedule);

            Assert.IsNotNull(capFloor.capFloorStream);
            Assert.IsNotNull(capFloor.capFloorStream.calculationPeriodAmount);

            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(capFloor.capFloorStream.calculationPeriodAmount);

            FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);
            Assert.IsNotNull(floatingRateCalculation.capRateSchedule);
            Assert.IsNull(floatingRateCalculation.floorRateSchedule);

            Assert.IsNull(capFloor.capFloorStream.cashflows);

            Debug.WriteLine("CapFloorGenerator.GenerateDefiniton (Cap):");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(capFloor));
        }

        [TestMethod]
        public void FloorGenerateDefitinion()
        {
            CapFloorLegParametersRange_Old capFloorLegParametersRange = GetCapFloorInputParameters(CapFloorType.Floor, "None");

            Schedule spreadSchedule = null;
            Schedule capOrFloorSchedule = null;
            NonNegativeSchedule notionalSchedule = null;

            CapFloor capFloor = CapFloorGenerator.GenerateDefiniton(capFloorLegParametersRange, spreadSchedule, capOrFloorSchedule, notionalSchedule);

            Assert.IsNotNull(capFloor.capFloorStream);
            Assert.IsNotNull(capFloor.capFloorStream.calculationPeriodAmount);

            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(capFloor.capFloorStream.calculationPeriodAmount);

            FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);
            Assert.IsNull(floatingRateCalculation.capRateSchedule);
            Assert.IsNotNull(floatingRateCalculation.floorRateSchedule);

            Assert.IsNull(capFloor.capFloorStream.cashflows);

            Debug.WriteLine("CapFloorGenerator.GenerateDefiniton (Cap):");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(capFloor));
        }


        [TestMethod]
        public void CapGenerateDefitinionAndCashflows()
        {
            CapFloor capFloor = GenerateCapDefinitionAndCashflows();
            Debug.WriteLine("CapFloorGenerator.GenerateDefiniton (Cap):");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(capFloor));

            #region post conditions

            Assert.IsNotNull(capFloor.capFloorStream);
            Assert.IsNotNull(capFloor.capFloorStream.calculationPeriodAmount);

            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(capFloor.capFloorStream.calculationPeriodAmount);

            FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);
            Assert.IsNotNull(floatingRateCalculation.capRateSchedule);
            Assert.IsNull(floatingRateCalculation.floorRateSchedule);
            Assert.IsNotNull(capFloor.capFloorStream.cashflows);

            #endregion
        }

        public CapFloor GenerateCapDefinitionAndCashflows()
        {
            CapFloorLegParametersRange_Old capFloorLegParametersRange = GetCapFloorInputParameters(CapFloorType.Cap, "None");

            NonNegativeAmountSchedule notionalSchedule = GetNotionalSchedule(capFloorLegParametersRange);

            Schedule capRateSchedule = GetStrikeRateSchedule(capFloorLegParametersRange);

            Schedule spreadSchedule = GetSpreadSchedule(capFloorLegParametersRange);

            return CapFloorGenerator.GenerateDefinitionCashflows(FixingCalendar, PaymentCalendar, capFloorLegParametersRange, spreadSchedule, capRateSchedule, notionalSchedule);
        }

        public CapFloor GenerateFloorDefinitionAndCashflows()
        {
            CapFloorLegParametersRange_Old capFloorLegParametersRange = GetCapFloorInputParameters(CapFloorType.Floor, "None");

            NonNegativeAmountSchedule notionalSchedule = GetNotionalSchedule(capFloorLegParametersRange);

            Schedule capRateSchedule = GetStrikeRateSchedule(capFloorLegParametersRange);

            Schedule spreadSchedule = GetSpreadSchedule(capFloorLegParametersRange);

            return CapFloorGenerator.GenerateDefinitionCashflows(FixingCalendar, PaymentCalendar, capFloorLegParametersRange, spreadSchedule, capRateSchedule, notionalSchedule);
        }

        [TestMethod]
        public void FloorGenerateDefitinionAndCashflows()
        {
            CapFloorLegParametersRange_Old capFloorLegParametersRange = GetCapFloorInputParameters(CapFloorType.Floor, "None");

            NonNegativeAmountSchedule notionalSchedule = GetNotionalSchedule(capFloorLegParametersRange);

            Schedule floorRateSchedule = GetStrikeRateSchedule(capFloorLegParametersRange);

            Schedule spreadSchedule = GetSpreadSchedule(capFloorLegParametersRange);

            CapFloor capFloor = CapFloorGenerator.GenerateDefinitionCashflows(FixingCalendar, PaymentCalendar, capFloorLegParametersRange, spreadSchedule, floorRateSchedule, notionalSchedule);

            #region post conditions

            Assert.IsNotNull(capFloor.capFloorStream);
            Assert.IsNotNull(capFloor.capFloorStream.calculationPeriodAmount);

            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(capFloor.capFloorStream.calculationPeriodAmount);

            FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);
            Assert.IsNull(floatingRateCalculation.capRateSchedule);
            Assert.IsNotNull(floatingRateCalculation.floorRateSchedule);
            Assert.IsNotNull(capFloor.capFloorStream.cashflows);

            #endregion

            Debug.WriteLine("CapFloorGenerator.GenerateDefiniton (Cap):");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(capFloor));

        }

        [TestMethod]
        public void CapGenerateDefitinionAndCashflowsAndAmounts()
        {
            CapFloorLegParametersRange_Old capFloorLegParametersRange = GetCapFloorInputParameters(CapFloorType.Cap, "None");

            NonNegativeAmountSchedule notionalSchedule = GetNotionalSchedule(capFloorLegParametersRange);


            Schedule capRateSchedule = GetStrikeRateSchedule(capFloorLegParametersRange);


            Schedule spreadSchedule = GetSpreadSchedule(capFloorLegParametersRange);

            DateTime valuationDT = capFloorLegParametersRange.EffectiveDate;


            //IRateCurve rateCurve = RateCurveTests.CreateAUD_LIBOR_BBA_3MonthFlat8PercentCurve_WithBankBillFutures(valuationDT, valuationDT, "AUD-LIBOR-BBA");

            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(valuationDT);
            Guid marketEnvironmentId = Guid.NewGuid();
            //MarketEnvironment marketEnvironment = new MarketEnvironment(marketEnvironmentId.ToString());
            marketEnvironment.Id = marketEnvironmentId.ToString();

            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);

            //marketEnvironment.Add(capFloorLegParametersRange.DiscountCurve, rateCurve);
            //MarketEnvironmentRegistry.Add(marketEnvironment);


            CapFloor capFloor = CapFloorGenerator.GenerateDefinitionCashflowsAmounts(FixingCalendar, PaymentCalendar, capFloorLegParametersRange, spreadSchedule, capRateSchedule, notionalSchedule, marketEnvironment, valuationDT);

            MarketEnvironmentRegistry.Remove(marketEnvironmentId.ToString());


            #region post conditions

            Assert.IsNotNull(capFloor.capFloorStream);
            Assert.IsNotNull(capFloor.capFloorStream.calculationPeriodAmount);

            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(capFloor.capFloorStream.calculationPeriodAmount);

            FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);
            Assert.IsNotNull(floatingRateCalculation.capRateSchedule);
            Assert.IsNull(floatingRateCalculation.floorRateSchedule);
            Assert.IsNotNull(capFloor.capFloorStream.cashflows);

            #endregion

            Debug.WriteLine("CapFloorGenerator.GenerateDefinitionCashflowsAmounts (Cap):");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(capFloor));
        }
        //
        //
        //            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(payLeg, receiveLeg,
        //                                                                        null, spreadSchedule, null,
        //                                                                        marketEnvironment, valuationDT);
        //

        [TestMethod]
        public void FloorGenerateDefitinionAndCashflowsAndAmounts()
        {
            CapFloorLegParametersRange_Old capFloorLegParametersRange = GetCapFloorInputParameters(CapFloorType.Floor, "None");

            NonNegativeAmountSchedule notionalSchedule = GetNotionalSchedule(capFloorLegParametersRange);

            Schedule floorRateSchedule = GetStrikeRateSchedule(capFloorLegParametersRange);

            Schedule spreadSchedule = GetSpreadSchedule(capFloorLegParametersRange);

            DateTime valuationDT = capFloorLegParametersRange.EffectiveDate;


            //RateCurve rateCurve = RateCurveTests.CreateAUD_LIBOR_BBA_3MonthFlat8PercentCurve_WithBankBillFutures(valuationDT, valuationDT, "AUD-LIBOR-BBA");

            //Guid marketEnvironmentId = Guid.NewGuid();
            //MarketEnvironment marketEnvironment = new MarketEnvironment(marketEnvironmentId.ToString());
            //marketEnvironment.Add(capFloorLegParametersRange.DiscountCurve, rateCurve);
            //MarketEnvironmentRegistry.Add(marketEnvironment);

            ISwapLegEnvironment marketEnvironment = CreateInterestRateStreamTestEnvironment(valuationDT);
            Guid marketEnvironmentId = Guid.NewGuid();
            //MarketEnvironment marketEnvironment = new MarketEnvironment(marketEnvironmentId.ToString());
            marketEnvironment.Id = marketEnvironmentId.ToString();

            MarketEnvironmentRegistry.Add((MarketEnvironment)marketEnvironment);

            CapFloor capFloor = CapFloorGenerator.GenerateDefinitionCashflowsAmounts(FixingCalendar, PaymentCalendar, capFloorLegParametersRange, spreadSchedule, floorRateSchedule, notionalSchedule, marketEnvironment, valuationDT);
            MarketEnvironmentRegistry.Remove(marketEnvironmentId.ToString());


            #region post conditions

            Assert.IsNotNull(capFloor.capFloorStream);
            Assert.IsNotNull(capFloor.capFloorStream.calculationPeriodAmount);

            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(capFloor.capFloorStream.calculationPeriodAmount);

            FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);
            Assert.IsNull(floatingRateCalculation.capRateSchedule);
            Assert.IsNotNull(floatingRateCalculation.floorRateSchedule);
            Assert.IsNotNull(capFloor.capFloorStream.cashflows);

            #endregion

            Debug.WriteLine("CapFloorGenerator.GenerateDefitinionAndCashflowsAndAmounts (Floor):");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(capFloor));
        }

        #endregion

        #endregion

        #region Test BondTransactionPricer

        [TestMethod]
        public void PriceableBondTransaction()
        {
            string buyer = "Party1";
            string seller = "Party2";
            var identifier = String.Format("{0}.{1}.{2}", "Trade", "bondTransaction", "0001");
            var productType = new object[] { ProductTypeHelper.Create("BondTransaction") };
            //var productId = new ProductId {Value = "BondTransation"};
            //var productIds = new[] {productId};
            var bondTransaction = new BondTransaction
            {
                notionalAmount = MoneyHelper.GetAmount(1000000m, "AUD"),
                buyerPartyReference =
                    PartyReferenceHelper.Parse(buyer),
                id = identifier,
                Items = productType,
                ItemsElementName = new[] { ItemsChoiceType2.productType },
                sellerPartyReference =
                    PartyReferenceHelper.Parse(seller),
                bond = new Bond(),
                price = new BondPrice()
            };
            var bondTemp = XmlSerializerHelper.SerializeToString(bondTransaction);
            Debug.Print(bondTemp);
            var result = XmlSerializerHelper.DeserializeFromString<BondTransaction>(bondTemp);
            Debug.Print(result.id);
            var trade = new Trade
            {
                Item = bondTransaction,
                ItemElementName = ItemChoiceType15.bondTransaction,//TODO this has been added to the enum, but is a non schema product.
                tradeHeader = new TradeHeader(), //TODO We need a new type here!
            };
            var party1 = PartyTradeIdentifierHelper.Parse("0001", "party1");
            var party2 = PartyTradeIdentifierHelper.Parse("0001", "party2");
            trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
            trade.tradeHeader.tradeDate = new IdentifiedDate { Value = new DateTime() };
            var tradeTemp = XmlSerializerHelper.SerializeToString(trade);
            Debug.Print(tradeTemp);
            var result2 = XmlSerializerHelper.DeserializeFromString<Trade>(tradeTemp);
            Debug.Print(result2.id);
        }

        #endregion

        #region Valuation Tests

        #region Methods

        public static string[] GetPayRelativeToEnumValues()
        {
            return new[]
                       {
                           "CalculationPeriodEndDate",
                           "CalculationPeriodStartDate",
                           "ResetDate"
                       };
        }

        public static List<PartyIdRangeItem> GetPartyList(string name1, string role1, string name2, string role2)
        {
            var partyIdList = new List<PartyIdRangeItem>();
            var item1 = new PartyIdRangeItem { PartyId = name1, IdOrRole = role1 };
            partyIdList.Add(item1);
            var item2 = new PartyIdRangeItem { PartyId = name2, IdOrRole = role2 };
            partyIdList.Add(item2);
            return partyIdList;
        }

        private static void UpdateValuationReportWithParties(ValuationReport valuationReport)
        {
            List<PartyIdRangeItem> partyList = GetPartyList(TestConstants.CyMLBookName, "book", TestConstants.CyMLCounterpartyName, "counterparty");
            if (null != partyList)
            {
                var listOfParties = new List<Party>();
                foreach (PartyIdRangeItem partyIdRangeItem in partyList)
                {
                    Party party = PartyFactory.Create(partyIdRangeItem.IdOrRole);
                    var partyId = new PartyId { Value = partyIdRangeItem.PartyId };
                    party.partyId = new[] { partyId };
                    listOfParties.Add(party);
                }
                valuationReport.party = listOfParties.ToArray();
            }
        }

        private static void UpdateValuationReportWithOtherPayments(ValuationReport valuationReport)
        {
            IEnumerable<OtherPartyPaymentRangeItem> otherPartyPaymentList = GetOtherPartyPaymentList("book", "cost centre");
            var otherPartyPayments = new List<Payment>();
            //  other party payments
            //  
            if (null != otherPartyPaymentList)
            {
                foreach (OtherPartyPaymentRangeItem item in otherPartyPaymentList)
                {
                    var otherPartyPayment = new Payment
                    {
                        payerPartyReference =
                            PartyReferenceFactory.Create(item.Payer),
                        receiverPartyReference =
                            PartyReferenceFactory.Create(item.Receiver),
                        paymentAmount = MoneyHelper.GetNonNegativeAmount(item.Amount),
                        paymentDate = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(item.PaymentDate),
                        paymentType = PaymentTypeHelper.Create(item.PaymentType)
                    };
                    otherPartyPayments.Add(otherPartyPayment);
                }
            }

            TradeValuationItem valuationItem = valuationReport.tradeValuationItem[0];
            Trade[] tradeArray = XsdClassesFieldResolver.TradeValuationItemGetTradeArray(valuationItem);
            Trade trade = tradeArray[0];
            trade.otherPartyPayment = otherPartyPayments.ToArray();
        }

        private static IEnumerable<OtherPartyPaymentRangeItem> GetOtherPartyPaymentList(string payer, string receiver)
        {
            var result = new List<OtherPartyPaymentRangeItem>();
            var item1 = new OtherPartyPaymentRangeItem
            {
                PaymentDate = DateTime.Today,
                PaymentType = "Value Add SOFT",
                Amount = 5000,
                Payer = payer,
                Receiver = receiver
            };
            result.Add(item1);

            var item2 = new OtherPartyPaymentRangeItem
            {
                PaymentDate = DateTime.Today,
                PaymentType = "Value Add HARD",
                Amount = 15000,
                Payer = payer,
                Receiver = receiver
            };
            result.Add(item2);

            return result;
        }

        private ValuationReport CreateValuationReport(string valuationReportId)
        {
            //var resultRange = new ValuationResultRange {PresentValue = 10000m, FutureValue = 20000m};
            //RateCurve rateCurve = RateCurveTests.CreateAudBbrBbsw3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);
            Swap swap = GenerateSwapParametricWithCashflows(false);
            //Pair<YieldCurve, YieldCurveValuation> pair = new RateCurveAssembler().CreateDTO(rateCurve);
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();
            var marketFactory = new CurveEngine.Factory.MarketFactory();
            marketFactory.AddYieldCurve(pair);
            Market market = marketFactory.Create();
            const string baseParty = "NAB";
            string tradeId = valuationReportId;
            DateTime tradeDate = DateTime.Today;
            List<StringObjectRangeItem> stringObjectRangeItems = CreateValuationSetList(123545, 12.34);
            AssetValuation valuationSet = InterestRateProduct.CreateAssetValuationFromValuationSet(stringObjectRangeItems);
            return ValuationReportGenerator.Generate(valuationReportId, baseParty, tradeId, tradeDate, swap, market, valuationSet);
        }


        private const string _NAB = "NAB";

        [TestMethod]
        public void TestCreateSwaptionValuationReport()
        {
            ValuationReport valuationReport = CreateSwaptionValuationReport();

            Debug.WriteLine("ValuationReport:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString<Document>(valuationReport));

            string s1 = XmlSerializerHelper.SerializeToString<Document>(valuationReport);

            XmlSerializerHelper.DeserializeFromString<ValuationReport>(typeof(Document), s1);

            string tempFileName = Path.GetTempFileName();
            XmlSerializerHelper.SerializeToFile(typeof(Document), valuationReport, tempFileName);

            XmlSerializerHelper.DeserializeFromFile<ValuationReport>(typeof(Document), tempFileName);
            File.Delete(tempFileName);
        }

        #region CreateXXXValuationReport

        public ValuationReport CreateCapValuationReport()
        {
            //var resultRange = new ValuationResultRange {PresentValue = 10000m, FutureValue = 20000m};
            //RateCurve rateCurve = RateCurveTests.CreateAudBbrBbsw3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);
            CapFloor cap = GenerateCapDefinitionAndCashflows();
            //Pair<YieldCurve, YieldCurveValuation> pair = new RateCurveAssembler().CreateDTO(rateCurve);
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();
            var marketFactory = new CurveEngine.Factory.MarketFactory();
            marketFactory.AddYieldCurve(pair);
            Market market = marketFactory.Create();
            const string baseParty = "NAB";
            string tradeId = Guid.NewGuid().ToString();
            DateTime tradeDate = DateTime.Today;
            List<StringObjectRangeItem> stringObjectRangeItems = CreateValuationSetList(123545, 12.34);
            AssetValuation valuationSet = InterestRateProduct.CreateAssetValuationFromValuationSet(stringObjectRangeItems);
            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty, tradeId, tradeDate, cap, market, valuationSet);
            UpdateValuationReportWithParties(valuationReport);
            UpdateValuationReportWithOtherPayments(valuationReport);
            return valuationReport;
        }

        public ValuationReport CreateFloorValuationReport()
        {
            //var resultRange = new ValuationResultRange {PresentValue = 10000m, FutureValue = 20000m};
            //RateCurve rateCurve = RateCurveTests.CreateAudBbrBbsw3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);
            CapFloor floor = GenerateFloorDefinitionAndCashflows();
            //Pair<YieldCurve, YieldCurveValuation> pair = new RateCurveAssembler().CreateDTO(rateCurve);
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();
            var marketFactory = new CurveEngine.Factory.MarketFactory();
            marketFactory.AddYieldCurve(pair);
            Market market = marketFactory.Create();
            const string baseParty = "NAB";
            string tradeId = Guid.NewGuid().ToString();
            DateTime tradeDate = DateTime.Today;
            List<StringObjectRangeItem> stringObjectRangeItems = CreateValuationSetList(123545, 12.34);
            AssetValuation valuationSet = InterestRateProduct.CreateAssetValuationFromValuationSet(stringObjectRangeItems);
            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty, tradeId, tradeDate, floor, market, valuationSet);
            UpdateValuationReportWithParties(valuationReport);
            UpdateValuationReportWithOtherPayments(valuationReport);
            return valuationReport;
        }

        public ValuationReport CreateSwapValuationReport2()
        {
            //var resultRange = new ValuationResultRange {PresentValue = 10000m, FutureValue = 20000m};
            //RateCurve rateCurve = RateCurveTests.CreateAudBbrBbsw3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);
            Swap swap = GenerateSwapParametricWithCashflows(false);
            //Pair<YieldCurve, YieldCurveValuation> pair = new RateCurveAssembler().CreateDTO(rateCurve);
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();
            var marketFactory = new Orion.CurveEngine.Factory.MarketFactory();
            marketFactory.AddYieldCurve(pair);
            Market market = marketFactory.Create();
            const string baseParty = "NAB";
            string tradeId = Guid.NewGuid().ToString();
            DateTime tradeDate = DateTime.Today;
            //CreateAssetValuationFromValuationSet
            List<StringObjectRangeItem> stringObjectRangeItems = CreateValuationSetList(123545, 12.34);
            AssetValuation valuationSet = InterestRateProduct.CreateAssetValuationFromValuationSet(stringObjectRangeItems);
            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty, tradeId, tradeDate, swap, market, valuationSet);
            UpdateValuationReportWithParties(valuationReport);
            UpdateValuationReportWithOtherPayments(valuationReport);
            return valuationReport;
        }

        public ValuationReport CreateCappedSwapValuationReport()
        {
            ValuationReport swap = CreateSwapValuationReport2();
            ValuationReport cap = CreateCapValuationReport();
            ValuationReport cappedSwap = ValuationReportGenerator.Merge(swap, cap);
            return cappedSwap;
        }

        //        NABOrigPV01
        //            NABOrigNPV

        public ValuationReport CreateSwapValuationReportPEX()
        {
            //var resultRange = new ValuationResultRange {PresentValue = 10000m, FutureValue = 20000m};
            //RateCurve rateCurve = RateCurveTests.CreateAudLiborBba3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);
            //RateCurve rateCurve = RateCurveTests.CreateAudBbrBbsw3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(new DateTime(1994, 12, 14), new DateTime(1994, 12, 14));
            Swap swap = GenerateSwapParametricWithCashflows(true);
            //Pair<YieldCurve, YieldCurveValuation> pair = new RateCurveAssembler().CreateDTO(rateCurve);
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();
            var marketFactory = new CurveEngine.Factory.MarketFactory();
            marketFactory.AddYieldCurve(pair);
            Market market = marketFactory.Create();
            const string baseParty = "NAB";
            string tradeId = Guid.NewGuid().ToString();
            DateTime tradeDate = DateTime.Today;
            List<StringObjectRangeItem> stringObjectRangeItems = CreateValuationSetList(123545, 12.34);
            AssetValuation valuationSet = InterestRateProduct.CreateAssetValuationFromValuationSet(stringObjectRangeItems);
            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty, tradeId, tradeDate, swap, market, valuationSet);
            UpdateValuationReportWithParties(valuationReport);
            UpdateValuationReportWithOtherPayments(valuationReport);
            return valuationReport;
        }

        public ValuationReport CreateSwaptionValuationReport()
        {
            //var resultRange = new ValuationResultRange { PresentValue = 10000m, FutureValue = 20000m };
            //RateCurve rateCurve = RateCurveTests.CreateAudBbrBbsw3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);
            Swaption swaption = GenerateSwaptionParametricWithCashflows(false);
            //Pair<YieldCurve, YieldCurveValuation> pair = new RateCurveAssembler().CreateDTO(rateCurve);
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();
            var marketFactory = new Orion.CurveEngine.Factory.MarketFactory();
            marketFactory.AddYieldCurve(pair);
            Market market = marketFactory.Create();
            const string baseParty = _NAB;
            List<StringObjectRangeItem> stringObjectRangeItems = CreateValuationSetList(123545, 12.34);
            AssetValuation valuationSet = InterestRateProduct.CreateAssetValuationFromValuationSet(stringObjectRangeItems);
            string tradeId = Guid.NewGuid().ToString();
            DateTime tradeDate = DateTime.Today;
            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty,
                                                                                tradeId, tradeDate,
                                                                                swaption, market, valuationSet);
            UpdateValuationReportWithParties(valuationReport);
            UpdateValuationReportWithOtherPayments(valuationReport);
            return valuationReport;
        }

        public ValuationReport CreateSwaptionPEXValuationReport()
        {
            //var resultRange = new ValuationResultRange { PresentValue = 10000m, FutureValue = 20000m };
            //RateCurve rateCurve = RateCurveTests.CreateAudBbrBbsw3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);
            Swaption swaption = GenerateSwaptionParametricWithCashflows(true);
            //Pair<YieldCurve, YieldCurveValuation> pair = new RateCurveAssembler().CreateDTO(rateCurve);
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();
            var marketFactory = new CurveEngine.Factory.MarketFactory();
            marketFactory.AddYieldCurve(pair);
            Market market = marketFactory.Create();
            const string baseParty = _NAB;
            List<StringObjectRangeItem> stringObjectRangeItems = CreateValuationSetList(123545, 12.34);
            AssetValuation valuationSet = InterestRateProduct.CreateAssetValuationFromValuationSet(stringObjectRangeItems);
            string tradeId = Guid.NewGuid().ToString();
            DateTime tradeDate = DateTime.Today;
            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty,
                                                                                tradeId, tradeDate,
                                                                                swaption, market, valuationSet);
            UpdateValuationReportWithParties(valuationReport);
            UpdateValuationReportWithOtherPayments(valuationReport);
            return valuationReport;
        }

        #endregion


        private static List<StringObjectRangeItem> CreateValuationSetList(double npv, double pv01)
        {
            var list = new List<StringObjectRangeItem>();

            //var npvItem = new StringObjectRangeItem {StringValue = "NABOrigNPV", ObjectValue = npv};
            var npvItem = new StringObjectRangeItem { StringValue = AssetMeasureScheme.GetEnumString(AssetMeasureEnum.NPV), ObjectValue = npv };
            list.Add(npvItem);

            //var dv01Item = new StringObjectRangeItem { StringValue = "NABOrigPV01", ObjectValue = pv01 };
            var dv01Item = new StringObjectRangeItem { StringValue = AssetMeasureScheme.GetEnumString(AssetMeasureEnum.DE_R), ObjectValue = pv01 };
            list.Add(dv01Item);

            //var stringItem = new StringObjectRangeItem {StringValue = "STRING_NAME", ObjectValue = "STRING_VALUE"};
            //list.Add(stringItem);

            return list;
        }

        //private static CapFloor GenerateCap_DefinitionAndCashflows()
        //{
        //    return new CapFloorGeneratorTests().Generate_Cap_DefinitionAndCashflows();
        //}

        //private static CapFloor GenerateFloor_DefinitionAndCashflows()
        //{
        //    return new CapFloorGeneratorTests().Generate_Floor_DefinitionAndCashflows();
        //}


        public Swap GenerateSwapParametricWithCashflows(bool generatePrincipalExchanges)
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                MaturityDate = new DateTime(1999, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR",
                DiscountingType = "None",
                Payer = "book",
                Receiver = "counterparty"
            };

            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                MaturityDate = new DateTime(1999, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR",
                ForecastCurve = "AUD-LIBOR",
                DiscountingType = "None",
                Receiver = "book",
                Payer = "counterparty"
            };


            payLeg.GeneratePrincipalExchanges = receiveLeg.GeneratePrincipalExchanges = generatePrincipalExchanges;

            //RateCurve rateCurve = RateCurveTests.CreateAUD_LIBOR_BBA_3MonthFlat8PercentCurve_WithBankBillFutures(new DateTime(1994, 12, 14), new DateTime(1994, 12, 14), "AUD-LIBOR-BBA");
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            //MarketEnvironment marketEnvironment = new MarketEnvironment("1234567");
            //marketEnvironment.Add("AUD-LIBOR", rateCurve);
            marketEnvironment.Id = "1234567";
            MarketEnvironmentRegistry.Add(marketEnvironment);

            var valuationDT = new DateTime(1994, 12, 01);

            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, null, null, marketEnvironment, valuationDT);

            MarketEnvironmentRegistry.Remove("1234567");

            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);


            return swap;
        }


        public Swaption GenerateSwaptionParametricWithCashflows(bool generatePrincipalExchanges)
        {
            var payLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                MaturityDate = new DateTime(1999, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                NotionalAmount = 1000000,
                LegType = LegType.Fixed,
                Currency = "AUD",
                CouponOrLastResetRate = 0.08m,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR",
                DiscountingType = "None"
            };
            var receiveLeg = new SwapLegParametersRange_Old
            {
                AdjustedType = AdjustedType.Unadjusted,
                EffectiveDate = new DateTime(1994, 12, 14),
                MaturityDate = new DateTime(1999, 12, 14),
                FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                RollConvention = "14",
                InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                NotionalAmount = 1000000,
                LegType = LegType.Floating,
                Currency = "AUD",
                FloatingRateSpread = 0,
                PaymentFrequency = "6M",
                DayCount = "Actual360",
                PaymentCalendar = "AUSY",
                PaymentBusinessDayAdjustments = "FOLLOWING",
                FixingCalendar = "AUSY-GBLO",
                FixingBusinessDayAdjustments = "MODFOLLOWING",
                DiscountCurve = "AUD-LIBOR",
                ForecastCurve = "AUD-LIBOR",
                DiscountingType = "None"
            };
            payLeg.GeneratePrincipalExchanges = receiveLeg.GeneratePrincipalExchanges = generatePrincipalExchanges;
            //RateCurve rateCurve = RateCurveTests.CreateAUD_LIBOR_BBA_3MonthFlat8PercentCurve_WithBankBillFutures(new DateTime(1994, 12, 14), new DateTime(1994, 12, 14), "AUD-LIBOR-BBA");
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            //marketEnvironment = new MarketEnvironment("1234567");
            marketEnvironment.Id = "1234567";
            //marketEnvironment.Add("AUD-LIBOR", rateCurve);
            MarketEnvironmentRegistry.Add(marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 01);
            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(CurveEngine.Logger, CurveEngine.Cache, CurveEngine.NameSpace, payLeg, null, receiveLeg, null, null, null, null, marketEnvironment, valuationDT);
            MarketEnvironmentRegistry.Remove("1234567");
            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);
            //            Swaption swaption = new Swaption();
            //            swaption.swap = swap;
            var premium = MoneyHelper.GetNonNegativeAmount(1234.56, "AUD");
            AdjustableDate expirationDate = DateTypesHelper.ToAdjustableDate(new DateTime(2000, 10, 11), "FOLLOWING", "AUSY");
            var paymentDate = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(new DateTime(2000, 10, 12), "FOLLOWING", "AUSY");
            Swaption swaption = SwaptionFactory.Create(swap, premium,
                                                       "counterparty", "NAB",
                                                       paymentDate, expirationDate,
                                                       new DateTime(2000, 10, 11, 10, 00, 0), new DateTime(2000, 10, 11, 11, 00, 0), true);
            return swaption;
        }

        #endregion

        #region Tests

        [TestMethod]
        public void AppendValuationIdToFilenameFullPath()
        {
            const string filename = @"C:\Folder1\Folder2\filename.xml";
            string guid = Guid.NewGuid().ToString();
            string s = Valuations.Valuation.AppendValuationIdToFilename(filename, guid);
            Debug.Print(s);
            Assert.AreEqual(@"C:\Folder1\Folder2\filename_" + guid + ".xml", s);

        }

        [TestMethod]
        public void AppendValuationIdToFilenameRelativePath()
        {
            const string filename = @"\Folder2\filename.xml";
            string guid = Guid.NewGuid().ToString();
            string s = Valuations.Valuation.AppendValuationIdToFilename(filename, guid);
            Debug.Print(s);
            Assert.AreEqual(@"\Folder2\filename_" + guid + ".xml", s);
        }

        [TestMethod]
        public void AppendValuationIdToFilenameJustFilename()
        {
            const string filename = @"filename.xml";
            string guid = Guid.NewGuid().ToString();
            string s = Valuations.Valuation.AppendValuationIdToFilename(filename, guid);
            Debug.Print(s);
            Assert.AreEqual(@"filename_" + guid + ".xml", s);
        }

        [TestMethod]
        public void TestCreateSwapValuationReport23()
        {
            //var resultRange = new ValuationResultRange {PresentValue = 10000m, FutureValue = 20000m};
            //RateCurve rateCurve = RateCurveTests.CreateAudBbrBbsw3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);
            Swap swap = GenerateSwapParametricWithCashflows(false);
            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();
            var marketFactory = new CurveEngine.Factory.MarketFactory();
            marketFactory.AddYieldCurve(pair);
            Market market = marketFactory.Create();
            const string baseParty = "NAB";
            string tradeId = Guid.NewGuid().ToString();
            DateTime tradeDate = DateTime.Today;
            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty, tradeId, tradeDate, swap, market, new AssetValuation());
            Debug.WriteLine("ValuationReport:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString<Document>(valuationReport));
            string s1 = XmlSerializerHelper.SerializeToString<Document>(valuationReport);
            XmlSerializerHelper.DeserializeFromString<ValuationReport>(typeof(Document), s1);
            string tempFileName = Path.GetTempFileName();
            XmlSerializerHelper.SerializeToFile(typeof(Document), valuationReport, tempFileName);
            XmlSerializerHelper.DeserializeFromFile<ValuationReport>(typeof(Document), tempFileName);
            File.Delete(tempFileName);
        }

        [TestMethod]
        public void TestMergeTwoValuationReports()
        {
            string valReportId1 = Guid.NewGuid().ToString();
            ValuationReport valuationReport1 = CreateValuationReport(valReportId1);

            Assert.AreEqual(
                valReportId1,
                (((Trade)valuationReport1.tradeValuationItem[0].Items[0]).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            string valReportId2 = Guid.NewGuid().ToString();
            ValuationReport valuationReport2 = CreateValuationReport(valReportId2);
            Assert.AreEqual(
                valReportId2,
                ((valuationReport2.tradeValuationItem[0].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            ValuationReport mergedReport = ValuationReportGenerator.Merge(valuationReport1, valuationReport2);

            Debug.WriteLine("ValuationReport:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(mergedReport));

            Assert.AreEqual(2, mergedReport.tradeValuationItem.Length);
            Assert.AreNotEqual(mergedReport.header.messageId.Value, valReportId1);
            Assert.AreNotEqual(mergedReport.header.messageId.Value, valReportId2);


            Assert.AreEqual(
                valReportId1,
                ((mergedReport.tradeValuationItem[0].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            Assert.AreEqual(
                valReportId2,
                ((mergedReport.tradeValuationItem[1].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );
        }

        [TestMethod]
        public void TestMerge_Three_ValuationReports()
        {
            string valReportId1 = Guid.NewGuid().ToString();
            ValuationReport valuationReport1 = CreateValuationReport(valReportId1);

            Assert.AreEqual(
                valReportId1,
                (((Trade)valuationReport1.tradeValuationItem[0].Items[0]).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            string valReportId2 = Guid.NewGuid().ToString();
            ValuationReport valuationReport2 = CreateValuationReport(valReportId2);
            Assert.AreEqual(
                valReportId2,
                (((Trade)valuationReport2.tradeValuationItem[0].Items[0]).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            string valReportId3 = Guid.NewGuid().ToString();
            ValuationReport valuationReport3 = CreateValuationReport(valReportId3);
            Assert.AreEqual(
                valReportId3,
                (((Trade)valuationReport3.tradeValuationItem[0].Items[0]).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            ValuationReport mergedReport12 = ValuationReportGenerator.Merge(valuationReport1, valuationReport2);
            ValuationReport mergedReport123 = ValuationReportGenerator.Merge(mergedReport12, valuationReport3);
            ValuationReport mergedReport312 = ValuationReportGenerator.Merge(valuationReport3, mergedReport12);

            Debug.WriteLine("ValuationReport(12 + 3 :");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(mergedReport123));

            Assert.AreEqual(3, mergedReport123.tradeValuationItem.Length);
            Assert.AreNotEqual(mergedReport123.header.messageId.Value, valReportId1);
            Assert.AreNotEqual(mergedReport123.header.messageId.Value, valReportId2);
            Assert.AreNotEqual(mergedReport123.header.messageId.Value, valReportId3);


            Assert.AreEqual(
                valReportId1,
                ((mergedReport123.tradeValuationItem[0].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            Assert.AreEqual(
                valReportId2,
                ((mergedReport123.tradeValuationItem[1].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            Assert.AreEqual(
                valReportId3,
                ((mergedReport123.tradeValuationItem[2].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            Debug.WriteLine("ValuationReport(3 + 12 :");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(mergedReport312));

            Assert.AreEqual(3, mergedReport312.tradeValuationItem.Length);
            Assert.AreNotEqual(mergedReport312.header.messageId.Value, valReportId1);
            Assert.AreNotEqual(mergedReport312.header.messageId.Value, valReportId2);
            Assert.AreNotEqual(mergedReport312.header.messageId.Value, valReportId3);


            Assert.AreEqual(
                valReportId3,
                ((mergedReport312.tradeValuationItem[0].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            Assert.AreEqual(
                valReportId1,
                ((mergedReport312.tradeValuationItem[1].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            Assert.AreEqual(
                valReportId2,
                ((mergedReport312.tradeValuationItem[2].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );
        }

        [TestMethod]
        public void TestCreateSwapValuationReport()
        {
            foreach (string paymentDateRelativeTo in GetPayRelativeToEnumValues())
            {
                //RateCurve rateCurve = RateCurveTests.CreateAudBbrBbsw3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);
                ISwapLegEnvironment marketEnvironment =
                    CreateInterestRateStreamTestEnvironment(DateTime.Now);
                Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();//new RateCurveAssembler().CreateDTO(rateCurve);

                var marketFactory = new CurveEngine.Factory.MarketFactory();
                marketFactory.AddYieldCurve(pair);

                Market market = marketFactory.Create();

                var valuationReport = new ValuationReport { market = market };

                var tradeValuationItem = new TradeValuationItem();
                valuationReport.tradeValuationItem = new[] { tradeValuationItem };

                Swap swap = SwapFactory.Create(
                    InterestRateStreamFactory.CreateFixedRateStream(EnumHelper.Parse<PayRelativeToEnum>(paymentDateRelativeTo)),
                    InterestRateStreamFactory.CreateFloatingRateStream(EnumHelper.Parse<PayRelativeToEnum>(paymentDateRelativeTo)));

                var trade = new Trade();
                XsdClassesFieldResolver.TradeSetSwap(trade, swap);

                tradeValuationItem.Items = new object[] { trade };

                tradeValuationItem.valuationSet = new ValuationSet
                {
                    assetValuation = new[] { new AssetValuation() }
                };
                AssetValuation assetValuation = tradeValuationItem.valuationSet.assetValuation[0];
                assetValuation.quote = new[] { new Quotation() };
                assetValuation.quote[0].measureType = new AssetMeasureType { Value = "NPV" };
                assetValuation.quote[0].valueSpecified = true;
                assetValuation.quote[0].value = (decimal)-200000.0;
                assetValuation.quote[0].currency = CurrencyHelper.Parse("AUD");

                Debug.WriteLine("ValuationReport:");
                Debug.WriteLine(XmlSerializerHelper.SerializeToString(valuationReport));
            }


        }

        [TestMethod]
        public void TestCreateSwapValuationReport2()
        {
            ValuationReport valuationReport = CreateSwapValuationReport2();

            Debug.WriteLine("ValuationReport:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString<Document>(valuationReport));

            string s1 = XmlSerializerHelper.SerializeToString<Document>(valuationReport);

            XmlSerializerHelper.DeserializeFromString<ValuationReport>(typeof(Document), s1);

            string tempFileName = Path.GetTempFileName();
            XmlSerializerHelper.SerializeToFile(typeof(Document), valuationReport, tempFileName);

            XmlSerializerHelper.DeserializeFromFile<ValuationReport>(typeof(Document), tempFileName);
            File.Delete(tempFileName);
        }

        [TestMethod]
        public void TestCreateFraValuationReport1()
        {
            //RateCurve rateCurve = RateCurveTests.CreateAudLiborBba3MonthFlat8PercentCurve_WithRelativeBankBill3MFutures(DateTime.Now, DateTime.Now);

            //Swap swap = GenerateSwap_ParametricWithCashflows();
            var fra = new Fra();

            //Pair<YieldCurve, YieldCurveValuation> pair = new RateCurveAssembler().CreateDTO(rateCurve);

            ISwapLegEnvironment marketEnvironment =
                CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Pair<YieldCurve, YieldCurveValuation> pair = marketEnvironment.GetDiscountRateCurveFpML();

            var marketFactory = new CurveEngine.Factory.MarketFactory();
            marketFactory.AddYieldCurve(pair);

            Market market = marketFactory.Create();

            const string baseParty = _NAB;

            var assetValuation = new AssetValuation();

            var listOfQuotations = new List<Quotation>();


            List<StringObjectRangeItem> valuationSet = CreateValuationSetList(54321, 123.5);

            foreach (StringObjectRangeItem item in valuationSet)
            {
                var quotation = new Quotation
                {
                    measureType = AssetMeasureTypeHelper.Parse(item.StringValue)
                };

                if (item.ObjectValue is double | item.ObjectValue is decimal)
                {
                    quotation.value = Convert.ToDecimal(item.ObjectValue);
                    quotation.valueSpecified = true;
                }
                else
                {
                    quotation.cashflowType = new CashflowType { Value = item.ObjectValue.ToString() };
                }

                listOfQuotations.Add(quotation);
            }

            assetValuation.quote = listOfQuotations.ToArray();

            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty, fra, market, assetValuation);

            Debug.WriteLine("ValuationReport:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(valuationReport));
        }

        #endregion

        #endregion
    }
}
