using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using National.QRSC.AnalyticModels.Generic.Coupons;
using National.QRSC.Engine.Helpers;
using National.QRSC.Engine.Tests.Helpers;
using National.QRSC.ObjectCache;
using National.QRSC.Runtime.Common;
using National.QRSC.Trades.Tests.Products.Rates.Swap;
using National.QRSC.TradeValuation;
using National.QRSC.Utility.Helpers;
using National.QRSC.Utility.Logging;
using National.QRSC.Utility.NamedValues;
using National.QRSC.Utility.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using National.QRSC.FpML.V47;
using National.QRSC.Constants;

namespace National.QRSC.Trades.Tests.Trades
{
    [TestClass]
    public class TestPortfolioPricer
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            var baseDate = new DateTime(1998, 2, 20);

            //Build the EUR curves.
            var discountCurveProperties = new NamedValueSet();
            discountCurveProperties.Set(CurveProp.PricingStructureType, "DiscountCurve");
            discountCurveProperties.Set("BuildDateTime", baseDate);
            discountCurveProperties.Set(CurveProp.BaseDate, baseDate);
            discountCurveProperties.Set(CurveProp.Market, "LIVE");
            discountCurveProperties.Set(CurveProp.MarketAndDate, "LIVE");
            discountCurveProperties.Set("Identifier", "DiscountCurve." + "EUR-LIBOR-SENIOR" + baseDate);
            discountCurveProperties.Set(CurveProp.Currency1, "EUR");
            discountCurveProperties.Set("CreditInstrumentId", "LIBOR");
            discountCurveProperties.Set("CreditSeniority", "SENIOR");
            discountCurveProperties.Set(CurveProp.CurveName, "EUR-LIBOR-SENIOR");
            discountCurveProperties.Set("Algorithm", "LinearZero");

            //Set the required test curves.Firstly the discount curve.
            var discountCurve = ControllerHelper.TestRateCurve(baseDate);
            var discountCurvefpML = discountCurve.GetFpMLData();
            const string disId = "Highlander.Market.LIVE.DiscountCurve.EUR-LIBOR-SENIOR";
            var discountCurvemarket = PricingStructureHelper.CreateMarketFromFpML(disId, discountCurvefpML);
            ServerStore.Client.SaveObject<Market>(discountCurvemarket, disId, discountCurveProperties, TimeSpan.MaxValue);

            var curvePropertiesa = new NamedValueSet();
            curvePropertiesa.Set(CurveProp.PricingStructureType, "RateCurve");
            curvePropertiesa.Set("BuildDateTime", baseDate);
            curvePropertiesa.Set(CurveProp.BaseDate, baseDate);
            curvePropertiesa.Set(CurveProp.Market, "LIVE");
            curvePropertiesa.Set(CurveProp.MarketAndDate, "LIVE");
            curvePropertiesa.Set("Identifier", "RateCurve." + "EUR-EURIBOR-Telerate" + baseDate);
            curvePropertiesa.Set(CurveProp.Currency1, "EUR");
            curvePropertiesa.Set(CurveProp.IndexName, "EUR-EURIBOR-Telerate");
            curvePropertiesa.Set(CurveProp.IndexTenor, "6M");
            curvePropertiesa.Set(CurveProp.CurveName, "EUR-EURIBOR-Telerate-6M");
            curvePropertiesa.Set("Algorithm", "LinearZero");

            //THe forecast ratecurve
            var forecastCurvea = ControllerHelper.TestRateCurve(baseDate);
            var forecastCurvefpMLa = forecastCurvea.GetFpMLData();
            const string forecastCurveIda = "Highlander.Market.LIVE.RateCurve.EUR-EURIBOR-Telerate-6M";
            var forecastCurvemarketa = PricingStructureHelper.CreateMarketFromFpML(forecastCurveIda, forecastCurvefpMLa);
            ServerStore.Client.SaveObject<Market>(forecastCurvemarketa, forecastCurveIda, curvePropertiesa, TimeSpan.MaxValue);

            var curvePropertiesb = new NamedValueSet();
            curvePropertiesb.Set(CurveProp.PricingStructureType, "RateCurve");
            curvePropertiesb.Set("BuildDateTime", baseDate);
            curvePropertiesb.Set(CurveProp.BaseDate, baseDate);
            curvePropertiesb.Set(CurveProp.Market, "LIVE");
            curvePropertiesb.Set(CurveProp.MarketAndDate, "LIVE");
            curvePropertiesb.Set("Identifier", "RateCurve." + "EUR-LIBOR-BBA" + baseDate);
            curvePropertiesb.Set(CurveProp.Currency1, "EUR");
            curvePropertiesb.Set(CurveProp.IndexName, "EUR-LIBOR-BBA");
            curvePropertiesb.Set(CurveProp.IndexTenor, "6M");
            curvePropertiesb.Set(CurveProp.CurveName, "EUR-LIBOR-BBA-6M");
            curvePropertiesb.Set("Algorithm", "LinearZero");

