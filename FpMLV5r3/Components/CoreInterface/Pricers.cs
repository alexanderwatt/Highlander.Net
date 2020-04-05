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
using System.Linq;
using System.Runtime.InteropServices;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Interface.V5r3.Helpers;
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.Markets;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.CurveEngine.V5r3.PricingStructures.Helpers;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.Models.V5r3.Assets;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.NamedValues;
using Highlander.ValuationEngine.V5r3;
using Highlander.ValuationEngine.V5r3.Generators;
using Highlander.ValuationEngine.V5r3.Helpers;
using Highlander.ValuationEngine.V5r3.Pricers;
using TradeIdentifier = Highlander.Reporting.Identifiers.V5r3.TradeIdentifier;
using XsdClassesFieldResolver = Highlander.Reporting.V5r3.XsdClassesFieldResolver;

#endregion

namespace Highlander.Core.Interface.V5r3
{
    public partial class PricingCache
    {      
        #region Query Functions

        /// <summary>
        /// Returns the information for the trade matching the tradeId supplied.
        /// </summary>
        /// <param name="valuationId">The valuation Id.</param>
        /// <returns>A matrix of flattened trade data.</returns>
        public object DisplaySwapValuationData(string valuationId)
        {
            return ValService.DisplaySwapValuationData(valuationId);
        }

        /// <summary>
        /// Returns the information for the trade matching the tradeId supplied.
        /// </summary>
        /// <param name="valuationId">The valuation Id.</param>
        /// <returns>A matrix of flattened trade data.</returns>
        public object DisplayFraValuationData(string valuationId)
        {
            return ValService.DisplayFraValuationData(valuationId);
        }

        /// <summary>
        /// Returns the header information for all trades matching the query properties.
        /// </summary>
        /// <param name="queryProperties">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public List<string> QueryTradeIds(NamedValueSet queryProperties)
        {
            return ValService.QueryTradeIds(queryProperties);
        }

        /// <summary>
        /// Returns the header information for all trades matching the query properties.
        /// </summary>
        /// <param name="queryProperties">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public List<TradeQueryData> QueryTradeData(NamedValueSet queryProperties)
        {
            return ValService.QueryTrades(queryProperties);
        }


        /// <summary>
        /// Returns the header information for all trades matching the query properties.
        /// </summary>
        /// <param name="query">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public object[,] QueryCurves(object[,] query)
        {
            return Engine.CurvesQuery(query);
        }

        /// <summary>
        /// Lists all valuation reports currently in memory.
        /// </summary>
        /// <param name="requestProperties">The request Properties.</param>
        /// <param name="metricToReturn">The metric To Return.</param>
        /// <returns>A list identifiers that can be uses as handles.</returns>
        public IDictionary<string, decimal> ShowValuationReports(NamedValueSet requestProperties, string metricToReturn)
        {
            if (requestProperties == null) throw new ArgumentNullException(nameof(requestProperties));
            return ValService.ShowValuationReports(requestProperties, metricToReturn);
        }

        /// <summary>
        /// Gets the bonds conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetQASUniqueIdentifiers(NamedValueSet requestProperties)
        {
            return ValService.GetQasUniqueIdentifiers(requestProperties);
        }

        /// <summary>
        /// Saves the qas to files.
        /// </summary>
        /// <param name="identifiers">An array of identifiers. This is the identifier returned by the qGetFixedIncomeISINs function. </param>
        /// <param name="directoryPath">The path to save to. </param>
        /// <param name="isShortName">If <true>isShortName</true> is true> then the id is of the form: Curve name.
        /// Otherwise it is of the form Orion.V5r3.QuotedAssetSet.CurveName.</param>
        /// <returns></returns>
        public string SaveQAS(List<string> identifiers, string directoryPath, bool isShortName)
        {
            var result = ValService.SaveQas(identifiers, directoryPath, isShortName);
            return result;
        }

        /// <summary>
        /// Gets the bonds conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetFixedIncomeUniqueIdentifiers(NamedValueSet requestProperties)
        {
            return ValService.GetFixedIncomeUniqueIdentifiers(requestProperties);
        }

        /// <summary>
        /// Gets the bonds conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetFixedIncomeDescription(NamedValueSet requestProperties)
        {
            return ValService.GetFixedIncomeDescription(requestProperties);
        }

        /// <summary>
        /// Gets the bond ISINs conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetFixedIncomeISINs(NamedValueSet requestProperties)
        {
            return ValService.GetFixedIncomeISINs(requestProperties);
        }

        /// <summary>
        /// Gets the equities conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetEquityUniqueIdentifiers(NamedValueSet requestProperties)
        {
            return ValService.GetEquityUniqueIdentifiers(requestProperties);
        }

        /// <summary>
        /// Gets the equities conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetEquityDescription(NamedValueSet requestProperties)
        {
            return ValService.GetEquityDescription(requestProperties);
        }

        /// <summary>
        /// Gets the equity ISINs conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public IList<string> GetEquityISINs(NamedValueSet requestProperties)
        {
            return ValService.GetEquityISINs(requestProperties);
        }

        /// <summary>
        /// Saves the equities to files.
        /// </summary>
        /// <param name="identifiers">An array of identifiers. This is the identifier returned by the qGetFixedIncomeISINs function. </param>
        /// <param name="directoryPath">The path to save to. </param>
        /// <param name="isShortName">If <true>isShortName</true> is true> then the id is of the form: FixedIncome.XXXX.YY.zz-zz-zzzz.
        /// Otherwise it is of the form Orion.ReferenceData.Equity.XXXX.YY.zz-zz-zzzz.</param>
        /// <returns></returns>
        public string SaveEquities(List<string> identifiers, string directoryPath, bool isShortName)
        {
            var result = ValService.SaveEquities(identifiers, directoryPath, isShortName);
            return result;
        }

        /// <summary>
        /// Gets the trade in the default name space.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public Trade GetTrade(string identifier)
        {
            return ValService.GetTrade(NameSpace, identifier);
        }

        #endregion

        #region Information Functions

        ///<summary>
        /// Gets the version of the assembly
        ///</summary>
        ///<returns></returns>
        public string GetCurveEngineVersionInfo() => Information.GetVersionInfo();

        /// <summary>
        /// A function to return the list of valid asset measures. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of asset measure types.</returns>
        public List<string> GetAssetMeasureTypes()
        {
            return Enum.GetNames(typeof(AssetMeasureEnum)).ToList();
        }

