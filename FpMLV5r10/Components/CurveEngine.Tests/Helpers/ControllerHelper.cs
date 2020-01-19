using System;
using System.Diagnostics;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Markets;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.CurveEngine.PricingStructures;
using National.QRSC.FpML.V47;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using National.QRSC.Utility.NamedValues;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using National.QRSC.Constants;
using FxCurve = Orion.CurveEngine.PricingStructures.Curves.FxCurve;

namespace Orion.CurveEngine.Tests.Helpers
{
    static public class ControllerHelper
    {
        public static string BuildAndCacheRateCurve(DateTime baseDate)
        {
            var curve = TestRateCurve(baseDate);

            string curveId = curve.GetPricingStructureId().UniqueIdentifier;

            ObjectCacheHelper.SetPricingStructureAsSerialisable(curve);

            return curveId;

        }

        public static IFxCurve TestFxCurve(DateTime baseDate)
        {
            const string curveName = "AUD-USD";
            const string algorithm = "LinearForward";
            //const double tolerance = 0.00000001;

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

            string[] instruments =  
                {   "AUDUSD-FxForward-1D", "AUDUSD-FxForward-1M", "AUDUSD-FxForward-2M", "AUDUSD-FxForward-3M"
                };

            decimal[] rates = { 0.90m, 0.90m, 0.90m, 0.90m };

            var curve = new FxCurve(fxProperties, instruments, rates);
            return curve;
        }


        public static IRateCurve TestRateCurve(DateTime baseDate)
        {
            const string curveName = "AUD-LIBOR-BBA-3M";
            const string indexTenor = "3M";
            const string algorithm = "FastLinearZero";
            const string marketEnvironment = "Bob";
            const double tolerance = 0.00000001;

            var props = new object[10, 2];
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

            var curve = new RateCurve(namevalues, instruments, rates, additional);
            return curve;
        }

        public static InterestRateStream GetCashflowsSchedule(SwapLegParametersRange legParametersRange)
        {
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);

            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream);

            stream.cashflows = cashflows;

            return stream;
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

            var curreny = Currency.Parse(reportingCurrency);

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

        //static internal IInstrumentControllerData CreateInstrumentModelData(string[] metrics, DateTime baseDate, ISwapLegEnvironment market)
        //{
        //    var bav = new AssetValuation();

        //    var quotes = new Quotation[metrics.Length];
        //    var index = 0;
        //    foreach (var metric in metrics)
        //    {
        //        quotes[index] = QuotationHelper.Create(0.0m, metric, "DecimalValue");
        //        index++;
        //    }
        //    bav.quote = quotes;
        //    return new InstrumentControllerData(bav, market, baseDate);
        //}

        static public void ProcessInstrumentControllerResults(InstrumentControllerBase instrumentController, string[] metrics, DateTime baseDate)
        {
            Assert.IsNotNull(instrumentController);

            //Double[] times = { 0, 1, 2, 3, 4, 5 };
            //Double[] dfs = { 1, 0.98, 0.96, 0.91, 0.88, 0.85};

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

            //Double[] times = { 0, 1, 2, 3, 4, 5 };
            //Double[] dfs = { 1, 0.98, 0.96, 0.91, 0.88, 0.85};

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

            //Double[] times = { 0, 1, 2, 3, 4, 5 };
            //Double[] dfs = { 1, 0.98, 0.96, 0.91, 0.88, 0.85};

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

            //Double[] times = { 0, 1, 2, 3, 4, 5 };
            //Double[] dfs = { 1, 0.98, 0.96, 0.91, 0.88, 0.85};

            IMarketEnvironment market = CreateFxLegTestEnvironment( baseDate);
            IInstrumentControllerData controllerData = CreateInstrumentModelData(metrics, baseDate, market, currency);
            Assert.IsNotNull(controllerData);
            var results = instrumentController.Calculate(controllerData);
            Debug.Print("Id : {0}", instrumentController.Id);
            foreach (var metric in results.quote)
            {
                Debug.Print("Id : {0} Metric Name : {1} Metric Value : {2}", instrumentController.Id, metric.measureType.Value, metric.value);
            }
        }

