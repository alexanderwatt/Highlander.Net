/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Highlander.CurveEngine.V5r3.Assets.Helpers;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.CurveEngine.V5r3.PricingStructures.Helpers;
using Highlander.CurveEngine.V5r3.PricingStructures.LPM;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Reporting.V5r3;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.ValuationEngine.V5r3.Pricers;
using Highlander.ValuationEngine.V5r3.Generators;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.Factory;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.ValuationEngine.V5r3.Helpers;
using BondHelper = Highlander.Reporting.Helpers.V5r3.BondHelper;
using XsdClassesFieldResolver = Highlander.Reporting.V5r3.XsdClassesFieldResolver;
using Highlander.ValuationEngine.V5r3.Reports;

#endregion

namespace Highlander.ValuationEngine.V5r3
{
    public class ValuationService : IValuationService
    {
        #region Private fields

        public ILogger Logger { get; private set; }
        public ICoreCache Cache { get; private set; }
        public String NameSpace { get; private set; }
        //private const string ClientNamespace = "ValuationEngine";

        #endregion
        
        #region Constructor

        /// <summary>
        /// THe main cinstructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        public ValuationService(ILogger logger, ICoreCache cache)
            : this(logger, cache, EnvironmentProp.DefaultNameSpace)
        {}


        /// <summary>
        /// THe main cinstructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        public ValuationService(ILogger logger, ICoreCache cache, String nameSpace)
        {
            Logger = logger;
            Cache = cache;
            NameSpace = nameSpace;
        }

        #endregion

        #region Interface Methods

        ///// <summary>
        ///// This interface provides a single point of entry to value all portfolios,  
        ///// </summary>
        ///// <param name="valuationRequest">The valuation request contains all the trades to be valued and all the markets
        ///// for generating a market environment for valuations. The pricing structures will  have all the market quotes</param>
        ///// <param name="valuationProperties">The valuation Properties must contain all extra data required to build the market environment.</param>
        ///// <param name="modelData">The model data contains all the information around metrics and base currency.
        ///// The market environment, if null, will be generated from the Market contained in the valuation report
        ///// using default parameters.</param>
        ///// <param name="calendars">The calendars to be used.</returns>
        ///// <returns>A valuation report with the calculated data.</returns>
        //public ValuationReport GetPortfolioValuation(ValuationReport valuationRequest, NamedValueSet valuationProperties,
        //    IInstrumentControllerData modelData, List<BusinessCenterCalendar> calendars)
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// The portfolioPricer exposes the basic portfolio functionality.
        ///// </summary>
        ///// <param name="curveList"></param>
        ///// <param name="currencyList"></param>
        ///// <returns></returns>
        //public PortfolioPricer GetPortfolioPricer(IEnumerable<CurveStressPair> curveList, IEnumerable<CurveStressPair> currencyList)
        //{
        //    var portfolioPricer = new PortfolioPricer(curveList, currencyList);
        //    return portfolioPricer;
        //}

        ///// <summary>
        ///// The trade pricer is the lowest levl method on this interface.
        ///// </summary>
        ///// <param name="legCalendars">A list of pairs of leg calenders: one pair for each leg of the swap.
        ///// If this list is null then a calendar is created.</param>
        ///// <param name="trade">An valid fpml trade.</param>
        ///// <param name="tradeProps">A namedvalue set of trade proerties.</param>
        ///// <param name="forecastRateInterpolation">A boolan flag.</param>
        //public TradePricer GetTradePricer(List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars, 
        //    Trade trade, NamedValueSet tradeProps, bool forecastRateInterpolation)
        //{
        //    var tradePricer = new TradePricer(_Logger, _Cache, legCalendars, trade, tradeProps, forecastRateInterpolation);
        //    return tradePricer;
        //}

        //#endregion

        //#region Pricing methods

        /////<summary>
        ///// Prices the trade.
        /////</summary>
        ///// <param name="legCalendars">A list of pairs of leg calenders: one pair for each leg of the swap.
        ///// If this list is null then a calendar is created.</param>
        ///// <param name="trade">An valid fpml trade.</param>
        ///// <param name="tradeProps">A namedvalue set of trade proerties.</param>
        ///// <param name="forecastRateInterpolation">A boolan flag.</param>
        /////<returns>A valuation report.</returns>
        //public ValuationReport PriceTrade(List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars, 
        //    Trade trade, NamedValueSet tradeProps, bool forecastRateInterpolation,
        //    IInstrumentControllerData modelData, ValuationReportType reportType)
        //{
        //    //Currently the leg calendars are ignore. They will eventially be published into a separate client namespace.
        //    var tradePricer = GetTradePricer(legCalendars, trade, tradeProps, forecastRateInterpolation);
        //    var price = tradePricer.Price(modelData, reportType);
        //    return price;
        //}

        /////<summary>
        ///// Prices the portfolio results.
        /////</summary>
        /////<returns></returns>
        ///// <param name="curveList"></param>
        ///// <param name="currencyList"></param>
        ///// <param name="resolvedCurveCache"></param>
        ///// <param name="reportNameCache"></param>
        ///// <param name="response"></param>
        ///// <param name="tradeItemNames"></param>
        ///// <param name="request"></param>
        ///// <param name="irScenario"></param>
        ///// <param name="fxScenario"></param>
        ///// <param name="reportingCurrency"></param>
        ///// <param name="baseParty"></param>
        ///// <param name="metrics"></param>
        ///// <param name="backOfficePricing"></param>
        ///// <returns></returns>
        //public ICollection<ICoreItem> PortfolioPriceAndPublish(
        //    IEnumerable < CurveStressPair > curveList, 
        //    IEnumerable < CurveStressPair > currencyList,
        //    Dictionary<string, ICurve> resolvedCurveCache,
        //    Dictionary<string, string> reportNameCache,
        //    HandlerResponse response,
        //    IEnumerable<string> tradeItemNames,
        //    ValuationRequest request,
        //    string irScenario, string fxScenario,
        //    string reportingCurrency, string baseParty,
        //    IEnumerable<String> metrics, bool backOfficePricing)
        //{
        //    var portfolioPricer = GetPortfolioPricer(curveList, currencyList);
        //    var result = portfolioPricer.PriceAndPublish(_Logger, _Cache, resolvedCurveCache, reportNameCache, response, tradeItemNames,
        //        request, irScenario, fxScenario, reportingCurrency, baseParty, metrics, backOfficePricing);
        //    return result;
        //}

        #endregion

        #region Create Calendars

        /// <summary>
        /// Creates a user defined calendar.
        /// </summary>
        /// <param name="calendarProperties">THe calendar properties must include a valid FpML business center name.</param>
        /// <param name="holidaysDates">The dates that are in the defined calendar.</param>
        /// <returns></returns>
        public string CreateCalendar(NamedValueSet calendarProperties, List<DateTime> holidaysDates)
        {
            return CreateCalendar(Logger, Cache, NameSpace, calendarProperties, holidaysDates);
        }

        /// <summary>
        /// Creates a user defined calendar.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="calendarProperties">The calendar properties must include a valid FpML business center name.</param>
        /// <param name="holidaysDates">The dates that are in the defined calendar.</param>
        /// <returns></returns>
        public string CreateCalendar(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet calendarProperties, List<DateTime> holidaysDates)
        {
            var identifier = calendarProperties.GetValue<string>("UniqueIdentifier");
            var calendar = BusinessCalendarHelper.CreateCalendar(calendarProperties, holidaysDates);
            //Save the calendar
            cache.SaveObject(calendar, nameSpace + "." + identifier, calendarProperties);
            logger.LogDebug("Loaded business center holiday dates: {0}", identifier);
            return identifier;
        }

        #endregion

