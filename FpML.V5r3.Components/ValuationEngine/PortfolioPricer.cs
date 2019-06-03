#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.CurveEngine.Factory;
using Orion.Constants;
using Orion.Contracts;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;
using Orion.ModelFramework.Instruments;
using Orion.Util.Serialisation;
using Orion.ModelFramework.MarketEnvironments;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.ValuationEngine
{
    public class PortfolioPricer : IPortfolioPricer
    {
        private readonly Dictionary<string, CurveStressPair> _irCurveMap = new Dictionary<string, CurveStressPair>();
        private readonly Dictionary<string, CurveStressPair> _fxCurveMap = new Dictionary<string, CurveStressPair>();

        private readonly NamedValueSet _portfolioProperties = new NamedValueSet();
        public NamedValueSet PortfolioProperties
        {
            get => _portfolioProperties;
            set { _portfolioProperties.Clear(); _portfolioProperties.Add(value); }
        }

        // public properties
        public string ReportingCurrency { get; set; }
        public IIdentifier BaseParty { get; set; }

        // IPortfolioPricer methods
        public IEnumerable<String> GetRequiredPricingStructures() { return _irCurveMap.Values.Select(a => a.Curve); }

        public PortfolioPricer()
        {}

        public PortfolioPricer(NamedValueSet portfolioProps)
        {
            PortfolioProperties.Add(portfolioProps);
        }

        public PortfolioPricer(IEnumerable<CurveStressPair> curveList, IEnumerable<CurveStressPair> currencyList)
        {
            foreach (var item in curveList)
                _irCurveMap[item.Curve] = item;
            foreach (var item in currencyList)
                _fxCurveMap[item.Curve] = item;
        }

        private static IInstrumentControllerData CreateInstrumentModelData(string[] metrics, DateTime valuationDate, IMarketEnvironment market, string reportingCurrency, IIdentifier baseCounterParty)
        {
            var bav = new AssetValuation();
            var currency = CurrencyHelper.Parse(reportingCurrency);
            var quotes = new Quotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                quotes[index] = QuotationHelper.Create(0.0m, metric);
                index++;
            }
            bav.quote = quotes;
            return new InstrumentControllerData(bav, market, valuationDate, currency, baseCounterParty);
        }

        ///<summary>
        /// Prices the portfolio results.
        ///</summary>
        ///<returns></returns>
        //ICollection<ICoreItem>
        public void PriceAndPublish(
            ILogger logger, ICoreCache cache,
            Dictionary<string, ICurve> resolvedCurveCache,
            Dictionary<string, string> reportNameCache,
            HandlerResponse response,
            IEnumerable<string> tradeItemNames,
            ValuationRequest request,
            string irScenario, string fxScenario,
            string reportingCurrency, string baseParty,
            IList<String> metrics, bool backOfficePricing)
        {
            //var results = new List<ICoreItem>();
            var nameSpace = request.NameSpace;
            DateTime lastStatusPublishedAt = DateTime.Now;
            string scenarioPair = ScenarioConst.ScenarioId(irScenario, fxScenario);
            ReportingCurrency = reportingCurrency;
            if (baseParty != null)
            {
                BaseParty = new Identifier(baseParty);
            }
            DateTime baseDate = request.BaseDate;
            //Common properties
            //var requestId = new Guid(request.RequestId);
            //var portfolioId = new Guid(request.PortfolioId);
            string marketName = request.MarketName;
            string market = marketName;
            if (request.MarketDate.HasValue)
                market += ("." + request.MarketDate.Value.ToString(CurveProp.MarketDateFormat));
            // build the market environment
            var marketEnvironment = new MarketEnvironment();
            // Load FX curves
            foreach (var fxItem in _fxCurveMap.Values.Where(a => a.Curve != reportingCurrency))
            {
                string curveTypeAndName = MarketEnvironmentHelper.ResolveFxCurveNames(fxItem.Curve, reportingCurrency);
                string curveSignature = CurveLoader.FxCurveSignature(market, fxItem.Curve, reportingCurrency, fxItem.Stress);
                if (!resolvedCurveCache.TryGetValue(curveSignature, out var curve))
                {
                    // not cached - resolve and cache
                    curve = CurveLoader.LoadFxCurve(logger, cache, nameSpace, market, fxItem.Curve, reportingCurrency, fxItem.Stress);
                    resolvedCurveCache[curveSignature] = curve;
                }
                if (curve == null)
                {
                    logger.LogWarning(
                        "FX curve not found: '{3}' (market={2},scenario={0},stress={1})",
                        scenarioPair, fxItem.Stress ?? "None", market, curveTypeAndName);
                }
                marketEnvironment.AddPricingStructure(curveTypeAndName, curve);
            }
            // Load IR Curves
            foreach (var irItem in _irCurveMap.Values)
            {
                MarketEnvironmentHelper.ResolveRateCurveIdentifier(irItem.Curve, out var curveName);
                string curveSignature = CurveLoader.IrCurveSignature(market, irItem.Curve, irItem.Stress);
                if (!resolvedCurveCache.TryGetValue(curveSignature, out var curve))
                {
                    // not cached - resolve and cache
                    curve = CurveLoader.LoadCurve(logger, cache, nameSpace, null, null, market, irItem.Curve, irItem.Stress);
                    resolvedCurveCache[curveSignature] = curve;
                }
                if (curve == null)
                {
                    logger.LogWarning(
                        "IR curve not found: '{3}' (market={2},scenario={0},stress={1})",
                        scenarioPair, irItem.Stress ?? "None", market, curveName);
                }
                marketEnvironment.AddPricingStructure(irItem.Curve, curve);
            }
            //Now loop through the trades in the portfolio
            foreach (var tradeItemName in tradeItemNames)
            {
                // publish progress update (throttled)
                DateTime dtNow = DateTime.Now;
                if (dtNow - lastStatusPublishedAt > TimeSpan.FromSeconds(5))
                {
                    lastStatusPublishedAt = dtNow;
                    response.Status = RequestStatusEnum.InProgress;
                    cache.SaveObject(response);
                }
                //TODO either reporting or confirmation!
                var tradeItem = tradeItemName.Contains(FpML5R3NameSpaces.Confirmation) ? cache.LoadItem<FpML.V5r3.Confirmation.Trade>(tradeItemName)
                : cache.LoadItem<Trade>(tradeItemName);
                if (tradeItem == null) continue;
                //int errorCount = 0;
                //construct valuation report name
                //var report = new ValuationReport(); // empty
                var reportProps = BuildReportProperties(request, tradeItem, reportingCurrency, scenarioPair, irScenario, fxScenario, request.RealTimePricing);
                string reportName = new ValuationReportIdentifier(reportProps).UniqueIdentifier;
                try
                {
                    // only calculate report if not already calculated
                    //string reportSignature = BuildReportSignature(tradeItem, _irCurveMap, _fxCurveMap);
                    //This was removed as all reports are now calculated.
                    //
                    //string cachedReportName;
                    //if (reportNameCache.TryGetValue(reportSignature, out cachedReportName))
                    //{
                    //    // already calculated - load and republish for this scenario
                    //    report = cache.LoadObject<ValuationReport>(nameSpace + "." + cachedReportName);
                    //}
                    //else
                    //{
                        // calculate
                        PartyIdentifier basePartyId;
                        if (BaseParty == null)
                        {
                            baseParty = tradeItem.AppProps.GetValue<string>(TradeProp.Party1, null);
                            basePartyId = new PartyIdentifier(baseParty);
                        }
                        else
                        {
                            basePartyId = new PartyIdentifier(BaseParty.UniqueIdentifier);
                        }
                        IInstrumentControllerData modelData = CreateInstrumentModelData(
                            metrics.ToArray(), baseDate, marketEnvironment, reportingCurrency, basePartyId);
                        //if (tradeItem.AppProps.GetValue<string>(WFPropName.ExcpName, null) != null)
                        //    errorCount = 1;
                        var schema = tradeItem.AppProps.GetValue(EnvironmentProp.Schema, EnvironmentProp.DefaultNameSpace);
                        TradePricer tradePricer;
                        if (schema.Contains(FpML5R3NameSpaces.Confirmation))
                        {
                            var xml = XmlSerializerHelper.SerializeToString(tradeItem.Data);
                            var newxml = xml.Replace("FpML-5/confirmation", "FpML-5/reporting");
                            var reportingTrade = XmlSerializerHelper.DeserializeFromString<Trade>(newxml);
                            tradePricer = new TradePricer(logger, cache, nameSpace, null, reportingTrade, tradeItem.AppProps, !backOfficePricing);
                        }
                        else
                        {
                            tradePricer = new TradePricer(logger, cache, nameSpace, null, (Trade)tradeItem.Data, tradeItem.AppProps, !backOfficePricing);//TODO Add the calendars!!
                        }
                        var report = tradePricer.Price(modelData, ValuationReportType.Summary);
                        //Added this back so that I don't need the database.
                        cache.SaveObject(report, nameSpace + "." + reportName, reportProps);
                        //TODO
                        // Add the NPV and any other interesting properties to the valuation report
                        //
                        logger.LogDebug("Valued and stored the results for trade: {0}", reportName);
                        //reportNameCache[reportSignature] = reportName;
                    //}
                    // success
                    response.IncrementItemsPassed();
                }
                // expected exceptions
                catch (ApplicationException applicExcp)
                {
                    response.IncrementItemsFailed();
                    string excpName = WFHelper.GetExcpName(applicExcp);
                    string excpText = WFHelper.GetExcpText(applicExcp);
                    logger.LogWarning("{0}: {1}", excpName, excpText);
                    reportProps.Set(WFPropName.ExcpName, excpName);
                    reportProps.Set(WFPropName.ExcpText, excpText);
                }
                catch (NotSupportedException notSuppExcp)
                {
                    response.IncrementItemsFailed();
                    string excpName = WFHelper.GetExcpName(notSuppExcp);
                    string excpText = WFHelper.GetExcpText(notSuppExcp);
                    logger.LogWarning("{0}: {1}", excpName, excpText);
                    reportProps.Set(WFPropName.ExcpName, excpName);
                    reportProps.Set(WFPropName.ExcpText, excpText);
                }
                    // unexpected failure
                catch (System.Exception unexpected)
                {
                    response.IncrementItemsFailed();
                    logger.Log(unexpected);
                    reportProps.Set(WFPropName.ExcpName, WFHelper.GetExcpName(unexpected));
                    reportProps.Set(WFPropName.ExcpText, WFHelper.GetExcpText(unexpected));
                }
                // ================================================================================
                // publish value with lifetime specifified in request
                //results.Add(cache.MakeItem(report, nameSpace + "." + reportName, reportProps, true, TimeSpan.FromMinutes(30)));
            } // foreach trade
            //return results;
        }

        ///<summary>
        /// Prices the portfolio results.
        ///</summary>
        ///<returns></returns>
        //ICollection<ICoreItem>
        public void PriceAndPublish(
            ILogger logger, ICoreCache cache,            
            HandlerResponse response,
            List<string> curvenamesList,
            List<string> currenciesList,
            IList<string> tradeItemNames,
            ValuationRequest request,
            IList<String> metrics, bool backOfficePricing)
        {
            //var results = new List<ICoreItem>();
            var resolvedCurveCache = new Dictionary<string, ICurve>();
            //var reportNameCache = new Dictionary<string, string>();
            var nameSpace = request.NameSpace;
            DateTime lastStatusPublishedAt = DateTime.Now;
            ReportingCurrency = request.ReportingCurrency;
            if (request.BaseParty != null)
            {
                BaseParty = new Identifier(request.BaseParty);
            }
            DateTime baseDate = request.BaseDate;
            //Common properties
            //var requestId = new Guid(request.RequestId);
            //var portfolioId = new Guid(request.PortfolioId);
            string marketName = request.MarketName;
            //Loops through each date specified.
            foreach (var marketDate in request.DateScenarios)
            {
                var market = marketName  + ("." + marketDate.ToString(CurveProp.MarketDateFormat));
                // build the market environment
                var marketEnvironment = new MarketEnvironment();
                // Load FX curves
                foreach (var fxItem in currenciesList.Where(a => a != ReportingCurrency))
                {
                    string curveTypeAndName = MarketEnvironmentHelper.ResolveFxCurveNames(fxItem, ReportingCurrency);
                    string curveSignature = CurveLoader.FxCurveSignature(market, fxItem, ReportingCurrency, null);
                    if (!resolvedCurveCache.TryGetValue(curveSignature, out var curve))
                    {
                        // not cached - resolve and cache
                        curve = CurveLoader.LoadFxCurve(logger, cache, nameSpace, market, fxItem, ReportingCurrency,
                                                        null);
                        resolvedCurveCache[curveSignature] = curve;
                    }
                    if (curve == null)
                    {
                        logger.LogWarning(
                            "FX curve not found: '{2}' (market={0},date={1},stress={1})",
                            market, marketDate, curveTypeAndName);
                    }
                    marketEnvironment.AddPricingStructure(curveTypeAndName, curve);
                }
                // Load IR Curves
                foreach (var irItem in curvenamesList)
                {
                    MarketEnvironmentHelper.ResolveRateCurveIdentifier(irItem, out var curveName);
                    string curveSignature = CurveLoader.IrCurveSignature(market, irItem, null);
                    if (!resolvedCurveCache.TryGetValue(curveSignature, out var curve))
                    {
                        // not cached - resolve and cache
                        curve = CurveLoader.LoadCurve(logger, cache, nameSpace, null, null, market,
                                                                  irItem, null);
                        resolvedCurveCache[curveSignature] = curve;
                    }
                    if (curve == null)
                    {
                        logger.LogWarning(
                            "IR curve not found: '{2}' (market={0},date={1})",
                            market, marketDate, curveName);
                    }
                    marketEnvironment.AddPricingStructure(irItem, curve);
                }
                //Now loop through the trades in the portfolio
                var itemNames = tradeItemNames as string[] ?? tradeItemNames.ToArray();
                foreach (var tradeItemName in itemNames)
                {
                    // publish progress update (throttled)
                    DateTime dtNow = DateTime.Now;
                    if ((dtNow - lastStatusPublishedAt) > TimeSpan.FromSeconds(5))
                    {
                        lastStatusPublishedAt = dtNow;
                        response.Status = RequestStatusEnum.InProgress;
                        cache.SaveObject(response);
                    }
                    //TODO either reporting or confirmation!
                    var tradeItem = tradeItemName.Contains(FpML5R3NameSpaces.Confirmation)
                                        ? cache.LoadItem<FpML.V5r3.Confirmation.Trade>(tradeItemName)
                                        : cache.LoadItem<Trade>(tradeItemName);
                    if (tradeItem == null) continue;
                    //construct valuation report name
                    //var sourceSystem = tradeItem.AppProps.GetValue<string>(TradeProp.SourceSystem, null);
                    var tradeType = tradeItem.AppProps.GetValue<string>(TradeProp.TradeType, null);
                    var tradeId = tradeItem.AppProps.GetValue<string>(TradeProp.TradeId, null);
                    //var report = new ValuationReport(); // empty
                    var reportProps = BuildReportProperties(request, tradeItem, ReportingCurrency, BaseParty.Id, market,
                                                            request.MarketName, marketDate, tradeType, tradeId, request.RealTimePricing);
                    string reportName = new ValuationReportIdentifier(reportProps).UniqueIdentifier;
                    try
                    {
                        // only calculate report if not already calculated
                        //string reportSignature = BuildReportSignature(tradeItem, market);//, market
                        //string cachedReportName;
                        //if (reportNameCache.TryGetValue(reportSignature, out cachedReportName))
                        //{
                        //    // already calculated - load and republish for this scenario
                        //    report = cache.LoadObject<ValuationReport>(nameSpace + "." + cachedReportName);
                        //}
                        //else
                        //{
                            // calculate
                            PartyIdentifier basePartyId;
                            if (BaseParty == null)
                            {
                                var baseParty = tradeItem.AppProps.GetValue<string>(TradeProp.Party1, null);
                                basePartyId = new PartyIdentifier(baseParty);
                            }
                            else
                            {
                                basePartyId = new PartyIdentifier(BaseParty.UniqueIdentifier);
                            }
                            IInstrumentControllerData modelData = CreateInstrumentModelData(
                                metrics.ToArray(), baseDate, marketEnvironment, ReportingCurrency, basePartyId);
                            //if (tradeItem.AppProps.GetValue<string>(WFPropName.ExcpName, null) != null)
                            //    errorCount = 1;
                            var schema = tradeItem.AppProps.GetValue(EnvironmentProp.Schema,
                                                                     EnvironmentProp.DefaultNameSpace);
                            TradePricer tradePricer;
                            if (schema.Contains(FpML5R3NameSpaces.Confirmation))
                            {
                                var xml = XmlSerializerHelper.SerializeToString(tradeItem.Data);
                                var newxml = xml.Replace("FpML-5/confirmation", "FpML-5/reporting");
                                var reportingTrade = XmlSerializerHelper.DeserializeFromString<Trade>(newxml);
                                tradePricer = new TradePricer(logger, cache, nameSpace, null, reportingTrade,
                                                              tradeItem.AppProps, !backOfficePricing);
                            }
                            else
                            {
                                tradePricer = new TradePricer(logger, cache, nameSpace, null, (Trade) tradeItem.Data,
                                                              tradeItem.AppProps, !backOfficePricing);
                                    //TODO Add the calendars!!
                            }
                            var report = tradePricer.Price(modelData, ValuationReportType.Summary);
                            //Added this back so that I don't need the database.
                            cache.SaveObject(report, nameSpace + "." + reportName, reportProps, true,
                                               TimeSpan.FromMinutes(30));
                            logger.LogDebug("Valued and stored the results for trade: {0}", reportName);
                            //reportNameCache[reportSignature] = reportName;
                        //}
                        // success
                        response.IncrementItemsPassed();
                    }
                        // expected exceptions
                    catch (ApplicationException applicExcp)
                    {
                        response.IncrementItemsFailed();
                        string excpName = WFHelper.GetExcpName(applicExcp);
                        string excpText = WFHelper.GetExcpText(applicExcp);
                        logger.LogWarning("{0}: {1}", excpName, excpText);
                        reportProps.Set(WFPropName.ExcpName, excpName);
                        reportProps.Set(WFPropName.ExcpText, excpText);
                    }
                    catch (NotSupportedException notSuppExcp)
                    {
                        response.IncrementItemsFailed();
                        string excpName = WFHelper.GetExcpName(notSuppExcp);
                        string excpText = WFHelper.GetExcpText(notSuppExcp);
                        logger.LogWarning("{0}: {1}", excpName, excpText);
                        reportProps.Set(WFPropName.ExcpName, excpName);
                        reportProps.Set(WFPropName.ExcpText, excpText);
                    }
                        // unexpected failure
                    catch (System.Exception unexpected)
                    {
                        response.IncrementItemsFailed();
                        logger.Log(unexpected);
                        reportProps.Set(WFPropName.ExcpName, WFHelper.GetExcpName(unexpected));
                        reportProps.Set(WFPropName.ExcpText, WFHelper.GetExcpText(unexpected));
                    }
                    // ================================================================================
                    // publish value with lifetime specifified in request
                    //results.Add(cache.MakeItem(report, nameSpace + "." + reportName, reportProps, true, TimeSpan.FromMinutes(30)));
                } // foreach trade
            }
            //return results;
        }

        public static void Parse(IEnumerable<Pair<Trade, NamedValueSet>> trades, NamedValueSet portfolioProps,
            out List<String> uniquePricingStructures, out List<String> uniqueCurrencies, out List<Pair<Trade, NamedValueSet>> validTrades)
        {
            //Initalise the out parameters.
            validTrades = new List<Pair<Trade, NamedValueSet>>();
            var resultValuationReports = new List<ValuationReport>();
            var pricingStructures = new List<string>();
            var currencies = new List<string>();
            uniquePricingStructures = new List<string>();
            uniqueCurrencies = new List<string>();
            foreach (var trade in trades)
            {
                var temp = trade.First.Item;
                //Only add trades that have these methods implemented. Otherwise remove them
                if (temp.GetRequiredPricingStructures() != null && temp.GetRequiredCurrencies() != null)
                {
                    var requiredCurves = temp.GetRequiredPricingStructures();
                    var requiredCurrencies = temp.GetRequiredCurrencies();
                    pricingStructures.AddRange(requiredCurves);
                    currencies.AddRange(requiredCurrencies);
                    validTrades.Add(trade);
                }
                else
                {
                    var report = new ValuationReport();
                    var tradeValuationItem = new TradeValuationItem();
                    //Populate tradeValuationItem with all the error and information required.
                    report.tradeValuationItem = new[] { tradeValuationItem };
                    resultValuationReports.Add(report);
                }
            }
            uniquePricingStructures.AddRange(pricingStructures.Distinct());
            uniqueCurrencies.AddRange(currencies.Distinct());
            //return resultValuationReports;
        }

        //private static string BuildReportSignature(
        //    ICoreItem tradeItem, string market)
        //{
        //    var sb = new StringBuilder();
        //    sb.Append(tradeItem.AppProps.GetValue<string>(TradeProp.TradeSource, null));
        //    sb.Append(".");
        //    sb.Append(tradeItem.AppProps.GetValue<string>(TradeProp.TradeId, null));
        //    foreach (string irCurveId in tradeItem.AppProps.GetArray<string>(TradeProp.RequiredPricingStructures).Where(x => !String.IsNullOrEmpty(x)))
        //    {
        //        sb.Append("_");
        //        sb.Append(irCurveId);
        //    }
        //    foreach (string fxCurveId in tradeItem.AppProps.GetArray<string>(TradeProp.RequiredCurrencies).Where(x => !String.IsNullOrEmpty(x)))
        //    {
        //        sb.Append("_");
        //        sb.Append(fxCurveId);
        //    }
        //    sb.Append("_");
        //    sb.Append(market);
        //    return sb.ToString().ToLower();
        //}

        //private static string BuildReportSignature(
        //    ICoreItem tradeItem,
        //    Dictionary<string, CurveStressPair> irCurveMap,
        //    Dictionary<string, CurveStressPair> fxCurveMap)
        //{
        //    var sb = new StringBuilder();
        //    sb.Append(tradeItem.AppProps.GetValue<string>(TradeProp.TradeSource, null));
        //    sb.Append(".");
        //    sb.Append(tradeItem.AppProps.GetValue<string>(TradeProp.TradeId, null));
        //    foreach (string irCurveId in tradeItem.AppProps.GetArray<string>(TradeProp.RequiredPricingStructures).Where(x => !String.IsNullOrEmpty(x)))
        //    {
        //        sb.Append("_");
        //        sb.Append(irCurveId);
        //        CurveStressPair csp = irCurveMap[irCurveId];
        //        if (csp.Stress != null)
        //        {
        //            sb.Append(".");
        //            sb.Append(csp.Stress);
        //        }
        //    }
        //    foreach (string fxCurveId in tradeItem.AppProps.GetArray<string>(TradeProp.RequiredCurrencies).Where(x => !String.IsNullOrEmpty(x)))
        //    {
        //        sb.Append("_");
        //        sb.Append(fxCurveId);
        //        CurveStressPair csp = fxCurveMap[fxCurveId];
        //        if (csp.Stress != null)
        //        {
        //            sb.Append(".");
        //            sb.Append(csp.Stress);
        //        }
        //    }
        //    return sb.ToString().ToLower();
        //}

        private static NamedValueSet BuildReportProperties(ValuationRequest request, ICoreItem trade, string reportingCurrency, string baseParty, 
            string marketName, string market, DateTime marketDate, string tradeType, string tradeId, bool realTimePricing)
        {
            // construct valuation report name
            DateTime baseDate = request.BaseDate;
            var requestId = new Guid(request.RequestId);
            var reportProps = new NamedValueSet();
            reportProps.Set(EnvironmentProp.NameSpace, request.NameSpace);
            reportProps.Set(CurveProp.BaseDate, baseDate);
            reportProps.Set(CurveProp.Market, request.MarketName);
            reportProps.Set(RequestBase.Prop.RequestId, requestId);
            reportProps.Set(RequestBase.Prop.Requester, request.RequesterId.Name);
            reportProps.Set(ValueProp.ReportingCurrency, reportingCurrency);
            reportProps.Set(ValueProp.MarketName, marketName);
            reportProps.Set(CurveProp.Market, market);
            reportProps.Set(CurveProp.MarketDate, marketDate);
            reportProps.Set(ValueProp.BaseParty, baseParty);
            reportProps.Set(TradeProp.TradeId, tradeId);
            reportProps.Set(TradeProp.TradeType, tradeType);
            reportProps.Set(ValueProp.Aggregation, null);
            reportProps.Set(ValueProp.RealTimePricing, false);
            if (realTimePricing)
            {
                reportProps.Set(ValueProp.RealTimePricing, true);
            }
            reportProps.Add(trade.AppProps.Clone());
            reportProps.Set(CurveProp.UniqueIdentifier, null);
            reportProps.Set(EnvironmentProp.Function, FunctionProp.ValuationReport);
            return reportProps;
        }

        private static NamedValueSet BuildReportProperties(ValuationRequest request, ICoreItem trade, string reportingCurrency, 
            string scenario, string irScenario, string fxScenario, bool realTimePricing)
        {
            // construct valuation report name
            DateTime baseDate = request.BaseDate;
            var requestId = new Guid(request.RequestId);
            var reportProps = new NamedValueSet();
            reportProps.Set(EnvironmentProp.NameSpace, request.NameSpace);
            reportProps.Set(CurveProp.BaseDate, baseDate);
            reportProps.Set(CurveProp.Market, request.MarketName);
            reportProps.Set(RequestBase.Prop.RequestId, requestId);
            reportProps.Set(RequestBase.Prop.Requester, request.RequesterId.Name);
            reportProps.Set(ValueProp.ReportingCurrency, reportingCurrency);
            reportProps.Set(ValueProp.Scenario, scenario);
            reportProps.Set(ValueProp.IrScenario, irScenario);
            reportProps.Set(ValueProp.FxScenario, fxScenario);
            reportProps.Set(ValueProp.Aggregation, null);
            reportProps.Set(ValueProp.RealTimePricing, false);
            if (realTimePricing)
            {
                reportProps.Set(ValueProp.RealTimePricing, true);
            }
            reportProps.Add(trade.AppProps.Clone());
            reportProps.Set(CurveProp.UniqueIdentifier, null);
            reportProps.Set(EnvironmentProp.Function, FunctionProp.ValuationReport);
            return reportProps;
        }
    }

}