        static public void ProcessAssetControllerResults(IPriceableAssetController assetController, string[] metrics, DateTime baseDate)
        {
            Assert.IsNotNull(assetController);

            Double[] times = { 0, 1, 5 };
            Double[] dfs = { 1, 0.9, 0.3 };

            ISimpleRateMarketEnvironment market = CreateSimpleRateMarketEnvironment(baseDate, times, dfs);
            IAssetControllerData controllerData = CreateModelData(metrics, baseDate, market);
            Assert.IsNotNull(controllerData);
            Assert.AreEqual(controllerData.BasicAssetValuation.quote.Length, metrics.Length);
            Assert.AreEqual(controllerData.BasicAssetValuation.quote[0].measureType.Value, metrics[0]);
            BasicAssetValuation results = assetController.Calculate(controllerData);
            Debug.Print("Id : {0}", assetController.Id);
            foreach (var metric in results.quote)
            {
                Debug.Print("Id : {0} Metric Name : {1} Metric Value : {2}", assetController.Id, metric.measureType.Value, metric.value);
            }
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

        static public IMarketEnvironment CreateFxLegTestEnvironment(DateTime baseDate)
        {          
            var market1 = new SwapLegEnvironment();
            var market2 = new SwapLegEnvironment();
            var discurve1 = TestRateCurve(baseDate);
            var fx1Curve = TestFxCurve(baseDate);
            market1.AddPricingStructure("DiscountCurve", discurve1);
            market2.AddPricingStructure("DiscountCurve", discurve1);
            market1.AddPricingStructure("ReportingCurrencyFxCurve", fx1Curve);
            market2.AddPricingStructure("ReportingCurrencyFxCurve", fx1Curve);
            var marketEnv = new FxLegEnvironment("temp", market1, market2);
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

        static public IMarketEnvironment CreateSimpleDiscountFactorCurve(string curveName, DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new MarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            var curve = new SimpleDiscountFactorCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure(curveName, curve);
            return market;
        }

        static public IMarketEnvironment CreateDFandVolCurves(string curveName, string volcurveName, DateTime baseDate, Double[] times, Double[] dfs, Double[] voltimes, Double[] volstrikes, Double[,] vols)
        {
            var market = new MarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LinearInterpolation");
            var curve = new SimpleDiscountFactorCurve(baseDate, interpMethod, true, times, dfs);
            var volcurve = new SimpleVolatilitySurface(baseDate, interpMethod, true, voltimes, volstrikes, vols);
            market.AddPricingStructure(curveName, curve);
            market.AddPricingStructure(volcurveName, volcurve);
            return market;
        }

        static public IMarketEnvironment CreateSimpleFxCurve(string curveName, DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new MarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LinearInterpolation");
            var curve = new SimpleFxCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure(curveName, curve);
            return market;
        }

        static public IMarketEnvironment CreateSimpleCommodityCurve(string curveName, DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new MarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LinearInterpolation");
            var curve = new SimpleCommodityCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure(curveName, curve);
            return market;
        }

        static public IMarketEnvironment CreateSimpleInflationCurve(string curveName, DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new MarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LinearInterpolation");
            var curve = new SimpleDiscountFactorCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure(curveName, curve);
            return market;
        }

        static public decimal[] AdjustValues(decimal baseValue, string[] instrumentNames, decimal step)
        {
            var adjustedValues = new decimal[instrumentNames.Length];
            adjustedValues[0] = baseValue;
            for (var i = 1; i < instrumentNames.Length; i++)
            {
                adjustedValues[i] = adjustedValues[i - 1] + step;
            }
            return adjustedValues;
        }

    }
}