        /// <summary>
        /// A function to return the list of valid price quote units. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of valid price quote units.</returns>
        public List<string> GetPriceQuoteUnits()
        {
            return Enum.GetNames(typeof(PriceQuoteUnitsEnum)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> GetRateMetrics()
        {
            return Enum.GetNames(typeof(RateMetrics)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented products.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedProductTypes()
        {
            var result = (from ProductTypeSimpleEnum value in Enum.GetValues(typeof(ProductTypeSimpleEnum))
                                                          where (value != ProductTypeSimpleEnum.Undefined) && (value != ProductTypeSimpleEnum._LAST_)
                                                          select value.ToString()).ToList();
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented products.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedProductTaxonomyTypes()
        {
            var indices = Enum.GetValues(typeof(ProductTaxonomyEnum)).Cast<ProductTaxonomyEnum>().Select(ProductTaxonomyScheme.GetEnumString).Where(index => index != null).ToList();
            return indices;
        }

        #endregion

        #region Generic Functions

        /// <summary>
        /// Gets the curve names necessary to value the trade.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="filename">The filename</param>
        /// <returns></returns>
        public string SaveTradeToFile(string uniqueTradeId, string filename)
        {
            return ValService.SaveTradeToFile(uniqueTradeId, filename);
        }

        /// <summary>
        /// Gets the curve names necessary to value the trade.
        /// </summary>
        /// <param name="uniqueValuationId">The unique valuation identifier.</param>
        /// <param name="filename">The filename</param>
        /// <returns></returns>
        public string SaveValuationToFile(string uniqueValuationId, string filename)
        {
            return ValService.SaveValuationToFile(uniqueValuationId, filename);
        }

        /// <summary>
        /// Gets the curve names necessary to value the trade.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <returns></returns>
        public object[,] GetRequiredCurves(string uniqueTradeId)
        {
            return ValService.GetRequiredCurves(uniqueTradeId);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metrics">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        ///  <returns></returns>
        public object[,] ViewExpectedCashFlows(string uniqueTradeId, string reportingParty, List<string> metrics, string reportingCurrency, string market,
            DateTime valuationDate)
        {
            return RangeHelper.ListedRangeToMatrix(ValService.ViewExpectedCashFlows(uniqueTradeId, reportingParty, metrics, reportingCurrency, market, valuationDate));
        }

        /// <summary>
        /// Views a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        public object[,] ViewTrade(string uniqueTradeId) => ValService.ViewTrade(uniqueTradeId);

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metrics">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMap">The curve mapping range.</param>
        /// <returns></returns>
        public string ValueTrade(string uniqueTradeId, string reportingParty, List<string> metrics, string reportingCurrency,
            DateTime valuationDate, List<Pair<string, string>> curveMap)//TODO change to a marketName
        {
            return ValService.ValueTrade(TradeSourceType.SpreadSheet, uniqueTradeId, reportingParty, metrics, reportingCurrency, valuationDate, curveMap);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metrics">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns></returns>
        public string ValueTradeFromMarket(string uniqueTradeId, string reportingParty, List<string> metrics, string reportingCurrency, string market,
            DateTime valuationDate)
        {
            return ValService.ValueTradeFromMarket(uniqueTradeId, reportingParty, metrics, reportingCurrency, market, valuationDate);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="valuationPortfolioId">The valuation portfolio Id</param>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metrics">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="markets">The markets as a vertical range.</param>
        /// <returns></returns>
        public string ValueTradeFromMarkets(string valuationPortfolioId, string uniqueTradeId, string reportingParty, 
            List<string> metrics, string reportingCurrency, List<string> markets, DateTime valuationDate)
        {
            var results = ValService.ValueTradeFromMarkets(valuationPortfolioId, uniqueTradeId, reportingParty, metrics, reportingCurrency, markets, valuationDate);
            return results;
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
            return ValService.ValueWithDetail(uniqueTradeId, reportingParty, reportingCurrency, market, valuationDate);
        }

        /// <summary>
        /// Views the calculated data in the valuation report.
        /// </summary>
        /// <param name="valuationId">THe id of the report to view.</param>
        /// <returns></returns>
        public object[,] ViewValuationReport(string valuationId) => ValService.ViewValuationReport(valuationId);

        #endregion

        #region ParRate and NPV

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns></returns>
        public double GetNPV(string uniqueTradeId, string reportingParty, string reportingCurrency, DateTime valuationDate, string market)//Use a range mapping area
        {
            return ValService.GetNPV(uniqueTradeId, reportingParty, reportingCurrency, valuationDate, market);
        }

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMap">The curve mapping range.</param>
        /// <returns></returns>
        public double GetNPVWithSpecifiedCurves(string uniqueTradeId, string reportingParty, string reportingCurrency, 
            DateTime valuationDate, List<Pair<string, string>> curveMap)//Use a range mapping area
        {
            return ValService.GetNPVWithSpecifiedCurves(uniqueTradeId, reportingParty, reportingCurrency, valuationDate, curveMap);
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns></returns>
        public double GetParRate(string uniqueTradeId, string reportingParty, string reportingCurrency, DateTime valuationDate, string market)//Use a range mapping area
        {
            return ValService.GetParRate(uniqueTradeId, reportingParty, reportingCurrency, valuationDate, market);
        }

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMap">The curve mapping range.</param>
        /// <returns></returns>
        public double GetParRateWithSpecifiedCurves(string uniqueTradeId, string reportingParty, DateTime valuationDate, List<Pair<string, string>> curveMap)
        {
            return ValService.GetParRateWithSpecifiedCurves(uniqueTradeId, reportingParty, valuationDate, curveMap);
        }

        #endregion

        #region Term Deposit

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
        /// <param name="dayCount">The dayCount basis. Must be a valid type.</param>
        /// <returns></returns>
        public string CreateTermDeposit(string tradeId, bool isLenderBase, string lenderParty,
            string borrowerParty, DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount)
        {
            var result = ValService.CreateTermDeposit(tradeId, isLenderBase, lenderParty, borrowerParty, tradeDate, 
                startDate, maturityDate, currency, notionalAmount, fixedRate, dayCount);
            return result;
        }

        /// <summary>
        /// Creates a simple term deposit
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="notionalAmount">The notional lent/borrowed.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="dayCount">The dayCount basis. Must be a valid type.</param>
        /// <param name="properties">The properties2DRange. This must contain: 
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateSimpleDepositWithProperties(string tradeId,
            DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount,
            NamedValueSet properties)
        {
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_TermDeposit));
            var tradeIdentifier = new TradeIdentifier(properties);
            var trade = TermDepositPricer.CreateSimpleTermDepositTrade(tradeIdentifier.Id, ProductTypeSimpleEnum.TermDeposit.ToString(), tradeDate, startDate,
                maturityDate, currency, Convert.ToDecimal(notionalAmount), Convert.ToDecimal(fixedRate), dayCount);
            var identifier = NameSpace + "." + tradeIdentifier.UniqueIdentifier;
            Engine.Cache.SaveObject(trade, identifier, properties);
            return identifier;
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
        /// <param name="dayCount">The dayCount basis. Must be a valid type.</param>
        /// <param name="properties">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateDepositWithProperties(DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount,
            NamedValueSet properties)
        {
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_TermDeposit));
            return ValService.CreateDepositWithProperties(tradeDate, startDate, maturityDate, currency, notionalAmount, fixedRate, dayCount, properties);
        }


        ///// <summary>
        ///// Views a trade that has already been created.
        ///// </summary>
        ///// <param name="tradeId">The unique identifier.</param>
        ///// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        //public object[,] ViewDepositCashFlows(string tradeId, string reportingParty)//TODO May need to calculate this first to get the function working!
        //{
        //    var id = new Orion.Identifiers.TradeIdentifier(ItemChoiceType15.termDeposit, ProductTypeSimpleEnum.TermDeposit, tradeId);
        //    var tradeItem = Engine.Cache.LoadItem<Trade>(NameSpace + "." + id.UniqueIdentifier);
        //    var properties = tradeItem.AppProps;
        //    var party1 = properties.GetValue<string>(TradeProp.Party1, true);
        //    var party2 = properties.GetValue<string>(TradeProp.Party2, true);
        //    var baseParty = reportingParty == party1 ? "Party1" : "Party2";
        //    var trade = tradeItem.Data as Trade;
        //    if (trade?.Item is TermDeposit termDeposit)
        //    {
        //        var pricer = new TermDepositPricer(Engine.Logger, Engine.Cache, termDeposit, baseParty);
        //        pricer.OrderedPartyNames.Add(party1);
        //        pricer.OrderedPartyNames.Add(party2);
        //        object[,] report = new TermDepositReporter().DoXLReport(pricer);
        //        return report;
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// Views a trade that has already been created.
        ///// </summary>
        ///// <param name="tradeId">The trade identifier.</param>
        ///// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        //public object[,] ViewTermDeposit(string tradeId, string reportingParty)//TODO May need to calculate this first to get the function working!
        //{
        //    object[,] report = null;
        //    var id = new Orion.Identifiers.TradeIdentifier(ItemChoiceType15.termDeposit, ProductTypeSimpleEnum.TermDeposit, tradeId);
        //    var tradeItem = Engine.Cache.LoadItem<Trade>(NameSpace + "." + id.UniqueIdentifier);
        //    var properties = tradeItem.AppProps;
        //    var party1 = properties.GetValue<string>(TradeProp.Party1, true);
        //    var party2 = properties.GetValue<string>(TradeProp.Party2, true);
        //    var baseParty = reportingParty == party1 ? "Party1" : "Party2";
        //    if (tradeItem.Data is Trade trade)
        //    {
        //        if (trade.Item is TermDeposit termDeposit)
        //        {
        //            var pricer = new TermDepositPricer(Engine.Logger, Engine.Cache, termDeposit, baseParty);
        //            pricer.OrderedPartyNames.Add(party1);
        //            pricer.OrderedPartyNames.Add(party2);
        //            var tempDeposit = pricer.BuildTheProduct();
        //            report = new TermDepositReporter().DoReport(tempDeposit, properties);
        //        }
        //    }
        //    return report;
        //}

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="tradeId">The trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metrics">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="discountCurveId">The discountCurve identifier.</param>
        /// <returns></returns>
        public string ValueTermDeposit(string tradeId, string reportingParty, List<string> metrics, string reportingCurrency,
            DateTime valuationDate, string discountCurveId)
        {
            var id = new TradeIdentifier(ItemChoiceType15.termDeposit, ProductTypeSimpleEnum.TermDeposit, tradeId);
            var tradeItem = Engine.Cache.LoadItem<Trade>(NameSpace + "." + id.UniqueIdentifier);
            var properties = tradeItem.AppProps;
            var party1 = properties.GetValue<string>(TradeProp.Party1, true);
            var party2 = properties.GetValue<string>(TradeProp.Party2, true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            if (tradeItem.Data is Trade trade)
            {
                var termDeposit = trade.Item as TermDeposit;
                var pricer = new TermDepositPricer(Engine.Logger, Engine.Cache, termDeposit, baseParty);

                //Get the market
                //
                var curve = Engine.GetCurve(discountCurveId, false);
                var market = new MarketEnvironment();
                if (termDeposit != null)
                {
                    var curveNames = termDeposit.GetRequiredPricingStructures();//TODO this is a quick fix.
                    market.AddPricingStructure(curveNames[0], curve);//TODO fix this.        
                }
                var controller = TradePricer.CreateInstrumentModelData(metrics, valuationDate, market, reportingCurrency, baseParty);
                var assetValuationReport = pricer.Calculate(controller);
                //Build the val report properties
                //
                var valProperties = new NamedValueSet();
                valProperties.Set(ValueProp.PortfolioId, CurveConst.LOCAL_USER);
                valProperties.Set(ValueProp.BaseParty, reportingParty);
                valProperties.Set(CurveProp.Market, CurveConst.LOCAL_USER);
                valProperties.Set(CurveProp.MarketDate, valuationDate);
                valProperties.Set(ValueProp.CalculationDateTime, valuationDate.ToUniversalTime());
                //The unique identifier for the valuation report
                var valuationIdentifier = new ValuationReportIdentifier(valProperties);
                var isParty1Base = reportingParty == party1;
                var valuationId = new Highlander.ValuationEngine.V5r3.Valuations.Valuation().CreateTradeValuationReport(Engine.Cache, NameSpace, valuationIdentifier.UniqueIdentifier, party1, party2, isParty1Base, trade, market.GetFpMLData(), assetValuationReport, valProperties);
                return valuationId;
            }
            return null;
        }

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="tradeId">The trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metrics">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="discountCurveId">The discountCurve identifier.</param>
        /// <returns></returns>
        public string TestValueTermDeposit(string tradeId, string reportingParty, List<string> metrics, string reportingCurrency,
            DateTime valuationDate, string discountCurveId)
        {
            var id = new TradeIdentifier(ItemChoiceType15.termDeposit, ProductTypeSimpleEnum.TermDeposit, tradeId);
            var tradeItem = Engine.Cache.LoadItem<Trade>(NameSpace + "." + id.UniqueIdentifier);
            var properties = tradeItem.AppProps;
            var party1 = properties.GetValue<string>(TradeProp.Party1, true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            if (tradeItem.Data is Trade trade)
            {
                var pricer = new TradePricer(Engine.Logger, Engine.Cache, NameSpace, null, trade, properties);
                //Get the market
                var curve = Engine.GetCurve(discountCurveId, false);
                var market = new MarketEnvironment();
                if (trade.Item is TermDeposit termDeposit)
                {
                    var curveNames = termDeposit.GetRequiredPricingStructures();//TODO this is a quick fix.
                    market.AddPricingStructure(curveNames[0], curve);//TODO fix this.        
                }
                var controller = TradePricer.CreateInstrumentModelData(metrics, valuationDate, market, reportingCurrency, baseParty);
                var assetValuationReport = pricer.Price(controller, ValuationReportType.Full);
                //Build the val report properties
                var valProperties = new NamedValueSet();
                valProperties.Set(ValueProp.PortfolioId, tradeId);
                valProperties.Set(ValueProp.BaseParty, reportingParty);
                valProperties.Set(ValueProp.CalculationDateTime, valuationDate.ToUniversalTime());
                //The unique identifier for the valuation report
                var valuationIdentifier = new ValuationReportIdentifier(valProperties);
                //var isParty1Base = reportingParty == party1 ? true : false;
                Engine.Cache.SaveObject(assetValuationReport, NameSpace + "." + valuationIdentifier.UniqueIdentifier, valProperties);
                return valuationIdentifier.UniqueIdentifier;
            }
            return null;
        }

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="uniqueId">The unique identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="discountCurveId">The discountCurve identifier.</param>
        /// <returns></returns>
        public double GetTermDepositNPV(string uniqueId, string reportingParty, string reportingCurrency, DateTime valuationDate, string discountCurveId)
        {
            var tradeItem = Engine.Cache.LoadItem<Trade>(NameSpace + "." + uniqueId);
            var properties = tradeItem.AppProps;
            var party1 = properties.GetValue<string>(TradeProp.Party1, true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            if (tradeItem.Data is Trade trade)
            {
                var termDeposit = trade.Item as TermDeposit;
                //Get the market
                var curve = Engine.GetCurve(discountCurveId, false);
                var market = new MarketEnvironment();
                if (termDeposit != null)
                {
                    var curveNames = termDeposit.GetRequiredPricingStructures();//TODO this is a quick fix.
                    market.AddPricingStructure(curveNames[0], curve);//TODO fix this. 
                }
                var npv = TermDepositPricer.GetNPV(Engine.Logger, Engine.Cache, baseParty, termDeposit, reportingCurrency, valuationDate, market);
                return npv;
            }
            return 0;
        }

        /// <summary>
        /// Values a trade that has already been created. Currently only implemented for deposits
        /// </summary>
        /// <param name="uniqueId">The unique identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="discountCurveId">The discountCurve identifier.</param>
        /// <returns></returns>
        public double GetTermDepositParRate(string uniqueId, string reportingParty, DateTime valuationDate, string discountCurveId)
        {
            var tradeItem = Engine.Cache.LoadItem<Trade>(NameSpace + "." + uniqueId);
            var properties = tradeItem.AppProps;
            var party1 = properties.GetValue<string>(TradeProp.Party1, true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            if (tradeItem.Data is Trade trade)
            {
                var termDeposit = trade.Item as TermDeposit;
                //Get the market
                var curve = Engine.GetCurve(discountCurveId, false);
                var market = new MarketEnvironment();
                if (termDeposit != null)
                {
                    var curveNames = termDeposit.GetRequiredPricingStructures();//TODO this is a quick fix.
                    market.AddPricingStructure(curveNames[0], curve);//TODO fix this. 
                }
                var npv = TermDepositPricer.GetParRate(Engine.Logger, Engine.Cache, baseParty, termDeposit, valuationDate, market);
                return npv;
            }
            return 0;
        }

        #endregion

        #region Equity Transaction

        /// <summary>
        /// Creates an equity transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="platform">The execution platform.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="unitPrice">The unit price.</param>
        /// <param name="unitPriceCurrency">The unit price currency.</param>
        /// <param name="equityIdentifier">The equity identifier. Currently assumed to be of the form:  Orion.ReferenceData.Equity.ANZ.AU </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="numberOfShares">The number of shares in the trade currency.</param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="tradingBook">The trading book.</param>
        /// <returns></returns>
        public string CreateEquityTrade(string tradeId, bool isParty1Buyer, string party1, string party2, string platform, 
            DateTime tradeDate, DateTime effectiveDate, decimal numberOfShares,
            decimal unitPrice, string unitPriceCurrency, string equityIdentifier, string tradingBook)
        {
            var properties = new NamedValueSet();
            properties.Set(TradeProp.Party1, party1);
            properties.Set(TradeProp.Party2, party2);
            if (isParty1Buyer)
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }
            else
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party2);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);
            }
            //Set the pricing information
            properties.Set(TradeProp.EffectiveDate, effectiveDate);
            //Set other properties
            properties.Set(TradeProp.TradingBookName, tradingBook);
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradingBookId, TradeState.Pricing.ToString());
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.EquityTransaction.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.equityTransaction.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Equity_OrdinaryShares));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(TradeProp.EffectiveDate, effectiveDate);
            properties.Set(TradeProp.MaturityDate, effectiveDate);
            properties.Set(TradeProp.Platform, platform);
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EquityProp.ReferenceEquity, equityIdentifier);
            return ValService.CreateEquityTransactionWithProperties(tradeId, isParty1Buyer, tradeDate, numberOfShares, unitPrice, unitPriceCurrency, equityIdentifier, properties);
        }

        /// <summary>
        /// Creates an equity transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="unitPrice">The unit price.</param>
        /// <param name="unitPriceCurrency">The unit price currency.</param>
        /// <param name="equityIdentifier">The equity identifier. Currently assumed to be of the form:  Orion.ReferenceData.Equity.ANZ.AU </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="numberOfShares">The number of shares in the trade currency.</param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="properties">The properties range</param>
        /// <returns></returns>
        public string CreateEquityTradeWithProperties(string tradeId, bool isParty1Buyer, string party1, string party2, DateTime tradeDate, 
            DateTime effectiveDate, decimal numberOfShares,
            decimal unitPrice, string unitPriceCurrency, string equityIdentifier, NamedValueSet properties)
        {
            properties.Set(TradeProp.Party1, party1);
            properties.Set(TradeProp.Party2, party2);
            if (isParty1Buyer)
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }
            else
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party2);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);
            }
            //Set the pricing information
            properties.Set(TradeProp.EffectiveDate, effectiveDate);
            //Set other properties
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.EquityTransaction.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.equityTransaction.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Equity_OrdinaryShares));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(TradeProp.EffectiveDate, effectiveDate);
            properties.Set(TradeProp.MaturityDate, effectiveDate);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(EquityProp.ReferenceEquity, equityIdentifier);
            return ValService.CreateEquityTransactionWithProperties(tradeId, isParty1Buyer, tradeDate, numberOfShares, unitPrice, unitPriceCurrency, equityIdentifier, properties);
        }

        #endregion

        #region Property Transaction

        /// <summary>
        /// Creates a property transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="purchaseAmount">The purchase Amount.</param>
        /// <param name="propertyType">The property type: Residential.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="propertyIdentifier">The property identifier. Currently assumed to be of the form:  Orion.ReferenceData.Property.... </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="paymentDate">The payment Date.</param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="tradingBook">The trading book.</param>
        /// <returns></returns>
        public string CreatePropertyTrade(string tradeId, bool isParty1Buyer, string party1, string party2, DateTime tradeDate, DateTime effectiveDate, decimal purchaseAmount,
            DateTime paymentDate, string propertyType, string currency, string propertyIdentifier, string tradingBook)
        {
            var properties = new NamedValueSet();
            properties.Set(TradeProp.Party1, party1);
            properties.Set(TradeProp.Party2, party2);
            if (isParty1Buyer)
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }
            else
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party2);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);
            }
            //Set the pricing information
            properties.Set(TradeProp.EffectiveDate, effectiveDate);
            properties.Set(TradeProp.PaymentDate, paymentDate);
            //Set other properties
            properties.Set(TradeProp.TradingBookName, tradingBook);
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradingBookId, TradeState.Pricing.ToString());
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.PropertyTransaction.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.propertyTransaction.ToString());
            //TODO Test for property type!
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Property_Commercial));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(TradeProp.EffectiveDate, effectiveDate);
            properties.Set(TradeProp.PaymentDate, paymentDate);
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(PropertyProp.PropertyIdentifier, propertyIdentifier);
            properties.Set(PropertyProp.PropertyType, propertyType);
            return ValService.CreatePropertyTransactionWithProperties(tradeId, isParty1Buyer, tradeDate, effectiveDate, purchaseAmount, 
                paymentDate, currency, propertyIdentifier, tradingBook, properties);
        }

        /// <summary>
        /// Creates a property transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="purchaseAmount">The purchase Amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="propertyIdentifier">The property identifier. Currently assumed to be of the form:  Orion.ReferenceData.Property.... </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="paymentDate">The payment Date.</param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="tradingBook">The trading book.</param>>
        /// /// <param name="propertyType">The property type: Residential.</param>
        /// <param name="namedValueSet">The properties range</param>
        /// <returns></returns>
        public string CreatePropertyTradeWithProperties(string tradeId, bool isParty1Buyer, string party1, string party2, DateTime tradeDate, DateTime effectiveDate, 
            decimal purchaseAmount, DateTime paymentDate, string propertyType, string currency, string propertyIdentifier, string tradingBook, NamedValueSet namedValueSet)
        {
            namedValueSet.Set(TradeProp.Party1, party1);
            namedValueSet.Set(TradeProp.Party2, party2);
            if (isParty1Buyer)
            {
                namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
                namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }
            else
            {
                namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party2);
                namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party1);
            }
            //Set the pricing information
            namedValueSet.Set(TradeProp.EffectiveDate, effectiveDate);
            //Set other properties
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.PropertyTransaction.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.propertyTransaction.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Property_Commercial));
            namedValueSet.Set(TradeProp.TradeDate, tradeDate);
            namedValueSet.Set(TradeProp.EffectiveDate, effectiveDate);
            namedValueSet.Set(TradeProp.MaturityDate, effectiveDate);
            namedValueSet.Set(TradeProp.TradeId, tradeId);
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            namedValueSet.Set(PropertyProp.PropertyIdentifier, propertyIdentifier);
            namedValueSet.Set(PropertyProp.PropertyType, propertyType);
            return ValService.CreatePropertyTransactionWithProperties(tradeId, isParty1Buyer, tradeDate, effectiveDate, purchaseAmount, 
                paymentDate, currency, propertyIdentifier, tradingBook, namedValueSet);
        }

        #endregion

        #region Lease Transaction

        /// <summary>
        /// Creates a lease transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Tenant">Is party1 the tenant. If not then it is the property owner. </param>
        /// <param name="party2">Party 2</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="leaseStartDate"></param>
        /// <param name="currency">The currency.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="leaseExpiryDate"></param>
        /// <param name="startGrossAmount"></param>
        /// <param name="leaseId">The lease identifier.</param>
        /// <param name="referencePropertyIdentifier">The reference property identifier.</param>
        /// <param name="description">The description.</param>
        /// <param name="party1">Party 1</param>
        /// <returns></returns>
        public string CreateLeaseTrade(string tradeId, bool isParty1Tenant, string party1, string party2,
            DateTime tradeDate, DateTime leaseStartDate, string currency, string portfolio, decimal startGrossAmount, string leaseId,
            DateTime leaseExpiryDate, string referencePropertyIdentifier, string description)
        {
            return CreateLeaseTradeWithProperties(tradeId, isParty1Tenant, party1, party2, tradeDate, leaseStartDate,
                currency, portfolio, startGrossAmount, leaseId, leaseExpiryDate, referencePropertyIdentifier, description, null);
        }

        /// <summary>
        /// Creates a lease transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Tenant">Is party1 the tenant. If not then it is the property owner. </param>
        /// <param name="party2">Party 2</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="leaseStartDate"></param>
        /// <param name="currency">The currency.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="leaseExpiryDate"></param>
        /// <param name="startGrossAmount"></param>
        /// <param name="leaseId">The lease identifier.</param>
        /// <param name="referencePropertyIdentifier">The reference property identifier.</param>
        /// <param name="description">The description.</param>
        /// <param name="party1">Party 1</param>
        /// <param name="properties">A trade properties range. This should include the following:
        /// upfrontAmount as a decimal,
        /// paymentDate for any upfront amount,
        /// leaseType,
        /// shopNumber - could be 1A,
        /// unitsOfArea: sqm or sqf,
        /// reviewFrequency as a period e.g. 1Y,
        /// nextReviewDate if known,
        /// reviewChange as a percentage.
        /// </param> 
        /// <returns></returns>
        public string CreateLeaseTradeWithProperties(string tradeId, bool isParty1Tenant, string party1, string party2, 
            DateTime tradeDate, DateTime leaseStartDate, string currency, string portfolio, decimal startGrossAmount, string leaseId, 
            DateTime leaseExpiryDate, string referencePropertyIdentifier, string description, NamedValueSet properties)
        {
            // string reviewFrequency, DateTime nextReviewDate, decimal reviewChange,
            //string leaseType, string shopNumber, string unitsOfArea,
            //decimal upfrontAmount, DateTime paymentDate,
            //Get the values required from the range data.
            NamedValueSet namedValueSet = new NamedValueSet();
            if (properties != null)
            {
                namedValueSet = properties.Clone();
            }

            if (properties != null)
            {
                var upfrontAmount = properties.GetValue(LeaseProp.UpfrontAmount, 0.0m);
                var leaseType = properties.GetValue(LeaseProp.LeaseType, "unspecified");
                var shopNumber = properties.GetValue(LeaseProp.ShopNumber, "unspecified");
                var area = properties.GetValue(LeaseProp.Area, 0.0m);
                var unitsOfArea = properties.GetValue(LeaseProp.UnitsOfArea, "sqm");
                var reviewFrequency = properties.GetValue(LeaseProp.ReviewFrequency, "1Y");
                var nextReviewDate = properties.GetValue(LeaseProp.NextReviewDate, leaseStartDate.AddYears(1));
                var reviewChange = properties.GetValue(LeaseProp.ReviewChange, 0.0m);
                //Set property values
                namedValueSet.Set(TradeProp.Party1, party1);
                namedValueSet.Set(TradeProp.Party2, party2);
                if (isParty1Tenant)
                {
                    namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
                    namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
                }
                else
                {
                    namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party2);
                    namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party1);
                }
                //Set the pricing information
                namedValueSet.Set(TradeProp.EffectiveDate, leaseStartDate);
                //Set other properties
                namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.LeaseTransaction.ToString());
                namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.leaseTransaction.ToString());
                namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Lease_Commercial));
                namedValueSet.Set(TradeProp.TradeDate, tradeDate);
                namedValueSet.Set(TradeProp.EffectiveDate, leaseStartDate);
                namedValueSet.Set(TradeProp.MaturityDate, leaseExpiryDate);
                namedValueSet.Set(TradeProp.TradeId, tradeId);
                namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
                namedValueSet.Set(LeaseProp.ReferenceProperty, referencePropertyIdentifier);
                return ValService.CreateLeaseTransactionWithProperties(tradeId, isParty1Tenant, tradeDate, leaseStartDate,
                    upfrontAmount, leaseStartDate, currency, portfolio, startGrossAmount, leaseId, leaseType, shopNumber, area, unitsOfArea,
                    leaseExpiryDate, reviewFrequency, nextReviewDate, reviewChange, referencePropertyIdentifier, description, namedValueSet);
            }
            return null;
        }

        #endregion

        #region Bond Transaction

        /// <summary>
        /// Creates a bond transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="platform">The execution platform.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="priceHeader">The price header range. This must contain either:
        /// DirtyPrice OR CleanPrice and Accruals.</param>
        /// <param name="priceData">The price data array. These are all decimal values.</param>
        /// <param name="bondIdentifier">The bond identifier. Currently assumed to be of the form:  Orion.ReferenceData.FixedIncome.Corp.ANZ.Fixed.5,25.01-16-14 </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="notional">The notional in the trade currency, which is assumed to be same currency as the coupon. </param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="tradingBook">The trading book.</param>
        /// <returns></returns>
        public string CreateBondTrade(string tradeId, bool isParty1Buyer, string party1, string party2, string platform, DateTime tradeDate, 
            DateTime effectiveDate, decimal notional,
            List<string> priceHeader, List<decimal> priceData, string bondIdentifier, string tradingBook)
        {
            var properties = new NamedValueSet();
            properties.Set(TradeProp.Party1, party1);
            properties.Set(TradeProp.Party2, party2);
            if (isParty1Buyer)
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }
            else
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party2);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);
            }
            //Set the pricing information
            properties.Set(TradeProp.EffectiveDate, effectiveDate);
            //Set other properties
            properties.Set(TradeProp.TradingBookName, tradingBook);
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradingBookId, TradeState.Pricing.ToString());
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.BondTransaction.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.bondTransaction.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_Debt_Government_Fixed_Bullet));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(TradeProp.EffectiveDate, effectiveDate);
            properties.Set(TradeProp.MaturityDate, effectiveDate);
            properties.Set(TradeProp.Platform, platform);
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            if (priceHeader.Count != priceData.Count) return "Price data not correct!";
            var nvs = new NamedValueSet();
            var index = 0;
            foreach (var item in priceHeader)
            {
                nvs.Set(item, priceData[index]);
                index++;
            }
            return ValService.CreateBondTransactionWithProperties(tradeId, isParty1Buyer, tradeDate, notional, nvs, bondIdentifier, properties);
        }

        /// <summary>
        /// Creates a bond transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="priceHeader">THe price header range. This must contain either:
        /// DirtyPrice OR CleanPrice and Accruals.</param>
        /// <param name="priceData">The price data array. These are all decimal values.</param>
        /// <param name="bondIdentifier">The bond identifier. Currently assumed to be of the form:  Orion.ReferenceData.FixedIncome.Corp.ANZ.Fixed.5,25.01-16-14 </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="notional">The notional in the trade currency, which is assumed to be same currency as the coupon. </param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="namedValueSet">The properties range.</param>
        /// <returns></returns>
        public string CreateBondTradeWithProperties(string tradeId, bool isParty1Buyer, string party1, string party2, 
            DateTime tradeDate, DateTime effectiveDate, decimal notional,
            List<string> priceHeader, List<decimal> priceData, string bondIdentifier, NamedValueSet namedValueSet)
        {
            namedValueSet.Set(TradeProp.Party1, party1);
            namedValueSet.Set(TradeProp.Party2, party2);
            if (isParty1Buyer)
            {
                namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
                namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }
            else
            {
                namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party2);
                namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party1);
            }
            //Set the pricing information
            namedValueSet.Set(TradeProp.EffectiveDate, effectiveDate);
            //Set other properties
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.BondTransaction.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.bondTransaction.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_Debt_Government_Fixed_Bullet));
            namedValueSet.Set(TradeProp.TradeDate, tradeDate);
            namedValueSet.Set(TradeProp.EffectiveDate, effectiveDate);
            namedValueSet.Set(TradeProp.MaturityDate, effectiveDate);
            namedValueSet.Set(TradeProp.TradeId, tradeId);
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            if (priceHeader.Count != priceData.Count) return "Price data not correct!";
            var nvs = new NamedValueSet();
            var index = 0;
            foreach (var item in priceHeader)
            {
                nvs.Set(item, priceData[index]);
                index++;
            }
            return ValService.CreateBondTransactionWithProperties(tradeId, isParty1Buyer, tradeDate, notional, nvs, bondIdentifier, namedValueSet);
        }

        #endregion

        #region IRFuture Transaction

        /// <summary>
        /// Creates a futures transaction
        /// </summary>
        /// <param name="tradeId">The trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the future buyer. If not then it is the seller. </param>
        /// <param name="platform">The execution platform.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="price">The purchase price information</param>
        /// <param name="futuresIdentifier">The futures identifier. Currently assumed to be of the form AUD-IRFuture-IR-6 OR AUD-IRFuture-IR-Z7</param>
        /// <param name="effectiveDate">THe effective date for margin payments.</param>
        /// <param name="numberOfContracts">The number of contracts.</param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="tradingBook">The trading book.</param>
        /// <returns></returns>
        public string CreateIRFutureTrade(string tradeId, bool isParty1Buyer, string party1, string party2, string platform, 
            DateTime tradeDate, DateTime effectiveDate, int numberOfContracts, double price, string futuresIdentifier, string tradingBook)
        {
            var properties = new NamedValueSet();
            properties.Set(TradeProp.Party1, party1);
            properties.Set(TradeProp.Party2, party2);
            if (isParty1Buyer)
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party1);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }
            else
            {
                properties.Set(TradeProp.BaseParty, TradeProp.Party2);
                properties.Set(TradeProp.CounterPartyName, TradeProp.Party1);
            }
            var purchasePrice = Convert.ToDecimal(price);
            //Set the pricing information
            properties.Set(TradeProp.EffectiveDate, tradeDate);
            properties.Set(TradeProp.TradingBookName, tradingBook);
            properties.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            properties.Set(TradeProp.TradingBookId, TradeState.Pricing.ToString());
            properties.Set(TradeProp.ProductType, ProductTypeSimpleEnum.FutureTransaction.ToString());
            properties.Set(TradeProp.TradeType, ItemChoiceType15.futureTransaction.ToString());
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Exchange_Traded_Future));
            properties.Set(TradeProp.TradeDate, tradeDate);
            properties.Set(TradeProp.EffectiveDate, effectiveDate);
            //properties.Set(TradeProp.MaturityDate, tradeDate);//Expiry date of the future
            properties.Set(TradeProp.Platform, platform);
            properties.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            properties.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            properties.Set(TradeProp.TradeId, tradeId);
            properties.Set(TradeProp.AsAtDate, DateTime.Today);
            properties.Set(FuturesProp.FuturesType, ExchangeContractTypeEnum.IRFuture.ToString());
            return ValService.CreateRateFutureTransactionWithProperties(tradeId, isParty1Buyer, tradeDate, numberOfContracts, purchasePrice, futuresIdentifier, properties);
        }

        /// <summary>
        /// Creates a futures transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="effectiveDate">THe effective date for margin payments.</param>
        /// <param name="purchasePrice">The purchase price information</param>
        /// <param name="futuresIdentifier">The futures identifier. Currently assumed to be of the form AUD-IRFuture-IR-6 OR AUD-IRFuture-IR-Z7</param>
        /// <param name="numberOfContracts">The number of contracts.</param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="namedValueSet">The properties range.</param>
        /// <returns></returns>
        public string CreateIRFutureTradeWithProperties(string tradeId, bool isParty1Buyer, string party1, 
            string party2, DateTime tradeDate, DateTime effectiveDate, int numberOfContracts,
            decimal purchasePrice, string futuresIdentifier, NamedValueSet namedValueSet)
        {
            namedValueSet.Set(TradeProp.Party1, party1);
            namedValueSet.Set(TradeProp.Party2, party2);
            if (isParty1Buyer)
            {
                namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
                namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            }
            else
            {
                namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party2);
                namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party1);
            }
            //Set the pricing information
            namedValueSet.Set(TradeProp.EffectiveDate, effectiveDate);
            //Set other properties
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.FutureTransaction.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.futureTransaction.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Exchange_Traded_Future));
            namedValueSet.Set(TradeProp.TradeDate, tradeDate);
            namedValueSet.Set(TradeProp.EffectiveDate, tradeDate);
            //namedValueSet.Set(TradeProp.MaturityDate, effectiveDate);//Expiry date
            namedValueSet.Set(TradeProp.TradeId, tradeId);
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            namedValueSet.Set(FuturesProp.FuturesType, ExchangeContractTypeEnum.IRFuture.ToString());
            return ValService.CreateRateFutureTransactionWithProperties(tradeId, isParty1Buyer, tradeDate, numberOfContracts, purchasePrice, futuresIdentifier, namedValueSet);
        }

        #endregion

        #region Bullet Payment

        /// <summary>
        /// Creates a bullet payment
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isPayerBase">The isPayerBase flag. If [true] then the payer party is Party1.</param>
        /// <param name="payer">The payer.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="paymentDate">The paymentDate.</param>
        /// <param name="businessDayAdjustments">The adjustments. </param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="businessDayCalendar">The calendar. </param>
        /// <returns></returns>
        public string CreateBullet(string tradeId, bool isPayerBase, string payer,
            string receiver, DateTime tradeDate, DateTime paymentDate, string businessDayCalendar, 
            string businessDayAdjustments, string currency, double amount)
        {
            return ValService.CreateBulletPayment(tradeId, isPayerBase, payer, receiver, tradeDate, paymentDate, businessDayCalendar, businessDayAdjustments, currency, amount, "SpreadSheet");
        }

        /// <summary>
        /// Creates a bullet payment
        /// </summary>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="paymentDate">The paymentDate.</param>
        /// <param name="businessDayAdjustments">The adjustments. </param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="properties">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <param name="businessDayCalendar">The calendar. </param>
        /// <returns></returns>
        public string CreateBulletWithProperties(DateTime tradeDate, DateTime paymentDate,
            string businessDayCalendar, string businessDayAdjustments, string currency, double amount, NamedValueSet properties)
        {
            var isPayerBase = properties.GetValue<Boolean>("PayerIsBase", true);
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Cash_Payment));
            return ValService.CreateBulletPaymentWithProperties(tradeDate, paymentDate, isPayerBase, businessDayCalendar, businessDayAdjustments, currency, amount, properties);
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
        /// <returns></returns>
        public string CreateFxSpot(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, decimal spotRate)
        {
            return ValService.CreateFxSpot(tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, "SpreadSheet");
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
        /// <param name="properties">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxSpotWithProperties(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, decimal spotRate, NamedValueSet properties)
        {
            return ValService.CreateFxSpotWithProperties(tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, properties);
        }

        #endregion

        #region FX Forward

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
        /// <param name="forwardRate">The forward Rate.</param>
        /// <param name="forwardPoints">The [Optional] forward Points.</param>
        /// <returns></returns>
        public string CreateFxForward(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, decimal spotRate, decimal forwardRate, [Optional] decimal forwardPoints)
        {
            return ValService.CreateFxForward(tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, forwardRate, forwardPoints, "SpreadSheet");
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
        /// <param name="forwardRate">The forward Rate.</param>
        /// <param name="forwardPoints">The [Optional] forward Points.</param>
        /// <param name="properties">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxForwardWithProperties(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, 
            decimal spotRate, decimal forwardRate, NamedValueSet properties, [Optional] decimal forwardPoints)
        {
            return ValService.CreateFxForwardWithProperties(tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, forwardRate, forwardPoints, properties);
        }

        #endregion

        #region FX Swap

        /// <summary>
        /// Creates an fx swap
        /// </summary>
        /// <param name="tradeId"></param>
        /// <param name="isCurrency1LenderBase"></param>
        /// <param name="tradeDate"></param>
        /// <param name="currency1Lender"></param>
        /// <param name="currency2Lender"></param>
        /// <param name="currency1Amount"></param>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="startValueDate"></param>
        /// <param name="forwardValueDate"></param>
        /// <param name="startRate"></param>
        /// <param name="forwardRate"></param>
        /// <param name="forwardPoints"></param>
        /// <returns></returns>
        public string CreateFxSwap(string tradeId, bool isCurrency1LenderBase, DateTime tradeDate, string currency1Lender, string currency2Lender,
            decimal currency1Amount, string currency1, string currency2, string quoteBasis,
            DateTime startValueDate, DateTime forwardValueDate, decimal startRate, decimal forwardRate, [Optional] decimal forwardPoints)
        {
            return ValService.CreateFxSwap(tradeId, isCurrency1LenderBase, tradeDate, currency1Lender, currency2Lender,
            currency1Amount, currency1, currency2, quoteBasis, startValueDate, forwardValueDate, startRate, forwardRate, forwardPoints, "SpreadSheet");
        }

        /// <summary>
        /// Creates an fx swap
        /// </summary>
        /// <param name="tradeId"></param>
        /// <param name="isCurrency1LenderBase"></param>
        /// <param name="tradeDate"></param>
        /// <param name="currency1Lender"></param>
        /// <param name="currency2Lender"></param>
        /// <param name="currency1Amount"></param>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="startValueDate"></param>
        /// <param name="forwardValueDate"></param>
        /// <param name="startRate"></param>
        /// <param name="forwardRate"></param>
        /// <param name="forwardPoints"></param>
        /// <param name="properties">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxSwapWithProperties(string tradeId, bool isCurrency1LenderBase, DateTime tradeDate, string currency1Lender, string currency2Lender,
            decimal currency1Amount, string currency1, string currency2, string quoteBasis,
            DateTime startValueDate, DateTime forwardValueDate, decimal startRate, decimal forwardRate, NamedValueSet properties, [Optional] decimal forwardPoints)
        {
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.ForeignExchange_Swap));
            return ValService.CreateFxSwapWithProperties(tradeId, isCurrency1LenderBase, tradeDate, currency1Lender, currency2Lender,
            currency1Amount, currency1, currency2, quoteBasis, startValueDate, forwardValueDate, startRate, forwardRate, forwardPoints, properties);
        }

        #endregion

        #region Fx Option - Still Some Stuff TODO

        /// <summary>
        /// Creates an fx option trade.
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="isBuyerBase">Is the buer of the option the base party. </param>
        /// <param name="buyerPartyReference">The option buyer.</param>
        /// <param name="sellerPartyReference">The option seller.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="valueDate">The valueDate.</param>
        /// <param name="isCall">Is this a call on the underlying currency pair or not (it is a put)</param>
        /// <param name="currencyPair">The currency pair in the same measure as the strike: AUD-USD.</param>
        /// <param name="strikeQuoteBasis">The strike quoteBasis.</param>
        /// <param name="strikePrice">The strike price.</param>
        /// <param name="expiryBusinessCenter">The expiry business center. </param>
        /// <param name="currency">The underlying currency.</param>
        /// <param name="expiryDate">The expiry date. </param>
        /// <param name="namedValueSet">The properties. </param>
        /// <param name="amount">The amount of currency.</param>
        /// <returns></returns>
        public string CreateVanillaFxOptionWithProperties(string tradeId, DateTime tradeDate,
        bool isBuyerBase, string buyerPartyReference, string sellerPartyReference, bool isCall, string currencyPair,
        decimal strikePrice, decimal amount, string currency, DateTime expiryDate, string expiryBusinessCenter,
        string strikeQuoteBasis, DateTime valueDate, NamedValueSet namedValueSet)
        {
            var currencies = currencyPair.Split('-');
            if (isCall)
            {
                
            }
            FxOptionParametersRange dataSet = new FxOptionParametersRange
            {
                BuyerPartyReference = buyerPartyReference,
                //CallCurrency = callCurrency,
                //CallCurrencyAmount = callCurrencyAmount,
                CutName = "Sydney",
                ExpiryBusinessCenter = expiryBusinessCenter,
                ExpiryDate = expiryDate,
                //ExpiryTime = time,
                //FaceOnCurrency = faceOnCurrency,
                IsBuyerBase = isBuyerBase,
                //OptionOnCurrency = optionOnCurrency,
                //Period = period,
                SellerPartyReference = sellerPartyReference,
                StrikePrice = strikePrice,
                StrikeQuoteBasis = strikeQuoteBasis,
                ValueDate = valueDate,
                TradeDate = tradeDate,
                TradeId = tradeId
            };
            //decimal? premiumValue = null;
            //DateTime? premiumDate = null;
            //string currency = null;
            //string quoteBasis = null;
            //if (!(premium is System.Reflection.Missing) &&
            //    !(premiumCurrency is System.Reflection.Missing) &&
            //    !(premiumSettlementDate is System.Reflection.Missing) &&
            //    !(premiumQuoteBasis is System.Reflection.Missing))
            //{
            //    var r2 = premium as Range;
            //    if (r2 != null) premiumValue = Convert.ToDecimal(r2.Value2);
            //    var r3 = premiumSettlementDate as Range;
            //    if (r3 != null) premiumDate = Convert.ToDateTime(r3.Value2);
            //    var r4 = premiumCurrency as Range;
            //    if (r4 != null) currency = Convert.ToString(r4.Value2);
            //    var r5 = premiumQuoteBasis as Range;
            //    if (r5 != null) quoteBasis = Convert.ToString(r5.Value2);
            //    dataSet.PremiumAmount = premiumValue;
            //    dataSet.PremiumSettlementDate = premiumDate;
            //    dataSet.PremiumQuoteBasis = quoteBasis;
            //    dataSet.PremiumCurrency = currency;
            //}
            var result = ValService.CreateVanillaFxOption(dataSet, namedValueSet);
            return result;
        }

        /// <summary>
        /// Creates an fx vanilla option trade.
        /// </summary>
        /// <param name="fxOptionParametersRange">The parameters range class. This contains: 
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
        /// <param name="namedValueSet">The properties. </param>
        public string CreateVanillaFxOption(FxOptionParametersRange fxOptionParametersRange, NamedValueSet namedValueSet)
        {
            var result = ValService.CreateVanillaFxOption(fxOptionParametersRange, namedValueSet);
            return result;
        }

        #endregion

        #region Forward Rate Agreement Functions

        /// <summary>
        /// returns an array of rates
        /// </summary>
        /// <returns></returns>
        public object[,] GetFraEquivalentRates(NamedValueSet propertiesRange, object[,] curveData)
        {
            int strCol = FraSolver.FindHeader(curveData, "Instrument");
            var instruments = FraSolver.GetObjects<string>(curveData, strCol);
            int rateCol = FraSolver.FindHeader(curveData, "Rate");
            var rates = FraSolver.GetObjects<double>(curveData, rateCol);
            var newRates = rates.Select(Convert.ToDecimal).ToList();
            int fraCol = FraSolver.FindHeader(curveData, "Guess");
            var fraGuesses = FraSolver.GetObjects<object>(curveData, fraCol);
            var fraEquivalents = ValService.CalculateFraEquivalents(propertiesRange, instruments, newRates, fraGuesses);
            var result = RangeHelper.ConvertArrayToRange(fraEquivalents);
            return result;
        }

        /// <summary>
        /// Builds a fra trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="isParty1Buyer">The isParty1Buyer flag.</param>
        /// <param name="party1">The party1 name.</param>
        /// <param name="party2">The party2 name.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="startTenor">The start tenor: based from the spot date.</param>
        /// <param name="indexTenor">The index tenor - this is relative to the base date as well!</param>
        /// <param name="discountingType">The discounting type: ISDA or AFMA.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="floatingRateIndex">The [Optional] floating rate index. If not provided the default will be used.</param>
        /// <returns></returns>
        public string CreateFra(string tradeId, DateTime tradeDate, bool isParty1Buyer, string party1, string party2, string currency, decimal notionalAmount, string startTenor,
            string indexTenor, string discountingType, decimal fixedRate, string floatingRateIndex)
        {
            return ValService.CreateFra(tradeId, tradeDate, isParty1Buyer, party1, party2, currency, notionalAmount, startTenor,
            indexTenor, discountingType, TradeSourceType.SpreadSheet, fixedRate, floatingRateIndex);
        }

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
        /// <remarks>
        ///  All the values in the input range must be provided as specified, otherwise the data 
        ///  will not be handled properly and #value will be returned.
        ///  </remarks>
        /// <returns>The Id the trade is cached under.</returns>
        public string CreateFraTrade(FraInputRange2 fraInputRange)
        {
            var result = ValService.CreateFraTrade(fraInputRange, TradeSourceType.SpreadSheet);
            return result;
        }

        /// <summary>
        ///   Creates a fra trade and caches it locally.
        /// </summary>
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
        /// <param name="namedValueSet">A list of properties, including mandatory ones.</param>
        /// <returns>The Id the valuation report is cached under.</returns>
        public string CreateFraTradeWithProperties(FraInputRange2 fraInputRange, NamedValueSet namedValueSet)
        {
            namedValueSet.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            var result = ValService.CreateFraTradeWithProperties(fraInputRange, namedValueSet);
            return result;
        }

        #endregion

        #region Interest Rate Swaps

        #region Main Pricing Functions

        /// <summary>
        /// Builds a swap trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="fixedLeg">The fixed leg.</param>
        /// <param name="floatingLeg">The floating leg.</param>
        /// <param name="namedValueSet">A list of properties, including mandatory ones: trade date, trade id, currency and notional</param>
        /// <returns></returns>
        public string VanillaIRSwap(string tradeId, DateTime tradeDate, DateTime effectiveDate, string maturityTenor,
            string adjustedType, string currency, decimal notionalAmount, bool isParty1Base,
            SwapLegSimpleRange fixedLeg, SwapLegSimpleRange floatingLeg, NamedValueSet namedValueSet)
        {
            var type = EnumHelper.Parse<AdjustedType>(adjustedType);
            return ValService.CreateIRSwap(tradeId, tradeDate, type, isParty1Base, effectiveDate, currency, notionalAmount, maturityTenor,
                fixedLeg, floatingLeg, namedValueSet);
        }

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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// </param>
        ///<param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// See above for a description of the range object.</param>
        public string CreateInterestRateSwap(string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Leg1Payer,
            SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange)
        {
            return ValService.CreateInterestRateSwap(tradeId, tradeDate, isParty1Base, party1, party2,
                leg1ParametersRange, leg2ParametersRange);
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	NAB</para>
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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// </param>
        ///<param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// See above for a description of the range object.</param>
        ///<param name="namedValueSet">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateInterestRateSwapWithProperties(SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange, 
            NamedValueSet namedValueSet)
        {
            namedValueSet.Set(TradeProp.TradingBookId, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.InterestRateSwap.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.swap.ToString());
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            namedValueSet.Set(EnvironmentProp.NameSpace, NameSpace);
            var valuationDate = namedValueSet.GetValue<DateTime>("ValuationDate", true);
            var tradeDate = namedValueSet.GetValue<DateTime>(TradeProp.TradeDate, true);
            var tradeId = namedValueSet.GetValue<string>(TradeProp.TradeId, true);
            var sourceSystem = namedValueSet.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            namedValueSet.Set(TradeProp.TradeSource, sourceSystem);
            namedValueSet.Set(TradeProp.EffectiveDate, leg1ParametersRange.EffectiveDate);
            namedValueSet.Set(TradeProp.MaturityDate, leg1ParametersRange.MaturityDate);
            namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
            namedValueSet.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            namedValueSet.Set(EnvironmentProp.SourceSystem, sourceSystem);
            namedValueSet.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            //Set the market curves.
            var market1 = new SwapLegEnvironment();
            if (leg1ParametersRange.DiscountCurve != null)
            {
                market1.AddPricingStructure("DiscountCurve", Engine.GetCurve(leg1ParametersRange.DiscountCurve, false));
            }
            if (leg1ParametersRange.ForecastCurve != null)
            {
                market1.AddPricingStructure("ForecastCurve", Engine.GetCurve(leg1ParametersRange.ForecastCurve, false));
            }
            var market2 = new SwapLegEnvironment();
            if (leg2ParametersRange.DiscountCurve != null)
            {
                market2.AddPricingStructure("DiscountCurve", Engine.GetCurve(leg2ParametersRange.DiscountCurve, false));
            }
            if (leg2ParametersRange.ForecastCurve!=null)
            {
                market2.AddPricingStructure("ForecastCurve", Engine.GetCurve(leg2ParametersRange.ForecastCurve, false));
            }
            //Create the swap.
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(Engine.Logger, Engine.Cache, Engine.NameSpace, leg1ParametersRange, null, leg2ParametersRange, null, null, null, null, market1, market2, valuationDate);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            var identifier = new TradeIdentifier(ItemChoiceType15.swap, ProductTypeSimpleEnum.InterestRateSwap, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            namedValueSet.Set(TradeProp.RequiredPricingStructures, curves);
            namedValueSet.Set(TradeProp.RequiredCurrencies, currencies);
            Engine.Cache.SaveObject(trade, NameSpace + "." + identifier.UniqueIdentifier, namedValueSet);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        /// Gets the dates and notionals for the swap leg.
        ///</summary>
        ///<param name="legParametersRange">Contains all the parameter information required by SwapLegParametersRange.
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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// <para>ValuationDate:	13/03/2010</para>
        /// </param>
        /// <param name="notionalValueItems">Contains all the parameter information required by DateTimeDoubleRangeItems.</param>
        ///<param name="valuationRange">Contains all the parameter information required by ValuationRange, 
        /// notably the valuation date.</param>
        ///<returns>A list of detailed cash flow items</returns>
        public List<DetailedCashflowRangeItem> GetSwapLegDetailedCashflowsWithNotionalSchedule(SwapLegParametersRange legParametersRange,
            List<DateTimeDoubleRangeItem> notionalValueItems, ValuationRange valuationRange)
        {
            return ValService.GetSwapLegDetailedCashflowsWithNotionalSchedule(legParametersRange, notionalValueItems, valuationRange);
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="legParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	NAB</para>
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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// <para>ValuationDate:	13/03/2010</para>
        /// </param>
        ///<param name="notionalValueItems">An array of date times and values.
        /// <para>System.DateTime DateTime</para>
        /// <para>double Value</para>
        ///</param>
        ///<param name="valuationRange">The is a range that contains the valuation date to use.</param>
        ///<returns>An array of principal exchanges.</returns>
        public List<PrincipalExchangeCashflowRangeItem> CreateIRSwapPrincipalExchanges(SwapLegParametersRange legParametersRange, List<DateTimeDoubleRangeItem> notionalValueItems,
            ValuationRange valuationRange)
        {
             var interestRatePricer = new InterestRateSwapPricer();
            return interestRatePricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, NameSpace, legParametersRange, notionalValueItems, valuationRange);
        }

        /// <summary>
        ///  This function builds a swap and then saves all the trade details into a file.
        /// </summary>
        /// <param name="namedValueSet">The properties range. </param>
        /// <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	NAB</para>
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
        ///  <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	NAB</para>
        ///  <para>ValuationDate:	13/03/2010</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        /// <param name="leg1DetailedCashflowsList">The details associated with the cash flows in this trade.
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
        /// <param name="leg2DetailedCashflowsList">See above for a description of the detailed object.</param>
        /// <param name="leg1PrincipalExchangeCashflowList">An optional array of principal cashflow details.
        ///  <para>System.DateTime PaymentDate</para>
        ///  <para>double Amount</para>
        ///  <para>double PresentValueAmount</para>
        ///  <para>double DiscountFactor</para>
        ///  </param>
        /// <param name="leg2PrincipalExchangeCashflowList">See above for a description of the detailed object.</param>
        /// <param name="leg1AdditionalPaymentList">Any Leg1 additional cash flows.</param>
        /// <param name="leg2AdditionalPaymentList">Any Leg2 additional cash flows.</param>
        /// <returns>The identifier string for this event.</returns>
        public string BuildSwap(
            NamedValueSet namedValueSet,
            SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange,
            List<InputCashflowRangeItem> leg1DetailedCashflowsList,
            List<InputCashflowRangeItem> leg2DetailedCashflowsList,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowList,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowList,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentList,
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentList)
        {
            return ValService.CreateSwapFromCashflows(Engine.Logger, Engine.Cache, NameSpace, leg1ParametersRange, leg2ParametersRange,
                leg1DetailedCashflowsList, leg2DetailedCashflowsList, leg1PrincipalExchangeCashflowList,
                leg2PrincipalExchangeCashflowList, leg1AdditionalPaymentList, leg2AdditionalPaymentList, namedValueSet);
        }

        ///<summary>
        ///</summary>
        ///<param name="namedValueSet">The properties, which contain all the valuation items. </param>
        /// <para>string Id</para>
        /// <para>System.DateTime TradeDate</para>
        ///<param name="leg1">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// <para>ValuationDate:	13/03/2010</para>
        /// </param>
        ///<param name="leg2">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// See above for a description of the range object.</param>
        ///<param name="leg1DetailedCashflows">The details associated with the cash flows in this trade.
        /// <para>public System.DateTime PaymentDate</para>
        /// <para>System.DateTime StartDate</para>
        /// <para>System.DateTime EndDate</para>
        /// <para>int NumberOfDays</para>
        /// <para>double FutureValue</para>
        /// <para>double PresentValue</para>
        /// <para>double DiscountFactor</para>
        /// <para>double NotionalAmount</para>
        /// <para>string CouponType;//Float,Fixed,PrincipalExchange?,Cap,Floor</para>
        /// <para>double Rate</para>
        /// <para>double Spread</para>
        /// <para>double StrikeRate</para>
        /// </param>
        ///<param name="leg2DetailedCashflows">See above for a description of the detailed object.</param>
        ///<param name="leg1PrincipalExchangeCashflow">An optional array of principal cashflow details.
        /// <para>System.DateTime PaymentDate</para>
        /// <para>double Amount</para>
        /// <para>double PresentValueAmount</para>
        /// <para>double DiscountFactor</para>
        /// </param>
        ///<param name="leg2PrincipalExchangeCashflow">See above for a description of the detailed object.</param>
        ///<param name="leg1AdditionalPayment">An optional array of additional payments in this trade.
        /// <para>DateTime PaymentDate</para>
        /// <para>double Amount</para>
        /// <para>string Currency</para>
        /// </param>
        ///<param name="leg2AdditionalPayment">See above for a description of the detailed object.</param>
        ///<returns>The calculated price of the swap, given the curve provided.</returns>
        public ValuationResultRange CreateSwapPrice(
            NamedValueSet namedValueSet,
            SwapLegParametersRange leg1,
            SwapLegParametersRange leg2,
            List<InputCashflowRangeItem> leg1DetailedCashflows,
            List<InputCashflowRangeItem> leg2DetailedCashflows,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflow,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflow,
            List<AdditionalPaymentRangeItem> leg1AdditionalPayment,
            List<AdditionalPaymentRangeItem> leg2AdditionalPayment)
        {
            //To classes
            var valRange = new ValuationRange
            {
                ValuationDate = namedValueSet.GetValue<DateTime>("ValuationDate", true),
            };
            var reportingParty = namedValueSet.GetValue<string>("ReportingParty", true);
            var party1 = namedValueSet.GetValue<string>("Party1", true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            valRange.BaseParty = baseParty;
            var irSwap = new InterestRateSwapPricer();
            //Get the curves and create a swap market environment.
            var leg1DiscountCurve = (RateCurve)Engine.GetCurve(leg1.DiscountCurve, false);
            var leg2DiscountCurve = (RateCurve)Engine.GetCurve(leg2.DiscountCurve, false);
            return irSwap.GetPrice(Engine.Logger, Engine.Cache, NameSpace, valRange, leg1, leg1DiscountCurve, leg2, leg2DiscountCurve,
                leg1DetailedCashflows, leg2DetailedCashflows, leg1PrincipalExchangeCashflow,
                leg2PrincipalExchangeCashflow, leg1AdditionalPayment, leg2AdditionalPayment);
        }

        #endregion

        #region Failing Functions

        ///// <summary>
        ///// Solves for floating rate margin to zero NPV of the swap.
        ///// </summary>
        ///// <param name="uniqueTradeId">The unique trade identifier.</param>
        ///// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        ///// <param name="reportingCurrency">The reporting currency</param>
        ///// <param name="valuationDate">The valuation date.</param>
        ///////  <param name="curveMapRange">The curve mapping range.</param>
        /////<returns>The fair spread.</returns>
        //public double GetInterestRateSwapFairSpread(string uniqueTradeId, string reportingParty, string reportingCurrency,
        //    DateTime valuationDate)
        //{
        //    var tradeItem = Engine.Cache.LoadItem<Trade>(NameSpace + "." + uniqueTradeId);
        //    var properties = tradeItem.AppProps;
        //    var party1 = properties.GetValue<string>(TradeProp.Party1, true);
        //    //var party2 = properties.GetValue<string>(TradeProp.Party2, true);
        //    var baseParty = reportingParty == party1 ? "Party1" : "Party2";
        //    var trade = tradeItem.Data as Trade;
        //    if (trade != null)
        //    {
        //        var swap = trade.Item as Swap;
        //        var pricer = new InterestRateSwapPricer(Engine.Logger, Engine.Cache, NameSpace, null, swap, baseParty);
        //        var result = pricer.GetFairSpread();
        //        return result;
        //    }
        //    return 0;
        //}

        /////<summary>
        ///// This calculates the par rate of a swap, using the cached curves referenced in the input ranges.
        /////</summary>
        ///// <param name="uniqueTradeId">The unique trade identifier.</param>
        ///// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        ///// <param name="reportingCurrency">The reporting currency</param>
        ///// <param name="valuationDate">The valuation date.</param>
        ///////  <param name="curveMapRange">The curve mapping range.</param>
        /////<returns>The par rate.</returns>
        //public double GetInterestRateSwapParRate(string uniqueTradeId, string reportingParty, string reportingCurrency,
        //    DateTime valuationDate)//, Excel.Range curveMapRange
        //{
        //    var tradeItem = Engine.Cache.LoadItem<Trade>(NameSpace + "." + uniqueTradeId);
        //    var properties = tradeItem.AppProps;
        //    var party1 = properties.GetValue<string>(TradeProp.Party1, true);
        //    //var party2 = properties.GetValue<string>(TradeProp.Party2, true);
        //    var baseParty = reportingParty == party1 ? "Party1" : "Party2";
        //    var trade = tradeItem.Data as Trade;
        //    if (trade != null)
        //    {
        //        var swap = trade.Item as Swap;
        //        var pricer = new InterestRateSwapPricer(Engine.Logger, Engine.Cache, NameSpace, null, swap, baseParty);//TODO Fix this!
        //        var parRate = pricer.GetParRate();//TODO This does not work because the discount factors have not been set in the legs.
        //        return parRate;
        //    }
        //    return 0;
        //}

        #endregion

        #endregion

        #region Cross Currency Swaps

        /// <summary>
        /// Builds a swap trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="fixedLeg">The fixed leg.</param>
        /// <param name="floatingLeg">The floating leg.</param>
        /// <param name="namedValueSet">A list of properties, including mandatory ones: trade date, trade id, currency and notional</param>
        /// <returns></returns>
        public string XccySwap(string tradeId, DateTime tradeDate, DateTime effectiveDate, string maturityTenor,
            string adjustedType, bool isParty1Base,
            SimpleXccySwapLeg fixedLeg, SimpleXccySwapLeg floatingLeg, NamedValueSet namedValueSet)
        {
            var type = EnumHelper.Parse<AdjustedType>(adjustedType);
            return ValService.CreateXccySwap(tradeId, tradeDate, type, isParty1Base, effectiveDate, maturityTenor,
                fixedLeg, floatingLeg, namedValueSet);
        }

        /// <summary>
        ///  Gets the principal exchanges in the defined swap.
        /// </summary>
        /// <param name="productTaxonomy">Must be part of the product taxonomy.</param>
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
        ///  <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	CBA</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        public string CreateXccySwap(string productTaxonomy, string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Leg1Payer,
            SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange)
        {
            return ValService.CreateCrossCurrencySwap(productTaxonomy, tradeId, tradeDate, isParty1Base, party1, party2, TradeSourceType.SpreadSheet,
                leg1ParametersRange, leg2ParametersRange);
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	NAB</para>
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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// </param>
        ///<param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// See above for a description of the range object.</param>
        ///<param name="namedValueSet">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateXccySwapWithProperties(SwapLegParametersRange leg1ParametersRange, SwapLegParametersRange leg2ParametersRange, NamedValueSet namedValueSet)
        {
            namedValueSet.Set(TradeProp.TradingBookId, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.CrossCurrencySwap.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.swap.ToString());
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            var valuationDate = namedValueSet.GetValue<DateTime>("ValuationDate", true);
            var tradeDate = namedValueSet.GetValue<DateTime>(TradeProp.TradeDate, true);
            var tradeId = namedValueSet.GetValue<string>(TradeProp.TradeId, true);
            var sourceSystem = namedValueSet.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            namedValueSet.Set(TradeProp.TradeSource, sourceSystem);
            namedValueSet.Set(EnvironmentProp.SourceSystem, sourceSystem);
            namedValueSet.Set(TradeProp.EffectiveDate, leg1ParametersRange.EffectiveDate);
            namedValueSet.Set(TradeProp.MaturityDate, leg1ParametersRange.MaturityDate);
            namedValueSet.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
            namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            namedValueSet.Set(EnvironmentProp.SourceSystem, sourceSystem);
            namedValueSet.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            //Set the market curves.
            var market1 = new SwapLegEnvironment();
            if (leg1ParametersRange.DiscountCurve != null)
            {
                market1.AddPricingStructure("DiscountCurve", Engine.GetCurve(leg1ParametersRange.DiscountCurve, false));
            }
            if (leg1ParametersRange.ForecastCurve != null)
            {
                market1.AddPricingStructure("ForecastCurve", Engine.GetCurve(leg1ParametersRange.ForecastCurve, false));
            }
            var market2 = new SwapLegEnvironment();
            if (leg2ParametersRange.DiscountCurve != null)
            {
                market2.AddPricingStructure("DiscountCurve", Engine.GetCurve(leg2ParametersRange.DiscountCurve, false));
            }
            if (leg2ParametersRange.ForecastCurve != null)
            {
                market2.AddPricingStructure("ForecastCurve", Engine.GetCurve(leg2ParametersRange.ForecastCurve, false));
            }
            //Create the swap.
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(Engine.Logger, Engine.Cache, Engine.NameSpace, leg1ParametersRange, null, leg2ParametersRange, null, null, null, null, market1, market2, valuationDate);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            var identifier = new TradeIdentifier(ItemChoiceType15.swap, ProductTypeSimpleEnum.CrossCurrencySwap, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            namedValueSet.Set(TradeProp.RequiredPricingStructures, curves);
            namedValueSet.Set(TradeProp.RequiredCurrencies, currencies);
            namedValueSet.Set(EnvironmentProp.NameSpace, NameSpace);
            namedValueSet.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            Engine.Cache.SaveObject(trade, NameSpace + "." + identifier.UniqueIdentifier, namedValueSet);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="legParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	NAB</para>
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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// <para>ValuationDate:	13/03/2010</para>
        /// </param>
        ///<param name="notionalValueItems">An array of date times and values.
        /// <para>System.DateTime DateTime</para>
        /// <para>double Value</para>
        ///</param>
        ///<param name="valuationRange">The is a range that contains the valuation date to use.</param>
        ///<returns>An array of principal exchanges.</returns>
        public List<PrincipalExchangeCashflowRangeItem> CreateXccySwapPrincipalExchanges(SwapLegParametersRange legParametersRange, List<DateTimeDoubleRangeItem> notionalValueItems,
            ValuationRange valuationRange)
        {
            var xccyPricer = new CrossCurrencySwapPricer();
            return xccyPricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, NameSpace, legParametersRange, notionalValueItems, valuationRange);
        }

        #endregion

        #region Swaptions

        /// <summary>
        /// Builds a swaption trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="fixedLeg">The fixed leg.</param>
        /// <param name="floatingLeg">The floating leg.</param>
        /// <param name="swaptionTerms">The swaption terms.</param>
        /// <param name="namedValueSet">A list of properties, including mandatory ones: trade date, trade id, currency and notional</param>
        /// <returns></returns>
        public string SimpleIRSwaption(string tradeId, DateTime tradeDate, DateTime effectiveDate, string maturityTenor,
            string adjustedType, string currency, decimal notionalAmount, bool isParty1Base,
            SwapLegSimpleRange fixedLeg, SwapLegSimpleRange floatingLeg, SwaptionParametersRange swaptionTerms, NamedValueSet namedValueSet)
        {
            var type = EnumHelper.Parse<AdjustedType>(adjustedType);
            return ValService.CreateSimpleIRSwaption(tradeId, tradeDate, type, isParty1Base, effectiveDate, currency, notionalAmount, maturityTenor,
                fixedLeg, floatingLeg, swaptionTerms, namedValueSet);
        }

        ///  <summary>
        ///  Gets the swaption premium using a Black-Scholes model.
        ///  </summary>
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
        ///  <param name="leg1ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  <para>Payer:	NAB</para>
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
        ///  <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        ///  <para>DiscountingType:	Standard</para>
        ///  <para>BaseParty:	NAB</para>
        ///  </param>
        /// <param name="leg2ParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        ///  See above for a description of the range object.</param>
        /// <param name="namedValueSet"></param>
        /// <returns>The identified for the swaption..</returns>
        public string CreateSwaptionTrade(
            SwaptionParametersRange swaptionTerms,
            SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange, 
            NamedValueSet namedValueSet
            )
        {
            namedValueSet.Set(TradeProp.TradingBookId, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.InterestRateSwaption.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.swaption.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_Option_Swaption));
            namedValueSet.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            namedValueSet.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            return ValService.CreateInterestRateSwaptionWithProperties(swaptionTerms, leg1ParametersRange, leg2ParametersRange, namedValueSet);
        }

        #endregion

        #region Caps/Floors

        /// <summary>
        /// Builds a swap trade using configuration data.
        /// </summary>
        /// <param name="tradeId">The trade id.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="strike">The strike. </param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="adjustedType">The adjustedType.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="maturityTenor">The maturityTenor: based from the spot date.</param>
        /// <param name="capLeg">The cap leg.</param>
        /// <param name="namedValueSet">A list of properties, including mandatory ones: trade date, trade id, currency and notional</param>
        /// <returns></returns>
        public string CapFloor(string tradeId, DateTime tradeDate, DateTime effectiveDate, string maturityTenor,
            string adjustedType, string currency, decimal notionalAmount, decimal strike, bool isParty1Base,
            CapFloorLegSimpleRange capLeg, NamedValueSet namedValueSet)
        {
            var type = EnumHelper.Parse<AdjustedType>(adjustedType);
            return ValService.CreateCapFloor(tradeId, tradeDate, type, isParty1Base, effectiveDate, currency, notionalAmount, maturityTenor, strike,
                capLeg, namedValueSet);
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        /// <param name="tradeId">The trade Id.</param>
        /// <param name="tradeDate">The trade Date.</param>
        /// <param name="isParty1Base">The isParty1Base flag.</param>
        /// <param name="party1">The party1.</param>
        /// <param name="party2">The party2.</param>
        ///<param name="capFloorParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// <para> CapFloorType CapOrFloor</para>
        /// <para>  StrikeRate</para>
        /// <para>  VolatilitySurface</para>
        /// </param>
        public string CreateCapFloor(string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Leg1Payer,
            CapFloorLegParametersRange capFloorParametersRange)
        {
            return ValService.CreateCapFloor(tradeId, tradeDate, isParty1Base, party1, party2,
                capFloorParametersRange);
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="capFloorParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
        /// <para>Payer:	NAB</para>
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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// <para> CapFloorType CapOrFloor</para>
        /// <para>  StrikeRate</para>
        /// <para>  VolatilitySurface</para>
        /// </param>
        ///<param name="namedValueSet">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateCapFloorWithProperties(CapFloorLegParametersRange capFloorParametersRange, NamedValueSet namedValueSet)
        {
            var sourceSystem = namedValueSet.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            namedValueSet.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            namedValueSet.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.CapFloor.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.capFloor.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_CapFloor));
            namedValueSet.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            namedValueSet.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            namedValueSet.Set(TradeProp.EffectiveDate, capFloorParametersRange.EffectiveDate);
            namedValueSet.Set(TradeProp.MaturityDate, capFloorParametersRange.MaturityDate);
            namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
            namedValueSet.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            var valuationDate = namedValueSet.GetValue<DateTime>(TradeProp.ValuationDate, true);
            var tradeDate = namedValueSet.GetValue<DateTime>(TradeProp.TradeDate, true);
            var tradeId = namedValueSet.GetValue<string>(TradeProp.TradeId, true);
            //Set the market curves.
            var market1 = new SwapLegEnvironment();
            if (capFloorParametersRange.DiscountCurve != null)
            {
                market1.AddPricingStructure("DiscountCurve", Engine.GetCurve(capFloorParametersRange.DiscountCurve, false));
            }
            if (capFloorParametersRange.ForecastCurve != null)
            {
                market1.AddPricingStructure("ForecastCurve", Engine.GetCurve(capFloorParametersRange.ForecastCurve, false));
            }
            if (capFloorParametersRange.VolatilitySurface != null)
            {
                market1.AddPricingStructure("VolatilitySurface", Engine.GetCurve(capFloorParametersRange.VolatilitySurface, false));
            }
            //Create the cap.
            var capFloor = CapFloorGenerator.GenerateDefinitionCashflowsAmounts(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, capFloorParametersRange, null, null, null, market1, valuationDate);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetCapFloor(trade, capFloor);
            var identifier = new TradeIdentifier(ItemChoiceType15.capFloor, ProductTypeSimpleEnum.CapFloor, tradeId, tradeDate, sourceSystem);
            var curves = trade.Item.GetRequiredPricingStructures().ToArray();
            var currencies = trade.Item.GetRequiredCurrencies().ToArray();
            namedValueSet.Set(TradeProp.RequiredPricingStructures, curves);
            namedValueSet.Set(TradeProp.RequiredCurrencies, currencies);
            namedValueSet.Set(EnvironmentProp.NameSpace, NameSpace);
            namedValueSet.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            Engine.Cache.SaveObject(trade, NameSpace + "." + identifier.UniqueIdentifier, namedValueSet);
            return identifier.UniqueIdentifier;
        }

        ///<summary>
        /// This function builds a swap and then saves all the trade details into a file.
        ///</summary>
        ///<param name="namedValueSet"> Any properties.</param>
        ///<param name="capFloorParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	NAB</para>
        /// <para>ValuationDate:	13/03/2010</para>
        /// </param>
        ///<param name="capFloorDetailedCashflowsListArray">The details associated with the cash flows in this trade.
        /// <para>public System.DateTime PaymentDate</para>
        /// <para>System.DateTime StartDate</para>
        /// <para>System.DateTime EndDate</para>
        /// <para>int NumberOfDays</para>
        /// <para>double FutureValue</para>
        /// <para>double PresentValue</para>
        /// <para>double DiscountFactor</para>
        /// <para>double NotionalAmount</para>
        /// <para>string CouponType;//Float,Fixed,PrincipalExchange?,Cap,Floor</para>
        /// <para>double Rate</para>
        /// <para>double Spread</para>
        /// <para>double StrikeRate</para>
        /// </param>
        ///<param name="capFloorPrincipalExchangeCashflowListArray">An optional array of principal cashflow details.
        /// <para>System.DateTime PaymentDate</para>
        /// <para>double Amount</para>
        /// <para>double PresentValueAmount</para>
        /// <para>double DiscountFactor</para>
        /// </param>
        ///<param name="capFloorAdditionalCashflowListArray">An optional array of additional payments in this trade.
        /// <para>DateTime PaymentDate</para>
        /// <para>double Amount</para>
        /// <para>string Currency</para>
        /// </param>
        ///<param name="feePaymentList">See above for a description of the detailed object.</param>
        ///<returns>The identifier string for this event.</returns>
        public string BuildCapFloor(
            NamedValueSet namedValueSet,
            CapFloorLegParametersRange capFloorParametersRange,
            List<InputCashflowRangeItem> capFloorDetailedCashflowsListArray,
            List<InputPrincipalExchangeCashflowRangeItem> capFloorPrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> capFloorAdditionalCashflowListArray,
            List<FeePaymentRangeItem> feePaymentList)
        {
            //var isParty1Base = namedValueSet.GetValue<bool>("IsParty1Base", false);
            var result = ValService.CreateCapFloorFromCashflows(Engine.Logger, Engine.Cache, NameSpace, capFloorParametersRange,
                capFloorDetailedCashflowsListArray, capFloorPrincipalExchangeCashflowListArray, capFloorAdditionalCashflowListArray, feePaymentList, namedValueSet);
            return result;
        }

        #endregion

        #region BillSwap Pricer

        ///<summary>
        ///</summary>
        ///<param name="billSwapTerms"></param>
        ///<returns></returns>
        public List<DateTimeRangeItem> BuildSwapDates(BillSwapPricerDatesRange billSwapTerms)
        {
            var billSwapPricer = new BillSwapPricer();
            List<DateTime> dates = billSwapPricer.BuildDates(Engine.Logger, Engine.Cache, NameSpace, billSwapTerms, null);
            var result = dates.Select(dts => new DateTimeRangeItem { Value = dts }).ToList();
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="dateAndNotional"></param>
        ///<param name="dayCounterAsString"></param>
        ///<param name="curveId"></param>
        ///<returns></returns>
        public double GetSimpleYield(List<BillSwapPricerDateNotional> dateAndNotional, string dayCounterAsString, string curveId)
        {
            var billSwapPricer = new BillSwapPricer();
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCounterAsString);
            List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(dateAndNotional, dayCounter, rc);
            double annYield = billSwapPricer.GetSimpleYield(forwardRates, dayCounter, rc);
            return annYield;
        }

        ///<summary>
        ///</summary>
        ///<param name="dateAndNotional"></param>
        ///<param name="dayCounterAsString"></param>
        ///<param name="curveId"></param>
        ///<returns></returns>
        public double GetAnnualCompoundingYield(List<BillSwapPricerDateNotional> dateAndNotional, string dayCounterAsString, string curveId)
        {
            var billSwapPricer = new BillSwapPricer();
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCounterAsString);
            List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(dateAndNotional, dayCounter, rc);
            double annYield = billSwapPricer.GetAnnualYield(forwardRates, dayCounter, rc);
            return annYield;
        }

        ///<summary>
        ///</summary>
        ///<param name="dateAndNotional"></param>
        ///<param name="dayCounterAsString"></param>
        ///<param name="curveId"></param>
        ///<returns></returns>
        public object[,] CalculatePurchaseCost(List<BillSwapPricerDateNotional> dateAndNotional, string dayCounterAsString, string curveId)
        {
            var billSwapPricer = new BillSwapPricer();
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCounterAsString);
            List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(dateAndNotional, dayCounter, rc);
            List<BillSwapPricerCashflowRow> pc = billSwapPricer.PopulatePurchaseCosts(forwardRates, dayCounter, rc);
            object[,] result = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(pc);
            return result;
        }

        ///<summary>
        /// Returns fair swap rate (calculated on floating margin).
        /// This rate is an annual rate. To convert it to the actual rate use 
        /// the <b>BillsSwapPricer, GetEffectiveRate</b> function.
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="floatMargin"></param>
        ///<param name="payTerms"></param>
        ///<param name="payRolls"></param>
        ///<param name="receiveTerms"></param>
        ///<param name="receiveRolls"></param>
        ///<param name="curveId"></param>
        ///<param name="bulletPaymentDate"></param>
        ///<param name="bulletPaymentValue"></param>
        ///<returns></returns>
        public double CalculateFixedRate(DateTime valuationDate, double floatMargin,
            BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
            BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue)
        {
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            double fixedRate = BillsSwapPricer2.CalculateFixedRate(valuationDate, floatMargin, payTerms, payRolls, receiveTerms,
                receiveRolls, rc, bulletPaymentDate, bulletPaymentValue);
            return fixedRate;
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="floatMargin"></param>
        ///<param name="fixedRate"></param>
        ///<param name="payTerms"></param>
        ///<param name="payRolls"></param>
        ///<param name="receiveTerms"></param>
        ///<param name="receiveRolls"></param>
        ///<param name="curveId"></param>
        ///<param name="bulletPaymentDate"></param>
        ///<param name="bulletPaymentValue"></param>
        ///<returns></returns>
        public double CalculateFixedRate2(DateTime valuationDate,
            double floatMargin, double fixedRate,
            BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
            BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue)
        {
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            double fixedSidePV = BillsSwapPricer2.CalculateFixedSidePV(valuationDate, floatMargin, fixedRate,
                payTerms, payRolls, receiveTerms, receiveRolls, rc, bulletPaymentDate, bulletPaymentValue);
            return fixedSidePV;
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="floatMargin"></param>
        ///<param name="fixedRate"></param>
        ///<param name="payTerms"></param>
        ///<param name="payRolls"></param>
        ///<param name="receiveTerms"></param>
        ///<param name="receiveRolls"></param>
        ///<param name="curveId"></param>
        ///<param name="bulletPaymentDate"></param>
        ///<param name="bulletPaymentValue"></param>
        ///<param name="curveInstrumentId"></param>
        ///<param name="perturbationAmount"></param>
        ///<returns></returns>
        public double CalculateFixedSideSensitivity(DateTime valuationDate, double floatMargin, double fixedRate,
            BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
            BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue,
            string curveInstrumentId, double perturbationAmount)
        {
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            var sensitivity = BillsSwapPricer2.CalculateFixedSideSensitivity(valuationDate,
                floatMargin, fixedRate, payTerms, payRolls, receiveTerms, receiveRolls, rc,
                bulletPaymentDate, bulletPaymentValue, curveInstrumentId, perturbationAmount);
            return sensitivity;
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="floatMargin"></param>
        ///<param name="fixedRate"></param>
        ///<param name="payTerms"></param>
        ///<param name="payRolls"></param>
        ///<param name="receiveTerms"></param>
        ///<param name="receiveRolls"></param>
        ///<param name="curveId"></param>
        ///<param name="bulletPaymentDate"></param>
        ///<param name="bulletPaymentValue"></param>
        ///<param name="listInstrumentIdAndQuotes"></param>
        ///<param name="listPerturbations"></param>
        ///<returns></returns>
        public object[,] CalculateFixedSideSensitivity2(
            DateTime valuationDate, double floatMargin, double fixedRate,
            BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
            BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue,
            List<InstrumentIdAndQuoteRangeItem> listInstrumentIdAndQuotes,
            List<DoubleRangeItem> listPerturbations)
        {
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            List<DoubleRangeItem> sensitivity = BillsSwapPricer2.CalculateFixedSideSensitivity2(valuationDate, floatMargin,
                fixedRate, payTerms, payRolls, receiveTerms, receiveRolls, rc, bulletPaymentDate,
                bulletPaymentValue, listInstrumentIdAndQuotes, listPerturbations);
            object[,] result = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(sensitivity);
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="floatMargin"></param>
        ///<param name="fixedRate"></param>
        ///<param name="payTerms"></param>
        ///<param name="payRolls"></param>
        ///<param name="receiveTerms"></param>
        ///<param name="receiveRolls"></param>
        ///<param name="curveId"></param>
        ///<param name="bulletPaymentDate"></param>
        ///<param name="bulletPaymentValue"></param>
        ///<param name="listInstrumentIdAndQuotes"></param>
        ///<param name="listPerturbations"></param>
        ///<param name="filterByInstruments"></param>
        ///<returns></returns>
        public double CalculateFixedSideDelta(
            DateTime valuationDate, double floatMargin, double fixedRate,
            BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
            BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue,
            List<InstrumentIdAndQuoteRangeItem> listInstrumentIdAndQuotes,
            List<DoubleRangeItem> listPerturbations, string filterByInstruments)
        {
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            var sensitivity = BillsSwapPricer2.CalculateFixedSideDelta(valuationDate, floatMargin, fixedRate,
                payTerms, payRolls, receiveTerms, receiveRolls, rc, bulletPaymentDate, bulletPaymentValue, listInstrumentIdAndQuotes, listPerturbations, filterByInstruments);
            return sensitivity;
        }

        ///<summary>
        ///</summary>
        ///<param name="cashflowsSchedule"></param>
        ///<param name="terms"></param>
        ///<returns></returns>
        public double GetEffectiveFrequency(List<AmortisingResultItem> cashflowsSchedule, BillsSwapPricer2TermsRange terms)
        {
            double effectiveFrequency = BillsSwapPricer2.GetEffectiveFrequency(cashflowsSchedule, terms);
            return effectiveFrequency;
        }

        ///<summary>
        ///</summary>
        ///<param name="termsRange"></param>
        ///<param name="metaScheduleDefinitionRange"></param>
        ///<returns></returns>
        public List<AmortisingResultItem> GenerateCashflowSchedule(BillsSwapPricer2TermsRange termsRange, List<MetaScheduleRangeItem> metaScheduleDefinitionRange)
        {
            return BillsSwapPricer2.GenerateCashflowSchedule(termsRange, metaScheduleDefinitionRange, null);
        }

        ///<summary>
        ///</summary>
        ///<param name="cfItems"></param>
        ///<param name="amortisingSchedule"></param>
        ///<returns></returns>
        public List<AmortisingResultItem> GenerateAmortisationSchedule(List<AmortisingResultItem> cfItems, List<AmortisingScheduleItem> amortisingSchedule)
        {
            var amortizationSchedule = BillsSwapPricer2.GenerateAmortisationSchedule(cfItems, amortisingSchedule);
            return amortizationSchedule;
        }

        #endregion
    }
}