            //THe forecast ratecurve
            var forecastCurveb = ControllerHelper.TestRateCurve(baseDate);
            var forecastCurvefpMLb = forecastCurveb.GetFpMLData();
            const string forecastCurveIdb = "Highlander.Market.LIVE.RateCurve.EUR-LIBOR-BBA-6M";
            var forecastCurvemarketb = PricingStructureHelper.CreateMarketFromFpML(forecastCurveIdb, forecastCurvefpMLb);
            ServerStore.Client.SaveObject<Market>(forecastCurvemarketb, forecastCurveIdb, curvePropertiesb, TimeSpan.MaxValue);

            //var curveProperties = new NamedValueSet();
            //curveProperties.Set(CurveProp.PricingStructureType, "RateCurve");
            //curveProperties.Set("BuildDateTime", baseDate);
            //curveProperties.Set(CurveProp.BaseDate, baseDate);
            //curveProperties.Set(CurveProp.Market, "LIVE");
            //curveProperties.Set(CurveProp.MarketAndDate, "LIVE");
            //curveProperties.Set("Identifier", "RateCurve." + "EUR-LIBOR-BBA" + baseDate);
            //curveProperties.Set(CurveProp.Currency1, "EUR");
            //curveProperties.Set(CurveProp.IndexName, "EUR-LIBOR-BBA");
            //curveProperties.Set(CurveProp.IndexTenor, "6M");
            //curveProperties.Set(CurveProp.CurveName, "EUR-LIBOR-BBA-6M");
            //curveProperties.Set("Algorithm", "LinearZero");

            ////THe forecast ratecurve
            //var forecastCurve = ControllerHelper.TestRateCurve(baseDate);
            //var forecastCurvefpML = forecastCurve.GetFpMLData();
            //const string forecastCurveId = "Highlander.Market.LIVE.RateCurve.EUR-LIBOR-BBA-6M";
            //var forecastCurvemarket = PricingStructureHelper.CreateMarketFromFpML(forecastCurveId, forecastCurvefpML);
            //ServerStore.Cache.SaveObject<Market>(forecastCurvemarket, forecastCurveId, curveProperties, TimeSpan.MaxValue);

            var fxProperties = new NamedValueSet();
            fxProperties.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties.Set("BuildDateTime", baseDate);
            fxProperties.Set(CurveProp.BaseDate, baseDate);
            fxProperties.Set(CurveProp.Market, "LIVE");
            fxProperties.Set(CurveProp.MarketAndDate, "LIVE");
            fxProperties.Set("Identifier", "FxCurve.EUR-USD" + baseDate);
            fxProperties.Set(CurveProp.Currency1, "EUR");
            fxProperties.Set(CurveProp.Currency2, "USD");
            fxProperties.Set(CurveProp.CurrencyPair, "EUR-USD");
            fxProperties.Set(CurveProp.QuoteBasis, "Currency2PerCurrency1");
            fxProperties.Set(CurveProp.CurveName, "EUR-USD");
            fxProperties.Set("Algorithm", "LinearForward");

            //The fx curve.
            var fxCurve = ControllerHelper.TestFxCurve(baseDate);
            var fxCurvefpML = fxCurve.GetFpMLData();
            const string fxCurveId = "Highlander.Market.LIVE.FxCurve.EUR-USD";
            var fxCurvemarket = PricingStructureHelper.CreateMarketFromFpML(fxCurveId, fxCurvefpML);
            ServerStore.Client.SaveObject<Market>(fxCurvemarket, fxCurveId, fxProperties, TimeSpan.MaxValue);

            //Build the USD Curves
            var discountCurveProperties2 = new NamedValueSet();
            discountCurveProperties2.Set(CurveProp.PricingStructureType, "DiscountCurve");
            discountCurveProperties2.Set("BuildDateTime", baseDate);
            discountCurveProperties2.Set(CurveProp.BaseDate, baseDate);
            discountCurveProperties2.Set(CurveProp.Market, "LIVE");
            discountCurveProperties2.Set(CurveProp.MarketAndDate, "LIVE");
            discountCurveProperties2.Set("Identifier", "DiscountCurve." + "USD-LIBOR-SENIOR" + baseDate);
            discountCurveProperties2.Set(CurveProp.Currency1, "USD");
            discountCurveProperties2.Set("CreditInstrumentId", "LIBOR");
            discountCurveProperties2.Set("CreditSeniority", "SENIOR");
            discountCurveProperties2.Set(CurveProp.CurveName, "USD-LIBOR-SENIOR");
            discountCurveProperties2.Set("Algorithm", "LinearZero");