        #region Create Curves

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional.</param>
        /// <param name="fixingCenters">The fixing Calendar.</param>
        /// <param name="paymentCenters">The payment Calendar.</param>
        /// <returns></returns>
        public string CreateCurve(NamedValueSet properties, string[] instruments, Decimal[] adjustedRates, Decimal[] additional,
            List<string> fixingCenters, List<string> paymentCenters)
        {
            return CreateCurve(Logger, Cache, NameSpace, properties, instruments, adjustedRates, additional, fixingCenters, paymentCenters);
        }

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional.</param>
        /// <param name="fixingCenters">The fixing Calendar.</param>
        /// <param name="paymentCenters">The payment Calendar.</param>
        /// <returns></returns>
        public string CreateCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, string[] instruments, Decimal[] adjustedRates, Decimal[] additional,
            List<string> fixingCenters, List<string> paymentCenters)
        {
            //Get the calendars
            var fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fixingCenters.ToArray(), NameSpace);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, paymentCenters.ToArray(), NameSpace);
            var curve = PricingStructureFactory.CreateCurve(Logger, Cache, NameSpace, fixingCalendar, paymentCalendar, properties, instruments, adjustedRates, additional);
            var market = curve.GetMarket();
            var identifier = curve.GetPricingStructureId().UniqueIdentifier;
            cache.SaveObject(market, nameSpace + "." + identifier, curve.GetPricingStructureId().Properties, TimeSpan.MaxValue);
            logger.LogDebug("Loaded new curve: {0}", identifier);
            return identifier;
        }

        #endregion

        #region Query Functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameSpace">The required nameSpace.</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <returns></returns>
        public Trade GetTrade(string uniqueTradeId)
        {
            return GetTrade(Logger, Cache, NameSpace, uniqueTradeId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameSpace">The required nameSpace.</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <returns></returns>
        public Trade GetTrade(string nameSpace, string uniqueTradeId)
        {
            return GetTrade(Logger, Cache, nameSpace, uniqueTradeId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="cache">The injected cache.</param>
        /// <param name="nameSpace">The required nameSpace.</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <returns></returns>
        public Trade GetTrade(ILogger logger, ICoreCache cache, string nameSpace, string uniqueTradeId)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            if (tradeItem != null)
            {
                //var properties = tradeItem.AppProps;
                if (tradeItem.Data is Trade trade)
                {
                    logger.LogDebug("Returned trade: " + uniqueTradeId);
                    return trade;
                }
            }
            logger.LogDebug("No trade: " + uniqueTradeId);
            return null;
        }

        /// <summary>
        /// Returns the asset identifiers for all property assets matching the query properties.
        /// </summary>
        /// <param name="queryProperties">The query properties.</param>
        /// <returns></returns>
        public List<string> QueryPropertyAssetIds(NamedValueSet queryProperties)
        {
            var assets = QueryPropertyAssetProperties(Logger, Cache, NameSpace, queryProperties);
            return assets;
        }

        /// <summary>
        /// Returns the trade identifiers for all trades matching the query properties.
        /// </summary>
        /// <param name="queryProperties">The query properties.</param>
        /// <returns></returns>
        public List<string> QueryTradeIds(NamedValueSet queryProperties)
        {
            var trades = QueryTrades(Logger, Cache, NameSpace, queryProperties);
            var result = new List<string>();
            foreach (var trade in trades)
            {
                result.Add(trade.UniqueIdentifier);
            }
            return result;
        }

        /// <summary>
        /// Returns the trades for all trades matching the query properties.
        /// </summary>
        /// <param name="queryProperties">The query properties.</param>
        /// <returns></returns>
        public List<TradeQueryData> QueryTrades(NamedValueSet queryProperties)
        {
            return QueryTrades(Logger, Cache, NameSpace, queryProperties);
        }

        /// <summary>
        /// Returns the trade identifiers for all trades matching the query properties.
        /// </summary>
        /// <param name="whereExpr">The query properties. A 3-column array of names, operations and values.</param>
        /// <returns></returns>
        public List<TradeQueryData> QueryTrades(List<Triplet<string, string, object>> query)
        {
            return QueryTrades(Logger, Cache, NameSpace, query);
        }

        /// <summary>
        /// Returns the property data for all trades matching the query properties.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="query">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public List<string> QueryPropertyAssetProperties(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet matchingPropertySet)
        {
            IExpression whereExpr = Expr.BoolAND(Expr.IsEQU("NameSpace", nameSpace));
            var result = new List<string>();
            if (matchingPropertySet != null)
            {
                foreach (var property in matchingPropertySet.ToArray())
                {
                    whereExpr = Expr.BoolAND(Expr.IsEQU(property.Name, property.Value), whereExpr);
                }
            }
            var assets = cache.LoadItems<PropertyNodeStruct>(whereExpr);
            foreach (var asset in assets)
            {
                var property = asset.AppProps;
                var id = property.GetString(PropertyProp.UniqueIdentifier, true);
                result.Add(id);
            }
            logger.LogDebug("{} property assets returned;", result.Count);
            return result;
        }

        /// <summary>
        /// Returns the property data for all properties matching the query properties.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="query">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public List<PropertyNodeStruct> QueryPropertyAssets(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet matchingPropertySet)
        {
            IExpression whereExpr = Expr.BoolAND(Expr.IsEQU("NameSpace", nameSpace));
            var result = new List<PropertyNodeStruct>();
            if (matchingPropertySet != null)
            {
                foreach (var property in matchingPropertySet.ToArray())
                {
                    whereExpr = Expr.BoolAND(Expr.IsEQU(property.Name, property.Value), whereExpr);
                }
            }
            var assets = cache.LoadItems<PropertyNodeStruct>(whereExpr);
            foreach (var asset in assets)
            {
                var property = (PropertyNodeStruct) asset.Data;
                result.Add(property);
            }
            logger.LogDebug("{} property assets returned;", result.Count);
            return result;
        }

        /// <summary>
        /// Returns the trade data for all trades matching the query properties.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="query">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public List<TradeQueryData> QueryTrades(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet matchingPropertySet)
        {
            IExpression whereExpr = Expr.BoolAND(Expr.IsEQU("NameSpace", nameSpace));
            if (matchingPropertySet != null)
            {
                foreach (var property in matchingPropertySet.ToArray())
                {
                    whereExpr = Expr.BoolAND(Expr.IsEQU(property.Name, property.Value), whereExpr);
                }
            }
            return QueryTrades(logger, cache, whereExpr);
        }

        /// <summary>
        /// Returns the trade data for all trades matching the query properties.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="query">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public List<TradeQueryData> QueryTrades(ILogger logger, ICoreCache cache, string nameSpace, List<Triplet<string, string, object>> query)
        {
            IExpression whereExpr = null;
            if (nameSpace != null)
            {
                whereExpr = Expr.IsEQU(EnvironmentProp.NameSpace, nameSpace);
            }
            foreach (var row in query)
            {
                var name = row.First;
                var op = row.Second;
                object value = row.Third;
                if (name != null && (op != null) && (value != null))
                {
                    op = op.ToLower().Trim();
                    if (op == "equ" || op == "==")
                        whereExpr = Expr.BoolAND(Expr.IsEQU(name, value), whereExpr);
                    else if (op == "neq" || op == "!=")
                        whereExpr = Expr.BoolAND(Expr.IsNEQ(name, value), whereExpr);
                    else if (op == "geq" || op == ">=")
                        whereExpr = Expr.BoolAND(Expr.IsGEQ(name, value), whereExpr);
                    else if (op == "leq" || op == "<=")
                        whereExpr = Expr.BoolAND(Expr.IsLEQ(name, value), whereExpr);
                    else if (op == "gtr" || op == ">")
                        whereExpr = Expr.BoolAND(Expr.IsGTR(name, value), whereExpr);
                    else if (op == "lss" || op == "<")
                        whereExpr = Expr.BoolAND(Expr.IsLSS(name, value), whereExpr);
                    else if (op == "starts")
                        whereExpr = Expr.BoolAND(Expr.StartsWith(name, value.ToString()), whereExpr);
                    else if (op == "ends")
                        whereExpr = Expr.BoolAND(Expr.EndsWith(name, value.ToString()), whereExpr);
                    else if (op == "contains")
                        whereExpr = Expr.BoolAND(Expr.Contains(name, value.ToString()), whereExpr);
                    else
                        throw new ApplicationException("Unknown Operator: '" + op + "'");
                }
            }
            return QueryTrades(logger, cache, whereExpr);
        }

        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="query">The query properties whereExpr.</param>
        /// <returns></returns>
        public List<TradeQueryData> QueryTrades(ILogger logger, ICoreCache cache, IExpression whereExpr)
        {
            List<ICoreItem> items = cache.LoadItems(typeof(Trade), whereExpr);//TODO what about confirmation?
            var result = new List<TradeQueryData>();
            // now add data rows
            int tradeNum = 1;
            foreach (ICoreItem item in items)
            {
                try
                {
                    var tradeData = new TradeQueryData
                    {
                        ProductType = item.AppProps.GetValue<string>(TradeProp.ProductType),
                        TradeId = item.AppProps.GetValue<string>(TradeProp.TradeId),
                        TradeDate = item.AppProps.GetValue<DateTime>(TradeProp.TradeDate),
                        MaturityDate = item.AppProps.GetValue<DateTime>(TradeProp.MaturityDate),
                        EffectiveDate = item.AppProps.GetValue<DateTime>(TradeProp.EffectiveDate),
                        TradeState = item.AppProps.GetValue<string>(TradeProp.TradeState),
                        RequiredCurrencies = string.Join(";",
                            item.AppProps.GetArray<string>(TradeProp.RequiredCurrencies)),
                        RequiredPricingStructures = string.Join(";",
                            item.AppProps.GetArray<string>(TradeProp.RequiredPricingStructures)),
                        ProductTaxonomy = item.AppProps.GetValue<string>(TradeProp.ProductTaxonomy, null),
                        AsAtDate = item.AppProps.GetValue<DateTime>(TradeProp.AsAtDate),
                        SourceSystem = item.AppProps.GetValue<string>(EnvironmentProp.SourceSystem),
                        TradingBookId = item.AppProps.GetValue<string>(TradeProp.TradingBookId),
                        TradingBookName = item.AppProps.GetValue<string>(TradeProp.TradingBookName),
                        BaseParty = item.AppProps.GetValue<string>(TradeProp.BaseParty),
                        Party1 = item.AppProps.GetValue<string>(TradeProp.Party1),
                        Party2 = item.AppProps.GetValue<string>(TradeProp.Party2),
                        UniqueIdentifier = item.AppProps.GetValue<string>(TradeProp.UniqueIdentifier),
                        CounterPartyName = item.AppProps.GetValue<string>(TradeProp.CounterPartyName),
                        OriginatingPartyName = item.AppProps.GetValue<string>(TradeProp.OriginatingPartyName)
                    };
                    tradeNum++;
                }
                catch (System.Exception e)
                {
                    logger.LogError($"TradeStore.QueryTrades: Exception: {0}", e);
                }
            }
            logger.LogInfo("{} trades retrieved from the cache.", tradeNum);
            return result;
        }

        /// <summary>
        /// Gets the valuation reports satisfying the properties..
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <param name="metricToReturn">The metric array to return.</param>
        /// <returns></returns>
        public IDictionary<string, decimal> ShowValuationReports(NamedValueSet requestProperties, string metricToReturn)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException("requestProperties");
            //The new filter with OR on arrays..
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            // Load items
            var reportItems = Cache.LoadItems<ValuationReport>(queryExpr);
            var structures = new Dictionary<string, decimal>();
            foreach (var reportItem in reportItems)
            {
                if (reportItem.Data is ValuationReport item)
                {
                    var identifier = new ValuationReportIdentifier(reportItem.AppProps);
                    var tvi = item.tradeValuationItem[0];
                    var assetValuationReport = tvi.valuationSet.assetValuation[0];
                    var matchedQuotes = assetValuationReport.quote.Where(q => q.measureType.Value.Equals(metricToReturn, StringComparison.OrdinalIgnoreCase));
                    var quotations = matchedQuotes as IList<Quotation> ?? matchedQuotes.ToList();
                    if (quotations.First() != null)
                    {
                        var metric = quotations.First().value;
                        //Check there is no duplication and if there is, then handle it.
                        if (structures.ContainsKey(identifier.UniqueIdentifier))
                        {
                            structures.Add(identifier.UniqueIdentifier + "_Duplicate", metric);
                        }
                        else
                        {
                            structures.Add(identifier.UniqueIdentifier, metric);
                        }
                        Logger.LogDebug("Valuation report metric: {0}-{1}-{2}", reportItem.Name, metricToReturn, metric);
                    }
                }
            }
            Logger.LogDebug("Number of Valuation reports returned: {0}", reportItems.Count());
            return structures;
        }

        /// <summary>
        /// Returns the information for the trade matching the tradeId supplied.
        /// </summary>
        /// <param name="valuationId">The valuation Id.</param>
        /// <returns>A matrix of flattened trade data.</returns>
        public object DisplaySwapValuationData(string valuationId)
        {
            var valuation = Cache.LoadObject<ValuationReport>(NameSpace + "." + valuationId);
            var trade = valuation.tradeValuationItem[0];
            Swap swap = null;
            if (trade.Items[0] is Trade swapTrade)
            {
                swap = swapTrade.Item as Swap;
            }
            //This is where we want to use the report function.
            return swap != null ? InterestRateSwapReporter.CouponDataReport(swap) : null;
        }

        /// <summary>
        /// Returns the information for the trade matching the tradeId supplied.
        /// </summary>
        /// <param name="valuationId">The valuation Id.</param>
        /// <returns>A matrix of flattened trade data.</returns>
        public object DisplayFraValuationData(string valuationId)
        {
            var valuation = Cache.LoadObject<ValuationReport>(NameSpace + "." + valuationId);
            var trade = valuation.tradeValuationItem[0];
            Fra fra = null;
            if (trade.Items[0] is Trade fraTrade)
            {
                fra = fraTrade.Item as Fra;
            }
            //This is where we want to use the report function.
            return fra != null ? new ForwardRateAgreementReporter().DoReport(fra) : null;
        }

        #endregion

        #region LpM and PPD.

        /// <summary>
        /// Publishes the LPM cap floor vol matrix.
        /// </summary>
        /// <param name="structurePropertiesRange">The structure properties range.</param>
        /// <param name="publishPropertiesRange">The publish properties range.</param>
        /// <param name="valuesRange">The values range.</param>
        /// <param name="rateCurveFiltersRange">The rate curve filters range.</param>
        /// <returns></returns>
        public string PublishLpmCapFloorVolMatrix(object[,] structurePropertiesRange, object[,] publishPropertiesRange, object[,] valuesRange, object[,] rateCurveFiltersRange)
        {
            // Translate into useful objects
            NamedValueSet structureProperties = structurePropertiesRange.ToNamedValueSet();
            NamedValueSet publishProperties = publishPropertiesRange.ToNamedValueSet();
            NamedValueSet rateCurveFilters = rateCurveFiltersRange.ToNamedValueSet();
            //object[,] values = valuesRange.ConvertArrayToMatrix();
            string[] columnNames = Array.ConvertAll(valuesRange.GetRow(0), Convert.ToString);
            object[] ppd = valuesRange.GetColumn(1);
            for (int index = 0; index < ppd.Length; index++)
            {
                ppd[index] = ppd[index] is Double ? Convert.ToDouble(ppd[index]) / 100 : ppd[index];
            }
            valuesRange.SetColumn(1, ppd);
            object[][] data = valuesRange.GetRows(1, valuesRange.RowCount());
            object[][] properties = structurePropertiesRange.GetRows(1, structurePropertiesRange.RowCount());
            // Create matrix
            var baseDate = structureProperties.GetValue<DateTime>("BaseDate", false);
            if (baseDate == DateTime.MinValue)
            {
                baseDate = structureProperties.GetValue<DateTime>("BuildDateTime", false).Date;
                if (baseDate == DateTime.MinValue)
                {
                    baseDate = DateTime.Today;
                }
            }
            string sourceName = structureProperties.GetString("Source", true);
            string currency = structureProperties.GetString("Currency", true);
            string marketName = structureProperties.GetString("MarketName", true);
            string indexName = structureProperties.GetString("IndexName", true);
            int year = baseDate.Year;
            int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(baseDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            string id = string.Format("LPMCapFloorCurve.{0}.{1:yyyy-MM-dd}", currency, baseDate);
            var matrix = new CapFloorATMMatrix(columnNames, data, properties, baseDate, id);
            // Load underlying curve
            Market market = GetCurve(Logger, Cache, NameSpace, rateCurveFilters);
            // Create the capFloors and properties
            Market capFloors = LPMCapFloorCurve.ProcessCapFloor(Logger, Cache, NameSpace, market, matrix);
            string newId = string.Format("{0}.{1}.CapFloor.{2}.{3}.{4}.Week{5}", marketName, indexName, currency, sourceName, year, weekOfYear);
            string name = string.Format("CapFloor-{0}-{1}-{2:dd/MM/yyyy}", currency, sourceName, baseDate);
            structureProperties.Set("Identifier", newId);
            structureProperties.Set("Name", name);
            // Save
            TimeSpan lifetime = GetLifetime(publishProperties);
            var uniqueIdentifier = NameSpace + "." + "Market." + newId;
            Cache.SaveObject(capFloors, uniqueIdentifier, structureProperties, lifetime);
            Logger.LogInfo("Published '{0}'.", uniqueIdentifier);
            return newId;
        }

        /// <summary>
        /// Publishes the LPM swaption vol matrix.
        /// </summary>
        /// <param name="structurePropertiesRange">The structure properties range.</param>
        /// <param name="publishPropertiesRange">The publish properties range.</param>
        /// <param name="valuesRange">The values range.</param>
        /// <param name="rateCurveFiltersRange">The rate curve filters range.</param>
        /// <returns></returns>
        public string PublishLpmSwaptionVolMatrix(object[,] structurePropertiesRange, object[,] publishPropertiesRange, object[,] valuesRange, object[,] rateCurveFiltersRange)
        {
            NamedValueSet structureProperties = structurePropertiesRange.ToNamedValueSet();
            NamedValueSet publishProperties = publishPropertiesRange.ToNamedValueSet();
            NamedValueSet rateCurveFilters = rateCurveFiltersRange.ToNamedValueSet();
            //object[,] values = valuesRange.ConvertArrayToMatrix();
            string[] tenors = Array.ConvertAll(valuesRange.GetRow(0).Skip(1).ToArray(), Convert.ToString);
            string[] expiries = Array.ConvertAll(valuesRange.GetColumn(0).Skip(1).ToArray(), Convert.ToString);
            object[][] dataObjects = valuesRange.GetRows(1, valuesRange.RowCount(), 1, valuesRange.ColumnCount());
            object[][] data = dataObjects.Select(a => a.Select(b => b).ToArray()).ToArray();
            // Setup base values needed to build
            var baseDate = structureProperties.GetValue<DateTime>("BaseDate", false);
            if (baseDate == DateTime.MinValue)
            {
                baseDate = structureProperties.GetValue<DateTime>("BuildDateTime", false).Date;
                if (baseDate == DateTime.MinValue)
                {
                    baseDate = DateTime.Today;
                }
            }
            string sourceName = structureProperties.GetString("Source", true);
            string currency = structureProperties.GetString("Currency", true);
            string marketName = structureProperties.GetString("MarketName", true);
            string indexName = structureProperties.GetString("IndexName", true);
            int year = baseDate.Year;
            int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(baseDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            string id = string.Format("LPMSwaptionCurve.{0}.{1:yyyy-MM-dd}", currency, baseDate);
            var ppdGrid = new SwaptionPPDGrid(expiries, tenors, data);
            // Load underlying curve
            Market market = GetCurve(Logger, Cache, NameSpace, rateCurveFilters);
            Market matrix = LPMSwaptionCurve.ProcessSwaption(Logger, Cache, market, ppdGrid, id, NameSpace);
            string newId = string.Format("{0}.{1}.Swaption.{2}.{3}.{4}.Week{5}", marketName, indexName, currency, sourceName, year, weekOfYear);
            string name = string.Format("Swaption-{0}-{1}-{2:dd/MM/yyyy}", currency, sourceName, baseDate);
            structureProperties.Set("Identifier", newId);
            structureProperties.Set("Name", name);
            // Save
            TimeSpan lifetime = GetLifetime(publishProperties);
            var uniqueIdentifier = NameSpace + "." + "Market." + newId;
            Cache.SaveObject(matrix, uniqueIdentifier, structureProperties, lifetime);
            Logger.LogInfo("Published '{0}'.", uniqueIdentifier);
            return newId;
        }

        private static Market GetCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet rateCurveFilters)
        {
            IExpression queryExpr = Expr.BoolAND(rateCurveFilters);
            List<ICoreItem> loadedCurves = cache.LoadItems<Market>(queryExpr);
            if (loadedCurves.Count == 0)
            {
                throw new InvalidOperationException("Requested curve is not available.");
            }
            if (loadedCurves.Count > 1)
            {
                throw new InvalidOperationException("More than 1 curve found for supplied filters.");
            }
            var loadedCurve = loadedCurves.Single();
            var market = (Market)loadedCurve.Data;
            var yieldCurveValuation = (YieldCurveValuation)market.Items1[0];
            DateTime baseDate = yieldCurveValuation.baseDate.Value;
            string currency = PropertyHelper.ExtractCurrency(loadedCurve.AppProps);
            yieldCurveValuation.discountFactorCurve = ConstructDiscountFactors(logger, cache, nameSpace, yieldCurveValuation.discountFactorCurve, baseDate, currency);
            return market;
        }

        private static TimeSpan GetLifetime(NamedValueSet publishProperties)
        {
            int expiryInterval = publishProperties.GetValue("ExpiryIntervalInMins", 60);
            var lifetime = new TimeSpan(0, expiryInterval, 0);
            return lifetime;
        }

        internal static TermCurve ConstructDiscountFactors(ILogger logger, ICoreCache cache, string nameSpace, TermCurve inputCurve, DateTime baseDate, string currency)
        {
            List<DateTime> dates = inputCurve.point.Select(a => (DateTime)a.term.Items[0]).ToList();
            List<decimal> values = inputCurve.point.Select(a => a.mid).ToList();
            var properties = new NamedValueSet();
            properties.Set(CurveProp.PricingStructureType, "RateCurve");
            properties.Set(CurveProp.Market, "ConstructDiscountFactors");
            properties.Set(CurveProp.IndexTenor, "0M");
            properties.Set(CurveProp.Currency1, currency);
            properties.Set(CurveProp.IndexName, "XXX-XXX");
            properties.Set(CurveProp.Algorithm, "FastLinearZero");
            properties.Set(CurveProp.BaseDate, baseDate);
            var curveId = new RateCurveIdentifier(properties);
            var algorithmHolder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, curveId.PricingStructureType, curveId.Algorithm);
            var curve = new RateCurve(properties, algorithmHolder, dates, values);
            var termPoints = new List<TermPoint>();
            for (DateTime date = dates.First(); date <= dates.Last(); date = date.AddMonths(1))
            {
                var discountFactor = (decimal)curve.GetDiscountFactor(date);
                var timeDimension = new TimeDimension();
                XsdClassesFieldResolver.TimeDimensionSetDate(timeDimension, date);
                var termPoint = new TermPoint
                {
                    mid = discountFactor,
                    midSpecified = true,
                    term = timeDimension
                };
                termPoints.Add(termPoint);
            }
            var termCurve = new TermCurve { point = termPoints.ToArray() };
            return termCurve;
        }

        #endregion

        #region Refresh Curves with Market Data

        public string RefreshPricingStructure(string pricingStructureId, string[] assetIdentifiers, decimal[] values,
                                           decimal[] additonal)
        {
            var qas = AssetHelper.Parse(assetIdentifiers, values, additonal);
            var result = Highlander.CurveEngine.V5r3.CurveEngine.RefreshPricingStructure(Logger, Cache, NameSpace, pricingStructureId, qas);
            return result;
        }

        #endregion

        #region Generic Functions

        /// <summary>
        /// Save the trade to a file.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="filename">The fileneame</param>
        /// <returns></returns>
        public string SaveTradeToFile(string uniqueTradeId, string filename)
        {
            var tradeItem = Cache.LoadItem<Trade>(NameSpace + "." + uniqueTradeId);
            if (tradeItem.Data is Trade)
            {
                //string baseFilename = Path.GetFullPath(filename);
                string xmlFilename = Path.ChangeExtension(filename, ".xml");
                string nvsFilename = Path.ChangeExtension(xmlFilename, ".nvs");
                using (var sr = new StreamWriter(xmlFilename))
                {
                    sr.Write(tradeItem.Text);
                }
                using (var sr = new StreamWriter(nvsFilename))
                {
                    sr.Write(tradeItem.AppProps.Serialise());
                }
                return filename;
            }
            return "No trade found and saved";
        }

        /// <summary>
        /// Save the valuation to a file.
        /// </summary>
        /// <param name="uniqueValuationId">The unique valuation identifier.</param>
        /// <param name="filename">The fileneame</param>
        /// <returns></returns>
        public string SaveValuationToFile(string uniqueValuationId, string filename)
        {
            var valItem = Cache.LoadItem<Valuation>(NameSpace + "." + uniqueValuationId);
            if (valItem.Data is Valuation)
            {
                //string baseFilename = Path.GetFullPath(filename);
                string xmlFilename = Path.ChangeExtension(filename, ".xml");
                string nvsFilename = Path.ChangeExtension(xmlFilename, ".nvs");
                using (var sr = new StreamWriter(xmlFilename))
                {
                    sr.Write(valItem.Text);
                }
                using (var sr = new StreamWriter(nvsFilename))
                {
                    sr.Write(valItem.AppProps.Serialise());
                }
                return filename;
            }
            return "No Valuation found and saved";
        }

        /// <summary>
        /// Gets the curve names necessary to value the trade.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <returns></returns>
        public object[,] GetRequiredCurves(string uniqueTradeId)
        {
            return GetRequiredCurves(Logger, Cache, NameSpace, uniqueTradeId);
        }

        /// <summary>
        /// Gets the curve names necessary to value the trade.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The namespace</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <returns></returns>
        public object[,] GetRequiredCurves(ILogger logger, ICoreCache cache, string nameSpace, string uniqueTradeId)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            if (tradeItem.Data is Trade trade)
            {
                var curves = trade.Item.GetRequiredPricingStructures();
                logger.LogDebug("Returned a list of required curves for the trade: {0}", uniqueTradeId);
                return RangeHelper.ConvertArrayToRange(curves);
            }
            var result = new object[1, 1];
            result[0, 0] = "The trade id uded does not return a valid trade!";
            return result;
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        ///  <returns></returns>
        public List<object[]> ViewExpectedCashFlows(string uniqueTradeId, string reportingParty, List<string> metricsArray, string reportingCurrency, string market,
            DateTime valuationDate)
        {
            return ViewExpectedCashFlows(Logger, Cache, NameSpace, uniqueTradeId, reportingParty, metricsArray, reportingCurrency, market, valuationDate);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        ///  <returns></returns>
        public List<object[]> ViewExpectedCashFlows(ILogger logger, ICoreCache cache, string nameSpace,
            string uniqueTradeId, string reportingParty, List<string> metricsArray, string reportingCurrency, string market,
            DateTime valuationDate)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            if (tradeItem != null)
            {
                var properties = tradeItem.AppProps;
                if (tradeItem.Data is Trade trade)
                {
                    var product = trade.Item;
                    var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties, true);
                    //Get the market
                    var marketEnviroment = Highlander.CurveEngine.V5r3.CurveEngine.GetMarket(logger, cache, nameSpace, product, market, reportingCurrency, false);
                    var controller = TradePricer.CreateInstrumentModelData(metricsArray, valuationDate, marketEnviroment, reportingCurrency, reportingParty);
                    pricer.Price(controller, ValuationReportType.Full);
                    logger.LogDebug("Returned a list of details for the trade: {0}", uniqueTradeId);
                    pricer.PriceableProduct.Id = uniqueTradeId;
                    List<object[]> report = pricer.DoExpectedCashflowReport(pricer.PriceableProduct);
                    return report;
                }
            }
            var result = new List<object[]> {new object[] {"The trade id used does not return a valid trade!"}};
            return result;
        }

        /// <summary>
        /// Views a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        public object[,] ViewTrade(string uniqueTradeId)
        {
            return ViewTrade(Logger, Cache, NameSpace, uniqueTradeId);
        }

        /// <summary>
        /// Views a trade that has already been created.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        public object[,] ViewTrade(ILogger logger, ICoreCache cache, String nameSpace, string uniqueTradeId)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            if (tradeItem != null)
            {
                var properties = tradeItem.AppProps;
                var trade = tradeItem.Data as Trade;
                var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties, true);
                var product = pricer.BuildTheProduct();
                logger.LogDebug("Returned a list of details for the trade: {0}", uniqueTradeId);
                var report = pricer.DoReport(product, properties);
                return report;
            }
            var result = new object[1, 1];
            result[0,0] = "The trade id uded does not return a valid trade!";
            return result;
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="sourceSystem">The source system.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMapRange">The curve mapping range.</param>
        /// <returns></returns>
        public string ValueTrade(string sourceSystem, string uniqueTradeId, string reportingParty, List<string> metricsArray, string reportingCurrency,
            DateTime valuationDate, List<Pair<string, string>> curveMapRange)
        {
            return ValueTrade(Logger, Cache, NameSpace, sourceSystem, uniqueTradeId, reportingParty, metricsArray, reportingCurrency, valuationDate, curveMapRange);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="sourceSystem">The source system.</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMapRange">The curve mapping range.</param>
        /// <returns></returns>
        public string ValueTrade(ILogger logger, ICoreCache cache, String nameSpace, String sourceSystem,
            string uniqueTradeId, string reportingParty, List<string> metricsArray, string reportingCurrency,
            DateTime valuationDate, List<Pair<string, string>> curveMapRange)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            if (tradeItem != null)
            {              
                var properties = tradeItem.AppProps;
                if (tradeItem.Data is Trade trade)
                {
                    var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties);
                    //Get the market
                    var marketEnviroment = CurveEngine.V5r3.CurveEngine.GetCurves(logger, cache, nameSpace, curveMapRange, false);
                    var controller = TradePricer.CreateInstrumentModelData(metricsArray, valuationDate, marketEnviroment, reportingCurrency, reportingParty);
                    var assetValuationReport = pricer.Price(controller, ValuationReportType.Full);
                    //Build the val report properties
                    var valProperties = properties.Clone();
                    valProperties.Set(ValueProp.PortfolioId, CurveConst.LOCAL_USER);
                    valProperties.Set(CurveProp.Market, CurveConst.LOCAL_USER);
                    valProperties.Set(CurveProp.MarketDate, valuationDate);
                    valProperties.Set(ValueProp.CalculationDateTime, valuationDate.ToUniversalTime());
                    valProperties.Set(ValueProp.BaseParty, reportingParty);
                    valProperties.Set(TradeProp.UniqueIdentifier, null);
                    valProperties.Set(EnvironmentProp.Function, FunctionProp.ValuationReport);
                    valProperties.Set(EnvironmentProp.DataGroup, null);
                    valProperties.Set(EnvironmentProp.Domain, null);
                    //The unique identifier for the valuation report
                    var valuationIdentifier = new ValuationReportIdentifier(valProperties);
                    cache.SaveObject(assetValuationReport, nameSpace + "." + valuationIdentifier.UniqueIdentifier, valProperties);
                    logger.LogDebug("Valued the trade: {0}", uniqueTradeId);
                    return valuationIdentifier.UniqueIdentifier;
                }
            }
            const string result = "The trade id used does not return a valid trade!";
                return result;
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns></returns>
        public string ValueTradeFromMarket(string uniqueTradeId, string reportingParty, List<string> metricsArray, string reportingCurrency, string market,
            DateTime valuationDate)
        {
            return ValueTradeFromMarket(Logger, Cache, NameSpace, uniqueTradeId, reportingParty, metricsArray, reportingCurrency, market, valuationDate);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns></returns>
        public string ValueTradeFromMarket(ILogger logger, ICoreCache cache, String nameSpace, 
            string uniqueTradeId, string reportingParty, List<string> metricsArray, string reportingCurrency, string market,
            DateTime valuationDate)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            if (tradeItem != null)
            { 
                var properties = tradeItem.AppProps;
                if (tradeItem.Data is Trade trade)
                {
                    var product = trade.Item;
                    var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties);
                    //Get the market
                    var marketEnviroment = Highlander.CurveEngine.V5r3.CurveEngine.GetMarket(logger, cache, nameSpace, product, market, reportingCurrency, false);
                    var controller = TradePricer.CreateInstrumentModelData(metricsArray, valuationDate, marketEnviroment, reportingCurrency, reportingParty);
                    var assetValuationReport = pricer.Price(controller, ValuationReportType.Full);
                    //var id = uniqueTradeId.Split('.')[uniqueTradeId.Split('.').Length - 1];
                    //Build the val report properties
                    var valProperties = properties.Clone();
                    valProperties.Set(ValueProp.PortfolioId, CurveConst.LOCAL_USER);
                    valProperties.Set(TradeProp.LongTradeId, uniqueTradeId);
                    valProperties.Set(ValueProp.BaseParty, reportingParty);
                    valProperties.Set(CurveProp.Market, market);
                    valProperties.Set(ValueProp.CalculationDateTime, valuationDate.ToUniversalTime());
                    valProperties.Set(TradeProp.UniqueIdentifier, null);
                    valProperties.Set(EnvironmentProp.Function, FunctionProp.ValuationReport);
                    valProperties.Set(EnvironmentProp.DataGroup, null);
                    valProperties.Set(EnvironmentProp.Domain, null);
                    valProperties.Set(ValueProp.MarketName, market);
                    //The unique identifier for the valuation report
                    var valuationIdentifier = new ValuationReportIdentifier(valProperties);
                    cache.SaveObject(assetValuationReport, nameSpace + "." + valuationIdentifier.UniqueIdentifier, valProperties);
                    logger.LogDebug("Valued and saved results for the trade: {0}", uniqueTradeId);
                    return valuationIdentifier.UniqueIdentifier;
                }
            }
            const string result = "The trade id used does not return a valid trade!";
            return result;
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="valuationPortfolioId">The valuation portfolio Id</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="markets">The markets to value against.</param>
        /// <returns></returns>
        public String ValueTradeFromMarkets(String valuationPortfolioId, string uniqueTradeId, string reportingParty, List<string> metricsArray, string reportingCurrency, List<string> markets,
            DateTime valuationDate)
        {
            return ValueTradeFromMarkets(Logger, Cache, NameSpace, valuationPortfolioId, uniqueTradeId, reportingParty, metricsArray, reportingCurrency, markets, valuationDate);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="valuationPortfolioId">The valuation portfolio Id</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="markets">The array of markets.</param>
        /// <returns></returns>
        public String ValueTradeFromMarkets(ILogger logger, ICoreCache cache, String nameSpace, String valuationPortfolioId,
            string uniqueTradeId, string reportingParty, List<string> metricsArray, string reportingCurrency, List<string> markets,
            DateTime valuationDate)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            const string result = "The trade id used does not return a valid trade!";          
            if (tradeItem != null)
            {
                var properties = tradeItem.AppProps;
                if (tradeItem.Data is Trade trade)
                {
                    var product = trade.Item;
                    //Build the val report properties
                    var valProperties = properties.Clone();
                    valProperties.Set(ValueProp.PortfolioId, valuationPortfolioId);
                    valProperties.Set(TradeProp.LongTradeId, uniqueTradeId);
                    valProperties.Set(ValueProp.BaseParty, reportingParty);
                    //valProperties.Set(TradeProp.UniqueIdentifier, null);
                    valProperties.Set(EnvironmentProp.Function, FunctionProp.ValuationReport);
                    valProperties.Set(EnvironmentProp.DataGroup, null);
                    valProperties.Set(EnvironmentProp.Domain, null);
                    //Instantiate the pricer
                    var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties);
                    //The unique identifier for the valuation report  
                    foreach (var market in markets)
                    {
                        valProperties.Set(ValueProp.CalculationDateTime, valuationDate.ToUniversalTime());
                        valProperties.Set(CurveProp.Market, null);
                        valProperties.Set(CurveProp.MarketDate, null);
                        var elements = market.Split('.');
                        if (elements.Length > 1)
                        {
                            valProperties.Set(CurveProp.Market, elements[0]);
                            valProperties.Set(CurveProp.MarketDate, elements[1]);
                        }
                        valProperties.Set(ValueProp.UniqueIdentifier, null);//This resets the previously set uniqueidentifier.
                        valProperties.Set(ValueProp.MarketName, market);
                        var valuationIdentifier = new ValuationReportIdentifier(valProperties);
                        valProperties.Set(ValueProp.UniqueIdentifier, valuationIdentifier.UniqueIdentifier);//This resets the previously set uniqueidentifier.
                        var marketEnviroment = Highlander.CurveEngine.V5r3.CurveEngine.GetMarket(logger, cache, nameSpace, product,
                                                                                 market, reportingCurrency, false);
                        var controller = TradePricer.CreateInstrumentModelData(metricsArray, valuationDate,
                                                                               marketEnviroment, reportingCurrency,
                                                                               reportingParty);
                        var assetValuationReport = pricer.Price(controller, ValuationReportType.Full);
                        cache.SaveObject(assetValuationReport, nameSpace + "." + valuationIdentifier.UniqueIdentifier, valProperties);
                        logger.LogDebug("Valued and saved results for the trade: {0}", uniqueTradeId);
                    }
                    return valuationPortfolioId;
                }
                return result;
            }
            return  result ;
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="market">The market.</param>
        /// <param name="valuationDate">The valuation date.</param>
        public object[,] ValueWithDetail(string uniqueTradeId, string reportingParty, string reportingCurrency, string market, DateTime valuationDate)
        {
            return ValueWithDetail(Logger, Cache, NameSpace, uniqueTradeId, reportingParty, reportingCurrency, market, valuationDate);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="market">The market.</param>
        /// <param name="valuationDate">The valuation date.</param>
        public object[,] ValueWithDetail(ILogger logger, ICoreCache cache, String nameSpace, string uniqueTradeId, 
            string reportingParty, string reportingCurrency, string market, DateTime valuationDate)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            if (tradeItem != null)
            {
                var properties = tradeItem.AppProps;
                if (tradeItem.Data is Trade trade)
                {
                    var product = trade.Item;
                    var tradeType = ProductTypeHelper.TradeTypeHelper(product);
                    var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties);
                    //Get the market
                    var marketEnviroment = CurveEngine.V5r3.CurveEngine.GetMarket(logger, cache, nameSpace, product, market, reportingCurrency, false);
                    //Use a pre-defined set of metrics temporarily.
                    var metrics = new List<string>
                                      {
                                          InstrumentMetrics.NPV.ToString(),
                                          InstrumentMetrics.ImpliedQuote.ToString(),
                                          InstrumentMetrics.ExpectedValue.ToString(),
                                          InstrumentMetrics.Delta0.ToString(),
                                          InstrumentMetrics.Delta1.ToString(),
                                          InstrumentMetrics.LocalCurrencyNPV.ToString(),
                                          InstrumentMetrics.LocalCurrencyExpectedValue.ToString(),
                                          InstrumentMetrics.LocalCurrencyDelta0.ToString(),
                                          InstrumentMetrics.LocalCurrencyDelta1.ToString()
                                      };
                    var controller = TradePricer.CreateInstrumentModelData(metrics.ToList(), valuationDate, marketEnviroment, reportingCurrency, reportingParty);
                    var assetValuationReport = pricer.Price(controller, ValuationReportType.Full);
                    var id = uniqueTradeId.Split('.')[uniqueTradeId.Split('.').Length - 1];
                    //Build the val report properties
                    var valProperties = properties.Clone();
                    valProperties.Set(ValueProp.PortfolioId, tradeType + "." + id);
                    valProperties.Set(ValueProp.BaseParty, reportingParty);
                    valProperties.Set(ValueProp.CalculationDateTime, valuationDate.ToUniversalTime());
                    valProperties.Set(TradeProp.UniqueIdentifier, null);
                    valProperties.Set(EnvironmentProp.Function, FunctionProp.ValuationReport);
                    valProperties.Set(EnvironmentProp.DataGroup, null);
                    valProperties.Set(EnvironmentProp.Domain, null);
                    //The unique identifier for the valuation report
                    var valuationIdentifier = new ValuationReportIdentifier(valProperties);
                    cache.SaveObject(assetValuationReport, nameSpace + "." + valuationIdentifier.UniqueIdentifier, valProperties);
                    object[,] report = pricer.DoXLReport(pricer.PriceableProduct);
                    logger.LogDebug("Valued and saved results for the trade: {0}", uniqueTradeId);
                    return report;
                }
            }
            var result = new object[1, 1];
            result[0, 0] = "The trade id uded does not return a valid trade!";
            return result;
        }

        /// <summary>
        /// Views the calculated data in the valuation report.
        /// </summary>
        /// <param name="valuationId">THe id of the report to view.</param>
        /// <returns></returns>
        public object[,] ViewValuationReport(string valuationId)
        {
            return ViewValuationReport(Logger, Cache, NameSpace, valuationId);
        }

        /// <summary>
        /// Views the calculated data in the valuation report.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="valuationId">THe id of the report to view.</param>
        /// <returns></returns>
        public object[,] ViewValuationReport(ILogger logger, ICoreCache cache, String nameSpace, string valuationId)
        {
            var id = nameSpace + "." + valuationId;
            var reportItem = cache.LoadObject<ValuationReport>(id);
            var tvi = reportItem.tradeValuationItem[0];
            var assetValuationReport = tvi.valuationSet.assetValuation[0];
            //Get the output data.
            IDictionary<string, decimal> results = new Dictionary<string, decimal>();
            foreach (var point in assetValuationReport.quote)
            {
                var riskType = point.measureType.Value;
                var risk = point.value;
                if (point.currency != null)
                {
                    riskType = riskType + '.' + point.currency.Value;
                }
                if (results.ContainsKey(riskType))
                {
                    results[riskType] = results[riskType] + risk;
                }
                else
                {
                    results.Add(riskType, risk);
                }               
                if (point.sensitivitySet == null) continue;
                foreach (var element in point.sensitivitySet[0].sensitivity)
                {
                    riskType = point.measureType.Value + '.' + element.name;
                    if (results.ContainsKey(riskType))
                    {
                        results[riskType] = results[riskType] + element.Value;
                    }
                    else
                    {
                        results.Add(riskType, element.Value);
                    }                  
                }
            }
            object[,] result = ArrayHelper.ConvertDictionaryTo2DArray(results);
            logger.LogDebug("Viewed the valuation report: {0}", valuationId);
            return result;
        }

        #endregion

        #region Get ParRate and NPV

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns></returns>
        public double GetNPV(string uniqueTradeId, string reportingParty, string reportingCurrency, DateTime valuationDate, string market)
        {
            return GetNPV(Logger, Cache, NameSpace, uniqueTradeId, reportingParty, reportingCurrency, valuationDate, market);
        }

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The String nameSpace</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns></returns>
        public double GetNPV(ILogger logger, ICoreCache cache, String nameSpace, 
            string uniqueTradeId, string reportingParty, string reportingCurrency, 
            DateTime valuationDate, string market)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            var properties = tradeItem.AppProps;
            var party1 = properties.GetValue<string>(TradeProp.Party1, true);
            //var party2 = properties.GetValue<string>(TradeProp.Party2, true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            if (tradeItem.Data is Trade trade)
            {
                var product = trade.Item;
                //Get the market
                var marketEnviroment = Highlander.CurveEngine.V5r3.CurveEngine.GetMarket(logger, cache, nameSpace, product, market, reportingCurrency, false);
                var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties);
                var npv = pricer.GetNPV(baseParty, reportingCurrency, valuationDate, marketEnviroment);
                logger.LogDebug("Returned the npv for the trade: {0}", uniqueTradeId);
                return npv;
            }
            return 0;
        }

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMapRange">The curve mapping range.</param>
        /// <returns></returns>
        public double GetNPVWithSpecifiedCurves(string uniqueTradeId, string reportingParty, string reportingCurrency,
            DateTime valuationDate, List<Pair<string, string>> curveMapRange)
        {
            return GetNPVWithSpecifiedCurves(Logger, Cache, NameSpace, uniqueTradeId, reportingParty, reportingCurrency, valuationDate, curveMapRange);
        }

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMapRange">The curve mapping range.</param>
        /// <returns></returns>
        public double GetNPVWithSpecifiedCurves(ILogger logger, ICoreCache cache, String nameSpace, 
            string uniqueTradeId, string reportingParty, string reportingCurrency,
            DateTime valuationDate, List<Pair<string, string>> curveMapRange)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            var properties = tradeItem.AppProps;
            var party1 = properties.GetValue<string>(TradeProp.Party1, true);
            //var party2 = properties.GetValue<string>(TradeProp.Party2, true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            if (tradeItem.Data is Trade trade)
            {
                //Get the market
                //var curveNames = product.GetRequiredPricingStructures();
                var marketEnviroment = Highlander.CurveEngine.V5r3.CurveEngine.GetCurves(logger, cache, nameSpace, curveMapRange, false);
                var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties);
                var npv = pricer.GetNPV(baseParty, reportingCurrency, valuationDate, marketEnviroment);
                logger.LogDebug("Returned the npv using provided curve map for the trade: {0}", uniqueTradeId);
                return npv;
            }
            return 0;
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns>The par rate for the trade using the cloud curves.</returns>
        public double GetParRate(string uniqueTradeId, string reportingParty, string reportingCurrency, DateTime valuationDate, string market)
        {
            return GetParRate(Logger, Cache, NameSpace, uniqueTradeId, reportingParty, reportingCurrency, valuationDate, market);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns>The par rate for the trade using the cloud curves.</returns>
        public double GetParRate(ILogger logger, ICoreCache cache, String nameSpace,
            string uniqueTradeId, string reportingParty, string reportingCurrency, 
            DateTime valuationDate, string market)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            var properties = tradeItem.AppProps;
            var party1 = properties.GetValue<string>(TradeProp.Party1, true);
            //var party2 = properties.GetValue<string>(TradeProp.Party2, true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            if (tradeItem.Data is Trade trade)
            {
                var product = trade.Item;
                //Get the market
                var marketEnviroment = Highlander.CurveEngine.V5r3.CurveEngine.GetMarket(logger, cache, nameSpace, product, market, reportingCurrency, false);
                var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties);
                var parRate = pricer.GetParRate(baseParty, valuationDate, marketEnviroment);
                logger.LogDebug("Returned the par rate for the trade: {0}", uniqueTradeId);
                return parRate;
            }
            return 0;
        }

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMapRange">The curve mapping range.</param>
        /// <returns></returns>
        public double GetParRateWithSpecifiedCurves(string uniqueTradeId, string reportingParty, DateTime valuationDate, List<Pair<string, string>> curveMapRange)
        {
            return GetParRateWithSpecifiedCurves(Logger, Cache, NameSpace, uniqueTradeId, reportingParty, valuationDate, curveMapRange);
        }

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMapRange">The curve mapping range.</param>
        /// <returns></returns>
        public double GetParRateWithSpecifiedCurves(ILogger logger, ICoreCache cache, String nameSpace,
            string uniqueTradeId, string reportingParty, DateTime valuationDate, List<Pair<string, string>> curveMapRange)
        {
            var tradeItem = cache.LoadItem<Trade>(nameSpace + "." + uniqueTradeId);
            var properties = tradeItem.AppProps;
            var party1 = properties.GetValue<string>(TradeProp.Party1, true);
            //var party2 = properties.GetValue<string>(TradeProp.Party2, true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            var trade = tradeItem.Data as Trade;
            var marketEnviroment = Highlander.CurveEngine.V5r3.CurveEngine.GetCurves(logger, cache, nameSpace, curveMapRange, false);
            var pricer = new TradePricer(logger, cache, nameSpace, null, trade, properties);
            var parRate = pricer.GetParRate(baseParty, valuationDate, marketEnviroment);
            logger.LogDebug("Returned the par rate using provided curvbe map for the trade: {0}", uniqueTradeId);
            return parRate;
        }

        #endregion

        #region Create Store Reference Data

        #region Exchange Data

        /// <summary>
        /// Loads exchange config to the Highlander.Core.
        /// </summary>
        /// <param name="exchangeData">The exchange data to load.
        /// This must be consistent with the Exchange class</param>
        /// <param name="exchangeSettlementData">THe setttlement data attached to each exchange. 
        /// There should be at least 4 columns: period; daytype; businessdayconvention; businessCentres. RelativeTo is optional</param>
        /// <returns></returns>
        public string LoadExchangeConfigData(string[,] exchangeData, string[,] exchangeSettlementData)
        {
            if (exchangeData.GetUpperBound(0) != exchangeSettlementData.GetUpperBound(0))
            return "Number of rows not equal";
            var id = NameSpace + "." + FunctionProp.Configuration + ".Exchanges.";
            for (int i = 0; i < exchangeData.GetLength(0); i++ )
            {
                var itemProps = new NamedValueSet();
                itemProps.Set(EnvironmentProp.DataGroup, "Highlander.V5r3.Reporting.Configuration.Exchange");
                itemProps.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
                itemProps.Set(EnvironmentProp.Function, FunctionProp.Configuration.ToString());
                itemProps.Set(EnvironmentProp.Type, "Exchange");
                itemProps.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                itemProps.Set(EnvironmentProp.NameSpace, NameSpace);
                var newid = id + exchangeData[i, 0];
                var exchange = ExchangeHelper.Create(exchangeData[i, 0], 
                    exchangeData[i, 1], exchangeData[i, 2], exchangeData[i, 3], exchangeData[i, 4], exchangeData[i, 5], exchangeData[i, 6],
                    exchangeData[i, 7], exchangeData[i, 8], exchangeData[i, 9], exchangeData[i, 10], exchangeData[i, 11], exchangeData[i, 12]);
                var dayType = EnumHelper.Parse<DayTypeEnum>(exchangeSettlementData[i, 1]);
                var settlement = RelativeDateOffsetHelper.Create(exchangeSettlementData[i, 0], dayType, exchangeSettlementData[i, 2], exchangeSettlementData[i, 3], null);
                var item = new ExchangeConfigData {ExchangeData = exchange, SettlementDate = settlement};
                Cache.SaveObject(item, newid, itemProps);
                Logger.LogInfo("Exchange data loaded: " + newid);
            }
            return "Exchange data loaded!";
        }

        /// <summary>
        /// Views exchange config to the Highlander.Core.
        /// </summary>
        /// <param name="exchangeId">The exchange data to load.
        /// This must be consistent with the Exchange class</param>
        /// <returns></returns>
        public string[,] ViewExchangeConfigData(string exchangeId)
        {
            var id = NameSpace + "." + FunctionProp.Configuration + ".Exchanges." + exchangeId;
            var item = Cache.LoadItem<Exchange>(id);
            if (item != null)
            {
                if (item.Data is ExchangeConfigData config)
                {
                    var data = config.ExchangeData;
                    if (data != null)
                    {
                        var result = new string[13, 2];
                        result[0, 0] = "Id";
                        result[0, 1] = data.Id;
                        result[1, 0] = "MIC";
                        result[1, 1] = data.MIC;
                        result[2, 0] = "Name";
                        result[2, 1] = data.Name;
                        result[3, 0] = "OperatingMIC";
                        result[3, 1] = data.OperatingMIC;
                        result[4, 0] = "OS";
                        result[4, 1] = data.OS;
                        result[5, 0] = "ISOCountryCode";
                        result[5, 1] = data.ISOCountryCode;
                        result[6, 0] = "Country";
                        result[6, 1] = data.Country;
                        result[7, 0] = "City";
                        result[7, 1] = data.City;
                        result[8, 0] = "Acronym";
                        result[8, 1] = data.Acronym;
                        result[9, 0] = "CreationDate";
                        result[9, 1] = data.CreationDate;
                        result[10, 0] = "Status";
                        result[10, 1] = data.Status;
                        result[11, 0] = "StatusDate";
                        result[11, 1] = data.StatusDate;
                        result[12, 0] = "Website";
                        result[12, 1] = data.Website;
                        return result;
                    }
                }
            }
            return new string[,] {{"Could not find this particular exchange."}};
        }

        /// <summary>
        /// Saves exchange config to a file.
        /// </summary>
        /// <param name="identifiers">The exchange data to save.
        /// This must be consistent with the Exchange class</param>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public string SaveExchangeConfigDataToFiles(IEnumerable<string> identifiers, string directoryPath)
        {
            var result = "Exchange data are not saved!";
            var baseid = NameSpace + "." + FunctionProp.Configuration + ".Exchanges.";
            foreach (var id in identifiers)
            {
                var newid = baseid + id;
                var item = Cache.LoadItem<Exchange>(newid);
                // save item
                string baseFilename = Path.GetFullPath(directoryPath);
                baseFilename += id;
                string xmlFilename = baseFilename + ".xml";
                string nvsFilename = baseFilename + ".nvs";
                using (var sr = new StreamWriter(xmlFilename))
                {
                    sr.Write(item.Text);
                }
                using (var sr = new StreamWriter(nvsFilename))
                {
                    sr.Write(item.AppProps.Serialise());
                }
                result = "Exchange data is saved!";
            }
            return result;
        }

        #endregion

        #region Quoted Asset Sets

        /// <summary>
        /// Gets the quotedassetset identifiers conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetQasUniqueIdentifiers(NamedValueSet requestProperties)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException("requestProperties");
            //The new filter with OR on arrays..
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            // Load items
            var items = Cache.LoadItems<QuotedAssetSet>(queryExpr);
            if (items != null)
            {
                var result = items.Select(item => item.Name).ToList();
                return result;
            }
            return new List<string> { "No QAS found!" };
        }

        /// <summary>
        /// Publishes the QuotedAssetSet.
        /// A quotedassetset is used for storing market data, to be consumed by a curve.
        /// The QAS is retrieved from the local cache and published into the cloud for consumption.
        /// A set of properties is associated with the cached QAS and these properties 
        /// are also associated with the published QAS.
        /// <example>
        /// <para>BuildDateTime	        15/03/2010</para>
        /// <para>PricingStructureType	DiscountCurve</para>
        /// <para>UniqueIdentifier	    Highlander.MarketData.QR_LIVE.DiscountCurve.AUD-LIBOR-SENIOR</para>
        /// <para>MarketName	        QR_LIVE</para>
        /// <para>SourceSystem	        Highlander</para>
        /// <para>DataGroup	            Highlander.MarketData</para>
        /// <para>Function	            MarketData</para>
        /// <para>CurveName	            AUD-LIBOR-SENIOR</para>
        /// <para>TimeToLive	        720</para>
        /// </example>
        /// </summary>
        /// <param name="properties">The properties: 
        /// <para>BuildDateTime,</para> 
        /// <para>PricingStructureType,</para> 
        /// <para>UniqueIdentifier,</para> 
        /// <para>MarketName,</para>
        /// <para>SourceSystem, </para>
        /// <para>DataGroup,</para> 
        /// <para>Function, </para>
        /// <para>CurveName, </para>
        /// <para>TimeToLive</para>
        /// </param>
        /// <remarks>
        /// <see cref="AssetTypesEnum"></see>
        /// </remarks>
        /// <param name="assetIdentifiers">The identifiers. 
        /// These must be valid asset identifiers for the pricingstructuretype to be bootstrapped.
        /// </param>
        /// <param name="values">The market quotes</param>
        /// <param name="additonal">The additional data. This will be volatilities, in the case of IRFutures, or spreads for all other assets.</param>
        /// <returns></returns>
        public string CreateQuotedAssetSet(NamedValueSet properties, string[] assetIdentifiers, decimal[] values, decimal[] additonal)
        {
            var result = "Not published. The properties must contain a UniqueIdentifier";
            var qas = AssetHelper.Parse(assetIdentifiers, values, additonal);
            var uniqueId = properties.Get(CurveProp.UniqueIdentifier).ValueString;
            properties.Set(EnvironmentProp.NameSpace, NameSpace);
            if (uniqueId != null)
            {
                Cache.SaveObject(qas, NameSpace + "." + uniqueId, properties);
                result = uniqueId + "- This QuotedAssetSet has been published successfully.";
                return result;
            }
            return result;
        }

        /// <summary>
        /// Publishes the QuotedAssetSet.
        /// A quotedassetset is used for storing market data, to be consumed by a curve.
        /// The QAS is retrieved from the local cache and published into the cloud for consumption.
        /// A set of properties is associated with the cached QAS and these properties 
        /// are also associated with the published QAS.
        /// <example>
        /// <para>BuildDateTime	        15/03/2010</para>
        /// <para>PricingStructureType	DiscountCurve</para>
        /// <para>UniqueIdentifier	    Highlander.MarketData.QR_LIVE.DiscountCurve.AUD-LIBOR-SENIOR</para>
        /// <para>MarketName	        QR_LIVE</para>
        /// <para>SourceSystem	        Highlander</para>
        /// <para>DataGroup	            Highlander.MarketData</para>
        /// <para>Function	            MarketData</para>
        /// <para>CurveName	            AUD-LIBOR-SENIOR</para>
        /// <para>TimeToLive	        720</para>
        /// </example>
        /// </summary>
        /// <remarks>
        /// <see cref="AssetTypesEnum">The types enum contains all valid AssetTypes. The list can be obtained by calling:
        /// <c>SupportedAssets()</c></see>
        /// </remarks>
        /// <param name="properties">The properties: 
        /// <para>BuildDateTime,</para> 
        /// <para>PricingStructureType,</para> 
        /// <para>UniqueIdentifier,</para> 
        /// <para>MarketName,</para>
        /// <para>SourceSystem, </para>
        /// <para>DataGroup,</para> 
        /// <para>Function, </para>
        /// <para>CurveName, </para>
        /// <para>TimeToLive</para>
        /// </param>
        /// <param name="assetIdentifiers">The identifiers. 
        /// These must be valid asset identifiers for the pricingstructuretype to be bootstrapped.
        /// </param>
        /// <param name="values">The market quotes. These will be rates when creating a RateCurve.</param>
        /// <param name="measureTypes">The measure type of the value. This would normally be MarketQuote.
        /// However, for a IRFuture a LogNormalVolatility may be required.</param>
        /// <param name="priceQuoteUnits">The price quote units. the most common example of this is DecimalRate.</param>
        /// <returns></returns>
        public string CreateQuotedAssetSetWithUnits(NamedValueSet properties, string[] assetIdentifiers,
            decimal[] values, string[] measureTypes, string[] priceQuoteUnits)
        {
            var result = "Not published. The properties must contain a UniqueIdentifier";
            var qas = AssetHelper.Parse(assetIdentifiers, values, measureTypes, priceQuoteUnits);
            properties.Set(EnvironmentProp.NameSpace, NameSpace);
            var uniqueId = properties.Get(CurveProp.UniqueIdentifier).ValueString;
            if (uniqueId != null)
            {
                Cache.SaveObject(qas, NameSpace + "." + uniqueId, properties);
                result = uniqueId + "- This QuotedAssetSet has been published successfully.";
                return result;
            }
            return result;
        }

        #endregion

        #region Market Properties

        /// <summary>
        /// Publishes the pricing structure configuration.
        /// A set of properties is associated with the cached market and these properties 
        /// are also associated with the published pricing structure.
        /// </summary>
        /// <param name="assetIdentifiers">The asset Identifiers.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="values">The values.</param>
        /// <param name="assetMeasures">The measures for each asset.</param>
        /// <param name="priceQuoteUnits">The quots for each measure.</param>
        /// <param name="includeMarketQuoteValues">A flag to be able to set the values to null.</param>
        /// <returns>This identifier is the key to use when requesting the cached pricing structure.</returns>
        public string CreatePricingStructureProperties(NamedValueSet properties, string[] assetIdentifiers,
            decimal[] values, string[] assetMeasures, string[] priceQuoteUnits, bool includeMarketQuoteValues)
        {
            var curveName = properties.GetValue<string>("CurveName", true);
            var pst = properties.GetValue<string>("PricingStructureType", true);
            properties.Set(EnvironmentProp.NameSpace, NameSpace);
            Market configuration;
            if (pst == "FxCurve")
            {
                var currency = properties.GetValue<string>("Currency", true);
                var currency2 = properties.GetValue<string>("Currency2", true);
                var quoteBasis = properties.GetValue<string>("QuoteBasis", true);
                var qas = AssetHelper.ParseToFxRateSet(assetIdentifiers, values, assetMeasures, priceQuoteUnits, includeMarketQuoteValues);
                configuration = PricingStructureConfigHelper.CreateFxCurveConfiguration(curveName, currency, currency2, quoteBasis, qas);
            }
            else
            {
                var qas = AssetHelper.Parse(assetIdentifiers, values, assetMeasures, priceQuoteUnits, includeMarketQuoteValues);
                configuration = PricingStructureConfigHelper.CreateYieldCurveConfiguration(curveName, qas);
            }
            var uniqueId = properties.GetValue<string>("UniqueIdentifier", false);
            if (uniqueId == null)
            {
                var market = properties.GetValue<string>(CurveProp.Market, true);
                uniqueId = "Configuration.PricingStructures." + market + "." + pst + "." + curveName;
            }
            Cache.SaveObject(configuration, NameSpace + "." + uniqueId, properties);
            return uniqueId;
        }

        #endregion

        #region Fixed Income

        /// <summary>
        /// Retrieves and saves bonds to a directory.
        /// </summary>
        /// <param name="identifiers">The identifier array to retrieve.</param>
        /// <param name="directoryPath">The path to save the files into.</param>
        /// <param name="isShortName">If <true>isShortName</true> is true> then the id is of the form: FixedIncome.XXXX.YY.zz-zz-zzzz.
        /// Otherwise it is of the form ReferenceData.FixedIncome.XXXX.YY.zz-zz-zzzz.</param>
        /// <returns></returns>
        public string SaveQas(IEnumerable<string> identifiers, string directoryPath, bool isShortName)
        {
            var result = "QAS are not saved!";
            foreach (var id in identifiers)
            {
                var newid = id;
                if (isShortName)
                {
                    newid = NameSpace + "." + id;
                }
                var item = Cache.LoadItem<Bond>(newid);
                // save item
                string baseFilename = Path.GetFullPath(directoryPath);
                baseFilename += id;
                string xmlFilename = baseFilename + ".xml";
                string nvsFilename = baseFilename + ".nvs";
                using (var sr = new StreamWriter(xmlFilename))
                {
                    sr.Write(item.Text);
                }
                using (var sr = new StreamWriter(nvsFilename))
                {
                    sr.Write(item.AppProps.Serialise());
                }
                result = "QAS are saved!";
            }
            return result;
        }

        /// <summary>
        /// Gets the bond identifiers conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetFixedIncomeUniqueIdentifiers(NamedValueSet requestProperties)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException("requestProperties");
            //The new filter with OR on arrays..
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            // Load items
            var items = Cache.LoadItems<Bond>(queryExpr);
            if (items != null)
            {
                var result = items.Select(item => item.Name).ToList();
                return result;
            }
            return new List<string> {"No Bonds found!"};
        }

        /// <summary>
        /// Gets the bonds conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetFixedIncomeDescription(NamedValueSet requestProperties)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException("requestProperties");
            //The new filter with OR on arrays..
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            // Load items
            var items = Cache.LoadItems<Bond>(queryExpr);
            if (items != null)
                return items.Select(element => element.Data).OfType<Bond>().Select(bond => bond.description).ToList();
            return null;
        }

        /// <summary>
        /// Gets the bond ISINs conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetFixedIncomeISINs(NamedValueSet requestProperties)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException("requestProperties");
            //The new filter with OR on arrays..
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            // Load items
            var items = Cache.LoadItems<Bond>(queryExpr);
            if (items != null)
                return items.Select(element => element.AppProps.GetValue<string>("ISIN")).Where(isin => isin != null).ToList();
            return null;
        }

        /// <summary>
        /// Retrieves and saves bonds to a directory.
        /// </summary>
        /// <param name="identifiers">The identifier array to retrieve.</param>
        /// <param name="directoryPath">The path to save the files into.</param>
        /// <param name="isShortName">If <true>isShortName</true> is true> then the id is of the form: FixedIncome.XXXX.YY.zz-zz-zzzz.
        /// Otherwise it is of the form ReferenceData.FixedIncome.XXXX.YY.zz-zz-zzzz.</param>
        /// <returns></returns>
        public string SaveBonds(IEnumerable<string> identifiers, string directoryPath, bool isShortName)
        {
            var result = "Bonds are not saved!";
            foreach (var id in identifiers)
            {
                var newid = id;
                if (isShortName)
                {
                    newid = NameSpace + "." + FunctionProp.ReferenceData + ".FixedIncome." + id;
                }
                var item = Cache.LoadItem<Bond>(newid);
                // save item
                string baseFilename = Path.GetFullPath(directoryPath);
                baseFilename = baseFilename + "FixedIncome." + id.Replace('/', '-');
                string xmlFilename = baseFilename + ".xml";
                string nvsFilename = baseFilename + ".nvs";
                using (var sr = new StreamWriter(xmlFilename))
                {
                    sr.Write(item.Text);
                }
                using (var sr = new StreamWriter(nvsFilename))
                {
                    sr.Write(item.AppProps.Serialise());
                }
                result = "Bonds are saved!";
            }
            return result;
        }

        /// <summary>
        /// Creates a bond in the data store.
        /// </summary>
        /// <param name="instrumentId">The instrument identfier.</param>
        /// <param name="clearanceSystem">A valid clearance system.</param>
        /// <param name="couponType">A valid coupon type: Fixed, Float or Struct as defined by the CouponTypeEnum.</param>
        /// <param name="couponRate">The coupon rate. If the bond is a floater, then the rate is the spread.</param>
        /// <param name="currency">A valid currency.</param>
        /// <param name="faceAmount">THe face amount. Normally 100.</param>
        /// <param name="maturityDate">THe maturitydate. Formatted if possible as MM/DD/YY</param>
        /// <param name="paymentFrequency">A valid payment frequency.</param>
        /// <param name="dayCountFraction">Defined by the DayCountFractionEnum types.</param>
        /// <param name="creditSeniority">The credit senirotiy. Of the form: CreditSeniorityEnum</param>
        /// <param name="description">The issuer identifier. e.g. BHP 5.3 12/23/18</param>
        /// <param name="exchangeId">The exchange identifier. These are sotroed as reference data.</param>
        /// <param name="issuerName">THe issuer name, but as a ticker.</param>
        /// <param name="properties">THe properties. This includes:
        /// BondTypeEnum
        /// MarketSectorEnum
        /// </param>
        /// <returns></returns>
        public string CreateBond(string instrumentId, string clearanceSystem, string couponType,
            decimal? couponRate, string currency, double faceAmount, DateTime maturityDate,
            string paymentFrequency, string dayCountFraction, string creditSeniority, 
            string description, string exchangeId, string issuerName, NamedValueSet properties)
        {
            return CreateBond(Logger, Cache, NameSpace, instrumentId, clearanceSystem, couponType, couponRate, currency, faceAmount,
                maturityDate, paymentFrequency, dayCountFraction, creditSeniority, description, exchangeId, issuerName, properties);
        }

        /// <summary>
        /// Creates a bond in the data store.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">A cache, which will hold the bond data.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="instrumentId">The instrument identfier.</param>
        /// <param name="clearanceSystem">A valid clearance system.</param>
        /// <param name="couponType">A valid coupon type: Fixed, Float or Struct as defined by the CouponTypeEnum.</param>
        /// <param name="couponRate">The coupon rate. If the bond is a floater, then the rate is the spread.</param>
        /// <param name="currency">A valid currency.</param>
        /// <param name="faceAmount">The face amount. Normally 100.</param>
        /// <param name="maturityDate">THe maturitydate. Formatted if possible as MM/DD/YY</param>
        /// <param name="paymentFrequency">A valid payment frequency.</param>
        /// <param name="dayCountFraction">Defined by the DayCountFractionEnum types.</param>
        /// <param name="creditSeniority">The credit senirotiy. Of the form: CreditSeniorityEnum</param>
        /// <param name="description">The issuer e.g. BHP 5.3 12/23/18.</param>
        /// <param name="exchangeId">The exchange identifier. These are sotroed as reference data.</param>
        /// <param name="issuerName">The issuer name, but as a ticker.</param>
        /// <param name="properties">The properties. This includes:
        /// BondTypeEnum
        /// MarketSectorEnum
        /// </param>
        /// <returns></returns>
        public string CreateBond(ILogger logger, ICoreCache cache, string nameSpace, string instrumentId, string clearanceSystem, string couponType,
            decimal? couponRate, string currency, double faceAmount, DateTime maturityDate, string paymentFrequency, string dayCountFraction, 
            string creditSeniority, string description, string exchangeId, string issuerName, NamedValueSet properties)
        {
            if (properties != null)
            {
                properties.Set(BondProp.Currency, currency);
                properties.Set(BondProp.AsAtDate, DateTime.Today);
                //properties.Set(BondProp.CreditSeniority, creditSeniority);
                properties.Set(BondProp.Function, FunctionProp.ReferenceData.ToString());
                properties.Set(BondProp.DataGroup, NameSpace + "." + FunctionProp.ReferenceData.ToString());
                properties.Set(BondProp.Domain, FunctionProp.ReferenceData.ToString() + ".FixedIncome");
                properties.Set(EnvironmentProp.SourceSystem, "SpreadSheet");
                properties.Set(TradeProp.TradeSource, "SpreadSheet");
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                properties.Set(EnvironmentProp.NameSpace, nameSpace);              
                properties.Set(BondProp.Description, description);
                var marketSector = properties.GetValue<string>("MarketSector", true);
                var ticker = properties.GetValue<string>("Ticker", true);
                var coupon = couponRate.ToString();//properties.GetValue<string>("Coupon", true);
                var identifier = new FixedIncomeIdentifier(ticker, coupon, marketSector, maturityDate, couponType);
                var bond = BondHelper.Create(new[] {instrumentId}, clearanceSystem, couponType, couponRate/100, currency,
                                             faceAmount, maturityDate, paymentFrequency, dayCountFraction, creditSeniority, 
                                             identifier.Id, description, exchangeId, issuerName);                
                properties.Set(BondProp.UniqueIdentifier, identifier.UniqueIdentifier);
                cache.SaveObject(bond, nameSpace + "." + identifier.UniqueIdentifier, properties);
                logger.LogDebug("Created new bond: {0}", identifier.UniqueIdentifier);
                return identifier.Id;
            }
            return "Insufficent information provided to create the bond.";
        }

        /// <summary>
        /// The securities market data that is updated real-time and is distributed. 
        /// This will become redundant once I work out how to use the market data service and eventuallt bond curves will be a type of discount curve.
        /// </summary>
        /// <param name="properties">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdList">A list of instrumentIds to update of the form Corp.Ticker.CouponType.Coupon.Maturity e.g. Corp.ANZ.Fixed.5,25.01-16-14</param>
        /// <param name="marketdataQuoteList">The quotes can be: DP(DirtyPrice), CP(CleanPrice), ASW (AssetSwapSpread),	DS (DiscountSPread), YTM (YieldToMaturity).</param>
        /// <param name="quotesList">The actual quote data consistent with the quote list.</param> 
        /// <returns></returns>
        public string UpdateSecuritiesMarkets(NamedValueSet properties,
                                              List<string> instrumentIdList, List<string> marketdataQuoteList,
                                              Double[,] quotesList)
        {
            return UpdateSecuritiesMarkets(Logger, Cache, NameSpace, properties, instrumentIdList, marketdataQuoteList,
                                           quotesList);
        }

        /// <summary>
        /// The securities market data that is updated real-time and is distributed. 
        /// This will become redundant once I work out how to use the market data service and eventuallt bond curves will be a type of discount curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The namespace.</param>
        /// <param name="properties">The market properties. This should inclue: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdList">A list of instrumentIds to update of the form Corp.Ticker.CouponType.Coupon.Maturity e.g. Corp.ANZ.Fixed.5,25.01-16-14</param>
        /// <param name="marketdataQuoteList">The quotes can be: DP(DirtyPrice), CP(CleanPrice), ASW (AssetSwapSpread),	DS (DiscountSPread), YTM (YieldToMaturity).</param>
        /// <param name="quotesMatrix">The actual quote data consistent with the quote list.</param> 
        /// <returns></returns>
        public string UpdateSecuritiesMarkets(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties,
            List<string> instrumentIdList, List<string> marketdataQuoteList, Double[,] quotesMatrix)
        {
            //Enrich the properties
            properties.Set(BondProp.Function, FunctionProp.Market.ToString());
            properties.Set(BondProp.DataGroup, NameSpace + "." + FunctionProp.Market.ToString());
            properties.Set(BondProp.Domain, FunctionProp.Market.ToString() + ".FixedIncome");
            properties.Set(EnvironmentProp.SourceSystem, "SpreadSheet");
            properties.Set(TradeProp.TradeSource, "SpreadSheet");
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            //Create the market
            var market = new Market {id = "SecuritiesQuotes"};
            //Populated the asset quotes
            var bavs = new List<BasicAssetValuation>();
            var index = 0;
            foreach (var instrument in instrumentIdList)
            {
                if (marketdataQuoteList.Count == quotesMatrix.GetLength(1) && instrumentIdList.Count == quotesMatrix.GetLength(0))
                {
                    var index2 = 0;
                    var bav = new BasicAssetValuation {id = instrument};
                    var bqs = new List<BasicQuotation>();
                    foreach (var quote in marketdataQuoteList)
                    {
                        var bq = new BasicQuotation {id = quote, value = Convert.ToDecimal(quotesMatrix[index, index2]), valueSpecified = true};
                        bqs.Add(bq);
                        index2++;
                    }
                    bav.quote = bqs.ToArray();
                    bavs.Add(bav);
                }
                index++;
            }
            var benchmarkQuotes = new QuotedAssetSet {assetQuote = bavs.ToArray()};
            //benchmarkQuotes.instrumentSet = new InstrumentSet();
            market.benchmarkQuotes = benchmarkQuotes;
            var marketEnvironment = properties.GetValue<string>("Market", true);
            var marketDate = properties.GetValue<string>("MarketDate", false);//TODO Currently this is ignored but will be eneeded for historical markets.
            if (marketDate != null)
            {
                //marketEnvironment = marketEnvironment + "." + marketDate.ToString("DD-MMM-YYYY");
            }
            var identifier = nameSpace + ".Market." + marketEnvironment + ".FixedIncome";
            cache.SaveObject(market, identifier, properties);
            logger.LogDebug("Created new bond market: {0}", identifier);
            return identifier + ": has been updated!";
        }

        #endregion

        #region Equities

        /// <summary>
        /// Creates a equity in the data store.
        /// </summary>
        /// <param name="instrumentId">The instrument identfier.</param>
        /// <param name="clearanceSystem">A valid clearance system.</param>
        /// <param name="currency">A valid currency.</param>
        /// <param name="assetId">The asset identifier. e.g. BHP 5.3 12/23/18</param>
        /// <param name="description">THe issuer description.</param>
        /// <param name="exchangeId">The exchange identifier. These are sotroed as reference data.</param>
        /// <param name="issuerName">THe issuer name, but as a ticker.</param>
        /// <param name="properties">THe properties. This includes:
        /// BondTypeEnum
        /// MarketSectorEnum
        /// </param>
        /// <returns></returns>
        public string CreateEquity(string instrumentId, string clearanceSystem, string currency, string assetId,
            string description, string exchangeId, string issuerName, NamedValueSet properties)
        {
            return CreateEquity(Logger, Cache, NameSpace, instrumentId, clearanceSystem, currency,
                assetId, description, exchangeId, issuerName, properties);
        }

        /// <summary>
        /// Creates an equity in the data store.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">A cache, which will hold the bond data.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="instrumentId">The instrument identfier.</param>
        /// <param name="clearanceSystem">A valid clearance system.</param>
        /// <param name="currency">A valid currency.</param>
        /// <param name="assetId">The asset identifier. e.g. BHP 5.3 12/23/18</param>
        /// <param name="description">The issuer description.</param>
        /// <param name="exchangeId">The exchange identifier. These are sotroed as reference data.</param>
        /// <param name="issuerName">The issuer name, but as a ticker.</param>
        /// <param name="properties">The properties. This includes:
        /// BondTypeEnum
        /// MarketSectorEnum
        /// </param>
        /// <returns></returns>
        public string CreateEquity(ILogger logger, ICoreCache cache, string nameSpace, string instrumentId, string clearanceSystem,
            string currency, string assetId, string description, string exchangeId, string issuerName, NamedValueSet properties)
        {
            if (properties != null)
            {
                properties.Set(EquityProp.Currency, currency);
                properties.Set(EquityProp.AsAtDate, DateTime.Today);
                //properties.Set(BondProp.CreditSeniority, creditSeniority);
                properties.Set(EquityProp.Function, FunctionProp.ReferenceData.ToString());
                properties.Set(EquityProp.DataGroup, NameSpace + "." + FunctionProp.ReferenceData.ToString());
                properties.Set(EquityProp.Domain, FunctionProp.ReferenceData.ToString() + ".Equity");
                properties.Set(EquityProp.SourceSystem, "Highlander");
                properties.Set(EnvironmentProp.NameSpace, nameSpace);
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                properties.Set(EquityProp.Description, description);
                var equity = EquityAssetHelper.Create(new[] { instrumentId }, clearanceSystem, currency,
                                             assetId, description, exchangeId, issuerName);
                var ticker = properties.GetValue<string>("Ticker", true);
                var pricing = properties.GetValue<string>("PricingSource", true);
                var identifier = new EquityIdentifier(ticker, pricing);
                properties.Set(EquityProp.UniqueIdentifier, identifier.UniqueIdentifier);
                cache.SaveObject(equity, nameSpace + "." + identifier.UniqueIdentifier, properties);
                logger.LogDebug("Created new equity: {0}", identifier.UniqueIdentifier);
                return identifier.Id;
            }
            return "Insufficent information provided to create the equity.";
        }

        /// <summary>
        /// Gets the equity identifiers conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetEquityUniqueIdentifiers(NamedValueSet requestProperties)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException("requestProperties");
            //The new filter with OR on arrays..
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            // Load items
            var items = Cache.LoadItems<EquityAsset>(queryExpr);
            if (items != null)
            {
                var result = items.Select(item => item.Name).ToList();
                return result;
            }
            return new List<string> { "No Equities found!" };
        }

        /// <summary>
        /// Gets the equity conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetEquityDescription(NamedValueSet requestProperties)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException("requestProperties");
            //The new filter with OR on arrays..
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            // Load items
            var items = Cache.LoadItems<EquityAsset>(queryExpr);
            if (items != null)
                return items.Select(element => element.Data).OfType<EquityAsset>().Select(equity => equity.description).ToList();
            return null;
        }

        /// <summary>
        /// Gets the equity ISINs conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetEquityISINs(NamedValueSet requestProperties)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException("requestProperties");
            //The new filter with OR on arrays..
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            // Load items
            var items = Cache.LoadItems<EquityAsset>(queryExpr);
            if (items != null)
                return items.Select(element => element.AppProps.GetValue<string>("ISIN")).Where(isin => isin != null).ToList();
            return null;
        }

        /// <summary>
        /// Retrieves and saves equities to a directory.
        /// </summary>
        /// <param name="identifiers">The identifier array to retrieve.</param>
        /// <param name="directoryPath">The path to save the files into.</param>
        /// <param name="isShortName">If <true>isShortName</true> is true> then the id is of the form: FixedIncome.XXXX.YY.zz-zz-zzzz.
        /// Otherwise it is of the form ReferenceData.Equity.XXXX.YY.zz-zz-zzzz.</param>
        /// <returns></returns>
        public string SaveEquities(IEnumerable<string> identifiers, string directoryPath, bool isShortName)
        {
            var result = "Equities are not saved!";
            foreach (var id in identifiers)
            {
                var newid = id;
                if (isShortName)
                {
                    newid = NameSpace + "." + FunctionProp.ReferenceData + ".Equity." + id;
                }
                var item = Cache.LoadItem<Bond>(newid);
                // save item
                string baseFilename = Path.GetFullPath(directoryPath);
                baseFilename = baseFilename + "Equity." + id.Replace('/', '-');
                string xmlFilename = baseFilename + ".xml";
                string nvsFilename = baseFilename + ".nvs";
                using (var sr = new StreamWriter(xmlFilename))
                {
                    sr.Write(item.Text);
                }
                using (var sr = new StreamWriter(nvsFilename))
                {
                    sr.Write(item.AppProps.Serialise());
                }
                result = "Equities are saved!";
            }
            return result;
        }

        #endregion

        #region Property Data

        /// <summary>
        /// Creates a property in the data store.
        /// </summary>
        /// <param name="propertyId">The property identfier.</param>
        /// <param name="shortName">A short name for the property.</param>
        /// <param name="streetIdentifier">A street Identifier.</param>
        /// <param name="streetName">A street Name.</param>
        /// <param name="postalCode">The postal code. This could be a number or a string.</param>
        /// <param name="suburb">The suburb</param>
        /// <param name="propertyType">THe property type: residential, commercial, investment etc</param>
        /// <param name="numParking">The number of car parking spots.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="description">The issuer description.</param>
        /// <param name="properties">An array of property values. All strings. </param>
        /// <param name="city">The city</param>
        /// <param name="state">The state</param>
        /// <param name="country">The country</param>
        /// <param name="numBedrooms">The number of bedrooms.</param>
        /// <param name="numBathrooms">The number of bathrooms</param>
        /// <returns></returns>
        public string CreatePropertyAsset(string propertyId, PropertyType propertyType, string shortName, string streetIdentifier, string streetName,
            string suburb, string city, string postalCode, string state, string country, string numBedrooms, string numBathrooms, string numParking,
            string currency, string description, NamedValueSet properties)
        {
            return CreatePropertyAsset(Logger, Cache, NameSpace, propertyId, propertyType, shortName, streetIdentifier, streetName, suburb,
                city, postalCode, state, country, numBedrooms, numBathrooms, numParking, currency, description, properties); 
        }

        /// <summary>
        /// Creates a property in the data store.
        /// </summary>
        /// <param name="cache">THe cache where the data is cached.</param>
        /// <param name="nameSpace">THe namespace.</param>
        /// <param name="propertyAssetIdentifier">The property asset identifier.</param>
        /// <param name="streetIdentifier">A street Identifier.</param>
        /// <param name="streetName">A street Name.</param>
        /// <param name="suburb">The suburb</param>
        /// <param name="propertyType">THe property type: residential, commercial, investment etc</param>
        /// <param name="numParking">The number of car parking spots.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="shortName">The short name alias.</param>
        /// <param name="properties">The properties. </param>
        /// <param name="postalCode">The postal code. This could be a number or a string.</param>
        /// <param name="city">The city</param>
        /// <param name="state">The state</param>
        /// <param name="country">The country</param>
        /// <param name="numBedrooms">The number of bedrooms.</param>
        /// <param name="numBathrooms">The number of bathrooms</param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public string CreatePropertyAsset(ILogger logger, ICoreCache cache, string nameSpace, string propertyAssetIdentifier, 
            PropertyType propertyType, string shortName, string streetIdentifier, string streetName, string suburb, string city, string postalCode, 
            string state, string country, string numBedrooms, string numBathrooms, string numParking, string currency, 
            string description, NamedValueSet properties)
        {
            if (properties is null)
            {
                properties = new NamedValueSet();
            }
            properties.Set(PropertyProp.Currency, currency);
            properties.Set(PropertyProp.AsAtDate, DateTime.Today);
            properties.Set(PropertyProp.Function, FunctionProp.ReferenceData.ToString());
            properties.Set(PropertyProp.DataGroup, NameSpace + "." + FunctionProp.ReferenceData.ToString());
            properties.Set(PropertyProp.Domain, FunctionProp.ReferenceData.ToString() + ".Property");
            properties.Set(PropertyProp.SourceSystem, "Highlander");
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(PropertyProp.Description, shortName);
            properties.Set(PropertyProp.PropertyType, propertyType);
            var propertyAsset = PropertyAssetHelper.Create(propertyAssetIdentifier, propertyType.ToString(), streetIdentifier, streetName,
            suburb, city, postalCode, state, country, numBedrooms, numBathrooms, numParking, currency, description);
            //This identifier is too complex.
            var identifier = new PropertyIdentifier(propertyType.ToString(), city, shortName, postalCode, propertyAssetIdentifier).UniqueIdentifier;
            propertyAsset.id = identifier;
            var assetIdentifier = NameSpace + "." + identifier;
            properties.Set(PropertyProp.UniqueIdentifier, identifier);
            var propertyInstrument = new PropertyNodeStruct {Property = propertyAsset};
            //TODO Add the loan information
            cache.SaveObject(propertyInstrument, assetIdentifier, properties);
            logger.LogDebug("Created new property asset: {0}", assetIdentifier);
            return identifier;
        }

        #endregion

        #endregion

        #region Create Products

        #region Lease Transaction

        #region Lease Properties

        /// <summary>
        /// Creates a lease in the data store.
        /// </summary>
        /// <param name="leaseId">The lease identifier.</param>
        /// <param name="leaseType">The lease type: retail, commercial, investment etc</param>
        /// <param name="reviewChange">THe review change as a percentage.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="startGrossPrice">The start gross price.</param>
        /// <param name="description">The lease description.</param>
        /// <param name="leaseExpiryDate">The lease expiry date.</param>
        /// <param name="referencePropertyIdentifier">The property reference identifier.</param>
        /// <param name="shopNumber">The shop number.</param>
        /// <param name="nextReviewDate">The next review date.</param>
        /// <param name="unitsOfArea">The units of area: money per square meter; money per square foot.</param>
        /// <returns></returns>
        private NamedValueSet CreateLeaseProperties(string nameSpace, string tenant, string leaseId, string leaseType,
            string referencePropertyIdentifier, string currency,  string description)
        {
            var properties = new NamedValueSet();
            properties.Set(LeaseProp.Currency, currency);
            properties.Set(LeaseProp.AsAtDate, DateTime.Today);
            properties.Set(LeaseProp.Function, FunctionProp.ReferenceData.ToString());
            properties.Set(LeaseProp.DataGroup, NameSpace + "." + FunctionProp.ReferenceData.ToString());
            properties.Set(LeaseProp.Domain, FunctionProp.ReferenceData.ToString() + ".Lease");
            properties.Set(LeaseProp.SourceSystem, "Highlander");
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(LeaseProp.Description, description);
            properties.Set(LeaseProp.LeaseType, leaseType);
            properties.Set(LeaseProp.LeaseType, tenant);
            properties.Set(LeaseProp.ReferencePropertyIdentifier, referencePropertyIdentifier);
            var identifier = new LeaseIdentifier(referencePropertyIdentifier, tenant, leaseType, leaseId);
            properties.Set(LeaseProp.UniqueIdentifier, identifier.UniqueIdentifier);
            return properties;
        }

        #endregion

        /// <summary>
        /// Creates a lease transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Tenant">Is party1 the tenant. If not then it is the property owner. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="leaseStartDate"></param>
        /// <param name="upfrontAmount">Any upfront payment amount. If there is no upfront this is zero.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="paymentDate">The payment Date.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="numberOfUnits">The number of untis of area.</param>
        /// <param name="unitsOfArea"></param>
        /// <param name="leaseExpiryDate"></param>
        /// <param name="reviewFrequency">The frequency  of review. Normally 1Y.</param>
        /// <param name="nextReviewDate">The next review date.</param>
        /// <param name="reviewChange">The percentage change on the gross amount.</param>
        /// <param name="startGrossAmount"></param>
        /// <param name="leaseId">The lease identifier.</param>
        /// <param name="leaseType">The lease type.</param>
        /// <param name="shopNumber">The shop identifier.</param>
        /// <param name="referencePropertyIdentifier">The reference property identifier.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public string CreateLeaseTransactionWithProperties(string tradeId, bool isParty1Tenant, DateTime tradeDate, DateTime leaseStartDate, decimal upfrontAmount, 
            DateTime paymentDate, string currency, string portfolio, decimal startGrossAmount, string leaseId, string leaseType, string shopNumber, decimal numberOfUnits, string unitsOfArea,
            DateTime leaseExpiryDate, string reviewFrequency, DateTime nextReviewDate, decimal reviewChange, string referencePropertyIdentifier, string description, NamedValueSet properties)
        {
            return CreateLeaseTransactionWithProperties(Logger, Cache, tradeId, isParty1Tenant, tradeDate, leaseStartDate, upfrontAmount, 
                paymentDate, currency, portfolio, startGrossAmount, leaseId, leaseType, shopNumber, numberOfUnits, unitsOfArea, leaseExpiryDate, reviewFrequency, nextReviewDate,
                reviewChange, referencePropertyIdentifier, description, properties);
        }

        /// <summary>
        /// Creates a lease transaction
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Tenant">Is party1 the tenant. If not then it is the property owner. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="leaseStartDate"></param>
        /// <param name="upfrontAmount">Any upfront payment amount. If there is no upfront this is zero.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="paymentDate">The payment Date.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="numberOfUnits">The number of untis of area.</param>
        /// <param name="unitsOfArea"></param>
        /// <param name="leaseExpiryDate"></param>
        /// <param name="reviewFrequency">The frequency  of review. Normally annual.</param>
        /// <param name="nextReviewDate">The next review date.</param>
        /// <param name="reviewChange">The percentage change on the gross amount.</param>
        /// <param name="startGrossAmount"></param>
        /// <param name="leaseId">The lease identifier.</param>
        /// <param name="leaseType">The lease type.</param>
        /// <param name="shopNumber">The shop identifier.</param>
        /// <param name="referencePropertyIdentifier">The reference property identifier.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public string CreateLeaseTransactionWithProperties(ILogger logger, ICoreCache cache, string tradeId, bool isParty1Tenant, DateTime tradeDate, DateTime leaseStartDate,
            decimal upfrontAmount, DateTime paymentDate, string currency, string portfolio, decimal startGrossAmount, string leaseId, string leaseType, string shopNumber, 
            decimal numberOfUnits, string unitsOfArea, DateTime leaseExpiryDate, string reviewFrequency, DateTime nextReviewDate, decimal reviewChange, 
            string referencePropertyIdentifier, string description, NamedValueSet properties)
        {
            //Party Information
            string buyer = "Party1";
            string seller = "Party2";
            if (!isParty1Tenant)
            {
                buyer = "Party2";
                seller = "Party1";
            }
            var partya = properties.GetValue<string>(TradeProp.Party1, true);
            var partyb = properties.GetValue<string>(TradeProp.Party2, true);
            //Currently only takeds a single calendar.
            //TODO extend to multiple calendars.
            var businessDayCalendar = properties.GetValue(LeaseProp.BusinessDayCalendar, "AUSY");
            var businessDayAdjustments = properties.GetValue(LeaseProp.BusinessDayAdjustments, "FOLLOWING");
            var defaultRollConvention = leaseStartDate.Day;
            var rollConvention = RollConventionEnumHelper.Parse(properties.GetValue(LeaseProp.RollConvention, defaultRollConvention.ToString()));
            var businessDayConvention = BusinessDayConventionHelper.Parse(businessDayAdjustments);
            var businessCenters = BusinessCentersHelper.Parse(businessDayCalendar);
            var paymentFrequency = properties.GetValue(LeaseProp.BusinessDayAdjustments, "1M");
            string tenant = partyb;
            if (isParty1Tenant)
            {
                tenant = partya;
            }
            var leaseProperties = CreateLeaseProperties(NameSpace, tenant, leaseId, leaseType,
                referencePropertyIdentifier, currency, description);
            properties.Set(TradeProp.TradingBookName, portfolio);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.GetValue(TradeProp.EffectiveDate, leaseExpiryDate);
            properties.GetValue(TradeProp.TradeDate, tradeDate);
            properties.GetValue(TradeProp.PaymentDate, paymentDate);
            var leaseTradeIdentifier = $"{"Trade"}.{sourceSystem}.{ItemChoiceType15.leaseTransaction}.{tradeId}";
            properties.Add(leaseProperties);
            var lease = LeaseAssetHelper.Create(tenant, leaseId, leaseType, leaseExpiryDate, referencePropertyIdentifier, shopNumber,
                leaseStartDate, reviewFrequency, nextReviewDate, reviewChange, currency, numberOfUnits, unitsOfArea, startGrossAmount, 
                paymentFrequency, rollConvention, businessDayConvention, businessCenters, description);
            //  1) Build and publish the equity transaction
            //
            properties.Set(TradeProp.UniqueIdentifier, null);
            var productType = new object[] { ProductTypeHelper.Create("LeaseTransaction") };
            var amount = Convert.ToDouble(upfrontAmount);
            var leaseTransaction = new LeaseTransaction
            {
                buyerPartyReference =
                    PartyReferenceHelper.Parse(buyer),
                id = leaseTradeIdentifier,
                Items = productType,
                ItemsElementName = new[] { ItemsChoiceType2.productType },
                sellerPartyReference =
                    PartyReferenceHelper.Parse(seller),
                lease = lease,
            };
            //  2) Create the trade
            //          
            var trade = new Trade
            {
                Item = leaseTransaction,
                ItemElementName = ItemChoiceType15.leaseTransaction,
                tradeHeader = new TradeHeader(), //TODO We need a new type here!
            };
            var party1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
            var party2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
            trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
            trade.tradeHeader.tradeDate = new IdentifiedDate { Value = tradeDate };
            //TODO there is no lease type to use.
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, NameSpace);
            //  3) Save the lease trade
            //
            cache.SaveObject(trade, NameSpace + "." + leaseTradeIdentifier, properties);
            //  4) Log the event
            //
            logger.LogDebug("Created new lease transaction: {0}", leaseTradeIdentifier);
            //
            //  5) Build and publish the cash transaction
            // TODO This cash amount does not have the correct payment date or roll conventions. Need to load some configuration.
            var cashIdentifier = CreateBulletPayment(tradeId, isParty1Tenant, partya, partyb, tradeDate, paymentDate, businessDayCalendar, businessDayAdjustments,
                currency, amount, sourceSystem);
            //  7) Log the cash trade.
            logger.LogDebug("Created new lease transaction: {0}", cashIdentifier);
            return leaseTradeIdentifier;
        }

        #endregion

        #region Property Transaction

        /// <summary>
        /// Creates a property transaction
        /// </summary>
        /// <param name="tradeId">The trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="purchaseAmount">The purchase Amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="propertyAssetIdentifier">The property asset identifier. Currently assumed to be of the form:  Highlander.ReferenceData.Property.... </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="paymentDate">The payment Date.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <returns></returns>
        public string CreatePropertyTransactionWithProperties(string tradeId, bool isParty1Buyer, DateTime tradeDate, DateTime effectiveDate, decimal purchaseAmount,
                DateTime paymentDate, string currency, string propertyAssetIdentifier, string portfolio, NamedValueSet properties)
        {
            return CreatePropertyTransactionWithProperties(Logger, Cache, tradeId, isParty1Buyer, tradeDate, effectiveDate, purchaseAmount, 
                paymentDate, currency, propertyAssetIdentifier, portfolio, properties);
        }

        /// <summary>
        /// Creates a property transaction
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="purchaseAmount">The purchase Amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="propertyAssetIdentifier">The property asset identifier. Currently assumed to be of the form:  Highlander.ReferenceData.Property.... </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="propertyType">The property type.</param>
        /// <param name="paymentDate">The payment Date.</param>
        /// <param name="portfolio">The portfoliok.</param>
        /// <param name="properties">A bunch of properties</param>
        /// <returns></returns>
        public string CreatePropertyTransactionWithProperties(ILogger logger, ICoreCache cache, string tradeId, bool isParty1Buyer, 
            DateTime tradeDate, DateTime effectiveDate, decimal purchaseAmount,  DateTime paymentDate, string currency, 
            string propertyAssetIdentifier, string portfolio, NamedValueSet properties)
        {
            //Party Information
            string buyer = "Party1";
            string seller = "Party2";
            if (!isParty1Buyer)
            {
                buyer = "Party2";
                seller = "Party1";
            }
            var partya = properties.GetValue<string>(TradeProp.Party1, true);
            var partyb = properties.GetValue<string>(TradeProp.Party2, true);
            var businessDayCalendar = properties.GetValue(PropertyProp.BusinessDayCalendar, "AUSY");
            var businessDayAdjustments = properties.GetValue(PropertyProp.BusinessDayAdjustments, "FOLLOWING");
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.Internal);
            properties.GetValue(TradeProp.EffectiveDate, effectiveDate);
            properties.GetValue(TradeProp.TradeDate, tradeDate);
            properties.GetValue(TradeProp.PaymentDate, paymentDate);
            properties.Set(TradeProp.TradingBookName, portfolio);
            var propertyTradeIdentifier = $"{FunctionProp.Trade}.{FpML5R3NameSpaces.Reporting}.{sourceSystem}.{ItemChoiceType15.propertyTransaction}.{tradeId}";
            var item = cache.LoadItem<PropertyNodeStruct>(propertyAssetIdentifier);
            if (item?.Data is PropertyNodeStruct propertyNode)
            {
                //  1) Build and publish the property transaction
                //
                properties.Set(TradeProp.UniqueIdentifier, NameSpace + "." + propertyTradeIdentifier);
                var productType = new object[] { ProductTypeHelper.Create("PropertyTransaction") };
                var amount = Convert.ToDouble(purchaseAmount);
                var propertyTransaction = new PropertyTransaction
                {
                    buyerPartyReference =
                        PartyReferenceHelper.Parse(buyer),
                    id = propertyTradeIdentifier,
                    Items = productType,
                    ItemsElementName = new[] { ItemsChoiceType2.productType },
                    sellerPartyReference =
                        PartyReferenceHelper.Parse(seller),
                    property = propertyNode?.Property,
                };
                //  2) Create the trade
                //          
                var trade = new Trade
                {
                    Item = propertyTransaction,
                    ItemElementName = ItemChoiceType15.propertyTransaction,
                    tradeHeader = new TradeHeader(), //TODO We need a new type here!
                };
                var party1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
                var party2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
                trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
                trade.tradeHeader.tradeDate = new IdentifiedDate { Value = tradeDate };
                //TODO there is no property type to use.
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(TradeProp.TradeSource, sourceSystem);
                properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                properties.Set(EnvironmentProp.NameSpace, NameSpace);
                //  3) Save the property trade
                //
                cache.SaveObject(trade, NameSpace + "." + propertyTradeIdentifier, properties);
                //  4) Log the event
                //
                logger.LogDebug("Created new property transaction: {0}", propertyTradeIdentifier);
                //
                //  5) Build and publish the cash transaction
                // TODO This cash amount does not have the correct payment date or roll conventions. Need to load some configuration.
                var cashIdentifier = CreateBulletPayment(tradeId, isParty1Buyer, partya, partyb, tradeDate, paymentDate, businessDayCalendar, businessDayAdjustments,
                    currency, amount, sourceSystem);
                //  7) Log the cash trade.
                logger.LogDebug("Created new cash transaction: {0}", cashIdentifier);
                return propertyTradeIdentifier;
            }
            return "Property transaction unsuccessful. Check there is data for the reference property.";
        }

        #endregion

        #region Equity Transaction

        /// <summary>
        /// Creates a equity transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the base party and buyer of the bond. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="unitPrice">The unit price.</param>
        /// <param name="unitPriceCurrency">The unti price currency.</param>
        /// <param name="equityIdentifier">The equity identifier. Currently assumed to be of the form:  Highlander.ReferenceData.Equity.ANZ.AU </param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <param name="numberOfShares">The number of shares. </param>
        /// <returns></returns>
        public string CreateEquityTransactionWithProperties(string tradeId, bool isParty1Buyer, DateTime tradeDate, decimal numberOfShares,
            decimal unitPrice, string unitPriceCurrency, string equityIdentifier, NamedValueSet properties)
        {
            return CreateEquityTransactionWithProperties(Logger, Cache, tradeId, isParty1Buyer, tradeDate, numberOfShares, unitPrice, unitPriceCurrency, equityIdentifier, properties);
        }

        /// <summary>
        /// Creates a equity transaction
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the base party and buyer of the bond. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="unitPrice">The unit price.</param>
        /// <param name="unitPriceCurrency">The unti price currency.</param>
        /// <param name="equityIdentifier">The equity identifier. Currently assumed to be of the form:  Highlander.ReferenceData.Equity.ANZ.AU </param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the buyer party 
        /// Party2 - the seller party.
        /// This is used to determine the base direction in calculations</param>
        /// <param name="numberOfShares">The number of shares. </param>
        /// <returns></returns>
        public string CreateEquityTransactionWithProperties(ILogger logger, ICoreCache cache, string tradeId, bool isParty1Buyer, DateTime tradeDate, decimal numberOfShares,
            decimal unitPrice, string unitPriceCurrency, string equityIdentifier, NamedValueSet properties)
        {
            //Party Information
            string buyer = "Party1";
            string seller = "Party2";
            if (!isParty1Buyer)
            {
                buyer = "Party2";
                seller = "Party1";
            }
            var partya = properties.GetValue<string>(TradeProp.Party1, true);
            var partyb = properties.GetValue<string>(TradeProp.Party2, true);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var bondSettlemetDate = properties.GetValue(TradeProp.EffectiveDate, tradeDate);
            var businessDayCalendar = properties.GetValue(BondProp.BusinessDayCalendar, "AUSY");
            var businessDayAdjustments = properties.GetValue(BondProp.BusinessDayAdjustments, "FOLLOWING");
            var equityTradeIdentifier = string.Format("{0}.{1}.{2}.{3}", FunctionProp.Trade, sourceSystem, ItemChoiceType15.equityTransaction, tradeId);
            var item = cache.LoadItem<EquityAsset>(NameSpace + "." + FunctionProp.ReferenceData + "." + ReferenceDataProp.Equity + "." + equityIdentifier);
            //properties.Add(item.AppProps);
            if (item.Data is EquityAsset equity)
            {
                //  1) Build and publish the equity transaction
                //
                properties.Set(TradeProp.UniqueIdentifier, null);
                var productType = new object[] { ProductTypeHelper.Create("EquityTransaction") };
                var amount = Convert.ToDouble(numberOfShares);
                var equityTransaction = new EquityTransaction
                {
                    numberOfUnits = numberOfShares,
                    unitPrice = MoneyHelper.GetAmount(unitPrice, unitPriceCurrency),
                    buyerPartyReference =
                        PartyReferenceHelper.Parse(buyer),
                    id = equityTradeIdentifier,
                    Items = productType,
                    ItemsElementName = new[] { ItemsChoiceType2.productType },
                    sellerPartyReference =
                        PartyReferenceHelper.Parse(seller),
                    equity = equity,
                };
                //  2) Create the trade
                //          
                var trade = new Trade
                {
                    Item = equityTransaction,
                    ItemElementName = ItemChoiceType15.equityTransaction,
                    tradeHeader = new TradeHeader(), //TODO We need a new type here!
                };
                var party1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
                var party2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
                trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
                trade.tradeHeader.tradeDate = new IdentifiedDate { Value = tradeDate };
                //TODO there is no Bond type to use.
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();//TODO This does not handle floaters!
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(TradeProp.TradeSource, sourceSystem);
                properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                properties.Set(EnvironmentProp.NameSpace, NameSpace);
                //  3) Save the bond trade
                //
                cache.SaveObject(trade, NameSpace + "." + equityTradeIdentifier, properties);
                //  4) Log the event
                //
                logger.LogDebug("Created new equity transaction: {0}", equityTradeIdentifier);
                //
                //  5) Build and publish the cash transaction
                // TODO This cash amount does not have the correct payment date or roll conventions. Need to load some configuration.
                var cashIdentifier = CreateBulletPayment(tradeId, isParty1Buyer, partya, partyb, tradeDate, bondSettlemetDate, businessDayCalendar, businessDayAdjustments,
                    unitPriceCurrency, amount * Convert.ToDouble(unitPrice), sourceSystem);
                //  7) Log the cash trade.
                logger.LogDebug("Created new payment transaction: {0}", cashIdentifier);
                return equityTradeIdentifier;
            }
            return "Equity transaction unsuccessful. Check there is data for the reference equity.";
        }

        #endregion

        #region Bond Transaction

        /// <summary>
        /// Creates a bond transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the base party and buyer of the bond. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="priceInformation">The bond price information</param>
        /// <param name="bondIdentifier">The bond identifier. Currently assumed to be of the form:  Highlander.ReferenceData.FixedIncome.Corp.ANZ.Fixed.5,25.01-16-14 </param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <param name="notional">The notional in the trade currency, which is assumed to be same currency as the coupon. </param>
        /// <returns></returns>
        public string CreateBondTransactionWithProperties(string tradeId, bool isParty1Buyer, DateTime tradeDate, decimal notional, NamedValueSet priceInformation, string bondIdentifier, NamedValueSet properties)
        {
            return CreateBondTransactionWithProperties(Logger, Cache, tradeId, isParty1Buyer, tradeDate, notional, priceInformation, bondIdentifier, properties);
        }

        /// <summary>
        /// Creates a bond transaction
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the base party and buyer of the bond. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="priceInformation">The bond price information</param>
        /// <param name="bondIdentifier">The bond identifier. Currently assumed to be of the form:  Highlander.ReferenceData.FixedIncome.Corp.ANZ.Fixed.5,25.01-16-14 </param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the buyer party 
        /// Party2 - the seller party.
        /// This is used to determine the base direction in calculations</param>
        /// <param name="notional">The notional in the trade currency, which is assumed to be same currency as the coupon. </param>
        /// <returns></returns>
        public string CreateBondTransactionWithProperties(ILogger logger, ICoreCache cache, string tradeId, bool isParty1Buyer, DateTime tradeDate, decimal notional, 
            NamedValueSet priceInformation, string bondIdentifier, NamedValueSet properties)
        {
            //Party Information
            string buyer = "Party1";
            string seller= "Party2";
            if (!isParty1Buyer)
            {
                buyer = "Party2";
                seller = "Party1";
            }
            //Get the required bond transaction properties.
            var partyA = properties.GetValue<string>(TradeProp.Party1, true);
            var partyB = properties.GetValue<string>(TradeProp.Party2, true);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var bondSettlemetDate = properties.GetValue(TradeProp.EffectiveDate, tradeDate);
            var businessDayCalendar = properties.GetValue(BondProp.BusinessDayCalendar, "AUSY");
            var businessDayAdjustments = properties.GetValue(BondProp.BusinessDayAdjustments, "FOLLOWING");
            var bondTradeIdentifier = String.Format("{0}.{1}.{2}.{3}", FunctionProp.Trade, sourceSystem, ItemChoiceType15.bondTransaction, tradeId);
            var item = cache.LoadItem<Bond>(NameSpace + "." + FunctionProp.ReferenceData + "." + ReferenceDataProp.FixedIncome + "." + bondIdentifier);
            var bondType = item.AppProps.GetValue(BondProp.BondType, BondTypesEnum.AGB.ToString());
            //Extract the necessary propertiess from the bond data.
            properties.Set(BondProp.BondType, bondType);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            //properties.Add(item.AppProps);
            if (item.Data is Bond bond)
            {
                //Reset the identifier for the bond curve mapping.
                bond.id = bondIdentifier;
                //  1) Build and publish the bond transaction
                //
                properties.Set(TradeProp.UniqueIdentifier, null);
                var productType = new object[] { ProductTypeHelper.Create("BondTransaction") };
                var amount = Convert.ToDouble(notional);
                var bondTransaction = new BondTransaction
                {
                    notionalAmount = MoneyHelper.GetAmount(notional, bond.currency.Value),
                    buyerPartyReference =
                                                  PartyReferenceHelper.Parse(buyer),
                    id = bondTradeIdentifier,
                    Items = productType,
                    ItemsElementName = new[] { ItemsChoiceType2.productType },
                    sellerPartyReference =
                                                  PartyReferenceHelper.Parse(seller),
                    bond = bond,
                    price = new BondPrice()
                };
                //  2) Get the bond Price information
                //
                var accruals = priceInformation.Get(BondPriceEnum.Accrued.ToString(), false);
                var cleanPrice = priceInformation.Get(BondPriceEnum.CleanPrice.ToString(), false);
                var dirtyPrice = priceInformation.Get(BondPriceEnum.DirtyPrice.ToString(), false);
                //var ytm = priceInformation.Get(BondPriceEnum.YieldToMaturity.ToString(), false);
                if (dirtyPrice != null)
                {
                    bondTransaction.price.dirtyPrice = Convert.ToDecimal(dirtyPrice.Value);
                    bondTransaction.price.dirtyPriceSpecified = true;
                    amount = amount * Convert.ToDouble(dirtyPrice.Value) / 100;
                }
                if (cleanPrice != null)
                {
                    bondTransaction.price.cleanPrice = Convert.ToDecimal(cleanPrice.Value);
                    bondTransaction.price.cleanOfAccruedInterest = true;
                }
                if (accruals != null)
                {
                    bondTransaction.price.accruals = Convert.ToDecimal(accruals.Value);
                    bondTransaction.price.accrualsSpecified = true;
                }
                var trade = new Trade
                {
                    Item = bondTransaction,
                    ItemElementName = ItemChoiceType15.bondTransaction,
                    tradeHeader = new TradeHeader(), //TODO We need a new type here!
                };
                var party1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
                var party2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
                trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
                trade.tradeHeader.tradeDate = new IdentifiedDate { Value = tradeDate };
                //TODO there is no Bond type to use.
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();//TODO This does not handle floaters!
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                properties.Set(EnvironmentProp.NameSpace, NameSpace);
                //  3) Save the bond trade
                //
                cache.SaveObject(trade, NameSpace + "." + bondTradeIdentifier, properties);
                //  4) Log the event
                //
                logger.LogDebug("Created new bond transaction: {0}", bondTradeIdentifier);
                //
                //  5) Build and publish the cash transaction
                // TODO Add a reference to the bond trade?
                var cashIdentifier = CreateBulletPayment(tradeId, isParty1Buyer, partyA, partyB, tradeDate, bondSettlemetDate, businessDayCalendar, businessDayAdjustments,
                    bondTransaction.notionalAmount.currency.Value, amount, sourceSystem);
                //  7) Log the cash trade.
                logger.LogDebug("Created new payment transaction: {0}", cashIdentifier);
                return bondTradeIdentifier;
            }
            return "Bond transaction unsuccessful. Check there is data for the reference bond.";
        }

        #endregion

        #region IR Future Transaction

        /// <summary>
        /// Creates a bond transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the base party and buyer of the bond. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="puchasePrice">The purchase price information</param>
        /// <param name="futureIdentifier">The futures identifier. Currently assumed to be of the form AUD-IRFuture-IR-6 OR AUD-IRFuture-IR-Z7</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the buyer party 
        /// Party2 - the seller party.
        /// This is used to determine the base direction in calculations</param>
        /// <param name="numberOfContracts">The number of contracts.</param>
        /// <returns></returns>
        public string CreateRateFutureTransactionWithProperties(string tradeId, bool isParty1Buyer, DateTime tradeDate, int numberOfContracts, 
            Decimal puchasePrice, string futureIdentifier, NamedValueSet properties)
        {
            return CreateRateFutureTransactionWithProperties(Logger, Cache, NameSpace, tradeId, isParty1Buyer, tradeDate, numberOfContracts, puchasePrice, futureIdentifier, properties);
        }

        /// <summary>
        /// Creates a futures transaction
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the base party and buyer of the bond. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="puchasePrice">The purchase price information</param>
        /// <param name="futuresIdentifier">The futures identifier. Currently assumed to be of the form AUD-IRFuture-IR-6 OR AUD-IRFuture-IR-Z7</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the buyer party 
        /// Party2 - the seller party.
        /// This is used to determine the base direction in calculations</param>
        /// <param name="numberOfContracts">The number of contracts.</param>
        /// <returns></returns>
        public string CreateRateFutureTransactionWithProperties(ILogger logger, ICoreCache cache, string nameSpace, string tradeId, 
            bool isParty1Buyer, DateTime tradeDate, int numberOfContracts, Decimal puchasePrice, string futuresIdentifier, NamedValueSet properties)
        {
            //Party Information
            string buyer = "Party1";
            string seller = "Party2";
            if (!isParty1Buyer)
            {
                buyer = "Party2";
                seller = "Party1";
            }
            //Get the required transaction properties.
            var partya = properties.GetValue<String>(TradeProp.Party1, true);
            var partyb = properties.GetValue<String>(TradeProp.Party2, true);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            //Map to find the instrument
            //Highlander.V5r3.Configuration.Instrument.IRFuture.AUD.IR
            NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForAssets(NameSpace, futuresIdentifier, tradeDate);
            var assetIdentifier = PropertyHelper.ExtractStringProperty("AssetId", namedValueSet);
            //create the asset-basicassetvaluation pair.
            var asset = AssetHelper.Parse(assetIdentifier, 0.0m, 0.0m);
            //sets up the uniqueidentifier.
            var uniqueIdentifier = PropertyHelper.ExtractStringProperty(CurveProp.UniqueIdentifier, properties);
            //create the priceable asset.
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, assetIdentifier);
            var priceableAsset = PriceableAssetFactory.Create(Logger, Cache, NameSpace, asset.Second, namedValueSet, null, null) as CurveEngine.V5r3.Assets.Rates.Futures.PriceableRateFuturesAsset;
            var lastTradeDate = priceableAsset.LastTradeDate;
            var futuresType = ExchangeContractTypeEnum.IRFuture.ToString();
            var contractType = EnumHelper.Parse<ExchangeContractTypeEnum>(futuresType);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.MaturityDate, lastTradeDate);
            var futuresTradeIdentifier = String.Format("{0}.{1}.{2}.{3}.{4}", FunctionProp.Trade, sourceSystem, ItemChoiceType15.futureTransaction, futuresType, tradeId);
            if (instrument.InstrumentNodeItem is IRFutureNodeStruct nodeStruct)
            {
                nodeStruct.Future.id = futuresIdentifier;
                //  1) Build and publish the futures transaction
                //
                properties.Set(TradeProp.UniqueIdentifier, null);
                var productType = new object[] { ProductTypeHelper.Create("FutureTransaction") };
                var amount = Convert.ToDouble(numberOfContracts);
                var futuresTransaction = new FutureTransaction
                { 
                    numberOfUnits = numberOfContracts,
                    buyerPartyReference =
                        PartyReferenceHelper.Parse(buyer),
                    id = futuresTradeIdentifier,
                    Items = productType,
                    ItemsElementName = new[] { ItemsChoiceType2.productType },
                    sellerPartyReference =
                        PartyReferenceHelper.Parse(seller)                      
                };
                //  2) Get the futures Price information
                //
                var unitPrice = MoneyHelper.GetAmount(puchasePrice, nodeStruct.Future.currency.Value);
                futuresTransaction.unitPrice = unitPrice;
                futuresTransaction.future = nodeStruct.Future;
                futuresTransaction.future.maturity = lastTradeDate;
                futuresTransaction.future.maturitySpecified = true;
                var trade = new Trade
                {
                    Item = futuresTransaction,
                    ItemElementName = ItemChoiceType15.futureTransaction,
                    tradeHeader = new TradeHeader(), //TODO We need a new type here!
                };
                var party1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
                var party2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
                trade.tradeHeader.partyTradeIdentifier = new[] { party1, party2 };
                trade.tradeHeader.tradeDate = new IdentifiedDate { Value = tradeDate };
                //TODO there is no Bond type to use.
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();//TODO This does not handle floaters!
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                properties.Set(EnvironmentProp.NameSpace, NameSpace);
                //  3) Save the futures trade
                //
                cache.SaveObject(trade, NameSpace + "." + futuresTradeIdentifier, properties);
                //  4) Log the event
                //
                logger.LogDebug("Created new future transaction: {0}", futuresTradeIdentifier);
                return futuresTradeIdentifier;
            }
            return "Future transaction unsuccessful. Check there is data for the reference future.";
        }

        #endregion

        #region Term Deposits

        /// <summary>
        /// Creates a simple term deposit
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isLenderBase">The isLender flag. If [true] then the base party is Party1.</param>
        /// <param name="lenderParty">The lender.</param>
        /// <param name="borrowerParty">The borrower.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="notionalAmount">The notional lent/borrowed.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="dayCount">THe daycount basis. Must be a valid type.</param>
        /// <returns></returns>
        public string CreateTermDeposit(string tradeId, bool isLenderBase, string lenderParty, string borrowerParty, DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount)
        {
            return CreateTermDeposit(Logger, Cache, NameSpace, tradeId, isLenderBase, lenderParty, borrowerParty, tradeDate, startDate, maturityDate, currency, notionalAmount, fixedRate, dayCount);
        }

        /// <summary>
        /// Creates a simple term deposit
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isLenderBase">The isLender flag. If [true] then the base party is Party1.</param>
        /// <param name="lenderParty">The lender.</param>
        /// <param name="borrowerParty">The borrower.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="notionalAmount">The notional lent/borrowed.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="dayCount">THe daycount basis. Must be a valid type.</param>
        /// <returns></returns>
        public string CreateTermDeposit(ILogger logger, ICoreCache cache, 
            String nameSpace, string tradeId, bool isLenderBase, string lenderParty,
            string borrowerParty, DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount)
        {
            var properties = new NamedValueSet();
            if (isLenderBase)
            {
                properties.Set(TradeProp.Party1, lenderParty);
                properties.Set(TradeProp.Party2, borrowerParty);
            }
            else
            {
                properties.Set(TradeProp.Party1, borrowerParty);
                properties.Set(TradeProp.Party2, lenderParty);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, startDate);
            properties.Set(TradeProp.MaturityDate, maturityDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.TermDeposit.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.termDeposit.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_TermDeposit));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trade = TermDepositPricer.CreateSimpleTermDepositTrade(tradeId, ProductTypeSimpleEnum.TermDeposit.ToString(), tradeDate, startDate,
                maturityDate, currency, Convert.ToDecimal(notionalAmount), Convert.ToDecimal(fixedRate), dayCount);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.termDeposit, ProductTypeSimpleEnum.TermDeposit, tradeId, tradeDate, TradeSourceType.SpreadSheet);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new term deposit: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        /// <summary>
        /// Creates a simple term deposit
        /// </summary>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="notionalAmount">The notional lent/borrowed.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="dayCount">THe daycount basis. Must be a valid type.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateDepositWithProperties(DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount, NamedValueSet properties)
        {
            return CreateDepositWithProperties(Logger, Cache, NameSpace, tradeDate, startDate, maturityDate, currency, notionalAmount, fixedRate, dayCount, properties);
        }

        /// <summary>
        /// Creates a simple term deposit
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSPace</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="notionalAmount">The notional lent/borrowed.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="dayCount">THe daycount basis. Must be a valid type.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateDepositWithProperties(ILogger logger, ICoreCache cache, 
            String nameSpace, DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount, NamedValueSet properties)
        {
            var tradeIDentifier = new Reporting.Identifiers.V5r3.TradeIdentifier(properties);
            var trade = TermDepositPricer.CreateSimpleTermDepositTrade(tradeIDentifier.Id, ProductTypeSimpleEnum.TermDeposit.ToString(), tradeDate, startDate,
                maturityDate, currency, Convert.ToDecimal(notionalAmount), Convert.ToDecimal(fixedRate), dayCount);
            var curves = trade.Item.GetRequiredPricingStructures();
            var currencies = trade.Item.GetRequiredCurrencies();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + tradeIDentifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new term deposit: {0}", tradeIDentifier.UniqueIdentifier);
            return tradeIDentifier.UniqueIdentifier;
        }

        #endregion

        #region Bullet Payments

        /// <summary>
        /// Creates a bullet payment
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="payer">The payer.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="paymentDate">The paymentDate.</param>
        /// <param name="businessDayCalendar">The  business Day Calendar.</param>
        /// <param name="businessDayAdjustements">The business Day Adjustements paymentDate.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="sourceSystem">The source system. This could be a spreadsheet.</param>
        /// <returns></returns>
        public string CreateBulletPayment(string tradeId, bool isPayerBase, string payer,
            string receiver, DateTime tradeDate, DateTime paymentDate, string businessDayCalendar,
            string businessDayAdjustements, string currency, double amount, string sourceSystem)
        {
            return CreateBulletPayment(Logger, Cache, NameSpace, tradeId, isPayerBase, payer, receiver, tradeDate, paymentDate, businessDayCalendar, businessDayAdjustements, currency, amount, sourceSystem);
        }

        /// <summary>
        /// Creates a bullet payment
        /// </summary>
        ///  <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="payer">The payer.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="paymentDate">The paymentDate.</param>
        /// <param name="businessDayCalendar">The  business Day Calendar.</param>
        /// <param name="businessDayAdjustements">The business Day Adjustements paymentDate.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="sourceSystem">The source system. This could be a spreadsheet.</param>
        /// <returns></returns>
        public string CreateBulletPayment(ILogger logger, ICoreCache cache, 
            String nameSpace, string tradeId, bool isPayerBase, string payer,
            string receiver, DateTime tradeDate, DateTime paymentDate, string businessDayCalendar,
            string businessDayAdjustements, string currency, double amount, string sourceSystem)
        {
            var properties = new NamedValueSet();
            if (isPayerBase)
            {
                properties.Set(TradeProp.Party1, payer);
                properties.Set(TradeProp.Party2, receiver);
            }
            else
            {
                properties.Set(TradeProp.Party1, receiver);
                properties.Set(TradeProp.Party2, payer);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.MaturityDate, paymentDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.BulletPayment.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.bulletPayment.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Cash_Payment));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(TradeProp.EffectiveDate, paymentDate);
            properties.Set(TradeProp.MaturityDate, paymentDate);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trade = BulletPaymentPricer.CreateBulletPayment(tradeId, tradeDate, ProductTypeSimpleEnum.BulletPayment.ToString(), isPayerBase, 
                paymentDate,  businessDayCalendar, businessDayAdjustements, currency, Convert.ToDecimal(amount));
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.bulletPayment, ProductTypeSimpleEnum.BulletPayment, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new bullet payment: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        /// <summary>
        /// Creates a bullet payment
        /// </summary>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="paymentDate">The paymentDate.</param>
        /// <param name="payerIsBase">THe payer is the base party i.e. the seller?</param>
        /// <param name="businessDayCalendar">The  business Day Calendar.</param>
        /// <param name="businessDayAdjustements">The business Day Adjustements paymentDate.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateBulletPaymentWithProperties(DateTime tradeDate, DateTime paymentDate, Boolean payerIsBase, 
            string businessDayCalendar, string businessDayAdjustements, string currency, double amount, NamedValueSet properties)
        {
            return CreateBulletPaymentWithProperties(Logger, Cache, NameSpace, tradeDate, paymentDate, payerIsBase, businessDayCalendar, businessDayAdjustements, currency, amount, properties);
        }

        /// <summary>
        /// Creates a bullet payment
        /// </summary>
        ///  <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="paymentDate">The paymentDate.</param>
        /// <param name="payerIsBase">THe payer is the base party i.e. the seller?</param>
        /// <param name="businessDayCalendar">The  business Day Calendar.</param>
        /// <param name="businessDayAdjustements">The business Day Adjustements paymentDate.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateBulletPaymentWithProperties(ILogger logger, ICoreCache cache, String nameSpace, DateTime tradeDate, DateTime paymentDate,
            Boolean payerIsBase, string businessDayCalendar, string businessDayAdjustements, string currency, double amount, NamedValueSet properties)
        {
            var tradeIDentifier = new Reporting.Identifiers.V5r3.TradeIdentifier(properties);
            var trade = BulletPaymentPricer.CreateBulletPayment(tradeIDentifier.Id, tradeDate, ProductTypeSimpleEnum.TermDeposit.ToString(), payerIsBase, paymentDate,
                businessDayCalendar, businessDayAdjustements, currency, Convert.ToDecimal(amount));
            var identifier = tradeIDentifier.UniqueIdentifier;
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new bullet payment: {0}", identifier);
            return identifier;
        }

        #endregion

        #region FX Spot

        /// <summary>
        /// Creates an fx spot trade.
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="exchangeCurrency1PayPartyReference">The currency1 payer.</param>
        /// <param name="exchangeCurrency2PayPartyReference">The currency2 payer.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="exchangeCurrency1Amount">The amount of currency1.</param>
        /// <param name="quoteBasis">The quoteBasis.</param>
        /// <param name="exchangeCurrency1">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="exchangeCurrency2">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="spotRate">The spot Rate.</param>
        /// <param name="sourceSystem">The source system</param>
        /// <returns></returns>
        public string CreateFxSpot(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, string sourceSystem)
        {
            return CreateFxSpot(Logger, Cache, NameSpace, tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, sourceSystem);
        }

        /// <summary>
        /// Creates an fx spot trade.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="exchangeCurrency1PayPartyReference">The currency1 payer.</param>
        /// <param name="exchangeCurrency2PayPartyReference">The currency2 payer.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="exchangeCurrency1Amount">The amount of currency1.</param>
        /// <param name="quoteBasis">The quoteBasis.</param>
        /// <param name="exchangeCurrency1">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="exchangeCurrency2">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="spotRate">The spot Rate.</param>
        /// <param name="sourceSystem">The source system</param>
        /// <returns></returns>
        public string CreateFxSpot(ILogger logger, ICoreCache cache, String nameSpace, 
            string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, string sourceSystem)
        {
            const ProductTypeSimpleEnum productType = ProductTypeSimpleEnum.FxSpot;
            var basis = EnumHelper.Parse<QuoteBasisEnum>(quoteBasis);
            var properties = new NamedValueSet();
            if (isPayerBase)
            {
                properties.Set(TradeProp.Party1, exchangeCurrency1PayPartyReference);
                properties.Set(TradeProp.Party2, exchangeCurrency2PayPartyReference);
            }
            else
            {
                properties.Set(TradeProp.Party1, exchangeCurrency2PayPartyReference);
                properties.Set(TradeProp.Party2, exchangeCurrency1PayPartyReference);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, valueDate);
            properties.Set(TradeProp.MaturityDate, valueDate);
            properties.Set(TradeProp.ProductType, productType.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.fxSingleLeg.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.ForeignExchange_Spot));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trade = FxSingleLegPricer.CreateFxLeg(tradeId, tradeDate, "Party1", "Party2",
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, basis, valueDate, spotRate, null, null);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fxSingleLeg, productType, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fx spot trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        /// <summary>
        /// Creates an fx spot trade.
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="exchangeCurrency1PayPartyReference">The currency1 payer.</param>
        /// <param name="exchangeCurrency2PayPartyReference">The currency2 payer.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="exchangeCurrency1Amount">The amount of currency1.</param>
        /// <param name="quoteBasis">The quoteBasis.</param>
        /// <param name="exchangeCurrency1">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="exchangeCurrency2">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="spotRate">The spot Rate.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxSpotWithProperties(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, NamedValueSet properties)
        {
            return CreateFxSpotWithProperties(Logger, Cache, NameSpace, tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, properties);
        }

        /// <summary>
        /// Creates an fx spot trade.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="exchangeCurrency1PayPartyReference">The currency1 payer.</param>
        /// <param name="exchangeCurrency2PayPartyReference">The currency2 payer.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="exchangeCurrency1Amount">The amount of currency1.</param>
        /// <param name="quoteBasis">The quoteBasis.</param>
        /// <param name="exchangeCurrency1">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="exchangeCurrency2">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="spotRate">The spot Rate.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxSpotWithProperties(ILogger logger, ICoreCache cache, String nameSpace, 
            string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, NamedValueSet properties)
        {
            const ProductTypeSimpleEnum productType = ProductTypeSimpleEnum.FxSpot;
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var basis = EnumHelper.Parse<QuoteBasisEnum>(quoteBasis);
            var trade = FxSingleLegPricer.CreateFxLeg(tradeId, tradeDate, "Party1", "Party2",
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, basis, valueDate, spotRate, null, null);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fxSingleLeg, productType, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.ForeignExchange_Spot));
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fx spot trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        #endregion

        #region FX Forwards

        /// <summary>
        /// Creates an fx forward trade.
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="exchangeCurrency1PayPartyReference">The currency1 payer.</param>
        /// <param name="exchangeCurrency2PayPartyReference">The currency2 payer.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="exchangeCurrency1Amount">The amount of currency1.</param>
        /// <param name="quoteBasis">The quoteBasis.</param>
        /// <param name="exchangeCurrency1">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="exchangeCurrency2">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="spotRate">The spot Rate.</param>
        /// <param name="forwardRate">The forward Rate.</param>
        /// <param name="forwardPoints">The [Optional] forward Points.</param>
        /// <param name="sourceSystem">The source system</param>
        /// <returns></returns>
        public string CreateFxForward(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, 
            Decimal spotRate, Decimal forwardRate, Decimal? forwardPoints, string sourceSystem)
        {
            return CreateFxForward(Logger, Cache, NameSpace, tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, forwardRate, forwardPoints, sourceSystem);
        }

        /// <summary>
        /// Creates an fx forward trade.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="sourceSystem">The source system.</param>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="exchangeCurrency1PayPartyReference">The currency1 payer.</param>
        /// <param name="exchangeCurrency2PayPartyReference">The currency2 payer.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="exchangeCurrency1Amount">The amount of currency1.</param>
        /// <param name="quoteBasis">The quoteBasis.</param>
        /// <param name="exchangeCurrency1">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="exchangeCurrency2">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="spotRate">The spot Rate.</param>
        /// <param name="forwardRate">The forward Rate.</param>
        /// <param name="forwardPoints">The [Optional] forward Points.</param>
        /// <returns></returns>
        public string CreateFxForward(ILogger logger, ICoreCache cache, String nameSpace, string tradeId, bool isPayerBase, DateTime tradeDate, 
            string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference, decimal exchangeCurrency1Amount, string exchangeCurrency1, 
            string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, Decimal forwardRate, Decimal? forwardPoints, string sourceSystem)
        {
            const ProductTypeSimpleEnum productType = ProductTypeSimpleEnum.FxForward;
            var basis = EnumHelper.Parse<QuoteBasisEnum>(quoteBasis);
            var properties = new NamedValueSet();
            if (isPayerBase)
            {
                properties.Set(TradeProp.Party1, exchangeCurrency1PayPartyReference);
                properties.Set(TradeProp.Party2, exchangeCurrency2PayPartyReference);
            }
            else
            {
                properties.Set(TradeProp.Party1, exchangeCurrency2PayPartyReference);
                properties.Set(TradeProp.Party2, exchangeCurrency1PayPartyReference);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, valueDate);
            properties.Set(TradeProp.MaturityDate, valueDate);
            properties.Set(TradeProp.ProductType, productType.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.fxSingleLeg.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.ForeignExchange_Forward));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trade = FxSingleLegPricer.CreateFxLeg(tradeId, tradeDate, "Party1", "Party2",
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, basis, valueDate, spotRate, forwardRate, null);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fxSingleLeg, productType, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fx forward trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        /// <summary>
        /// Creates an fx forward trade.
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="exchangeCurrency1PayPartyReference">The currency1 payer.</param>
        /// <param name="exchangeCurrency2PayPartyReference">The currency2 payer.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="exchangeCurrency1Amount">The amount of currency1.</param>
        /// <param name="quoteBasis">The quoteBasis.</param>
        /// <param name="exchangeCurrency1">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="exchangeCurrency2">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="spotRate">The spot Rate.</param>
        /// <param name="forwardRate">The forward Rate.</param>
        /// <param name="forwardPoints">The [Optional] forward Points.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxForwardWithProperties(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, Decimal forwardRate,
            Decimal? forwardPoints, NamedValueSet properties)
        {
            return CreateFxForwardWithProperties(Logger, Cache, NameSpace, tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, forwardRate, forwardPoints, properties);
        }

        /// <summary>
        /// Creates an fx forward trade.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="exchangeCurrency1PayPartyReference">The currency1 payer.</param>
        /// <param name="exchangeCurrency2PayPartyReference">The currency2 payer.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="exchangeCurrency1Amount">The amount of currency1.</param>
        /// <param name="quoteBasis">The quoteBasis.</param>
        /// <param name="exchangeCurrency1">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="exchangeCurrency2">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="spotRate">The spot Rate.</param>
        /// <param name="forwardRate">The forward Rate.</param>
        /// <param name="forwardPoints">The [Optional] forward Points.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxForwardWithProperties(ILogger logger, ICoreCache cache, String nameSpace,
            string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, Decimal forwardRate,
            Decimal? forwardPoints, NamedValueSet properties)
        {
            const ProductTypeSimpleEnum productType = ProductTypeSimpleEnum.FxForward;
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var basis = EnumHelper.Parse<QuoteBasisEnum>(quoteBasis);
            var trade = FxSingleLegPricer.CreateFxLeg(tradeId, tradeDate, "Party1", "Party2",
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, basis, valueDate, spotRate, forwardRate, forwardPoints);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fxSingleLeg, productType, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.ForeignExchange_Forward));
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fx forward trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        #endregion

        #region FX Swaps

        /// <summary>
        /// Creates an fx swap
        /// </summary>
        /// <param name="tradeId">THe unique trade identifier.</param>
        /// <param name="isCurrency1LenderBase">The isCurrency1LenderBase flag. If [true] then the base party is the lending of currency1.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="currency1Lender">The lender of currency1.</param>
        /// <param name="currency2Lender">The lender of currency2.</param>
        /// <param name="currency1Amount">The currency1 amount.</param>
        /// <param name="currency1">Currency1</param>
        /// <param name="currency2">Currency2</param>
        /// <param name="quoteBasis">The quote basis.</param>
        /// <param name="startValueDate">The start date.</param>
        /// <param name="forwardValueDate">The forward date.</param>
        /// <param name="startRate">The start date rate.</param>
        /// <param name="forwardRate">The forward date rate.</param>
        /// <param name="forwardPoints">The [optional] forward points.</param>
        /// <param name="sourceSystem">The source system.</param>
        /// <returns></returns>
        public string CreateFxSwap(string tradeId, bool isCurrency1LenderBase, DateTime tradeDate, string currency1Lender, string currency2Lender,
            decimal currency1Amount, string currency1, string currency2, string quoteBasis, DateTime startValueDate, DateTime forwardValueDate, 
            Decimal startRate, Decimal forwardRate, Decimal? forwardPoints, string sourceSystem)
        {
            return CreateFxSwap(Logger, Cache, NameSpace, tradeId, isCurrency1LenderBase, tradeDate, currency1Lender, currency2Lender,
            currency1Amount, currency1, currency2, quoteBasis, startValueDate, forwardValueDate, startRate, forwardRate, forwardPoints, sourceSystem);
        }

        /// <summary>
        /// Creates an fx swap
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">THe unique trade identifier.</param>
        /// <param name="isCurrency1LenderBase">The isCurrency1LenderBase flag. If [true] then the base party is the lending of currency1.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="currency1Lender">The lender of currency1.</param>
        /// <param name="currency2Lender">The lender of currency2.</param>
        /// <param name="currency1Amount">The currency1 amount.</param>
        /// <param name="currency1">Currency1</param>
        /// <param name="currency2">Currency2</param>
        /// <param name="quoteBasis">The quote basis.</param>
        /// <param name="startValueDate">The start date.</param>
        /// <param name="forwardValueDate">The forward date.</param>
        /// <param name="startRate">The start date rate.</param>
        /// <param name="forwardRate">The forward date rate.</param>
        /// <param name="forwardPoints">The [optional] forward points.</param>
        /// <param name="sourceSystem">The source system.</param>
        /// <returns></returns>
        public string CreateFxSwap(ILogger logger, ICoreCache cache, String nameSpace, string tradeId, bool isCurrency1LenderBase, DateTime tradeDate, 
            string currency1Lender, string currency2Lender, decimal currency1Amount, string currency1, string currency2, string quoteBasis, 
            DateTime startValueDate, DateTime forwardValueDate, Decimal startRate, Decimal forwardRate, Decimal? forwardPoints, string sourceSystem)
        {
            const ProductTypeSimpleEnum productType = ProductTypeSimpleEnum.FxSwap;
            var basis = EnumHelper.Parse<QuoteBasisEnum>(quoteBasis);
            var properties = new NamedValueSet();
            if (isCurrency1LenderBase)
            {
                properties.Set(TradeProp.Party1, currency1Lender);
                properties.Set(TradeProp.Party2, currency2Lender);
            }
            else
            {
                properties.Set(TradeProp.Party1, currency2Lender);
                properties.Set(TradeProp.Party2, currency1Lender);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, startValueDate);
            properties.Set(TradeProp.MaturityDate, forwardValueDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.FxSwap.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.fxSwap.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.ForeignExchange_Swap));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trade = FxSwapPricer.CreateFxSwap(tradeId, tradeDate, "Party1", "Party2", currency1Amount,
            currency1, currency2, basis, startValueDate, forwardValueDate, startRate, forwardRate, forwardPoints);
            var curves = trade.Item.GetRequiredPricingStructures();
            var currencies = trade.Item.GetRequiredCurrencies();
            properties.Set(TradeProp.RequiredPricingStructures, curves.ToArray());
            properties.Set(TradeProp.RequiredCurrencies, currencies.ToArray());
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fxSwap, productType, tradeId, tradeDate, sourceSystem);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fx swap trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        /// <summary>
        /// Creates an fx swap
        /// </summary>
        /// <param name="tradeId">THe unique trade identifier.</param>
        /// <param name="isCurrency1LenderBase">The isCurrency1LenderBase flag. If [true] then the base party is the lending of currency1.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="currency1Lender">The lender of currency1.</param>
        /// <param name="currency2Lender">The lender of currency2.</param>
        /// <param name="currency1Amount">The currency1 amount.</param>
        /// <param name="currency1">Currency1</param>
        /// <param name="currency2">Currency2</param>
        /// <param name="quoteBasis">The quote basis.</param>
        /// <param name="startValueDate">The start date.</param>
        /// <param name="forwardValueDate">The forward date.</param>
        /// <param name="startRate">The start date rate.</param>
        /// <param name="forwardRate">The forward date rate.</param>
        /// <param name="forwardPoints">The [optional] forward points.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxSwapWithProperties(string tradeId, bool isCurrency1LenderBase, DateTime tradeDate, string currency1Lender,
            string currency2Lender, decimal currency1Amount, string currency1, string currency2, string quoteBasis, DateTime startValueDate, DateTime forwardValueDate,
            Decimal startRate, Decimal forwardRate, Decimal? forwardPoints, NamedValueSet properties)
        {
            return CreateFxSwapWithProperties(Logger, Cache, NameSpace, tradeId, isCurrency1LenderBase, tradeDate, currency1Lender,
            currency2Lender, currency1Amount, currency1, currency2, quoteBasis, startValueDate, forwardValueDate,
            startRate, forwardRate, forwardPoints, properties);
        }

        /// <summary>
        /// Creates an fx swap
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">THe unique trade identifier.</param>
        /// <param name="isCurrency1LenderBase">The isCurrency1LenderBase flag. If [true] then the base party is the lending of currency1.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="currency1Lender">The lender of currency1.</param>
        /// <param name="currency2Lender">The lender of currency2.</param>
        /// <param name="currency1Amount">The currency1 amount.</param>
        /// <param name="currency1">Currency1</param>
        /// <param name="currency2">Currency2</param>
        /// <param name="quoteBasis">The quote basis.</param>
        /// <param name="startValueDate">The start date.</param>
        /// <param name="forwardValueDate">The forward date.</param>
        /// <param name="startRate">The start date rate.</param>
        /// <param name="forwardRate">The forward date rate.</param>
        /// <param name="forwardPoints">The [optional] forward points.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns>The fra trade identifier.</returns>
        public string CreateFxSwapWithProperties(ILogger logger, ICoreCache cache, String nameSpace,
            string tradeId, bool isCurrency1LenderBase, DateTime tradeDate, string currency1Lender,
            string currency2Lender, decimal currency1Amount, string currency1, string currency2, string quoteBasis, DateTime startValueDate, DateTime forwardValueDate,
            Decimal startRate, Decimal forwardRate, Decimal? forwardPoints, NamedValueSet properties)
        {
            const ProductTypeSimpleEnum productType = ProductTypeSimpleEnum.FxSwap;
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var basis = EnumHelper.Parse<QuoteBasisEnum>(quoteBasis);
            var trade = FxSwapPricer.CreateFxSwap(tradeId, tradeDate, "Party1", "Party2", currency1Amount,
            currency1, currency2, basis, startValueDate, forwardValueDate, startRate, forwardRate, forwardPoints);
            var curves = trade.Item.GetRequiredPricingStructures();
            var currencies = trade.Item.GetRequiredCurrencies();
            properties.Set(TradeProp.RequiredPricingStructures, curves.ToArray());
            properties.Set(TradeProp.RequiredCurrencies, currencies.ToArray());
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fxSwap, productType, tradeId, tradeDate, sourceSystem);
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.ForeignExchange_Swap));
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fx swap trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        #endregion

        #region Forward Rate Agreements

        #region Solvers

        /// <summary>
        /// returns an array of rates
        /// </summary>
        /// <returns></returns>
        public double[] CalculateFraEquivalents(NamedValueSet properties, List<string> instruments, IEnumerable<decimal> rates, List<object> fraGuesses)
        {
            var initialGuesses = new List<decimal>();
            var indicies = new List<int>();
            FraSolver.GetFraGuesses(fraGuesses, ref initialGuesses, ref indicies);
            var fraSolver = new FraSolver(Logger, Cache, NameSpace, null, null, properties, instruments, rates, initialGuesses, indicies.ToArray(), null);
            var adjustments = fraSolver.CalculateEquivalentFraValues(Logger, Cache, NameSpace);
            return adjustments.Select(a => Convert.ToDouble(a)*10000).ToArray();
        }

        #endregion

        #region Configuration Based

        /// <summary>
        /// Builds a fra trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="baseDate">The trade date.</param>
        /// <param name="isParty1Buyer">The isParty1Buyer flag.</param>
        /// <param name="party1">The party1 name.</param>
        /// <param name="party2">The party2 name.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="startTenor">The start tenor: based from the spot date.</param>
        /// <param name="indexTenor">The inindex tenor - this is relative to the base date as well!</param>
        /// <param name="discountingType">The discounting type: ISDA or AFMA.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="floatingRateIndex">The [Optional] floating rate index. If not providsed the default will be used.</param>
        /// <param name="sourceSystem">The source system.</param>
        /// <returns></returns>
        public string CreateFra(string tradeId, DateTime baseDate, bool isParty1Buyer, string party1, string party2, string currency, decimal notionalAmount, string startTenor,
            string indexTenor, string discountingType, string sourceSystem, decimal fixedRate, string floatingRateIndex)
        {
            return CreateFra(Logger, Cache, NameSpace, tradeId, baseDate, isParty1Buyer, party1, party2, currency, notionalAmount, startTenor,
            indexTenor, discountingType, sourceSystem, fixedRate, floatingRateIndex);
        }

        /// <summary>
        /// Builds a fra trade using configuration data.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="isParty1Buyer">The isParty1Buyer flag.</param>
        /// <param name="party1">The party1 name.</param>
        /// <param name="party2">The party2 name.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="startTenor">The start tenor: based from the spot date.</param>
        /// <param name="indexTenor">The inindex tenor - this is relative to the base date as well!</param>
        /// <param name="discountingType">The discounting type: ISDA or AFMA.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="floatingRateIndex">The [Optional] floating rate index. If not providsed the default will be used.</param>
        /// <param name="sourceSystem">THe source system</param>
        /// <returns></returns>
        public string CreateFra(ILogger logger, ICoreCache cache, String nameSpace,
            string tradeId, DateTime tradeDate, bool isParty1Buyer, string party1, string party2, 
            string currency, decimal notionalAmount, string startTenor, string indexTenor,
            string discountingType, string sourceSystem, decimal fixedRate, string floatingRateIndex)
        {
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fra, ProductTypeSimpleEnum.FRA, tradeId, tradeDate, sourceSystem);
                var properties = new NamedValueSet();
                properties.Set(TradeProp.Party1, party1);
                properties.Set(TradeProp.Party2, party2);
                if (isParty1Buyer)
                {
                    properties.Set(TradeProp.BaseParty, TradeProp.Party1);//party1
                    properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);//party2
                }
                else
                {
                    properties.Set(TradeProp.BaseParty, TradeProp.Party2);//party2
                    properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);//party1
                }
                properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
                properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
                properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
                properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.FRA.ToString());
                properties.Set(TradeProp.TradeType, ItemChoiceType15.fra.ToString());
                properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_FRA));
                properties.Set(TradeProp.TradeDate, tradeDate);
                properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
                properties.Set(TradeProp.TradeSource, sourceSystem);
                properties.Set(TradeProp.TradeId, tradeId);
                properties.Set(TradeProp.AsAtDate, DateTime.Today);
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                const string result = "Trade has not been created due to dodgey data!";
                var assetId = currency + "-FRA-" + startTenor + "-" + indexTenor;
                var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(Cache, NameSpace, assetId);
            if (instrument.InstrumentNodeItem is SimpleFraNodeStruct nodeStruct)
            {
                var fri = nodeStruct.RateIndex.floatingRateIndex.Value;
                if (!string.IsNullOrEmpty(floatingRateIndex))
                {
                    fri = floatingRateIndex;
                }
                var fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote");
                nodeStruct.SimpleFra.startTerm = PeriodHelper.Parse(startTenor);
                nodeStruct.SimpleFra.endTerm = nodeStruct.SimpleFra.startTerm.Sum(PeriodHelper.Parse(indexTenor));
                var fra = new CurveEngine.V5r3.Assets.Rates.Fra.PriceableSimpleFra(tradeDate, nodeStruct, fixingCalendar, paymentCalendar, notionalAmount, quote);  //The model in this is not correct!  
                var adjustedEffectiveDate = DateTypesHelper.ToRequiredIdentifierDate(fra.AdjustedStartDate);
                properties.Set(TradeProp.EffectiveDate, fra.AdjustedStartDate);
                var paymentDate = new AdjustableDate
                {
                    dateAdjustments = nodeStruct.BusinessDayAdjustments,
                    unadjustedDate = IdentifiedDateHelper.Create(fra.AdjustedStartDate)
                };
                var fixingDayOffset = nodeStruct.SpotDate;
                fixingDayOffset.dateRelativeTo = new DateReference { href = "resetDate" };
                var fraDiscounting = EnumHelper.Parse<FraDiscountingEnum>(discountingType);
                properties.Set(TradeProp.EffectiveDate, fra.AdjustedStartDate);
                properties.Set(TradeProp.MaturityDate, fra.RiskMaturityDate);
                var trade = FraPricer.CreateFraTrade(tradeId, adjustedEffectiveDate, fra.RiskMaturityDate, paymentDate, fixingDayOffset, nodeStruct.SimpleFra.dayCountFraction,
                    notionalAmount, currency, fixedRate, fri, indexTenor, fraDiscounting);
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.NameSpace, nameSpace);
                var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
                cache.SaveObject(trade, uniqueIdentifier, properties);
                logger.LogDebug("Created new fra trade: {0}", identifier.UniqueIdentifier);
                return identifier.UniqueIdentifier;
            }
            return result;
        }

        /// <summary>
        /// Builds a fra trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="baseDate">The trade date.</param>
        /// <param name="isParty1Buyer">The isParty1Buyer flag.</param>
        /// <param name="party1">The party1 name.</param>
        /// <param name="party2">The party2 name.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="startTenor">The start tenor: based from the spot date.</param>
        /// <param name="indexTenor">The inindex tenor - this is relative to the base date as well!</param>
        /// <param name="discountingType">The discounting type: ISDA or AFMA.</param>
        /// <param name="sourceSystem">THe source System</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="floatingRateIndex">The [Optional] floating rate index. If not providsed the default will be used.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string CreateFraWithProperties(string tradeId, DateTime baseDate, bool isParty1Buyer, string party1, 
            string party2, string currency, decimal notionalAmount, string startTenor, string indexTenor, string discountingType, 
            string sourceSystem, decimal fixedRate, string floatingRateIndex, NamedValueSet properties)
        {
            return CreateFraWithProperties(Logger, Cache, NameSpace, tradeId, baseDate, isParty1Buyer, party1, party2, currency, notionalAmount, startTenor,
            indexTenor, discountingType, sourceSystem, fixedRate, floatingRateIndex, properties);
        }

        /// <summary>
        /// Builds a fra trade using configuration data.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="isParty1Buyer">The isParty1Buyer flag.</param>
        /// <param name="party1">The party1 name.</param>
        /// <param name="party2">The party2 name.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="startTenor">The start tenor: based from the spot date.</param>
        /// <param name="indexTenor">The inindex tenor - this is relative to the base date as well!</param>
        /// <param name="discountingType">The discounting type: ISDA or AFMA.</param>
        /// <param name="sourceSystem">The source system</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="floatingRateIndex">The [Optional] floating rate index. If not providsed the default will be used.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string CreateFraWithProperties(ILogger logger, ICoreCache cache, String nameSpace,
            string tradeId, DateTime tradeDate, bool isParty1Buyer, string party1, string party2, string currency, decimal notionalAmount, string startTenor,
            string indexTenor, string discountingType, string sourceSystem, decimal fixedRate, string floatingRateIndex, NamedValueSet properties)
        {
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fra, ProductTypeSimpleEnum.FRA, tradeId, tradeDate, sourceSystem);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.FRA.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.fra.ToString());
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_FRA));
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            const string result = "Trade has not been created due to dodgey data!";
            var assetId = currency + "-FRA-" + startTenor + "-" + indexTenor;
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(Cache, NameSpace, assetId);
            if (instrument.InstrumentNodeItem is SimpleFraNodeStruct nodeStruct)
            {
                var fri = nodeStruct.RateIndex.floatingRateIndex.Value;
                if (!string.IsNullOrEmpty(floatingRateIndex))
                {
                    fri = floatingRateIndex;
                }
                var fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.BusinessDayAdjustments.businessCenters, nameSpace);
                var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote");
                nodeStruct.SimpleFra.startTerm = PeriodHelper.Parse(startTenor);
                nodeStruct.SimpleFra.endTerm = nodeStruct.SimpleFra.startTerm.Sum(PeriodHelper.Parse(indexTenor));
                var fra = new CurveEngine.V5r3.Assets.Rates.Fra.PriceableSimpleFra(tradeDate, nodeStruct, fixingCalendar, paymentCalendar, notionalAmount, quote);  //The model in this is not correct!  
                var adjustedEffectiveDate = DateTypesHelper.ToRequiredIdentifierDate(fra.AdjustedStartDate);
                properties.Set(TradeProp.EffectiveDate, fra.AdjustedStartDate);
                var paymentDate = new AdjustableDate
                {
                    dateAdjustments = nodeStruct.BusinessDayAdjustments,
                    unadjustedDate = IdentifiedDateHelper.Create(fra.AdjustedStartDate)
                };
                var fixingDayOffset = nodeStruct.SpotDate;
                fixingDayOffset.dateRelativeTo = new DateReference { href = "resetDate" };
                var fraDiscounting = EnumHelper.Parse<FraDiscountingEnum>(discountingType);
                properties.Set(TradeProp.MaturityDate, fra.RiskMaturityDate);
                var trade = FraPricer.CreateFraTrade(tradeId, adjustedEffectiveDate, fra.RiskMaturityDate, paymentDate, fixingDayOffset, nodeStruct.SimpleFra.dayCountFraction,
                    notionalAmount, currency, fixedRate, fri, indexTenor, fraDiscounting);
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                properties.Set(EnvironmentProp.NameSpace, nameSpace);
                var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
                cache.SaveObject(trade, uniqueIdentifier, properties);
                logger.LogDebug("Created new fra trade: {0}", identifier.UniqueIdentifier);
                return identifier.UniqueIdentifier;
            }
            return result;
        }

        #endregion

        #region Detailed Fra

        /// <summary>
        ///  Creates a Fra trade and caches it.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The trade id.</param>
        ///  <param name="tradeDate">The trade date.</param>
        ///  <param name="isParty1Buyer">The isParty1Buyer flag.</param>
        ///  <param name="party1">The party1 name.</param>
        ///  <param name="party2">The party2 name.</param>
        ///  <param name="adjustedEffectiveDate">The effective date.</param>
        ///  <param name="adjustedTerminationDate">The termination date.</param>
        ///  <param name="paymentDate">The payment date. Can be unadjusted.</param>
        ///  <param name="paymentDateBusinessDayConvention">The payment date business day convention.</param>
        ///  <param name="paymentDateBusinessCenters">The payment date business centers.</param>
        ///  <param name="fixingDayOffset">The fixing date offset. e.g. -2d.</param>
        ///  <param name="fixingDayOffsetDayType">THe offset type: calendar or business.</param>
        ///  <param name="fixingDayOffsetBusinessDayConvention">The fixing date business day convention.</param>
        ///  <param name="fixingDayOffsetBusinessCenters">The fixing date business centers.</param>
        ///  <param name="fixingDayOffsetDateRelativeTo"></param>
        ///  <param name="dayCountFraction">The day count fraction.</param>
        ///  <param name="notionalAmount">The notional amount.</param>
        ///  <param name="notionalCurrency">The currency.</param>
        ///  <param name="fixedRate">The fixed rate.</param>
        ///  <param name="floatingRateIndex">The floating rate index. This is used to value the trade by using the associated curve.</param>
        ///  <param name="indexTenor">The index tenor.</param>
        ///  <param name="fraDiscountingType">ISDA or AFMA.</param>
        ///  <remarks>
        ///  All the values in the input range must be provided as specified, otherwise the data 
        ///  will not be handled properly and #value will be returned.
        ///  </remarks>
        /// <returns>The Id the trade is cached under.</returns>
        public string CreateFra(ILogger logger, ICoreCache cache, String nameSpace,
            string tradeId, DateTime tradeDate, bool isParty1Buyer, string party1, string party2, DateTime adjustedEffectiveDate, DateTime adjustedTerminationDate,
            DateTime paymentDate, string paymentDateBusinessDayConvention, string paymentDateBusinessCenters, string fixingDayOffset, string fixingDayOffsetDayType, string fixingDayOffsetBusinessDayConvention,
            string fixingDayOffsetBusinessCenters, string fixingDayOffsetDateRelativeTo, string dayCountFraction, decimal notionalAmount, string notionalCurrency, decimal fixedRate, string floatingRateIndex,
            string indexTenor, string fraDiscountingType)
        {
            var fraDiscounting = EnumHelper.Parse<FraDiscountingEnum>(fraDiscountingType);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fra, ProductTypeSimpleEnum.FRA, tradeId);
            var properties = new NamedValueSet();
            properties.Set(TradeProp.Party1, party1);
            properties.Set(TradeProp.Party2, party2);
            if (isParty1Buyer)
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);//party1
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);//party2
            }
            else
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party2);//party2
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);//party1
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, adjustedEffectiveDate);
            properties.Set(TradeProp.MaturityDate, adjustedTerminationDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.FRA.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.fra.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_FRA));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trade = FraPricer.CreateFraTrade(tradeId, adjustedEffectiveDate, adjustedTerminationDate, paymentDate, paymentDateBusinessDayConvention,
                paymentDateBusinessCenters, fixingDayOffset, fixingDayOffsetDayType, fixingDayOffsetBusinessDayConvention, fixingDayOffsetBusinessCenters, fixingDayOffsetDateRelativeTo,
                dayCountFraction, notionalAmount, notionalCurrency, fixedRate, floatingRateIndex, indexTenor, fraDiscounting);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fra trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        /// Creates a Fra trade and caches it.
        ///</summary>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="adjustedEffectiveDate">The effective date.</param>
        /// <param name="adjustedTerminationDate">The termination date.</param>
        /// <param name="paymentDate">The payment date. Can be unadjusted.</param>
        /// <param name="paymentDateBusinessDayConvention">The payment date business day convention.</param>
        /// <param name="paymentDateBusinessCenters">The payment date business centers.</param>
        /// <param name="fixingDayOffset">The fixing date offset. e.g. -2d.</param>
        /// <param name="fixingDayOffsetDayType">THe offset type: calendar or business.</param>
        /// <param name="fixingDayOffsetBusinessDayConvention">The fixing date business day convention.</param>
        /// <param name="fixingDayOffsetBusinessCenters">The fixing date business centers.</param>
        /// <param name="fixingDayOffsetDateRelativeTo"></param>
        /// <param name="dayCountFraction">The day count fraction.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="notionalCurrency">The currency.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="floatingRateIndex">The floating rate index. This is used to value the trade by using the associated curve.</param>
        /// <param name="indexTenor">The index tenor.</param>
        /// <param name="fraDiscountingType">ISDA or AFMA.</param>
        /// <param name="properties">The properties. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <remarks>
        /// All the values in the input range must be provided as specified, otherwise the data 
        /// will not be handled properly and #value will be returned.
        /// </remarks>
        ///<returns>The Id the trade is cached under.</returns>
        public string CreateFraWithProperties(string tradeId, DateTime tradeDate, DateTime adjustedEffectiveDate, DateTime adjustedTerminationDate,
            DateTime paymentDate, string paymentDateBusinessDayConvention, string paymentDateBusinessCenters, string fixingDayOffset, string fixingDayOffsetDayType, string fixingDayOffsetBusinessDayConvention,
            string fixingDayOffsetBusinessCenters, string fixingDayOffsetDateRelativeTo, string dayCountFraction, decimal notionalAmount, string notionalCurrency, decimal fixedRate, string floatingRateIndex,
            string indexTenor, string fraDiscountingType, NamedValueSet properties)
        {
            return CreateFraWithProperties(Logger, Cache, NameSpace, tradeId, tradeDate, adjustedEffectiveDate, adjustedTerminationDate,
            paymentDate, paymentDateBusinessDayConvention, paymentDateBusinessCenters, fixingDayOffset, fixingDayOffsetDayType, fixingDayOffsetBusinessDayConvention,
            fixingDayOffsetBusinessCenters, fixingDayOffsetDateRelativeTo, dayCountFraction, notionalAmount, notionalCurrency, fixedRate, floatingRateIndex,
            indexTenor, fraDiscountingType, properties);
        }

        /// <summary>
        ///  Creates a Fra trade and caches it.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="tradeId">The trade id.</param>
        ///  <param name="tradeDate">The trade date.</param>
        /// <param name="adjustedEffectiveDate">The effective date.</param>
        ///  <param name="adjustedTerminationDate">The termination date.</param>
        ///  <param name="paymentDate">The payment date. Can be unadjusted.</param>
        ///  <param name="paymentDateBusinessDayConvention">The payment date business day convention.</param>
        ///  <param name="paymentDateBusinessCenters">The payment date business centers.</param>
        ///  <param name="fixingDayOffset">The fixing date offset. e.g. -2d.</param>
        ///  <param name="fixingDayOffsetDayType">THe offset type: calendar or business.</param>
        ///  <param name="fixingDayOffsetBusinessDayConvention">The fixing date business day convention.</param>
        ///  <param name="fixingDayOffsetBusinessCenters">The fixing date business centers.</param>
        ///  <param name="fixingDayOffsetDateRelativeTo"></param>
        ///  <param name="dayCountFraction">The day count fraction.</param>
        ///  <param name="notionalAmount">The notional amount.</param>
        ///  <param name="notionalCurrency">The currency.</param>
        ///  <param name="fixedRate">The fixed rate.</param>
        ///  <param name="floatingRateIndex">The floating rate index. This is used to value the trade by using the associated curve.</param>
        ///  <param name="indexTenor">The index tenor.</param>
        ///  <param name="fraDiscountingType">ISDA or AFMA.</param>
        ///  <param name="properties">The properties. This must contain:
        ///  TradeId - the trade Id w/o the trade type.
        ///  Party1 - the lender party 
        ///  Party2 - the borrower party.
        ///  This is used to determine the base direction in calculations</param>
        ///  <remarks>
        ///  All the values in the input range must be provided as specified, otherwise the data 
        ///  will not be handled properly and #value will be returned.
        ///  </remarks>
        /// <returns>The Id the trade is cached under.</returns>
        public string CreateFraWithProperties(ILogger logger, ICoreCache cache, String nameSpace,
            string tradeId, DateTime tradeDate, DateTime adjustedEffectiveDate, DateTime adjustedTerminationDate,
            DateTime paymentDate, string paymentDateBusinessDayConvention, string paymentDateBusinessCenters, string fixingDayOffset, string fixingDayOffsetDayType, string fixingDayOffsetBusinessDayConvention,
            string fixingDayOffsetBusinessCenters, string fixingDayOffsetDateRelativeTo, string dayCountFraction, decimal notionalAmount, string notionalCurrency, decimal fixedRate, string floatingRateIndex,
            string indexTenor, string fraDiscountingType, NamedValueSet properties)
        {
            var fraDiscounting = EnumHelper.Parse<FraDiscountingEnum>(fraDiscountingType);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fra, ProductTypeSimpleEnum.FRA, tradeId);
            var trade = FraPricer.CreateFraTrade(tradeId, adjustedEffectiveDate, adjustedTerminationDate, paymentDate, paymentDateBusinessDayConvention,
                paymentDateBusinessCenters, fixingDayOffset, fixingDayOffsetDayType, fixingDayOffsetBusinessDayConvention, fixingDayOffsetBusinessCenters, fixingDayOffsetDateRelativeTo,
                dayCountFraction, notionalAmount, notionalCurrency, fixedRate, floatingRateIndex, indexTenor, fraDiscounting);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_FRA));
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fra trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        #endregion

        #region TypeSafe Version

        /// <summary>
        ///  Creates a Fra trade and caches it locally.
        /// </summary>
        /// <param name="fraInputRange">An Nx2 range in Excel, with the first column a valid list of properties for a Fra.
        ///  properties for a Fra.
        ///  The required values are:
        ///  <para>TradeId	0001</para>
        ///  <para>Party1	CBA</para>
        ///  <para>Party2	NAB</para>
        ///  <para>IsParty1Buyer	TRUE</para>
        ///  <para>TradeDate	11/06/2010</para>
        ///  <para>AdjustedResetDate	11/06/2010</para>
        ///  <para>AdjustedTerminationDate	10/09/2010</para>
        ///  <para>PaymentDate-UnadjustedDate	11/06/2010</para>
        ///  <para>PaymentDate-BusinessDayConvention	FOLLOWING</para>
        ///  <para>PaymentDate-BusinessCenters	AUSY</para>
        ///  <para>FixingDayOffset-Period	-2d</para>
        ///  <para>FixingDayOffset-DayType	Business</para>
        ///  <para>FixingDayOffset-BusinessDayConvention	NONE</para>
        ///  <para>FixingDayOffset-BusinessCenters 	AUSY-GBLO</para>
        ///  <para>FixingDayOffset-DateRelativeTo	resetDate</para>
        ///  <para>DayCountFraction	ACT/360</para>
        ///  <para>notional-amount	10000000.00</para>
        ///  <para>notional-currency	AUD</para>
        ///  <para>fixedRate	0.0648000</para>
        ///  <para>floatingRateIndex	AUD-BBR-ISDC</para>
        ///  <para>indexTenor	6m</para>
        ///  <para>fraDiscounting	ISDA</para>
        ///  </param>
        /// <param name="sourceSystem">THe source system</param>
        /// <remarks>
        ///  All the values in the input range must be provided as specified, otherwise the data 
        ///  will not be handled properly and #value will be returned.
        ///  </remarks>
        /// <returns>The Id the trade is cached under.</returns>
        public string CreateFraTrade(FraInputRange2 fraInputRange, string sourceSystem)
        {
            return CreateFraTrade(Logger, Cache, NameSpace, fraInputRange, sourceSystem);
        }

        /// <summary>
        ///  Creates a Fra trade and caches it locally.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fraInputRange">An Nx2 range in Excel, with the first column a valid list of properties for a Fra.
        ///  properties for a Fra.
        ///  The required values are:
        ///  <para>TradeId	0001</para>
        ///  <para>Party1	CBA</para>
        ///  <para>Party2	NAB</para>
        ///  <para>IsParty1Buyer	TRUE</para>
        ///  <para>TradeDate	11/06/2010</para>
        ///  <para>AdjustedResetDate	11/06/2010</para>
        ///  <para>AdjustedTerminationDate	10/09/2010</para>
        ///  <para>PaymentDate-UnadjustedDate	11/06/2010</para>
        ///  <para>PaymentDate-BusinessDayConvention	FOLLOWING</para>
        ///  <para>PaymentDate-BusinessCenters	AUSY</para>
        ///  <para>FixingDayOffset-Period	-2d</para>
        ///  <para>FixingDayOffset-DayType	Business</para>
        ///  <para>FixingDayOffset-BusinessDayConvention	NONE</para>
        ///  <para>FixingDayOffset-BusinessCenters 	AUSY-GBLO</para>
        ///  <para>FixingDayOffset-DateRelativeTo	resetDate</para>
        ///  <para>DayCountFraction	ACT/360</para>
        ///  <para>notional-amount	10000000.00</para>
        ///  <para>notional-currency	AUD</para>
        ///  <para>fixedRate	0.0648000</para>
        ///  <para>floatingRateIndex	AUD-BBR-ISDC</para>
        ///  <para>indexTenor	6m</para>
        ///  <para>fraDiscounting	ISDA</para>
        ///  </param>
        /// <param name="sourceSystem">The source system.</param>
        /// <remarks>
        ///  All the values in the input range must be provided as specified, otherwise the data 
        ///  will not be handled properly and #value will be returned.
        ///  </remarks>
        /// <returns>The Id the trade is cached under.</returns>
        public string CreateFraTrade(ILogger logger, ICoreCache cache, String nameSpace, FraInputRange2 fraInputRange, string sourceSystem)
        {
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fra, ProductTypeSimpleEnum.FRA, fraInputRange.TradeId, fraInputRange.TradeDate, sourceSystem);
            //new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fxSwap, productType, tradeId, tradeDate, sourceSystem);
            var properties = new NamedValueSet();
            properties.Set(TradeProp.Party1, fraInputRange.Party1);
            properties.Set(TradeProp.Party2, fraInputRange.Party2);
            if (bool.Parse(fraInputRange.IsParty1Buyer))
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);//party1
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);//party2
            }
            else
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party2);//party2
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);//party1
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, fraInputRange.AdjustedEffectiveDate);
            properties.Set(TradeProp.MaturityDate, fraInputRange.AdjustedTerminationDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.FRA.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.fra.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_FRA));
            properties.Set(TradeProp.TradeDate, fraInputRange.TradeDate);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.TradeId, fraInputRange.TradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var trade = FraPricer.CreateFraTrade(fraInputRange);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fra trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        ///  Creates a fra trade and caches it locally.
        ///</summary>
        ///<param name="fraInputRange">An Nx2 range in Excel, with the first column a valid list of properties for a Fra.
        /// properties for a Fra.
        /// The required values are:
        ///  <para>TradeId	0001</para>
        /// <para>Party1	CBA</para>
        /// <para>Party2	NAB</para>
        /// <para>IsParty1Buyer	TRUE</para>
        /// <para>TradeDate	11/06/2010</para>
        /// <para>AdjustedResetDate	11/06/2010</para>
        /// <para>AdjustedTerminationDate	10/09/2010</para>
        /// <para>PaymentDate-UnadjustedDate	11/06/2010</para>
        /// <para>PaymentDate-BusinessDayConvention	FOLLOWING</para>
        /// <para>PaymentDate-BusinessCenters	AUSY</para>
        /// <para>FixingDayOffset-Period	-2d</para>
        /// <para>FixingDayOffset-DayType	Business</para>
        /// <para>FixingDayOffset-BusinessDayConvention	NONE</para>
        /// <para>FixingDayOffset-BusinessCenters 	AUSY-GBLO</para>
        /// <para>FixingDayOffset-DateRelativeTo	resetDate</para>
        /// <para>DayCountFraction	ACT/360</para>
        /// <para>notional-amount	10000000.00</para>
        /// <para>notional-currency	AUD</para>
        /// <para>fixedRate	0.0648000</para>
        /// <para>floatingRateIndex	AUD-BBR-ISDC</para>
        /// <para>indexTenor	6m</para>
        /// <para>fraDiscounting	ISDA</para>
        /// </param>
        /// <remarks>
        /// All the values in the input range must be provided as specified, otherwise the data 
        /// will not be handled properly and #value will be returned.
        /// </remarks>
        ///<param name="properties">A list of properties, including mandatory ones.</param>
        ///<returns>The Id the valuation report is cached under.</returns>
        public string CreateFraTradeWithProperties(FraInputRange2 fraInputRange, NamedValueSet properties)
        {
            return CreateFraTradeWithProperties(Logger, Cache, NameSpace, fraInputRange, properties);
        }

        /// <summary>
        ///   Creates a fra trade and caches it locally.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fraInputRange">An Nx2 range in Excel, with the first column a valid list of properties for a Fra.
        ///  properties for a Fra.
        ///  The required values are:
        ///   <para>TradeId	0001</para>
        ///  <para>Party1	CBA</para>
        ///  <para>Party2	NAB</para>
        ///  <para>IsParty1Buyer	TRUE</para>
        ///  <para>TradeDate	11/06/2010</para>
        ///  <para>AdjustedResetDate	11/06/2010</para>
        ///  <para>AdjustedTerminationDate	10/09/2010</para>
        ///  <para>PaymentDate-UnadjustedDate	11/06/2010</para>
        ///  <para>PaymentDate-BusinessDayConvention	FOLLOWING</para>
        ///  <para>PaymentDate-BusinessCenters	AUSY</para>
        ///  <para>FixingDayOffset-Period	-2d</para>
        ///  <para>FixingDayOffset-DayType	Business</para>
        ///  <para>FixingDayOffset-BusinessDayConvention	NONE</para>
        ///  <para>FixingDayOffset-BusinessCenters 	AUSY-GBLO</para>
        ///  <para>FixingDayOffset-DateRelativeTo	resetDate</para>
        ///  <para>DayCountFraction	ACT/360</para>
        ///  <para>notional-amount	10000000.00</para>
        ///  <para>notional-currency	AUD</para>
        ///  <para>fixedRate	0.0648000</para>
        ///  <para>floatingRateIndex	AUD-BBR-ISDC</para>
        ///  <para>indexTenor	6m</para>
        ///  <para>fraDiscounting	ISDA</para>
        ///  </param>
        ///  <remarks>
        ///  All the values in the input range must be provided as specified, otherwise the data 
        ///  will not be handled properly and #value will be returned.
        ///  </remarks>
        /// <param name="properties">A list of properties, including mandatory ones.</param>
        /// <returns>The Id the valuation report is cached under.</returns>
        public string CreateFraTradeWithProperties(ILogger logger, ICoreCache cache, String nameSpace, FraInputRange2 fraInputRange, NamedValueSet properties)
        {
            var tradeIdentifier = new Reporting.Identifiers.V5r3.TradeIdentifier(properties);
            var trade = FraPricer.CreateFraTrade(fraInputRange);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_FRA));
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIdentifier = nameSpace + "." + tradeIdentifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIdentifier, properties);
            logger.LogDebug("Created new fra trade: {0}", tradeIdentifier.UniqueIdentifier);
            return tradeIdentifier.UniqueIdentifier;
        }

        #endregion

        #endregion

        #region Interest Rate Swaps

        #region Configuration Based

        /// <summary>
        /// Builds a swap trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The tradeId.</param>
        /// <param name="tradeDate">The tradeDate.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="leg1">The leg1 details.</param>
        /// <param name="leg2">The leg2 details.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, paryy1, party2, isParty1FixedPayer etc</param>
        /// <returns></returns>
        public string CreateIRSwap(string tradeId, DateTime tradeDate,
            AdjustedType adjustedType, bool isParty1Base, DateTime effectiveDate, string currency,
            decimal notionalAmount, string maturityTenor, SwapLegSimpleRange leg1, SwapLegSimpleRange leg2,
            NamedValueSet properties)
        {
            return CreateIRSwap(Logger, Cache, NameSpace, tradeId, tradeDate, adjustedType, isParty1Base, effectiveDate, currency, notionalAmount, maturityTenor,
            leg1, leg2, properties);
        }

        /// <summary>
        /// Builds a swap trade using configuration data.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The tradeId.</param>
        /// <param name="tradeDate">The tradeDate.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="leg1">The leg1 details.</param>
        /// <param name="leg2">The leg2 details.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, paryy1, party2, isParty1FixedPayer etc</param>
        /// <returns></returns>
        public string CreateIRSwap(ILogger logger, ICoreCache cache, String nameSpace, string tradeId, DateTime tradeDate,
            AdjustedType adjustedType, bool isParty1Base, DateTime effectiveDate, string currency, 
            decimal notionalAmount, string maturityTenor, SwapLegSimpleRange leg1, SwapLegSimpleRange leg2, 
            NamedValueSet properties)
        {
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swap, ProductTypeSimpleEnum.InterestRateSwap, tradeId, tradeDate, sourceSystem);
            string payer = "Party1";
            string receiver= "Party2";
            if (!isParty1Base)
            {
                payer = "Party2";
                receiver = "Party1";
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.InterestRateSwap.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.swap.ToString());
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            const string result = "Trade has not been created due to dodgey data!";
            var assetId = currency + "-IRSwap-" + maturityTenor;
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(Cache, nameSpace, assetId);
            if (instrument.InstrumentNodeItem is SimpleIRSwapNodeStruct nodeStruct)
            {
                //Shared parameters
                var fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                var forecastIndexName = nodeStruct.UnderlyingRateIndex.floatingRateIndex.Value;
                var fixingBusinessDayAdjustments = nodeStruct.SpotDate.businessDayConvention.ToString();
                var paymentCenters = BusinessCentersHelper.BusinessCentersString(nodeStruct.DateAdjustments.businessCenters.businessCenter);
                var fixingCenters = BusinessCentersHelper.BusinessCentersString(nodeStruct.SpotDate.businessCenters.businessCenter);
                Period rollPeriod = PeriodHelper.Parse(maturityTenor);
                var maturityDate = paymentCalendar.Advance(effectiveDate, OffsetHelper.FromInterval(rollPeriod, DayTypeEnum.Calendar), nodeStruct.DateAdjustments.businessDayConvention);
                //The first Leg: normally fixed
                //The fixed Leg
                var leg1ParametersRange = new SwapLegParametersRange
                {
                    Payer = leg1.Payer,
                    Receiver = leg1.Payer == payer ? receiver : payer,
                    EffectiveDate = effectiveDate,
                    AdjustedType = adjustedType,
                    NotionalAmount = notionalAmount
                };
                leg1ParametersRange.Payer = payer;
                leg1ParametersRange.Receiver = receiver;
                leg1ParametersRange.DayCount = nodeStruct.SimpleIRSwap.dayCountFraction.Value;
                leg1ParametersRange.LegType = leg1.LegType;
                leg1ParametersRange.CouponPaymentType = CouponPaymentType.InArrears;
                if ((leg1.LegType == LegType.Floating))
                {
                    leg1ParametersRange.ForecastIndexName = forecastIndexName;
                    leg1ParametersRange.FixingCalendar = fixingCenters;
                    leg1ParametersRange.FixingBusinessDayAdjustments = fixingBusinessDayAdjustments;
                    leg1ParametersRange.FloatingRateSpread = leg1.Spread;
                    if (!string.IsNullOrEmpty(leg1.ForecastIndexName))
                    {
                        leg1ParametersRange.ForecastIndexName = leg1.ForecastIndexName;
                    }
                }
                //leg1ParametersRange.DiscountingType =;
                leg1ParametersRange.FirstCouponType = leg1.FirstCouponType;
                leg1ParametersRange.Currency = currency;
                leg1ParametersRange.CouponOrLastResetRate = leg1.Rate;
                leg1ParametersRange.FloatingRateSpread = leg1.Spread;
                leg1ParametersRange.MaturityDate = maturityDate;
                leg1ParametersRange.FirstRegularPeriodStartDate = leg1.FirstRegularPeriodStartDate;
                leg1ParametersRange.PaymentBusinessDayAdjustments = nodeStruct.DateAdjustments.businessDayConvention.ToString();
                leg1ParametersRange.RollConvention = leg1.RollConvention;
                leg1ParametersRange.PaymentFrequency = leg1.PaymentFrequency;
                leg1ParametersRange.LastRegularPeriodEndDate = DateTime.MinValue;
                leg1ParametersRange.PaymentCalendar = paymentCenters;
                //The floating leg.
                var leg2ParametersRange = new SwapLegParametersRange
                {
                    Payer = leg2.Payer,
                    Receiver = leg2.Payer == payer ? receiver : payer,
                    EffectiveDate = effectiveDate,
                    AdjustedType = adjustedType,
                    NotionalAmount = notionalAmount
                };
                leg2ParametersRange.Payer = receiver;
                leg2ParametersRange.Receiver = payer;
                if (leg2.LegType == LegType.Floating)
                {
                    leg2ParametersRange.ForecastIndexName = forecastIndexName;
                    leg2ParametersRange.FixingCalendar = fixingCenters;
                    leg2ParametersRange.FixingBusinessDayAdjustments = fixingBusinessDayAdjustments;
                    leg2ParametersRange.FloatingRateSpread = leg2.Spread;
                    if (!string.IsNullOrEmpty(leg2.ForecastIndexName))
                    {
                        leg2ParametersRange.ForecastIndexName = leg2.ForecastIndexName;
                    }
                }
                leg2ParametersRange.FloatingRateSpread = leg2.Spread;
                leg2ParametersRange.DayCount = nodeStruct.Calculation.dayCountFraction.Value;
                leg2ParametersRange.LegType = leg2.LegType;
                leg2ParametersRange.CouponPaymentType = CouponPaymentType.InArrears;
                leg2ParametersRange.FirstCouponType = leg2.FirstCouponType;
                leg2ParametersRange.Currency = currency;
                leg2ParametersRange.MaturityDate = maturityDate;
                leg2ParametersRange.FirstRegularPeriodStartDate = leg2.FirstRegularPeriodStartDate;
                leg2ParametersRange.PaymentBusinessDayAdjustments = nodeStruct.DateAdjustments.businessDayConvention.ToString();
                leg2ParametersRange.RollConvention = leg2.RollConvention;
                leg2ParametersRange.PaymentFrequency = leg2.PaymentFrequency;
                leg2ParametersRange.LastRegularPeriodEndDate = DateTime.MinValue;
                leg2ParametersRange.FixingBusinessDayAdjustments = nodeStruct.SpotDate.businessDayConvention.ToString();
                leg2ParametersRange.PaymentCalendar = paymentCenters;
                leg2ParametersRange.CouponOrLastResetRate = leg2.Rate;
                leg2ParametersRange.FloatingRateSpread = leg2.Spread;
                properties.Set(TradeProp.EffectiveDate, effectiveDate);
                properties.Set(TradeProp.MaturityDate, maturityDate);
                //Create the swap.
                var trade = InterestRateSwapPricer.CreateTrade(leg1ParametersRange, fixingCalendar, paymentCalendar, leg2ParametersRange, fixingCalendar, paymentCalendar, null, null, null);
                var tradeHeader = new TradeHeader { tradeDate = IdentifiedDateHelper.Create(tradeDate) };
                var p1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
                var p2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
                tradeHeader.partyTradeIdentifier = new[] { p1, p2 };
                trade.tradeHeader = tradeHeader;
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.NameSpace, nameSpace);
                var uniqueIdentifier = nameSpace + "." + identifier.UniqueIdentifier;
                cache.SaveObject(trade, uniqueIdentifier, properties);
                logger.LogDebug("Created new interst rate swap trade: {0}", identifier.UniqueIdentifier);
                return identifier.UniqueIdentifier;
            }
            return result;
        }

        #endregion

        #region Parameterised

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        /// <param name="tradeId">The trade Id.</param>
        /// <param name="tradeDate">The trade Date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="party1">The party1.</param>
        /// <param name="party2">The party2.</param>
        ///<param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	CBA</para>
        /// <para>Receiver:	ABN</para>
        /// <para>NotionalAmount:	10000000.00</para>
        /// <para>EffectiveDate:	13/03/2010</para>
        /// <para>MaturityDate:	4/02/2015</para>
        /// <para>FirstRegularPeriodStartDate</para>	
        /// <para>LastRegularPeriodEndDate</para>	
        /// <para>AdjustedType:	Adjusted</para>
        /// <para>InitialStubType:	</para>
        /// <para>FirstCouponType:	Full</para>
        /// <para>CouponPaymentType:	InArrears</para>
        /// <para>FinalStubType	</para>
        /// <para>RollConvention:	4</para>
        /// <para>LegType:	Floating</para>
        /// <para>Currency:	AUD</para>
        /// <para>CouponOrLastResetRate:	0.05</para>
        /// <para>FloatingRateSpread:	0.0000000</para>
        /// <para>PaymentFrequency:	6m</para>
        /// <para>DayCount:	ACT/360</para>
        /// <para>PaymentCalendar:	AUSY-GBLO</para>
        /// <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        /// <para>FixingCalendar:	AUSY-GBLO</para>
        /// <para>FixingBusinessDayAdjustments:	NONE</para>
        /// <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        /// <para>StubIndexTenor</para>	
        /// <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// </param>
        ///<param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// See above for a description of the range object.</param>
        public string CreateInterestRateSwap(string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Leg1Payer,
            SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange)
        {
            return CreateInterestRateSwap(Logger, Cache, NameSpace, tradeId, tradeDate, isParty1Base, party1, party2, //isParty1Leg1Payer,
            leg1ParametersRange, leg2ParametersRange);
        }

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The trade Id.</param>
        ///  <param name="tradeDate">The trade Date.</param>
        ///  <param name="isParty1Base">The isParty1Base flag.</param>
        ///  <param name="party1">The party1.</param>
        ///  <param name="party2">The party2.</param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType:	</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType	</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        public string CreateInterestRateSwap(ILogger logger, ICoreCache cache, String nameSpace,
            string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Base,
            SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange)
        {
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swap, ProductTypeSimpleEnum.InterestRateSwap, tradeId);
            var properties = new NamedValueSet();
            if (isParty1Base)
            {
                properties.Set(TradeProp.Party1, party1);
                properties.Set(TradeProp.Party2, party2);
            }
            else
            {
                properties.Set(TradeProp.Party1, party2);
                properties.Set(TradeProp.Party2, party1);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, leg1ParametersRange.EffectiveDate);
            properties.Set(TradeProp.MaturityDate, leg1ParametersRange.MaturityDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.InterestRateSwap.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.swap.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_IRSwap_FixedFloat));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            //Create the swap.
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(logger, cache, nameSpace, leg1ParametersRange, null, leg2ParametersRange, null, null, null, null);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new interest rate swap trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	CBA</para>
        /// <para>Receiver:	ABN</para>
        /// <para>NotionalAmount:	10000000.00</para>
        /// <para>EffectiveDate:	13/03/2010</para>
        /// <para>MaturityDate:	4/02/2015</para>
        /// <para>FirstRegularPeriodStartDate</para>	
        /// <para>LastRegularPeriodEndDate</para>	
        /// <para>AdjustedType:	Adjusted</para>
        /// <para>InitialStubType:	</para>
        /// <para>FirstCouponType:	Full</para>
        /// <para>CouponPaymentType:	InArrears</para>
        /// <para>FinalStubType	</para>
        /// <para>RollConvention:	4</para>
        /// <para>LegType:	Floating</para>
        /// <para>Currency:	AUD</para>
        /// <para>CouponOrLastResetRate:	0.05</para>
        /// <para>FloatingRateSpread:	0.0000000</para>
        /// <para>PaymentFrequency:	6m</para>
        /// <para>DayCount:	ACT/360</para>
        /// <para>PaymentCalendar:	AUSY-GBLO</para>
        /// <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        /// <para>FixingCalendar:	AUSY-GBLO</para>
        /// <para>FixingBusinessDayAdjustments:	NONE</para>
        /// <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        /// <para>StubIndexTenor</para>	
        /// <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// </param>
        ///<param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// See above for a description of the range object.</param>
        ///<param name="properties">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateInterestRateSwapWithProperties(SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange, NamedValueSet properties)
        {
            return CreateInterestRateSwapWithProperties(Logger, Cache, NameSpace, leg1ParametersRange, leg2ParametersRange, properties);
        }

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType:	</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType	</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateInterestRateSwapWithProperties(ILogger logger, ICoreCache cache, String nameSpace, 
            SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange, NamedValueSet properties)
        {
            var tradeDate = properties.GetValue<DateTime>(TradeProp.TradeDate, true);
            var tradeId = properties.GetValue<string>(TradeProp.TradeId, true);
            var baseParty = properties.GetValue<string>(TradeProp.BaseParty, false);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            if (baseParty == null)
            {
                //baseParty = properties.GetValue<string>("Party1", true);
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            }
            var counterParty = properties.GetValue<string>(TradeProp.CounterPartyName, false);
            if (counterParty == null)
            {
                //counterParty = properties.GetValue<string>("Party2", true);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }           
            //Create the swap.
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(logger, cache, nameSpace, leg1ParametersRange, null, leg2ParametersRange, null, null, null, null);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swap, ProductTypeSimpleEnum.InterestRateSwap, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new interest rate swap trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        #endregion

        #region Cashflow Based

        /////<param name="isParty1Base">The isParty1Base flag.</param>
        /////<param name="tradeRange">The detailsa associated with this trade.
        ///// <para>string Id</para>
        ///// <para>System.DateTime TradeDate</para>
        ///// </param>
        /// <summary>
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	NAB</para>
        ///  <para>ValuationDate:	13/03/2010</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        /// <param name="leg1DetailedCashflowsListArray">The details associated with the cash flows in this trade.
        ///  <para>public System.DateTime PaymentDate</para>
        ///  <para>System.DateTime StartDate</para>
        ///  <para>System.DateTime EndDate</para>
        ///  <para>int NumberOfDays</para>
        ///  <para>double FutureValue</para>
        ///  <para>double PresentValue</para>
        ///  <para>double DiscountFactor</para>
        ///  <para>double NotionalAmount</para>
        ///  <para>string CouponType;//Float,Fixed,PrincipalExchange?,Cap,Floor</para>
        ///  <para>double Rate</para>
        ///  <para>double Spread</para>
        ///  <para>double StrikeRate</para>
        ///  </param>
        /// <param name="leg2DetailedCashflowsListArray">See above for a description of the detailed object.</param>
        /// <param name="leg1PrincipalExchangeCashflowListArray">An optional array of principal cashflow details.
        ///  <para>System.DateTime PaymentDate</para>
        ///  <para>double Amount</para>
        ///  <para>double PresentValueAmount</para>
        ///  <para>double DiscountFactor</para>
        ///  </param>
        /// <param name="leg2PrincipalExchangeCashflowListArray">See above for a description of the detailed object.</param>
        /// <param name="leg1AdditionalPaymentListArray">An optional array of additional payments in this trade.
        ///  <para>DateTime PaymentDate</para>
        ///  <para>double Amount</para>
        ///  <para>string Currency</para>
        ///  </param>
        /// <param name="leg2AdditionalPaymentListArray">See above for a description of the detailed object.</param>
        /// <param name="properties"> </param>
        /// <returns>The calulated price of the swap, given the curve provided.</returns>
        public string CreateSwapFromCashflows(
            ILogger logger, ICoreCache cache,
            String nameSpace,
            //bool isParty1Base,
            //TradeRange tradeRange,
            SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange,
            List<InputCashflowRangeItem> leg1DetailedCashflowsListArray,
            List<InputCashflowRangeItem> leg2DetailedCashflowsListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentListArray,
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentListArray,
            NamedValueSet properties)
        {
            var tradeDate = properties.GetValue<DateTime>(TradeProp.TradeDate, true);
            var isParty1Base = properties.GetValue<bool>(TradeProp.IsParty1Base, true);
            var productType = ProductTypeSimpleEnum.InterestRateSwap;
            var isXccySwap = properties.GetValue(TradeProp.IsXccySwap, false);
            if (isXccySwap)
            {
                productType = ProductTypeSimpleEnum.CrossCurrencySwap;
            }
            properties.Set(TradeProp.ProductType, productType.ToString());
            var tradeId = properties.GetValue<string>(TradeProp.TradeId, true);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swap, productType, tradeId, tradeDate, sourceSystem);
            if (isParty1Base)
            {
                properties.Set(TradeProp.Party1, leg1ParametersRange.Payer);
                properties.Set(TradeProp.Party2, leg1ParametersRange.Receiver);
            }
            else
            {
                properties.Set(TradeProp.Party1, leg2ParametersRange.Payer);
                properties.Set(TradeProp.Party2, leg2ParametersRange.Receiver);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, leg1ParametersRange.EffectiveDate);
            properties.Set(TradeProp.MaturityDate, leg1ParametersRange.MaturityDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.InterestRateSwap.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.swap.ToString());
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var swap = InterestRateSwapPricer.GeneratedFpMLSwap(logger, cache, leg1ParametersRange, leg2ParametersRange,
                                                                                    leg1DetailedCashflowsListArray, leg2DetailedCashflowsListArray,
                                                                                    leg1PrincipalExchangeCashflowListArray, leg2PrincipalExchangeCashflowListArray,
                                                                                    leg1AdditionalPaymentListArray, leg2AdditionalPaymentListArray);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new interest rate swap trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        /// Gets the dates and notionals for the swap leg.
        ///</summary>
        ///<param name="legParametersRange">Contains all the parameter information required by SwapLegParametersRange_Old.
        /// <para>Payer:	NAB</para>
        /// <para>Receiver:	ABN</para>
        /// <para>NotionalAmount:	10000000.00</para>
        /// <para>EffectiveDate:	13/03/2010</para>
        /// <para>MaturityDate:	4/02/2015</para>
        /// <para>FirstRegularPeriodStartDate</para>	
        /// <para>LastRegularPeriodEndDate</para>	
        /// <para>AdjustedType:	Adjusted</para>
        /// <para>InitialStubType</para>
        /// <para>FirstCouponType:	Full</para>
        /// <para>CouponPaymentType:	InArrears</para>
        /// <para>FinalStubType</para>
        /// <para>RollConvention:	4</para>
        /// <para>LegType:	Floating</para>
        /// <para>Currency:	AUD</para>
        /// <para>CouponOrLastResetRate:	0.05</para>
        /// <para>FloatingRateSpread:	0.0000000</para>
        /// <para>PaymentFrequency:	6m</para>
        /// <para>DayCount:	ACT/360</para>
        /// <para>PaymentCalendar:	AUSY-GBLO</para>
        /// <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        /// <para>FixingCalendar:	AUSY-GBLO</para>
        /// <para>FixingBusinessDayAdjustments:	NONE</para>
        /// <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        /// <para>StubIndexTenor</para>	
        /// <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// <para>ValuationDate:	13/03/2010</para>
        /// </param>
        /// <param name="datesAndNotionals">Contains all the parameter information required by DateTimeDoubleRangeItems.</param>
        ///<param name="valuationRange">Contains all the parameter information required by ValuationRange, 
        /// notably the valuation date.</param>
        ///<returns>A list of detailed cash flow items</returns>
        public List<DetailedCashflowRangeItem> GetSwapLegDetailedCashflowsWithNotionalSchedule(SwapLegParametersRange legParametersRange,
                                                                                        List<DateTimeDoubleRangeItem> datesAndNotionals,
                                                                                        ValuationRange valuationRange)
        {
            var irSwap = new InterestRateSwapPricer();
            var cashflows = irSwap.GetDetailedCashflowsWithNotionalSchedule(Logger, Cache, NameSpace, legParametersRange, datesAndNotionals, valuationRange);
            return cashflows;
        }

        #endregion

        #endregion

        #region Cross Currency Swaps

        #region Configuration Based

        /// <summary>
        /// Builds a cross currency trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The tradeId.</param>
        /// <param name="tradeDate">The tradeDate.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="leg1">The leg1 details.</param>
        /// <param name="leg2">The leg2 details.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, paryy1, party2, isParty1FixedPayer etc</param>
        /// <returns></returns>
        public string CreateXccySwap(string tradeId, DateTime tradeDate,
            AdjustedType adjustedType, bool isParty1Base, DateTime effectiveDate,
            string maturityTenor, SimpleXccySwapLeg leg1, SimpleXccySwapLeg leg2,
            NamedValueSet properties)
        {
            return CreateXccySwap(Logger, Cache, NameSpace, tradeId, tradeDate, adjustedType, isParty1Base, effectiveDate, maturityTenor,
            leg1, leg2, properties);
        }

        /// <summary>
        /// Builds a fra trade using configuration data.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The tradeId.</param>
        /// <param name="tradeDate">The tradeDate.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="leg1">The leg1 details.</param>
        /// <param name="leg2">The leg2 details.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, paryy1, party2, isParty1FixedPayer etc</param>
        /// <returns></returns>
        public string CreateXccySwap(ILogger logger, ICoreCache cache, 
            String nameSpace, string tradeId, DateTime tradeDate,
            AdjustedType adjustedType, bool isParty1Base, DateTime effectiveDate,
            string maturityTenor, SimpleXccySwapLeg leg1, SimpleXccySwapLeg leg2, NamedValueSet properties)
        {
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, "SpreadSheet");
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swap, ProductTypeSimpleEnum.CrossCurrencySwap, tradeId, tradeDate, sourceSystem);
            string payer = "Party1";
            string receiver = "Party2";
            if (!isParty1Base)
            {
                payer = "Party2";
                receiver = "Party1";
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookName, "Test");
            properties.Set(TradeProp.TradeState, "Pricing");
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.CrossCurrencySwap.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.swap.ToString());
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            const string result = "Trade has not been created due to dodgey data!";
            var assetId = leg1.Currency + "-XccySwap-" + maturityTenor;//TODO there are various other config defaults that can be used here depending on the instrument type.
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(Cache, nameSpace, assetId);
            if (instrument.InstrumentNodeItem is SimpleIRSwapNodeStruct nodeStruct)
            {
                //Shared parameters
                var fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                string forecastIndexName = null;
                if (nodeStruct.UnderlyingRateIndex != null && nodeStruct.UnderlyingRateIndex.floatingRateIndex != null)
                {
                    forecastIndexName = nodeStruct.UnderlyingRateIndex.floatingRateIndex.Value;
                }
                var fixingBusinessDayAdjustments = nodeStruct.SpotDate.businessDayConvention.ToString();
                var paymentCenters = BusinessCentersHelper.BusinessCentersString(nodeStruct.DateAdjustments.businessCenters.businessCenter);
                var fixingCenters = BusinessCentersHelper.BusinessCentersString(nodeStruct.SpotDate.businessCenters.businessCenter);
                Period rollPeriod = PeriodHelper.Parse(maturityTenor);
                var maturityDate = paymentCalendar.Advance(effectiveDate, OffsetHelper.FromInterval(rollPeriod, DayTypeEnum.Calendar), nodeStruct.DateAdjustments.businessDayConvention);
                //The first Leg: normally fixed
                var leg1ParametersRange = new SwapLegParametersRange
                {
                    Payer = leg1.Payer,
                    Receiver = leg1.Payer == payer ? receiver : payer,
                    EffectiveDate = effectiveDate,
                    AdjustedType = adjustedType,
                    NotionalAmount = leg1.Notional
                };
                leg1ParametersRange.Payer = payer;
                leg1ParametersRange.Receiver = receiver;
                leg1ParametersRange.DayCount = nodeStruct.SimpleIRSwap.dayCountFraction.Value;
                leg1ParametersRange.LegType = leg1.LegType;
                leg1ParametersRange.CouponPaymentType = CouponPaymentType.InArrears;
                leg1ParametersRange.GeneratePrincipalExchanges = true;
                if ((leg1.LegType == LegType.Floating))
                {
                    leg1ParametersRange.ForecastIndexName = !string.IsNullOrEmpty(leg1.ForecastIndexName) ? leg1.ForecastIndexName : forecastIndexName;
                    leg1ParametersRange.FixingCalendar = fixingCenters;
                    leg1ParametersRange.FixingBusinessDayAdjustments = fixingBusinessDayAdjustments;
                    leg1ParametersRange.FloatingRateSpread = leg1.Spread;
                }
                //leg1ParametersRange.DiscountingType =;
                leg1ParametersRange.FirstCouponType = leg1.FirstCouponType;
                leg1ParametersRange.Currency = leg1.Currency;
                leg1ParametersRange.CouponOrLastResetRate = leg1.Rate;
                leg1ParametersRange.FloatingRateSpread = leg1.Spread;
                leg1ParametersRange.MaturityDate = maturityDate;
                leg1ParametersRange.FirstRegularPeriodStartDate = leg1.FirstRegularPeriodStartDate;
                leg1ParametersRange.PaymentBusinessDayAdjustments = nodeStruct.DateAdjustments.businessDayConvention.ToString();
                leg1ParametersRange.RollConvention = leg1.RollConvention;
                leg1ParametersRange.PaymentFrequency = leg1.PaymentFrequency;
                leg1ParametersRange.LastRegularPeriodEndDate = DateTime.MinValue;
                leg1ParametersRange.PaymentCalendar = paymentCenters;
                //The second leg: normally floating
                var leg2ParametersRange = new SwapLegParametersRange
                {
                    Payer = leg2.Payer,
                    Receiver = leg2.Payer == payer ? receiver : payer,
                    EffectiveDate = effectiveDate,
                    AdjustedType = adjustedType,
                    NotionalAmount = leg2.Notional
                };
                leg2ParametersRange.Payer = receiver;
                leg2ParametersRange.Receiver = payer;
                leg2ParametersRange.GeneratePrincipalExchanges = true;
                if (leg2.LegType == LegType.Floating)
                {
                    leg2ParametersRange.ForecastIndexName = !string.IsNullOrEmpty(leg2.ForecastIndexName) ? leg2.ForecastIndexName : forecastIndexName;
                    leg2ParametersRange.FixingCalendar = fixingCenters;
                    leg2ParametersRange.FixingBusinessDayAdjustments = fixingBusinessDayAdjustments;
                    leg2ParametersRange.FloatingRateSpread = leg2.Spread;
                }
                leg2ParametersRange.CouponOrLastResetRate = leg2.Rate;
                leg2ParametersRange.DayCount = nodeStruct.Calculation.dayCountFraction.Value;
                leg2ParametersRange.LegType = leg2.LegType;
                leg2ParametersRange.CouponPaymentType = CouponPaymentType.InArrears;
                leg2ParametersRange.FirstCouponType = leg2.FirstCouponType;
                leg2ParametersRange.Currency = leg2.Currency;
                leg2ParametersRange.MaturityDate = maturityDate;
                leg2ParametersRange.FirstRegularPeriodStartDate = leg2.FirstRegularPeriodStartDate;
                leg2ParametersRange.PaymentBusinessDayAdjustments = nodeStruct.DateAdjustments.businessDayConvention.ToString();
                leg2ParametersRange.RollConvention = leg2.RollConvention;
                leg2ParametersRange.PaymentFrequency = leg2.PaymentFrequency;
                leg2ParametersRange.LastRegularPeriodEndDate = DateTime.MinValue;
                leg2ParametersRange.PaymentCalendar = paymentCenters;
                properties.Set(TradeProp.EffectiveDate, effectiveDate);
                properties.Set(TradeProp.MaturityDate, maturityDate);
                //Create the swap.
                var trade = InterestRateSwapPricer.CreateTrade(leg1ParametersRange, fixingCalendar, paymentCalendar, leg2ParametersRange, fixingCalendar, paymentCalendar, null, null, null);
                var tradeHeader = new TradeHeader { tradeDate = IdentifiedDateHelper.Create(tradeDate) };
                var p1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
                var p2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
                tradeHeader.partyTradeIdentifier = new[] { p1, p2 };
                trade.tradeHeader = tradeHeader;
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(EnvironmentProp.NameSpace, nameSpace);
                var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
                cache.SaveObject(trade, uniqueIDentifier, properties);
                logger.LogDebug("Created new cross currency swap trade: {0}", identifier.UniqueIdentifier);
                return identifier.UniqueIdentifier;
            }
            return result;
        }

        #endregion

        #region Parameterised

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="productTaxonomy">Must be in the product taxonomy.</param>
        /// <param name="tradeId">The trade Id.</param>
        ///  <param name="tradeDate">The trade Date.</param>
        ///  <param name="isParty1Base">The isParty1Base flag.</param>
        ///  <param name="party1">The party1.</param>
        ///  <param name="party2">The party2.</param>
        /// <param name="sourceSystem">The source system</param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType:	</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType	</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        public string CreateCrossCurrencySwap(String productTaxonomy, string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, string sourceSystem,//bool isParty1Leg1Payer,
            SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange)
        {
            return CreateCrossCurrencySwap(Logger, Cache, NameSpace, productTaxonomy, tradeId, tradeDate, isParty1Base, party1, party2, sourceSystem,//isParty1Leg1Payer,
            leg1ParametersRange, leg2ParametersRange);
        }

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="productTaxonomy">Must be a  in the product taxonomy.</param>
        /// <param name="tradeId">The trade Id.</param>
        ///  <param name="tradeDate">The trade Date.</param>
        ///  <param name="isParty1Base">The isParty1Base flag.</param>
        ///  <param name="party1">The party1.</param>
        ///  <param name="party2">The party2.</param>
        /// <param name="sourceSystem">The source system.</param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType:	</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType	</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        public string CreateCrossCurrencySwap(ILogger logger, ICoreCache cache, String nameSpace, String productTaxonomy,
            string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, string sourceSystem,//bool isParty1Base,
            SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange)
        {
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swap, ProductTypeSimpleEnum.CrossCurrencySwap, tradeId, tradeDate, sourceSystem);
            var properties = new NamedValueSet();
            if (isParty1Base)
            {
                properties.Set(TradeProp.Party1, party1);
                properties.Set(TradeProp.Party2, party2);
            }
            else
            {
                properties.Set(TradeProp.Party1, party2);
                properties.Set(TradeProp.Party2, party1);
                
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, leg1ParametersRange.EffectiveDate);
            properties.Set(TradeProp.MaturityDate, leg1ParametersRange.MaturityDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.CrossCurrencySwap.ToString());
            properties.Set(TradeProp.ProductTaxonomy, productTaxonomy);
            properties.Set(TradeProp.TradeType, ItemChoiceType15.swap.ToString());
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            //Create the swap.
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(logger, cache, nameSpace, leg1ParametersRange, null, leg2ParametersRange, null, null, null, null);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties); 
            logger.LogDebug("Created new cross currency swap trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	CBA</para>
        /// <para>Receiver:	ABN</para>
        /// <para>NotionalAmount:	10000000.00</para>
        /// <para>EffectiveDate:	13/03/2010</para>
        /// <para>MaturityDate:	4/02/2015</para>
        /// <para>FirstRegularPeriodStartDate</para>	
        /// <para>LastRegularPeriodEndDate</para>	
        /// <para>AdjustedType:	Adjusted</para>
        /// <para>InitialStubType:	</para>
        /// <para>FirstCouponType:	Full</para>
        /// <para>CouponPaymentType:	InArrears</para>
        /// <para>FinalStubType	</para>
        /// <para>RollConvention:	4</para>
        /// <para>LegType:	Floating</para>
        /// <para>Currency:	AUD</para>
        /// <para>CouponOrLastResetRate:	0.05</para>
        /// <para>FloatingRateSpread:	0.0000000</para>
        /// <para>PaymentFrequency:	6m</para>
        /// <para>DayCount:	ACT/360</para>
        /// <para>PaymentCalendar:	AUSY-GBLO</para>
        /// <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        /// <para>FixingCalendar:	AUSY-GBLO</para>
        /// <para>FixingBusinessDayAdjustments:	NONE</para>
        /// <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        /// <para>StubIndexTenor</para>	
        /// <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// </param>
        ///<param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// See above for a description of the range object.</param>
        ///<param name="properties">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateCrossCurrencySwapWithProperties(SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange, NamedValueSet properties)
        {
            return CreateCrossCurrencySwapWithProperties(Logger, Cache, NameSpace, leg1ParametersRange, leg2ParametersRange, properties);
        }

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType:	</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType	</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateCrossCurrencySwapWithProperties(ILogger logger, ICoreCache cache, String nameSpace,
            SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange, NamedValueSet properties)
        {
            var tradeDate = properties.GetValue<DateTime>(TradeProp.TradeDate, true);
            var tradeId = properties.GetValue<string>(TradeProp.TradeId, true);
            var baseParty = properties.GetValue<string>(TradeProp.BaseParty, false);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            if (baseParty == null)
            {
                //baseParty = properties.GetValue<string>("Party1", true);
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            }
            var counterParty = properties.GetValue<string>(TradeProp.CounterPartyName, false);
            if (counterParty == null)
            {
                //counterParty = properties.GetValue<string>("Party2", true);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }  
            //Create the swap.
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(logger, cache, nameSpace, leg1ParametersRange, null, leg2ParametersRange, null, null, null, null);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swap, ProductTypeSimpleEnum.CrossCurrencySwap, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new cross currency swap trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        #endregion

        #region Cashflow Based

        /////<param name="isParty1Base">The isParty1Base flag.</param>
        /////<param name="tradeRange">The detailsa associated with this trade.
        ///// <para>string Id</para>
        ///// <para>System.DateTime TradeDate</para>
        ///// </param>
        /// <summary>
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	NAB</para>
        ///  <para>ValuationDate:	13/03/2010</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        /// <param name="leg1DetailedCashflowsListArray">The details associated with the cash flows in this trade.
        ///  <para>public System.DateTime PaymentDate</para>
        ///  <para>System.DateTime StartDate</para>
        ///  <para>System.DateTime EndDate</para>
        ///  <para>int NumberOfDays</para>
        ///  <para>double FutureValue</para>
        ///  <para>double PresentValue</para>
        ///  <para>double DiscountFactor</para>
        ///  <para>double NotionalAmount</para>
        ///  <para>string CouponType;//Float,Fixed,PrincipalExchange?,Cap,Floor</para>
        ///  <para>double Rate</para>
        ///  <para>double Spread</para>
        ///  <para>double StrikeRate</para>
        ///  </param>
        /// <param name="leg2DetailedCashflowsListArray">See above for a description of the detailed object.</param>
        /// <param name="leg1PrincipalExchangeCashflowListArray">An optional array of principal cashflow details.
        ///  <para>System.DateTime PaymentDate</para>
        ///  <para>double Amount</para>
        ///  <para>double PresentValueAmount</para>
        ///  <para>double DiscountFactor</para>
        ///  </param>
        /// <param name="leg2PrincipalExchangeCashflowListArray">See above for a description of the detailed object.</param>
        /// <param name="leg1AdditionalPaymentListArray">An optional array of additional payments in this trade.
        ///  <para>DateTime PaymentDate</para>
        ///  <para>double Amount</para>
        ///  <para>string Currency</para>
        ///  </param>
        /// <param name="leg2AdditionalPaymentListArray">See above for a description of the detailed object.</param>
        /// <param name="properties"> </param>
        /// <returns>The calulated price of the swap, given the curve provided.</returns>
        public string CreateXccySwapFromCashflows(
            ILogger logger, ICoreCache cache,
            String nameSpace,
            //TradeRange tradeRange,
            SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange,
            List<InputCashflowRangeItem> leg1DetailedCashflowsListArray,
            List<InputCashflowRangeItem> leg2DetailedCashflowsListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentListArray,
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentListArray,
            NamedValueSet properties)
        {
            var tradeDate = properties.GetValue<DateTime>("TradeDate", true);
            var isParty1Base = properties.GetValue<bool>("IsParty1Base", true);
            var tradeId = properties.GetValue<string>("TradeId", true);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, "SpreadSheet");
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swap, ProductTypeSimpleEnum.CrossCurrencySwap, tradeId, tradeDate, sourceSystem);
            if (isParty1Base)//TODO
            {
                properties.Set(TradeProp.Party1, leg1ParametersRange.Payer);
                properties.Set(TradeProp.Party2, leg1ParametersRange.Receiver);
                //properties.Set(TradeProp.BaseParty, leg1ParametersRange.Payer);
                //properties.Set(TradeProp.CounterPartyName, leg1ParametersRange.Receiver);
            }
            else
            {
                properties.Set(TradeProp.Party1, leg2ParametersRange.Payer);
                properties.Set(TradeProp.Party2, leg2ParametersRange.Receiver);
                //properties.Set(TradeProp.BaseParty, leg2ParametersRange.Payer);
                //properties.Set(TradeProp.CounterPartyName, leg2ParametersRange.Receiver);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);           
            properties.Set(TradeProp.TradingBookName, "Test");
            properties.Set(TradeProp.TradeState, "Pricing");
            properties.Set(TradeProp.EffectiveDate, leg1ParametersRange.EffectiveDate);
            properties.Set(TradeProp.MaturityDate, leg1ParametersRange.MaturityDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.CrossCurrencySwap.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.swap.ToString());
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(TradeProp.TradeSource, "SpreadSheet");
            properties.Set(EnvironmentProp.SourceSystem, "SpreadSheet");
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var swap = InterestRateSwapPricer.GeneratedFpMLSwap(logger, cache, leg1ParametersRange, leg2ParametersRange,
                                                                                    leg1DetailedCashflowsListArray, leg2DetailedCashflowsListArray,
                                                                                    leg1PrincipalExchangeCashflowListArray, leg2PrincipalExchangeCashflowListArray,
                                                                                    leg1AdditionalPaymentListArray, leg2AdditionalPaymentListArray);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new interest rate swap trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        /// Gets the dates and notionals for the swap leg.
        ///</summary>
        ///<param name="legParametersRange">Contains all the parameter information required by SwapLegParametersRange_Old.
        /// <para>Payer:	NAB</para>
        /// <para>Receiver:	ABN</para>
        /// <para>NotionalAmount:	10000000.00</para>
        /// <para>EffectiveDate:	13/03/2010</para>
        /// <para>MaturityDate:	4/02/2015</para>
        /// <para>FirstRegularPeriodStartDate</para>	
        /// <para>LastRegularPeriodEndDate</para>	
        /// <para>AdjustedType:	Adjusted</para>
        /// <para>InitialStubType</para>
        /// <para>FirstCouponType:	Full</para>
        /// <para>CouponPaymentType:	InArrears</para>
        /// <para>FinalStubType</para>
        /// <para>RollConvention:	4</para>
        /// <para>LegType:	Floating</para>
        /// <para>Currency:	AUD</para>
        /// <para>CouponOrLastResetRate:	0.05</para>
        /// <para>FloatingRateSpread:	0.0000000</para>
        /// <para>PaymentFrequency:	6m</para>
        /// <para>DayCount:	ACT/360</para>
        /// <para>PaymentCalendar:	AUSY-GBLO</para>
        /// <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        /// <para>FixingCalendar:	AUSY-GBLO</para>
        /// <para>FixingBusinessDayAdjustments:	NONE</para>
        /// <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        /// <para>StubIndexTenor</para>	
        /// <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// <para>ValuationDate:	13/03/2010</para>
        /// </param>
        /// <param name="datesAndNotionals">Contains all the parameter information required by DateTimeDoubleRangeItems.</param>
        ///<param name="valuationRange">Contains all the parameter information required by ValuationRange, 
        /// notably the valuation date.</param>
        ///<returns>A list of detailed cash flow items</returns>
        public List<DetailedCashflowRangeItem> GetXccyCashflows(SwapLegParametersRange legParametersRange,
                                                                                        List<DateTimeDoubleRangeItem> datesAndNotionals,
                                                                                        ValuationRange valuationRange)
        {
            var xccySwap = new CrossCurrencySwapPricer();
            var cashflows = xccySwap.GetDetailedCashflowsWithNotionalSchedule(Logger, Cache, NameSpace, legParametersRange, datesAndNotionals, valuationRange);
            return cashflows;
        }

        #endregion

        #endregion

        #region Caps and Floors

        #region Configuration Based

        /// <summary>
        /// Builds a fra trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The tradeId.</param>
        /// <param name="tradeDate">The tradeDate.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="leg1">The leg1 details.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, paryy1, party2, isParty1FixedPayer etc</param>
        /// <returns></returns>
        public string CreateCapFloor(string tradeId, DateTime tradeDate,
            AdjustedType adjustedType, bool isParty1Base, DateTime effectiveDate, string currency,
            decimal notionalAmount, string maturityTenor, decimal strike, CapFloorLegSimpleRange leg1,
            NamedValueSet properties)
        {
            return CreateCapFloor(Logger, Cache, NameSpace, tradeId, tradeDate, adjustedType, isParty1Base, effectiveDate, currency, notionalAmount, maturityTenor,
            strike, leg1, properties);
        }

        /// <summary>
        /// Builds a cap trade using configuration data.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The tradeId.</param>
        /// <param name="tradeDate">The tradeDate.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="leg1">The leg1 details.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, paryy1, party2, isParty1FixedPayer etc</param>
        /// <returns></returns>
        public string CreateCapFloor(ILogger logger, ICoreCache cache, String nameSpace, string tradeId, DateTime tradeDate,
            AdjustedType adjustedType, bool isParty1Base, DateTime effectiveDate, string currency,
            decimal notionalAmount, string maturityTenor, decimal strike, CapFloorLegSimpleRange leg1, NamedValueSet properties)
        {
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.capFloor, ProductTypeSimpleEnum.CapFloor, tradeId, tradeDate, sourceSystem);
            string receiver = "Party1";
            string payer = "Party2";
            properties.Set("Buyer", leg1.Buyer);
            if (isParty1Base)
            {
                if (leg1.Buyer != "Party1")
                {
                    receiver = "Party2";
                    payer = "Party1";
                }
            }
            else
            {
                if (leg1.Buyer != "Party2")
                {
                    receiver = "Party2";
                    payer = "Party1";
                }
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.CapFloor.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.capFloor.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_CapFloor));
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            const string result = "Trade has not been created due to dodgey data!";
            var assetId = currency + "-IRCap";// +maturityTenor;
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, assetId);
            if (instrument.InstrumentNodeItem is SimpleIRCapNodeStruct nodeStruct)
            {
                //Shared parameters
                var fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                var forecastIndexName = nodeStruct.UnderlyingRateIndex.floatingRateIndex.Value;
                var fixingBusinessDayAdjustments = nodeStruct.SpotDate.businessDayConvention.ToString();
                var paymentCenters = BusinessCentersHelper.BusinessCentersString(nodeStruct.DateAdjustments.businessCenters.businessCenter);
                var fixingCenters = BusinessCentersHelper.BusinessCentersString(nodeStruct.SpotDate.businessCenters.businessCenter);
                Period rollPeriod = PeriodHelper.Parse(maturityTenor);
                var maturityDate = paymentCalendar.Advance(effectiveDate, OffsetHelper.FromInterval(rollPeriod, DayTypeEnum.Calendar), nodeStruct.DateAdjustments.businessDayConvention);
                //The first Leg: normally fixed
                //The fixed Leg
                var leg1ParametersRange = new CapFloorLegParametersRange
                {
                    LegType = LegType.Floating,
                    StrikeRate = strike,
                    Payer = payer,
                    Receiver = receiver,
                    EffectiveDate = effectiveDate,
                    AdjustedType = adjustedType,
                    NotionalAmount = notionalAmount
                };
                leg1ParametersRange.Payer = payer;
                leg1ParametersRange.Receiver = receiver;
                leg1ParametersRange.DayCount = nodeStruct.Calculation.dayCountFraction.Value;
                leg1ParametersRange.CapOrFloor = leg1.CapOrFloor;
                leg1ParametersRange.CouponPaymentType = CouponPaymentType.InArrears;
                leg1ParametersRange.ForecastIndexName = forecastIndexName;
                leg1ParametersRange.FixingCalendar = fixingCenters;
                leg1ParametersRange.FixingBusinessDayAdjustments = fixingBusinessDayAdjustments;
                leg1ParametersRange.FloatingRateSpread = leg1.Spread;
                if (!string.IsNullOrEmpty(leg1.ForecastIndexName))
                {
                    leg1ParametersRange.ForecastIndexName = leg1.ForecastIndexName;
                }
                //leg1ParametersRange.DiscountingType =;
                leg1ParametersRange.FirstCouponType = leg1.FirstCouponType;
                leg1ParametersRange.Currency = currency;
                leg1ParametersRange.CouponOrLastResetRate = leg1.Rate;
                leg1ParametersRange.FloatingRateSpread = leg1.Spread;
                leg1ParametersRange.MaturityDate = maturityDate;
                leg1ParametersRange.FirstRegularPeriodStartDate = leg1.FirstRegularPeriodStartDate;
                leg1ParametersRange.PaymentBusinessDayAdjustments = nodeStruct.DateAdjustments.businessDayConvention.ToString();
                leg1ParametersRange.RollConvention = leg1.RollConvention;
                leg1ParametersRange.PaymentFrequency = leg1.PaymentFrequency;
                leg1ParametersRange.LastRegularPeriodEndDate = DateTime.MinValue;
                leg1ParametersRange.PaymentCalendar = paymentCenters;
                properties.Set(TradeProp.EffectiveDate, effectiveDate);
                properties.Set(TradeProp.MaturityDate, maturityDate);
                //Create the swap.
                var trade = CapFloorPricer.CreateTrade(leg1ParametersRange, fixingCalendar, paymentCalendar, null, null, null, null);
                var tradeHeader = new TradeHeader { tradeDate = IdentifiedDateHelper.Create(tradeDate) };
                var p1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
                var p2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
                tradeHeader.partyTradeIdentifier = new[] { p1, p2 };
                trade.tradeHeader = tradeHeader;
                if (trade.Item is CapFloor capFloor)
                {
                    var curves = capFloor.GetRequiredPricingStructures();
                    var currencies = capFloor.GetRequiredCurrencies().ToArray();
                    properties.Set(TradeProp.RequiredPricingStructures, curves.ToArray());
                    properties.Set(TradeProp.RequiredCurrencies, currencies);
                }
                properties.Set(EnvironmentProp.NameSpace, nameSpace);
                var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
                cache.SaveObject(trade, uniqueIDentifier, properties);
                logger.LogDebug("Created new cap/floor trade: {0}", identifier.UniqueIdentifier);
                return identifier.UniqueIdentifier;
            }
            return result;
        }

        #endregion

        #region Parameterised

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        /// <param name="tradeId">The trade Id.</param>
        /// <param name="tradeDate">The trade Date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="party1">The party1.</param>
        /// <param name="party2">The party2.</param>
        ///<param name="capfloorParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	CBA</para>
        /// <para>Receiver:	ABN</para>
        /// <para>NotionalAmount:	10000000.00</para>
        /// <para>EffectiveDate:	13/03/2010</para>
        /// <para>MaturityDate:	4/02/2015</para>
        /// <para>FirstRegularPeriodStartDate</para>	
        /// <para>LastRegularPeriodEndDate</para>	
        /// <para>AdjustedType:	Adjusted</para>
        /// <para>InitialStubType:	</para>
        /// <para>FirstCouponType:	Full</para>
        /// <para>CouponPaymentType:	InArrears</para>
        /// <para>FinalStubType	</para>
        /// <para>RollConvention:	4</para>
        /// <para>LegType:	Floating</para>
        /// <para>Currency:	AUD</para>
        /// <para>CouponOrLastResetRate:	0.05</para>
        /// <para>FloatingRateSpread:	0.0000000</para>
        /// <para>PaymentFrequency:	6m</para>
        /// <para>DayCount:	ACT/360</para>
        /// <para>PaymentCalendar:	AUSY-GBLO</para>
        /// <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        /// <para>FixingCalendar:	AUSY-GBLO</para>
        /// <para>FixingBusinessDayAdjustments:	NONE</para>
        /// <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        /// <para>StubIndexTenor</para>	
        /// <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// <para> CapFloorType CapOrFloor</para>
        /// <para>  StrikeRate</para>
        /// <para>  VolatilitySurface</para>
        /// </param>
        public string CreateCapFloor(string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Leg1Payer,
            CapFloorLegParametersRange capfloorParametersRange)
        {
            return CreateCapFloor(Logger, Cache, NameSpace, tradeId, tradeDate, isParty1Base, party1, party2, //isParty1Leg1Payer,
            capfloorParametersRange);
        }

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The trade Id.</param>
        ///  <param name="tradeDate">The trade Date.</param>
        ///  <param name="isParty1Base">The isParty1Base flag.</param>
        ///  <param name="party1">The party1.</param>
        ///  <param name="party2">The party2.</param>
        /// <param name="capfloorParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType:	</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType	</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  <para> CapFloorType CapOrFloor</para>
        ///  <para>  StrikeRate</para>
        ///  <para>  VolatilitySurface</para>
        ///  </param>
        public string CreateCapFloor(ILogger logger, ICoreCache cache, String nameSpace, 
            string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Base,
            CapFloorLegParametersRange capfloorParametersRange)
        {
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.capFloor, ProductTypeSimpleEnum.CapFloor, tradeId);
            var properties = new NamedValueSet();
            if (isParty1Base)
            {
                properties.Set(TradeProp.Party1, party1);
                properties.Set(TradeProp.Party2, party2);
            }
            else
            {
                properties.Set(TradeProp.Party1, party2);
                properties.Set(TradeProp.Party2, party1);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, capfloorParametersRange.EffectiveDate);
            properties.Set(TradeProp.MaturityDate, capfloorParametersRange.MaturityDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.CapFloor.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.capFloor.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_CapFloor));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            //Create the swap.
            var capfloor = CapFloorGenerator.GenerateDefinitionCashflowsAmounts(logger, cache, nameSpace, null, null, capfloorParametersRange, null, null, null);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetCapFloor(trade, capfloor);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new capfloor trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="capfloorParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	CBA</para>
        /// <para>Receiver:	ABN</para>
        /// <para>NotionalAmount:	10000000.00</para>
        /// <para>EffectiveDate:	13/03/2010</para>
        /// <para>MaturityDate:	4/02/2015</para>
        /// <para>FirstRegularPeriodStartDate</para>	
        /// <para>LastRegularPeriodEndDate</para>	
        /// <para>AdjustedType:	Adjusted</para>
        /// <para>InitialStubType:	</para>
        /// <para>FirstCouponType:	Full</para>
        /// <para>CouponPaymentType:	InArrears</para>
        /// <para>FinalStubType	</para>
        /// <para>RollConvention:	4</para>
        /// <para>LegType:	Floating</para>
        /// <para>Currency:	AUD</para>
        /// <para>CouponOrLastResetRate:	0.05</para>
        /// <para>FloatingRateSpread:	0.0000000</para>
        /// <para>PaymentFrequency:	6m</para>
        /// <para>DayCount:	ACT/360</para>
        /// <para>PaymentCalendar:	AUSY-GBLO</para>
        /// <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        /// <para>FixingCalendar:	AUSY-GBLO</para>
        /// <para>FixingBusinessDayAdjustments:	NONE</para>
        /// <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        /// <para>StubIndexTenor</para>	
        /// <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// <para> CapFloorType CapOrFloor</para>
        /// <para>  StrikeRate</para>
        /// <para>  VolatilitySurface</para>
        /// </param>
        ///<param name="properties">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateCapFloorWithProperties(CapFloorLegParametersRange capfloorParametersRange, NamedValueSet properties)
        {
            return CreateCapFloorWithProperties(Logger, Cache, NameSpace, capfloorParametersRange, properties);
        }

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="capfloorParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType:	</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType	</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  <para> CapFloorType CapOrFloor</para>
        ///  <para>  StrikeRate</para>
        ///  <para>  VolatilitySurface</para>
        ///  </param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateCapFloorWithProperties(ILogger logger, ICoreCache cache, String nameSpace, CapFloorLegParametersRange capfloorParametersRange, NamedValueSet properties)
        {
            var tradeDate = properties.GetValue<DateTime>(TradeProp.TradeDate, true);
            var tradeId = properties.GetValue<string>(TradeProp.TradeId, true);
            var baseParty = properties.GetValue<string>(TradeProp.BaseParty, false);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            var tradeState = properties.GetValue(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, capfloorParametersRange.EffectiveDate);
            properties.Set(TradeProp.MaturityDate, capfloorParametersRange.MaturityDate);
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_CapFloor));
            properties.Set(TradeProp.TradeState, tradeState);
            if (baseParty == null)
            {
                //baseParty = properties.GetValue<string>("Party1", true);
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            }
            var counterParty = properties.GetValue<string>(TradeProp.CounterPartyName, false);
            if (counterParty == null)
            {
                //counterParty = properties.GetValue<string>("Party2", true);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }  
            //Create the swap.
            var capfloor = CapFloorGenerator.GenerateDefinitionCashflowsAmounts(logger, cache, nameSpace, null, null, capfloorParametersRange, null, null, null);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetCapFloor(trade, capfloor);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.capFloor, ProductTypeSimpleEnum.CapFloor, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new interest rate swap trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        #endregion

        #region Cashflow Based

        /// <summary>
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="capFloorParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	NAB</para>
        ///  <para>ValuationDate:	13/03/2010</para>
        ///  </param>
        /// <param name="capFloorDetailedCashflowsListArray">The details associated with the cash flows in this trade.
        ///  <para>public System.DateTime PaymentDate</para>
        ///  <para>System.DateTime StartDate</para>
        ///  <para>System.DateTime EndDate</para>
        ///  <para>int NumberOfDays</para>
        ///  <para>double FutureValue</para>
        ///  <para>double PresentValue</para>
        ///  <para>double DiscountFactor</para>
        ///  <para>double NotionalAmount</para>
        ///  <para>string CouponType;//Float,Fixed,PrincipalExchange?,Cap,Floor</para>
        ///  <para>double Rate</para>
        ///  <para>double Spread</para>
        ///  <para>double StrikeRate</para>
        ///  </param>
        /// <param name="capFloorPrincipalExchangeCashflowListArray">An optional array of principal cashflow details.
        ///  <para>System.DateTime PaymentDate</para>
        ///  <para>double Amount</para>
        ///  <para>double PresentValueAmount</para>
        ///  <para>double DiscountFactor</para>
        ///  </param>
        /// <param name="capFloorAdditionalPaymentListArray">An optional array of additional payments in this trade.
        ///  <para>DateTime PaymentDate</para>
        ///  <para>double Amount</para>
        ///  <para>string Currency</para>
        ///  </param>
        /// <param name="feePaymentList"> </param>
        /// <param name="properties"> </param>
        /// <returns>The calulated price of the swap, given the curve provided.</returns>
        public string CreateCapFloorFromCashflows(
            ILogger logger, ICoreCache cache, String nameSpace,
            CapFloorLegParametersRange capFloorParametersRange,
            List<InputCashflowRangeItem> capFloorDetailedCashflowsListArray,
            List<InputPrincipalExchangeCashflowRangeItem> capFloorPrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> capFloorAdditionalPaymentListArray,
            List<FeePaymentRangeItem> feePaymentList,
            NamedValueSet properties)
        {
            var tradeDate = properties.GetValue<DateTime>(TradeProp.TradeDate, true);
            //var isParty1Base = properties.GetValue<bool>("IsParty1Base", true);
            var tradeId = properties.GetValue<string>(TradeProp.TradeId, true);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.capFloor, ProductTypeSimpleEnum.CapFloor, tradeId, tradeDate, sourceSystem);
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, capFloorParametersRange.EffectiveDate);
            properties.Set(TradeProp.MaturityDate, capFloorParametersRange.MaturityDate);
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.CapFloor.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_CapFloor));
            properties.Set(TradeProp.TradeType, ItemChoiceType15.capFloor.ToString());
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            var capfloor = CapFloorPricer.GeneratedFpMLCapFloor(logger, cache, capFloorParametersRange, capFloorDetailedCashflowsListArray,
                                                                                    capFloorPrincipalExchangeCashflowListArray, capFloorAdditionalPaymentListArray,
                                                                                    feePaymentList);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetCapFloor(trade, capfloor);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new capfloor trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        #endregion

        #endregion

        #region European Interest Rate Swaptions

        #region Configuration Based

        /// <summary>
        /// Builds a swaption trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The tradeId.</param>
        /// <param name="tradeDate">The tradeDate.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="swaptionTerms">Swaption details range:
        /// <para>DateTime ExpirationDate</para>	
        /// <para>string   ExpirationDateCalendar</para>	
        /// <para>string   ExpirationDateBusinessDayAdjustments</para>	
        /// <para>double   EarliestExerciseTime;//TimeSpan.FromDays()</para>	
        /// <para>double   ExpirationTime;//TimeSpan.FromDays()</para>	
        /// <para>bool     AutomaticExcercise</para>	        
        /// <para>DateTime PaymentDate</para>	
        /// <para>string   PaymentDateCalendar</para>	
        /// <para>string   PaymentDateBusinessDayAdjustments</para>	
        /// <para>string   PremiumPayer</para>	
        /// <para>string   PremiumReceiver</para>	
        /// <para>decimal  Premium</para>	
        /// <para>string   PremiumCurrency</para>	
        /// <para>decimal  StrikeRate</para>	
        /// <para>decimal  Volatility</para>	
        /// </param>
        /// <param name="leg1">The leg1 details.</param>
        /// <param name="leg2">The leg2 details.</param>
        /// <param name="properties"> </param>
        /// <returns></returns>
        public string CreateSimpleIRSwaption(string tradeId, DateTime tradeDate,
            AdjustedType adjustedType, bool isParty1Base, DateTime effectiveDate, string currency,
            decimal notionalAmount, string maturityTenor, SwapLegSimpleRange leg1, SwapLegSimpleRange leg2,
            SwaptionParametersRange swaptionTerms, NamedValueSet properties)
        {
            return CreateSimpleIRSwaption(Logger, Cache, NameSpace, tradeId, tradeDate, adjustedType, isParty1Base, effectiveDate, currency, notionalAmount, maturityTenor,
            leg1, leg2, swaptionTerms, properties);
        }

        /// <summary>
        /// Builds a swaption trade using configuration data.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSPace</param>
        /// <param name="tradeId">The tradeId.</param>
        /// <param name="tradeDate">The tradeDate.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="swaptionTerms">Swaption details range:
        /// <para>DateTime ExpirationDate</para>	
        /// <para>string   ExpirationDateCalendar</para>	
        /// <para>string   ExpirationDateBusinessDayAdjustments</para>	
        /// <para>double   EarliestExerciseTime;//TimeSpan.FromDays()</para>	
        /// <para>double   ExpirationTime;//TimeSpan.FromDays()</para>	
        /// <para>bool     AutomaticExcercise</para>	        
        /// <para>DateTime PaymentDate</para>	
        /// <para>string   PaymentDateCalendar</para>	
        /// <para>string   PaymentDateBusinessDayAdjustments</para>	
        /// <para>string   PremiumPayer</para>	
        /// <para>string   PremiumReceiver</para>	
        /// <para>decimal  Premium</para>	
        /// <para>string   PremiumCurrency</para>	
        /// <para>decimal  StrikeRate</para>	
        /// <para>decimal  Volatility</para>	
        /// </param>
        /// <param name="leg1">The leg1 details.</param>
        /// <param name="leg2">The leg2 details.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, paryy1, party2, isParty1FixedPayer etc</param>
        /// <returns></returns>
        public string CreateSimpleIRSwaption(ILogger logger, ICoreCache cache, 
            String nameSpace, string tradeId, DateTime tradeDate, 
            AdjustedType adjustedType, bool isParty1Base, DateTime effectiveDate, string currency,
            decimal notionalAmount, string maturityTenor, SwapLegSimpleRange leg1, SwapLegSimpleRange leg2,
            SwaptionParametersRange swaptionTerms, NamedValueSet properties)
        {
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swaption, ProductTypeSimpleEnum.InterestRateSwaption, tradeId, tradeDate, sourceSystem);
            string payer = "Party1";
            string receiver = "Party2";
            if (isParty1Base)
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }
            else
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party2);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);
                payer = "Party2";
                receiver = "Party1";
            }
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.InterestRateSwaption.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.swaption.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_Option_Swaption));
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            const string result = "Trade has not been created due to dodgey data!";
            var assetId = currency + "-IRSwap-" + maturityTenor;
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, assetId);
            if (instrument.InstrumentNodeItem is SimpleIRSwapNodeStruct nodeStruct)
            {
                //Shared parameters
                var fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.SpotDate.businessCenters, nameSpace);
                var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, nodeStruct.DateAdjustments.businessCenters, nameSpace);
                var forecastIndexName = nodeStruct.UnderlyingRateIndex.floatingRateIndex.Value;
                var fixingBusinessDayAdjustments = nodeStruct.SpotDate.businessDayConvention.ToString();
                var paymentCenters = BusinessCentersHelper.BusinessCentersString(nodeStruct.DateAdjustments.businessCenters.businessCenter);
                var fixingCenters = BusinessCentersHelper.BusinessCentersString(nodeStruct.SpotDate.businessCenters.businessCenter);
                Period rollPeriod = PeriodHelper.Parse(maturityTenor);
                var maturityDate = paymentCalendar.Advance(effectiveDate, OffsetHelper.FromInterval(rollPeriod, DayTypeEnum.Calendar), nodeStruct.DateAdjustments.businessDayConvention);
                //The first Leg: normally fixed
                //The fixed Leg
                var leg1ParametersRange = new SwapLegParametersRange
                {
                    Payer = leg1.Payer,
                    Receiver = leg1.Payer == payer ? receiver : payer,
                    EffectiveDate = effectiveDate,
                    AdjustedType = adjustedType,
                    NotionalAmount = notionalAmount
                };
                leg1ParametersRange.Payer = payer;
                leg1ParametersRange.Receiver = receiver;
                leg1ParametersRange.DayCount = nodeStruct.SimpleIRSwap.dayCountFraction.Value;
                leg1ParametersRange.LegType = leg1.LegType;
                leg1ParametersRange.CouponPaymentType = CouponPaymentType.InArrears;
                if ((leg1.LegType == LegType.Floating))
                {
                    leg1ParametersRange.ForecastIndexName = forecastIndexName;
                    leg1ParametersRange.FixingCalendar = fixingCenters;
                    leg1ParametersRange.FixingBusinessDayAdjustments = fixingBusinessDayAdjustments;
                    leg1ParametersRange.FloatingRateSpread = leg1.Spread;
                    if (!string.IsNullOrEmpty(leg1.ForecastIndexName))
                    {
                        leg1ParametersRange.ForecastIndexName = leg1.ForecastIndexName;
                    }
                }
                //leg1ParametersRange.DiscountingType =;
                leg1ParametersRange.FirstCouponType = leg1.FirstCouponType;
                leg1ParametersRange.Currency = currency;
                leg1ParametersRange.CouponOrLastResetRate = leg1.Rate;
                leg1ParametersRange.FloatingRateSpread = leg1.Spread;
                leg1ParametersRange.MaturityDate = maturityDate;
                leg1ParametersRange.FirstRegularPeriodStartDate = leg1.FirstRegularPeriodStartDate;
                leg1ParametersRange.PaymentBusinessDayAdjustments = nodeStruct.DateAdjustments.businessDayConvention.ToString();
                leg1ParametersRange.RollConvention = leg1.RollConvention;
                leg1ParametersRange.PaymentFrequency = leg1.PaymentFrequency;
                leg1ParametersRange.LastRegularPeriodEndDate = DateTime.MinValue;
                leg1ParametersRange.PaymentCalendar = paymentCenters;
                //The floating leg.
                var leg2ParametersRange = new SwapLegParametersRange
                {
                    Payer = leg2.Payer,
                    Receiver = leg2.Payer == payer ? receiver : payer,
                    EffectiveDate = effectiveDate,
                    AdjustedType = adjustedType,
                    NotionalAmount = notionalAmount
                };
                leg2ParametersRange.Payer = receiver;
                leg2ParametersRange.Receiver = payer;
                if (leg2.LegType == LegType.Floating)
                {
                    leg2ParametersRange.ForecastIndexName = forecastIndexName;
                    leg2ParametersRange.FixingCalendar = fixingCenters;
                    leg2ParametersRange.FixingBusinessDayAdjustments = fixingBusinessDayAdjustments;
                    leg2ParametersRange.FloatingRateSpread = leg2.Spread;
                    if (!string.IsNullOrEmpty(leg2.ForecastIndexName))
                    {
                        leg2ParametersRange.ForecastIndexName = leg2.ForecastIndexName;
                    }
                }
                leg2ParametersRange.FloatingRateSpread = leg2.Spread;
                leg2ParametersRange.DayCount = nodeStruct.Calculation.dayCountFraction.Value;
                leg2ParametersRange.LegType = leg2.LegType;
                leg2ParametersRange.CouponPaymentType = CouponPaymentType.InArrears;
                leg2ParametersRange.FirstCouponType = leg2.FirstCouponType;
                leg2ParametersRange.Currency = currency;
                leg2ParametersRange.MaturityDate = maturityDate;
                leg2ParametersRange.FirstRegularPeriodStartDate = leg2.FirstRegularPeriodStartDate;
                leg2ParametersRange.PaymentBusinessDayAdjustments = nodeStruct.DateAdjustments.businessDayConvention.ToString();
                leg2ParametersRange.RollConvention = leg2.RollConvention;
                leg2ParametersRange.PaymentFrequency = leg2.PaymentFrequency;
                leg2ParametersRange.LastRegularPeriodEndDate = DateTime.MinValue;
                leg2ParametersRange.FixingBusinessDayAdjustments = nodeStruct.SpotDate.businessDayConvention.ToString();
                leg2ParametersRange.PaymentCalendar = paymentCenters;
                leg2ParametersRange.CouponOrLastResetRate = leg2.Rate;
                leg2ParametersRange.FloatingRateSpread = leg2.Spread;
                properties.Set(TradeProp.EffectiveDate, effectiveDate);
                properties.Set(TradeProp.MaturityDate, maturityDate);
                //Create the swap.
                var calendars = new Pair<IBusinessCalendar, IBusinessCalendar>(fixingCalendar, paymentCalendar);
                var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(logger, cache, nameSpace, leg1ParametersRange, calendars, leg2ParametersRange, calendars, null, null, null);
                //var trade = InterestRateSwapPricer.CreateTrade(leg1ParametersRange, fixingCalendar, paymentCalendar, leg2ParametersRange, fixingCalendar, paymentCalendar, null, null, null);
                var trade = SwaptionPricer.CreateSwaptionTrade(swaptionTerms, null, swap);
                var tradeHeader = new TradeHeader { tradeDate = IdentifiedDateHelper.Create(tradeDate) };
                var p1 = PartyTradeIdentifierHelper.Parse(tradeId, "party1");
                var p2 = PartyTradeIdentifierHelper.Parse(tradeId, "party2");
                tradeHeader.partyTradeIdentifier = new[] { p1, p2 };
                trade.tradeHeader = tradeHeader;
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.NameSpace, nameSpace);
                var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
                cache.SaveObject(trade, uniqueIDentifier, properties);
                logger.LogDebug("Created new interst rate swaption trade: {0}", identifier.UniqueIdentifier);
                return identifier.UniqueIdentifier;
            }
            return result;
        }

        #endregion

        #region Parameterised

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        /// <param name="tradeId">The trade Id.</param>
        /// <param name="tradeDate">The trade Date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="party1">The party1.</param>
        /// <param name="party2">The party2.</param>
        /// <param name="swaptionTerms">Swaption details range:
        /// <para>DateTime ExpirationDate</para>	
        /// <para>string   ExpirationDateCalendar</para>	
        /// <para>string   ExpirationDateBusinessDayAdjustments</para>	
        /// <para>double   EarliestExerciseTime;//TimeSpan.FromDays()</para>	
        /// <para>double   ExpirationTime;//TimeSpan.FromDays()</para>	
        /// <para>bool     AutomaticExcercise</para>	        
        /// <para>DateTime PaymentDate</para>	
        /// <para>string   PaymentDateCalendar</para>	
        /// <para>string   PaymentDateBusinessDayAdjustments</para>	
        /// <para>string   PremiumPayer</para>	
        /// <para>string   PremiumReceiver</para>	
        /// <para>decimal  Premium</para>	
        /// <para>string   PremiumCurrency</para>	
        /// <para>decimal  StrikeRate</para>	
        /// <para>decimal  Volatility</para>	
        /// </param>
        ///<param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	CBA</para>
        /// <para>Receiver:	ABN</para>
        /// <para>NotionalAmount:	10000000.00</para>
        /// <para>EffectiveDate:	13/03/2010</para>
        /// <para>MaturityDate:	4/02/2015</para>
        /// <para>FirstRegularPeriodStartDate</para>	
        /// <para>LastRegularPeriodEndDate</para>	
        /// <para>AdjustedType:	Adjusted</para>
        /// <para>InitialStubType:	</para>
        /// <para>FirstCouponType:	Full</para>
        /// <para>CouponPaymentType:	InArrears</para>
        /// <para>FinalStubType	</para>
        /// <para>RollConvention:	4</para>
        /// <para>LegType:	Floating</para>
        /// <para>Currency:	AUD</para>
        /// <para>CouponOrLastResetRate:	0.05</para>
        /// <para>FloatingRateSpread:	0.0000000</para>
        /// <para>PaymentFrequency:	6m</para>
        /// <para>DayCount:	ACT/360</para>
        /// <para>PaymentCalendar:	AUSY-GBLO</para>
        /// <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        /// <para>FixingCalendar:	AUSY-GBLO</para>
        /// <para>FixingBusinessDayAdjustments:	NONE</para>
        /// <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        /// <para>StubIndexTenor</para>	
        /// <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// </param>
        ///<param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// See above for a description of the range object.</param>
        public string CreateInterestRateSwaption(string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Leg1Payer,
            SwaptionParametersRange swaptionTerms, SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange)
        {
            return CreateInterestRateSwaption(Logger, Cache, NameSpace, tradeId, tradeDate, isParty1Base, party1, party2, //isParty1Leg1Payer,
            swaptionTerms, leg1ParametersRange, leg2ParametersRange);
        }

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="tradeId">The trade Id.</param>
        ///  <param name="tradeDate">The trade Date.</param>
        ///  <param name="isParty1Base">The isParty1Base flag.</param>
        ///  <param name="party1">The party1.</param>
        ///  <param name="party2">The party2.</param>
        ///  <param name="swaptionTerms">Swaption details range:
        ///  <para>DateTime ExpirationDate</para>	
        ///  <para>string   ExpirationDateCalendar</para>	
        ///  <para>string   ExpirationDateBusinessDayAdjustments</para>	
        ///  <para>double   EarliestExerciseTime;//TimeSpan.FromDays()</para>	
        ///  <para>double   ExpirationTime;//TimeSpan.FromDays()</para>	
        ///  <para>bool     AutomaticExcercise</para>	        
        ///  <para>DateTime PaymentDate</para>	
        ///  <para>string   PaymentDateCalendar</para>	
        ///  <para>string   PaymentDateBusinessDayAdjustments</para>	
        ///  <para>string   PremiumPayer</para>	
        ///  <para>string   PremiumReceiver</para>	
        ///  <para>decimal  Premium</para>	
        ///  <para>string   PremiumCurrency</para>	
        ///  <para>decimal  StrikeRate</para>	
        ///  <para>decimal  Volatility</para>	
        ///  </param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType:	</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType	</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        public string CreateInterestRateSwaption(ILogger logger, ICoreCache cache, String nameSpace,
            string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Base,
            SwaptionParametersRange swaptionTerms, SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange)
        {
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swaption, ProductTypeSimpleEnum.InterestRateSwaption, tradeId);
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(logger, cache, nameSpace, leg1ParametersRange, null, leg2ParametersRange, null, null, null, null);
            if (swap != null)
            {
                InterestRateSwapPricer swapPricer;
                var properties = new NamedValueSet();
                if (isParty1Base)
                {
                    swapPricer = new InterestRateSwapPricer(logger, cache, nameSpace, null, swap, party1);
                    properties.Set(TradeProp.Party1, party1);
                    properties.Set(TradeProp.Party2, party2);
                    properties.Set(TradeProp.BaseParty, TradeProp.Party1);
                    properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
                }
                else
                {
                    swapPricer = new InterestRateSwapPricer(logger, cache, nameSpace, null, swap, party2);
                    properties.Set(TradeProp.Party1, party2);
                    properties.Set(TradeProp.Party2, party1);
                    properties.Set(TradeProp.BaseParty, TradeProp.Party2);
                    properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);
                }
                properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
                properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
                properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
                properties.Set(TradeProp.EffectiveDate, swapPricer.EffectiveDate);
                properties.Set(TradeProp.MaturityDate, swapPricer.RiskMaturityDate);
                properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.InterestRateSwaption.ToString());
                properties.Set(TradeProp.TradeType, ItemChoiceType15.swaption.ToString());
                properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_Option_Swaption));
                properties.Set(TradeProp.TradeDate, tradeDate);
                properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
                properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
                properties.Set(TradeProp.TradeId, tradeId);
                properties.Set(TradeProp.AsAtDate, DateTime.Today);
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                //Create the swap.
                var trade = SwaptionPricer.CreateSwaptionTrade(swaptionTerms, null, swap);
                //var swaptionPricer = new InterestRateSwaptionPricer(logger, cache, (Swaption)swaption.Item, baseParty);
                //var trade = new Trade();
                //XsdClassesFieldResolver.Trade_SetSwap(trade, swap);
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();
                //Add the vol surface - > how to handle the default?
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.NameSpace, nameSpace);
                var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
                cache.SaveObject(trade, uniqueIDentifier, properties);
                logger.LogDebug("Created new interest rate swaption trade: {0}", identifier.UniqueIdentifier);
                return identifier.UniqueIdentifier;
            }
            return "Invalid Underlying Swap";
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="swaptionTerms">Swaption details range:
        /// <para>DateTime ExpirationDate</para>	
        /// <para>string   ExpirationDateCalendar</para>	
        /// <para>string   ExpirationDateBusinessDayAdjustments</para>	
        /// <para>double   EarliestExerciseTime;//TimeSpan.FromDays()</para>	
        /// <para>double   ExpirationTime;//TimeSpan.FromDays()</para>	
        /// <para>bool     AutomaticExcercise</para>	        
        /// <para>DateTime PaymentDate</para>	
        /// <para>string   PaymentDateCalendar</para>	
        /// <para>string   PaymentDateBusinessDayAdjustments</para>	
        /// <para>string   PremiumPayer</para>	
        /// <para>string   PremiumReceiver</para>	
        /// <para>decimal  Premium</para>	
        /// <para>string   PremiumCurrency</para>	
        /// <para>decimal  StrikeRate</para>	
        /// <para>decimal  Volatility</para>	
        /// </param>
        ///<param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	CBA</para>
        /// <para>Receiver:	ABN</para>
        /// <para>NotionalAmount:	10000000.00</para>
        /// <para>EffectiveDate:	13/03/2010</para>
        /// <para>MaturityDate:	4/02/2015</para>
        /// <para>FirstRegularPeriodStartDate</para>	
        /// <para>LastRegularPeriodEndDate</para>	
        /// <para>AdjustedType:	Adjusted</para>
        /// <para>InitialStubType:	</para>
        /// <para>FirstCouponType:	Full</para>
        /// <para>CouponPaymentType:	InArrears</para>
        /// <para>FinalStubType	</para>
        /// <para>RollConvention:	4</para>
        /// <para>LegType:	Floating</para>
        /// <para>Currency:	AUD</para>
        /// <para>CouponOrLastResetRate:	0.05</para>
        /// <para>FloatingRateSpread:	0.0000000</para>
        /// <para>PaymentFrequency:	6m</para>
        /// <para>DayCount:	ACT/360</para>
        /// <para>PaymentCalendar:	AUSY-GBLO</para>
        /// <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        /// <para>FixingCalendar:	AUSY-GBLO</para>
        /// <para>FixingBusinessDayAdjustments:	NONE</para>
        /// <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        /// <para>StubIndexTenor</para>	
        /// <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// </param>
        ///<param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// See above for a description of the range object.</param>
        ///<param name="properties">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateInterestRateSwaptionWithProperties(SwaptionParametersRange swaptionTerms, SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange, NamedValueSet properties)
        {
            return CreateInterestRateSwaptionWithProperties(Logger, Cache, NameSpace, swaptionTerms, leg1ParametersRange, leg2ParametersRange, properties);
        }

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="logger">The logger.</param>
        ///  <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="swaptionTerms">Swaption details range:
        ///  <para>DateTime ExpirationDate</para>	
        ///  <para>string   ExpirationDateCalendar</para>	
        ///  <para>string   ExpirationDateBusinessDayAdjustments</para>	
        ///  <para>double   EarliestExerciseTime;//TimeSpan.FromDays()</para>	
        ///  <para>double   ExpirationTime;//TimeSpan.FromDays()</para>	
        ///  <para>bool     AutomaticExcercise</para>	        
        ///  <para>DateTime PaymentDate</para>	
        ///  <para>string   PaymentDateCalendar</para>	
        ///  <para>string   PaymentDateBusinessDayAdjustments</para>	
        ///  <para>string   PremiumPayer</para>	
        ///  <para>string   PremiumReceiver</para>	
        ///  <para>decimal  Premium</para>	
        ///  <para>string   PremiumCurrency</para>	
        ///  <para>decimal  StrikeRate</para>	
        ///  <para>decimal  Volatility</para>	
        ///  </param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	CBA</para>
        ///  <para>Receiver:	ABN</para>
        ///  <para>NotionalAmount:	10000000.00</para>
        ///  <para>EffectiveDate:	13/03/2010</para>
        ///  <para>MaturityDate:	4/02/2015</para>
        ///  <para>FirstRegularPeriodStartDate</para>	
        ///  <para>LastRegularPeriodEndDate</para>	
        ///  <para>AdjustedType:	Adjusted</para>
        ///  <para>InitialStubType:	</para>
        ///  <para>FirstCouponType:	Full</para>
        ///  <para>RollConvention:	4</para>
        ///  <para>LegType:	Floating</para>
        ///  <para>Currency:	AUD</para>
        ///  <para>CouponOrLastResetRate:	0.05</para>
        ///  <para>FloatingRateSpread:	0.0000000</para>
        ///  <para>PaymentFrequency:	6m</para>
        ///  <para>DayCount:	ACT/360</para>
        ///  <para>CouponPaymentType:	InArrears</para>
        ///  <para>FinalStubType	</para>
        ///  <para>PaymentCalendar:	AUSY-GBLO</para>
        ///  <para>PaymentBusinessDayAdjustments:	FOLLOWING</para>
        ///  <para>FixingCalendar:	AUSY-GBLO</para>
        ///  <para>FixingBusinessDayAdjustments:	NONE</para>
        ///  <para>ForecastIndexName:	AUD-BBR-BBSW-6M</para>
        ///  <para>StubIndexTenor</para>	
        ///  <para>DiscountCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Highlander.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        /// <param name="properties">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateInterestRateSwaptionWithProperties(ILogger logger, ICoreCache cache, String nameSpace,
            SwaptionParametersRange swaptionTerms, SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange, NamedValueSet properties)
        {
            properties.Set(TradeProp.EffectiveDate, leg2ParametersRange.EffectiveDate);
            properties.Set(TradeProp.MaturityDate, leg1ParametersRange.MaturityDate);
            var tradeDate = properties.GetValue<DateTime>(TradeProp.TradeDate, true);
            var tradeId = properties.GetValue<string>(TradeProp.TradeId, true);
            var baseParty = properties.GetValue<string>(TradeProp.BaseParty, false);
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            
            if (baseParty == null)
            {
                //baseParty = properties.GetValue<string>("Party1", true);
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            }
            var counterParty = properties.GetValue<string>(TradeProp.CounterPartyName, false);
            if (counterParty == null)
            {
                //counterParty = properties.GetValue<string>("Party2", true);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }  
            //Create the swap.
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(logger, cache, nameSpace, leg1ParametersRange, null, leg2ParametersRange, null, null, null, null);
            if (swap != null)
            {
                var trade = SwaptionPricer.CreateSwaptionTrade(swaptionTerms, null, swap);
                var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.swaption, ProductTypeSimpleEnum.InterestRateSwaption, tradeId, tradeDate, sourceSystem);
                var curves = trade.Item.GetRequiredPricingStructures().ToArray();
                var currencies = trade.Item.GetRequiredCurrencies().ToArray();
                properties.Set(TradeProp.RequiredPricingStructures, curves);
                properties.Set(TradeProp.RequiredCurrencies, currencies);            
                properties.Set(EnvironmentProp.Function, FunctionProp.Trade.ToString());
                properties.Set(EnvironmentProp.NameSpace, nameSpace);
                properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
                var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
                cache.SaveObject(trade, uniqueIDentifier, properties);
                logger.LogDebug("Created new interest rate swaption trade: {0}", identifier.UniqueIdentifier);
                return identifier.UniqueIdentifier;
            }
            return "Invalid Underlying Swap";
        }

        #endregion

        #endregion

        #region Vanilla FX Options

        /// <summary>
        /// Creates an fx vanilla option trade.
        /// </summary>
        /// <param name="fxOptionParametersRange">THe parameters range class. This contains: 
        /// string TradeId;
        /// DateTime TradeDate;
        /// bool IsBuyerBase;
        /// string BuyerPartyReference;
        /// string SellerPartyReference;
        /// string FaceOnCurrency;
        /// string OptionOnCurrency;
        /// string Period;
        /// DateTime ExpiryDate;
        /// DateTime ExpiryTime;
        /// string ExpiryBusinessCenter;
        /// string CutName;
        /// decimal PutCurrencyAmount;
        /// string PutCurrency;
        /// decimal CallCurrencyAmount;
        /// string CallCurrency;
        /// string StrikeQuoteBasis;
        /// DateTime valueDate;
        /// Decimal StrikePrice;
        /// Decimal? PremiumAmount: there may or may not be a premium.
        /// string PremiumCurrency: there may or may not be a premium.
        /// DateTime? PremiumSettlementDate: there may or may not be a premium.
        /// </param>
        /// <param name="properties">THe properties. </param>
        /// <returns></returns>
        public string CreateVanillaFxOption(FxOptionParametersRange fxOptionParametersRange, NamedValueSet properties)
        {
            var cutname = new CutName {Value = fxOptionParametersRange.CutName};
            List<FxOptionPremium> fxOptionPremiumList = null;
            if (fxOptionParametersRange.PremiumCurrency != null && fxOptionParametersRange.PremiumAmount != null && fxOptionParametersRange.PremiumSettlementDate != null)
            {
                var fxOptionPremium = new FxOptionPremium
                                      {
                                          paymentAmount =
                                              MoneyHelper.GetNonNegativeAmount((decimal) fxOptionParametersRange.PremiumAmount,
                                                                    fxOptionParametersRange.PremiumCurrency),
                                          quote = new PremiumQuote()
                                      };
                var basis = EnumHelper.Parse<PremiumQuoteBasisEnum>(fxOptionParametersRange.PremiumQuoteBasis);
                //settlementInformation =
                //(DateTime) fxOptionParametersRange.PremiumSettlementDate,
                if(fxOptionParametersRange.PremiumValue != null)
                {
                    fxOptionPremium.quote.quoteBasis = basis;
                    fxOptionPremium.quote.quoteBasisSpecified = true;
                    fxOptionPremium.quote.value = (decimal)fxOptionParametersRange.PremiumValue;
                    fxOptionPremium.quote.valueSpecified = true;
                }
                fxOptionPremiumList = new List<FxOptionPremium> { fxOptionPremium };
            }
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.ForeignExchange_VanillaOption));
            var result = CreateVanillaFxOption(Logger, Cache, NameSpace, fxOptionParametersRange.TradeId, fxOptionParametersRange.TradeDate, fxOptionParametersRange.IsBuyerBase,
                fxOptionParametersRange.BuyerPartyReference, fxOptionParametersRange.SellerPartyReference,
            fxOptionParametersRange.FaceOnCurrency, fxOptionParametersRange.OptionOnCurrency, fxOptionParametersRange.Period, fxOptionParametersRange.ExpiryDate,
            fxOptionParametersRange.ExpiryTime, fxOptionParametersRange.ExpiryBusinessCenter, cutname,
            fxOptionParametersRange.PutCurrencyAmount, fxOptionParametersRange.PutCurrency, fxOptionParametersRange.CallCurrencyAmount, fxOptionParametersRange.CallCurrency,
            fxOptionParametersRange.StrikeQuoteBasis, fxOptionParametersRange.ValueDate,
            fxOptionParametersRange.StrikePrice, fxOptionPremiumList, properties);
            return result;
        }

        /// <summary>
        /// Creates an fx option trade.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe nameSpace</param>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isBuyerBase">Is the buer of the option the base party. </param>
        /// <param name="buyerPartyReference">The option buyer.</param>
        /// <param name="sellerPartyReference">The option seller.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="faceOnCurrency">The amount of currency1.</param>
        /// <param name="callCurrency">The call currency. </param>
        /// <param name="strikeQuoteBasis">The strike quoteBasis.</param>
        /// <param name="optionOnCurrency">The option currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="putCurrencyAmount">The put currency amount.</param>
        /// <param name="strikePrice">The strik Rate.</param>
        /// <param name="premia">The [Optional] premia.</param>
        /// <param name="expiryBusinessCenter">The expiry business centyer. </param>
        /// <param name="cutName">THe cut name: e.g. New York </param>
        /// <param name="putCurrency">The put currency. </param>
        /// <param name="period">The strike period. </param>
        /// <param name="expiryDate">The expiry date. </param>
        /// <param name="time">The expiry time. </param>
        /// <param name="callCurrencyAmount">The call currency amount. </param>
        /// <param name="properties">The properties. </param>
        /// <returns></returns>
        public string CreateVanillaFxOption(ILogger logger, ICoreCache cache, String nameSpace, string tradeId, DateTime tradeDate, 
        bool isBuyerBase, string buyerPartyReference, string sellerPartyReference, string faceOnCurrency, 
        string optionOnCurrency, string period, DateTime expiryDate, DateTime time, string expiryBusinessCenter,
        CutName cutName, decimal putCurrencyAmount, string putCurrency, decimal callCurrencyAmount,
        string callCurrency, string strikeQuoteBasis, DateTime valueDate, Decimal strikePrice, List<FxOptionPremium> premia, NamedValueSet properties)
        {
            const ProductTypeSimpleEnum productType = ProductTypeSimpleEnum.FxOption;
            var basis = EnumHelper.Parse<StrikeQuoteBasisEnum>(strikeQuoteBasis);
            if (properties == null)
            {
                properties = new NamedValueSet();
            }
            var sourceSystem = properties.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            if (isBuyerBase)
            {
                properties.Set(TradeProp.Party1, buyerPartyReference);
                properties.Set(TradeProp.Party2, sellerPartyReference);
            }
            else
            {
                properties.Set(TradeProp.Party1, sellerPartyReference);
                properties.Set(TradeProp.Party2, buyerPartyReference);
            }
            properties.Set(TradeProp.BaseParty, TradeProp.Party1);
            properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            properties.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            properties.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.EffectiveDate, expiryDate);
            properties.Set(TradeProp.MaturityDate, valueDate);
            properties.Set(TradeProp.ProductType, productType.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.fxOption.ToString());
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(TradeProp.TradeSource, sourceSystem);
            properties.Set(EnvironmentProp.SourceSystem, sourceSystem);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            decimal? spotRate = null;
            double? rate = properties.GetValue<double>("SpotRate", false);
            if(rate != null) spotRate = Convert.ToDecimal(rate);
            var cashSettlement = properties.GetValue<bool>("CashSettlement", false);
            QuotedCurrencyPair quotedCurrencyPair = null;
            DateTime? fixingDate = null;
            Currency settlementCurrency = null;
            if (cashSettlement)
            {
                var cashSettlementCurrency = properties.GetString("SettlementCurrency", true);
                settlementCurrency = CurrencyHelper.Parse(cashSettlementCurrency);
                fixingDate = properties.GetValue<DateTime>("FixingDate", true);
                var currencyPair = properties.GetString("QuotedCurrencyPair", true);
                var quoteCurrencies = currencyPair.Split('-');
                var quoteBasis = properties.GetString("QuoteBasis", true);
                var quoteBasisEnum = EnumHelper.Parse<QuoteBasisEnum>(quoteBasis);
                quotedCurrencyPair = QuotedCurrencyPair.Create(quoteCurrencies[0], quoteCurrencies[1], quoteBasisEnum);
            }
            var trade = VanillaEuropeanFxOptionPricer.CreateVanillaFxOption(tradeId, tradeDate, TradeProp.Party1, TradeProp.Party2, 
            null, period, expiryDate, time, expiryBusinessCenter, cutName, putCurrencyAmount, putCurrency, callCurrencyAmount, callCurrency, 
            basis, valueDate, strikePrice, spotRate, cashSettlement, settlementCurrency, fixingDate, quotedCurrencyPair, premia);
            var identifier = new Reporting.Identifiers.V5r3.TradeIdentifier(ItemChoiceType15.fxOption, productType, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            properties.Set(TradeProp.RequiredPricingStructures, curves);
            properties.Set(TradeProp.RequiredCurrencies, currencies);
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            var uniqueIDentifier = nameSpace + "." + identifier.UniqueIdentifier;
            cache.SaveObject(trade, uniqueIDentifier, properties);
            logger.LogDebug("Created new fx forward trade: {0}", identifier.UniqueIdentifier);
            return identifier.UniqueIdentifier;
        }

        #endregion

        #region Strucutred Trade

        #endregion

        #endregion
    }
}