            //Set the required test curves.Firstly the discount curve.
            var discountCurve2 = ControllerHelper.TestRateCurve(baseDate);
            var discountCurvefpML2 = discountCurve2.GetFpMLData();
            const string disId2 = "Highlander.Market.LIVE.DiscountCurve.USD-LIBOR-SENIOR";
            var discountCurvemarket2 = PricingStructureHelper.CreateMarketFromFpML(disId2, discountCurvefpML2);
            ServerStore.Client.SaveObject<Market>(discountCurvemarket2, disId2, discountCurveProperties2, TimeSpan.MaxValue);

            var curveProperties2 = new NamedValueSet();
            curveProperties2.Set(CurveProp.PricingStructureType, "RateCurve");
            curveProperties2.Set("BuildDateTime", baseDate);
            curveProperties2.Set(CurveProp.BaseDate, baseDate);
            curveProperties2.Set(CurveProp.Market, "LIVE");
            curveProperties2.Set(CurveProp.MarketAndDate, "LIVE");
            curveProperties2.Set("Identifier", "RateCurve." + "USD-LIBOR-BBA" + baseDate);
            curveProperties2.Set(CurveProp.Currency1, "USD");
            curveProperties2.Set(CurveProp.IndexName, "USD-LIBOR-BBA");
            curveProperties2.Set(CurveProp.IndexTenor, "6M");
            curveProperties2.Set(CurveProp.CurveName, "USD-LIBOR-BBA-6M");
            curveProperties2.Set("Algorithm", "LinearZero");

            //THe forecast ratecurve
            var forecastCurve2 = ControllerHelper.TestRateCurve(baseDate);
            var forecastCurvefpML2 = forecastCurve2.GetFpMLData();
            const string forecastCurveId2 = "Highlander.Market.LIVE.RateCurve.USD-LIBOR-BBA-6M";
            var forecastCurvemarket2 = PricingStructureHelper.CreateMarketFromFpML(forecastCurveId2, forecastCurvefpML2);
            ServerStore.Client.SaveObject<Market>(forecastCurvemarket2, forecastCurveId2, curveProperties2, TimeSpan.MaxValue);

            var curvePropertiesn = new NamedValueSet();
            curvePropertiesn.Set(CurveProp.PricingStructureType, "RateCurve");
            curvePropertiesn.Set("BuildDateTime", baseDate);
            curvePropertiesn.Set(CurveProp.BaseDate, baseDate);
            curvePropertiesn.Set(CurveProp.Market, "LIVE");
            curvePropertiesn.Set(CurveProp.MarketAndDate, "LIVE");
            curvePropertiesn.Set("Identifier", "RateCurve." + "USD-LIBOR-BBA" + baseDate);
            curvePropertiesn.Set(CurveProp.Currency1, "USD");
            curvePropertiesn.Set(CurveProp.IndexName, "USD-LIBOR-BBA");
            curvePropertiesn.Set(CurveProp.IndexTenor, "3M");
            curvePropertiesn.Set(CurveProp.CurveName, "USD-LIBOR-BBA-3M");
            curvePropertiesn.Set("Algorithm", "LinearZero");

            //THe forecast ratecurve
            var forecastCurven = ControllerHelper.TestRateCurve(baseDate);
            var forecastCurvefpMLn = forecastCurven.GetFpMLData();
            const string forecastCurveIdn = "Highlander.Market.LIVE.RateCurve.USD-LIBOR-BBA-3M";
            var forecastCurvemarketn = PricingStructureHelper.CreateMarketFromFpML(forecastCurveIdn, forecastCurvefpMLn);
            ServerStore.Client.SaveObject<Market>(forecastCurvemarketn, forecastCurveIdn, curvePropertiesn, TimeSpan.MaxValue);

            //var fxProperties2 = new NamedValueSet();
            //fxProperties2.Set(CurveProp.PricingStructureType, "FxCurve");
            //fxProperties2.Set("BuildDateTime", baseDate);
            //fxProperties2.Set(CurveProp.BaseDate, baseDate);
            //fxProperties2.Set(CurveProp.Market, "LIVE");
            //fxProperties2.Set("Identifier", "FxCurve.USD-AUD" + baseDate);
            //fxProperties2.Set(CurveProp.Currency1, "AUD");
            //fxProperties2.Set(CurveProp.Currency2, "USD");
            //fxProperties2.Set(CurveProp.CurrencyPair, "USD-AUD");
            //fxProperties2.Set(CurveProp.CurveName, "USD-AUD");
            //fxProperties2.Set("Algorithm", "LinearForward");

            ////The fx curve.
            //var fxCurve2 = ControllerHelper.TestFxCurve(baseDate);
            //var fxCurvefpML2 = fxCurve2.GetFpMLData();
            //const string fxCurveId2 = "Highlander.Market.LIVE.FxCurve.USD-AUD";
            //var fxCurvemarket2 = PricingStructureHelper.CreateMarketFromFpML(fxCurveId2, fxCurvefpML2);
            //ServerStore.Cache.SaveObject<Market>(fxCurvemarket2, fxCurveId2, fxProperties2, TimeSpan.MaxValue);

            //Build the JPY Curves
            var DiscountCurveProperties3 = new NamedValueSet();
            DiscountCurveProperties3.Set(CurveProp.PricingStructureType, "DiscountCurve");
            DiscountCurveProperties3.Set("BuildDateTime", baseDate);
            DiscountCurveProperties3.Set(CurveProp.BaseDate, baseDate);
            DiscountCurveProperties3.Set(CurveProp.Market, "LIVE");
            DiscountCurveProperties3.Set(CurveProp.MarketAndDate, "LIVE");
            DiscountCurveProperties3.Set("Identifier", "DiscountCurve." + "JPY-LIBOR-SENIOR" + baseDate);
            DiscountCurveProperties3.Set(CurveProp.Currency1, "JPY");
            DiscountCurveProperties3.Set("CreditInstrumentId", "LIBOR");
            DiscountCurveProperties3.Set("CreditSeniority", "SENIOR");
            DiscountCurveProperties3.Set(CurveProp.CurveName, "JPY-LIBOR-SENIOR");
            DiscountCurveProperties3.Set("Algorithm", "LinearZero");

            //Set the required test curves.Firstly the discount curve.
            var discountCurve3 = ControllerHelper.TestRateCurve(baseDate);
            var discountCurvefpML3 = discountCurve3.GetFpMLData();
            const string disId3 = "Highlander.Market.LIVE.DiscountCurve.JPY-LIBOR-SENIOR";
            var discountCurvemarket3 = PricingStructureHelper.CreateMarketFromFpML(disId3, discountCurvefpML3);
            ServerStore.Client.SaveObject<Market>(discountCurvemarket3, disId3, DiscountCurveProperties3, TimeSpan.MaxValue);

            var curveProperties3 = new NamedValueSet();
            curveProperties3.Set(CurveProp.PricingStructureType, "RateCurve");
            curveProperties3.Set("BuildDateTime", baseDate);
            curveProperties3.Set(CurveProp.BaseDate, baseDate);
            curveProperties3.Set(CurveProp.Market, "LIVE");
            curveProperties3.Set(CurveProp.MarketAndDate, "LIVE");
            curveProperties3.Set("Identifier", "RateCurve." + "JPY-LIBOR-BBA" + baseDate);
            curveProperties3.Set(CurveProp.Currency1, "JPY");
            curveProperties3.Set(CurveProp.IndexName, "JPY-LIBOR-BBA");
            curveProperties3.Set(CurveProp.IndexTenor, "6M");
            curveProperties3.Set(CurveProp.CurveName, "JPY-LIBOR-BBA-6M");
            curveProperties3.Set("Algorithm", "LinearZero");

            //THe forecast ratecurve
            var forecastCurve3 = ControllerHelper.TestRateCurve(baseDate);
            var forecastCurvefpML3 = forecastCurve3.GetFpMLData();
            const string forecastCurveId3 = "Highlander.Market.LIVE.RateCurve.JPY-LIBOR-BBA-6M";
            var forecastCurvemarket3 = PricingStructureHelper.CreateMarketFromFpML(forecastCurveId3, forecastCurvefpML3);
            ServerStore.Client.SaveObject<Market>(forecastCurvemarket3, forecastCurveId3, curveProperties3, TimeSpan.MaxValue);

            var fxProperties3 = new NamedValueSet();
            fxProperties3.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties3.Set("BuildDateTime", baseDate);
            fxProperties3.Set(CurveProp.BaseDate, baseDate);
            fxProperties3.Set(CurveProp.Market, "LIVE");
            fxProperties3.Set(CurveProp.MarketAndDate, "LIVE");
            fxProperties3.Set("Identifier", "FxCurve.JPY-USD" + baseDate);
            fxProperties3.Set(CurveProp.Currency1, "JPY");
            fxProperties3.Set(CurveProp.Currency2, "USD");
            fxProperties3.Set(CurveProp.CurrencyPair, "JPY-USD");
            fxProperties3.Set(CurveProp.QuoteBasis, "Currency2PerCurrency1");
            fxProperties3.Set(CurveProp.CurveName, "JPY-AUD");
            fxProperties3.Set("Algorithm", "LinearForward");

            //The fx curve.
            var fxCurve3 = ControllerHelper.TestFxCurve(baseDate);
            var fxCurvefpML3 = fxCurve3.GetFpMLData();
            const string fxCurveId3 = "Highlander.Market.LIVE.FxCurve.JPY-USD";
            var fxCurvemarket3 = PricingStructureHelper.CreateMarketFromFpML(fxCurveId3, fxCurvefpML3);
            ServerStore.Client.SaveObject<Market>(fxCurvemarket3, fxCurveId3, fxProperties3, TimeSpan.MaxValue);

            //Build the CHF curves.
            var discountCurveProperties4 = new NamedValueSet();
            discountCurveProperties4.Set(CurveProp.PricingStructureType, "DiscountCurve");
            discountCurveProperties4.Set("BuildDateTime", baseDate);
            discountCurveProperties4.Set(CurveProp.BaseDate, baseDate);
            discountCurveProperties4.Set(CurveProp.Market, "LIVE");
            discountCurveProperties4.Set(CurveProp.MarketAndDate, "LIVE");
            discountCurveProperties4.Set("Identifier", "DiscountCurve." + "CHF-LIBOR-SENIOR" + baseDate);
            discountCurveProperties4.Set(CurveProp.Currency1, "CHF");
            discountCurveProperties4.Set("CreditInstrumentId", "LIBOR");
            discountCurveProperties4.Set("CreditSeniority", "SENIOR");
            discountCurveProperties4.Set(CurveProp.CurveName, "CHF-LIBOR-SENIOR");
            discountCurveProperties4.Set("Algorithm", "LinearZero");

            //Set the required test curves.Firstly the discount curve.
            var discountCurve4 = ControllerHelper.TestRateCurve(baseDate);
            var discountCurvefpML4 = discountCurve4.GetFpMLData();
            const string disId4 = "Highlander.Market.LIVE.DiscountCurve.CHF-LIBOR-SENIOR";
            var discountCurvemarket4 = PricingStructureHelper.CreateMarketFromFpML(disId4, discountCurvefpML4);
            ServerStore.Client.SaveObject<Market>(discountCurvemarket4, disId4, discountCurveProperties4, TimeSpan.MaxValue);

            var curveProperties4 = new NamedValueSet();
            curveProperties4.Set(CurveProp.PricingStructureType, "RateCurve");
            curveProperties4.Set("BuildDateTime", baseDate);
            curveProperties4.Set(CurveProp.BaseDate, baseDate);
            curveProperties4.Set(CurveProp.Market, "LIVE");
            curveProperties4.Set(CurveProp.MarketAndDate, "LIVE");
            curveProperties4.Set("Identifier", "RateCurve." + "CHF-LIBOR-BBA" + baseDate);
            curveProperties4.Set(CurveProp.Currency1, "CHF");
            curveProperties4.Set(CurveProp.IndexName, "CHF-LIBOR-BBA");
            curveProperties4.Set(CurveProp.IndexTenor, "6M");
            curveProperties4.Set(CurveProp.CurveName, "CHF-LIBOR-BBA-6M");
            curveProperties4.Set("Algorithm", "LinearZero");

            //THe forecast ratecurve
            var forecastCurve4 = ControllerHelper.TestRateCurve(baseDate);
            var forecastCurvefpML4 = forecastCurve4.GetFpMLData();
            const string forecastCurveId4 = "Highlander.Market.LIVE.RateCurve.CHF-LIBOR-BBA-6M";
            var forecastCurvemarket4 = PricingStructureHelper.CreateMarketFromFpML(forecastCurveId4, forecastCurvefpML4);
            ServerStore.Client.SaveObject<Market>(forecastCurvemarket4, forecastCurveId4, curveProperties4, TimeSpan.MaxValue);

            var fxProperties4 = new NamedValueSet();
            fxProperties4.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties4.Set("BuildDateTime", baseDate);
            fxProperties4.Set(CurveProp.BaseDate, baseDate);
            fxProperties4.Set(CurveProp.Market, "LIVE");
            fxProperties4.Set(CurveProp.MarketAndDate, "LIVE");
            fxProperties4.Set("Identifier", "FxCurve.CHF-USD" + baseDate);
            fxProperties4.Set(CurveProp.Currency1, "USD");
            fxProperties4.Set(CurveProp.Currency2, "CHF");
            fxProperties4.Set(CurveProp.QuoteBasis, "Currency2PerCurrency1");
            fxProperties4.Set(CurveProp.CurrencyPair, "CHF-USD");
            fxProperties4.Set(CurveProp.CurveName, "CHF-USD");
            fxProperties4.Set("Algorithm", "LinearForward");

            //The fx curve.
            var fxCurve4 = ControllerHelper.TestFxCurve(baseDate);
            var fxCurvefpML4 = fxCurve4.GetFpMLData();
            const string fxCurveId4 = "Highlander.Market.LIVE.FxCurve.CHF-USD";
            var fxCurvemarket4 = PricingStructureHelper.CreateMarketFromFpML(fxCurveId4, fxCurvefpML4);
            ServerStore.Client.SaveObject<Market>(fxCurvemarket4, fxCurveId4, fxProperties4, TimeSpan.MaxValue);

            var fxProperties5 = new NamedValueSet();
            fxProperties5.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties5.Set("BuildDateTime", baseDate);
            fxProperties5.Set(CurveProp.BaseDate, baseDate);
            fxProperties5.Set(CurveProp.Market, "LIVE");
            fxProperties5.Set(CurveProp.MarketAndDate, "LIVE");
            fxProperties5.Set("Identifier", "FxCurve.AUD-USD" + baseDate);
            fxProperties5.Set(CurveProp.Currency1, "AUD");
            fxProperties5.Set(CurveProp.Currency2, "USD");
            fxProperties5.Set(CurveProp.CurrencyPair, "AUD-USD");
            fxProperties5.Set(CurveProp.QuoteBasis, "Currency2PerCurrency1");
            fxProperties5.Set(CurveProp.CurveName, "AUD-USD");
            fxProperties5.Set("Algorithm", "LinearForward");

            //The fx curve.
            var fxCurve5 = ControllerHelper.TestFxCurve(baseDate);
            var fxCurvefpML5 = fxCurve5.GetFpMLData();
            const string fxCurveId5 = "Highlander.Market.LIVE.FxCurve.AUD-USD";
            var fxCurvemarket5 = PricingStructureHelper.CreateMarketFromFpML(fxCurveId5, fxCurvefpML5);
            ServerStore.Client.SaveObject<Market>(fxCurvemarket5, fxCurveId5, fxProperties5, TimeSpan.MaxValue);
        }

        private readonly DateTime _baseDate = new DateTime(1998, 2, 20);

        private readonly string[] _metrics = GetSwapMetrics();

        // needs rewrite
        //[TestMethod]
        //public void TradeIRSwapPricerWithReportingCurrency1()
        //{
        //    var trade1 = FpMLTestsSwapHelper.GetTrade01_ExampleObject();

        //    //TODO Add the other trade examples.

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    //trade.ItemElementName = ItemChoiceType16.swap;
        //    //XsdClassesFieldResolver.Trade_SetSwap(trade, swap);
        //    //NamedValueSet tradeProps = new NamedValueSet();
        //    //Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
        //    //    ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));

        //    //Create the tradepricer.
        //    //var tradePricer = new TradePricer(trade, tradeProps);
        //    //trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
        //    //var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    //var tradePair = new Pair<Trade, NamedValueSet>(trade1, tradeProps);
        //    //var tradeList = new List<Pair<Trade, NamedValueSet>> {tradePair};

        //    var item = ServerStore.Client.MakeItem(trade1, "trade00001", tradeProps, false, new TimeSpan(1000));
        //    var tradeList = new List<ICoreItem> { item };
        //    var portfolioPricer = new PortfolioPricer(tradeList);
        //    //Calculate the metrics.
        //    //Price.
        //    var logger = new TraceLogger(false);
        //    //var modelData = CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, new PartyIdentifier("Unknown"));
        //    var calcresult = portfolioPricer.Price(logger, tradeList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Summary, false);
        //    var result = XmlSerializerHelper.SerializeToString(calcresult);
        //    Debug.Print(result);
        //}

        // needs rewrite
        //[TestMethod]
        //public void TradeIRSwapPricerWithReportingCurrency1WithDetail()
        //{
        //    var trade1 = FpMLTestsSwapHelper.GetTrade01_ExampleObject();

        //    //TODO Add the other trade examples.

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    //trade.ItemElementName = ItemChoiceType16.swap;
        //    //XsdClassesFieldResolver.Trade_SetSwap(trade, swap);
        //    //NamedValueSet tradeProps = new NamedValueSet();
        //    //Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
        //    //    ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));

        //    //Create the tradepricer.
        //    //var tradePricer = new TradePricer(trade, tradeProps);
        //    //trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
        //    //var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    //var tradePair = new Pair<Trade, NamedValueSet>(trade1, tradeProps);
        //    //var tradeList = new List<Pair<Trade, NamedValueSet>> { tradePair };

        //    var item = ServerStore.Client.MakeItem(trade1, "trade00001", tradeProps, false, new TimeSpan(1000));
        //    var tradeList = new List<ICoreItem> { item };
        //    var portfolioPricer = new PortfolioPricer(tradeList);
        //    //Calculate the metrics.
        //    //Price.
        //    var logger = new TraceLogger(false);
        //    //var modelData = CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, new PartyIdentifier("Unknown"));
        //    var calcresult = portfolioPricer.Price(logger, tradeList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Full, false);
        //    var result = XmlSerializerHelper.SerializeToString(calcresult);
        //    Debug.Print(result);
        //}


        // needs rewrite
        //[TestMethod]
        //public void TradeFxSwapPricerWithReportingCurrency1WithDetail()
        //{
        //    var trade1 = FpMLTestsSwapHelper.GetTrade08_ExampleObject();

        //    //TODO Add the other trade examples.

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    //trade.ItemElementName = ItemChoiceType16.swap;
        //    //XsdClassesFieldResolver.Trade_SetSwap(trade, swap);
        //    //NamedValueSet tradeProps = new NamedValueSet();
        //    //Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
        //    //    ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxSpot.nvs"));

        //    //Create the tradepricer.
        //    //var tradePricer = new TradePricer(trade, tradeProps);
        //    //trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
        //    //var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    //var tradePair = new Pair<Trade, NamedValueSet>(trade1, tradeProps);
        //    //var tradeList = new List<Pair<Trade, NamedValueSet>> { tradePair };
        //    var item = ServerStore.Client.MakeItem(trade1, "trade00001", tradeProps, false, new TimeSpan(1000));
        //    var tradeList = new List<ICoreItem> { item };
        //    var portfolioPricer = new PortfolioPricer(tradeList);
        //    //Calculate the metrics.
        //    //Price.
        //    var logger = new TraceLogger(false);
        //    //var modelData = CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, new PartyIdentifier("Unknown"));
        //    var calcresult = portfolioPricer.Price(logger, tradeList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Full, false);
        //    var result = XmlSerializerHelper.SerializeToString(calcresult);
        //    Debug.Print(result);
        //    //summary
        //    var calcresult2 = portfolioPricer.Price(logger, tradeList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Summary, false);
        //    var result2 = XmlSerializerHelper.SerializeToString(calcresult2);
        //    Debug.Print(result2);
        //}

        // needs rewrite
        //[TestMethod]
        //public void PortfolioPricerWithReportingCurrency()
        //{
        //    //ird-ex01-vanilla-swap.xml
        //    var trade1 = FpMLTestsSwapHelper.GetTrade01_ExampleObject();

        //    //ird-ex02-stub-amort-swap.xml
        //    var trade2 = FpMLTestsSwapHelper.GetTrade02_ExampleObject();

        //    //ird-ex03-compound-swap.xml
        //    //var trade3 = FpMLTestsSwapHelper.GetTrade03_ExampleObject();

        //    //ird-ex04-arrears-stepup-fee-swap.xml
        //    var trade4 = FpMLTestsSwapHelper.GetTrade04_ExampleObject();

        //    //ird-ex05-long-stub-swap.xml
        //    var trade5 = FpMLTestsSwapHelper.GetTrade05_ExampleObject();

        //    //ird-ex06-xccy-swap.xml
        //    var trade6 = FpMLTestsSwapHelper.GetTrade06_ExampleObject();

        //    //fx-ex03-fx-fwd.xml
        //    var trade7 = FpMLTestsSwapHelper.GetTrade07_ExampleObject();

        //    //fx-ex01-fx-spot.xml
        //    var trade8 = FpMLTestsSwapHelper.GetTrade08_ExampleObject();

        //    //ird-ex07-ois-swap.xml
        //    //var trade9 = FpMLTestsSwapHelper.GetTrade09_ExampleObject();

        //    //ird-ex08-fra.xml
        //    var trade10 = FpMLTestsSwapHelper.GetTrade10_ExampleObject();

        //    //ird-ex28-bullet-payments.xml
        //    var trade11 = FpMLTestsSwapHelper.GetTradeBullet_ExampleObject();

        //    //TODO Add the other trade examples.

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    //trade.ItemElementName = ItemChoiceType16.swap;
        //    //XsdClassesFieldResolver.Trade_SetSwap(trade, swap);
        //    //NamedValueSet tradeProps = new NamedValueSet();
        //    //Trade trade = XmlSerializerHelper.DeserializeFromString<Trade>(
        //    //    ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.xml"));
        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));
        //    var tradePropsxccy = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleXccySwap.nvs"));
        //    var tradePropsfxfwd = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxForward.nvs"));
        //    var tradePropsfxspot = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFxSpot.nvs"));
        //    var tradePropsfra = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleFra.nvs"));
        //    var tradePropsbullet = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleBullet.nvs"));

        //    //Create the tradepricer.
        //    //var tradePricer = new TradePricer(trade, tradeProps);
        //    //trade.id = tradePricer.TradeIdentifier.UniqueIdentifier;
        //    //var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    var tradePair1 = new Pair<Trade, NamedValueSet>(trade1, tradeProps);
        //    var tradePair2 = new Pair<Trade, NamedValueSet>(trade2, tradeProps);
        //    //var tradePair3 = new Pair<Trade, NamedValueSet>(trade3, tradeProps);
        //    var tradePair4 = new Pair<Trade, NamedValueSet>(trade4, tradeProps);
        //    var tradePair5 = new Pair<Trade, NamedValueSet>(trade5, tradeProps);
        //    var tradePair6 = new Pair<Trade, NamedValueSet>(trade6, tradePropsxccy);
        //    var tradePair7 = new Pair<Trade, NamedValueSet>(trade7, tradePropsfxfwd);
        //    var tradePair8 = new Pair<Trade, NamedValueSet>(trade8, tradePropsfxspot);
        //    //var tradePair9 = new Pair<Trade, NamedValueSet>(trade9, tradeProps);
        //    var tradePair10 = new Pair<Trade, NamedValueSet>(trade10, tradePropsfra);
        //    var tradePair11 = new Pair<Trade, NamedValueSet>(trade11, tradePropsbullet);
        //    var tradeList = new List<Pair<Trade, NamedValueSet>>
        //                        {
        //                            tradePair1,
        //                            tradePair2,
        //                            //tradePair3,
        //                            tradePair4,
        //                            tradePair5,
        //                            tradePair6,
        //                            tradePair7,
        //                            tradePair8,
        //                            //tradePair9,
        //                            tradePair10,
        //                            tradePair11
        //                        };
        //    var coreList = tradeList.Select(trade => ServerStore.Client.MakeItem(trade.First, "trade00001", trade.Second, false, new TimeSpan(1000))).ToList();
        //    var portfolioPricer = new PortfolioPricer(coreList);
        //    //Calculate the metrics.
        //    //Price.
        //    var logger = new TraceLogger(false);
        //    //var modelData = CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, new PartyIdentifier("Unknown"));
        //    var calcresult = portfolioPricer.Price(logger, coreList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Summary, false);
        //    foreach (var report in calcresult)
        //    {
        //        var result = XmlSerializerHelper.SerializeToString(report);
        //        Debug.Print(result);
        //    }
        //}


        // needs rewrite
        //[TestMethod]
        //public void TradeSetIRSwapPricerWithReportingCurrency1()
        //{
        //    var trade1 = FpMLTestsSwapHelper.GetTrade01_ExampleObject();

        //    //TODO Add the other trade examples.
        //    var stopwatch = new Stopwatch();

        //    var portfolioProps = new NamedValueSet();
        //    portfolioProps.Set("PortfolioIdentifier", "0001");
        //    portfolioProps.Set("PartyReference", "DBLondon");
        //    portfolioProps.Set("PortfolioName", "Test");

        //    var tradeProps = new NamedValueSet(
        //        ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), "SampleSwap.nvs"));

        //    //Create the tradepricer.
        //    //var portfolioPricer = new PortfolioPricer(portfolioProps);
        //    //var tradePair = new Pair<Trade, NamedValueSet>(trade1, tradeProps);
        //    //var tradeList = new List<Pair<Trade, NamedValueSet>>();
        //    var item = ServerStore.Client.MakeItem(trade1, "trade00001", tradeProps, false, new TimeSpan(1000));
        //    var coreList = new List<ICoreItem> { item };
        //    stopwatch.Start();

        //    for(var i = 0; i < 100; i++)
        //    {
        //        coreList.Add(item);
        //    }
        //    var portfolioPricer = new PortfolioPricer(coreList);
        //    Debug.Print("Load Trades : {0}", stopwatch.Elapsed);
        //    //Calculate the metrics.
        //    //Price.
        //    //var logger = new TraceLogger(false);
        //    var calcresult = portfolioPricer.Price(null, coreList, "LIVE", null, null, "AUD", "X-1131063516", _baseDate, _metrics, ValuationReportType.Summary, false);
        //    Debug.Print("Calculate Trades : {0}", stopwatch.Elapsed);
        //    var result = XmlSerializerHelper.SerializeToString(calcresult);
        //    Debug.Print(result);
        //}

        private static string[] GetSwapMetrics()
        {
            var metrics = Enum.GetNames(typeof(CouponMetrics));

            var result = new List<string>(metrics) { "BreakEvenRate" };
            return result.ToArray();
        }
    }
}