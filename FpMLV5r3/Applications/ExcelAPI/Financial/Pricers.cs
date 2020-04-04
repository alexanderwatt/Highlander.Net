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
using Highlander.ValuationEngine.V5r3.Pricers;
using HLV5r3.Helpers;
using Microsoft.Office.Interop.Excel;
using TradeIdentifier = Highlander.Reporting.Identifiers.V5r3.TradeIdentifier;
using XsdClassesFieldResolver = Highlander.Reporting.V5r3.XsdClassesFieldResolver;

#endregion

namespace HLV5r3.Financial
{
    public partial class Cache
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
        /// <param name="queryRange">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public object[,] QueryTrades(Range queryRange)
        {
            var query = queryRange.Value[System.Reflection.Missing.Value] as object[,];
            var result = ValService.QueryTrades(DataRangeHelper.MapTradeQueryObject(query));
            return DataRangeHelper.MapTradeQueryDataToObjectMatrix(result);
        }

        /// <summary>
        /// Returns the header information for all trades matching the query properties.
        /// </summary>
        /// <param name="queryRange">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public object[,] QueryCurves(Range queryRange)
        {
            var query = queryRange.Value[System.Reflection.Missing.Value] as object[,];
            return Engine.CurvesQuery(query);
        }

        /// <summary>
        /// Lists all valuation reports currently in memory.
        /// </summary>
        /// <param name="requestProperties">The request Properties.</param>
        /// <param name="metricToReturn">The metric To Return.</param>
        /// <returns>A list identifiers that can be uses as handles.</returns>
        public object[,] ShowValuationReports(Range requestProperties, String metricToReturn)
        {
            var properties = requestProperties.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            if (requestProperties == null) throw new ArgumentNullException(nameof(requestProperties));
            var namedValueSet = properties.ToNamedValueSet();
            var loadedObjects = ValService.ShowValuationReports(namedValueSet, metricToReturn);
            object[,] results = ArrayHelper.ConvertDictionaryTo2DArray(loadedObjects);
            return results;
        }

        /// <summary>
        /// Gets the bonds conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public object[,] GetQASUniqueIdentifiers(Range requestProperties)
        {
            var properties = requestProperties.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = RangeHelper.ConvertArrayToRange(ValService.GetQasUniqueIdentifiers(namedValueSet).ToArray());
            return result;
        }

        /// <summary>
        /// Saves the qas to files.
        /// </summary>
        /// <param name="identifierArray">An array of identifiers. This is the identifier returned by the qGetFixedIncomeISINs function. </param>
        /// <param name="directoryPath">The path to save to. </param>
        /// <param name="isShortName">If <true>isShortName</true> is true> then the id is of the form: Curve name.
        /// Otherwise it is of the form Orion.V5r3.QuotedAssetSet.CurveName.</param>
        /// <returns></returns>
        public string SaveQAS(Range identifierArray, string directoryPath, bool isShortName)
        {
            var identifiers = DataRangeHelper.StripRange(identifierArray);
            var result = ValService.SaveQas(identifiers, directoryPath, isShortName);
            return result;
        }

        /// <summary>
        /// Gets the bonds conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public object[,] GetFixedIncomeUniqueIdentifiers(Range requestProperties)
        {
            var properties = requestProperties.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = RangeHelper.ConvertArrayToRange(ValService.GetFixedIncomeUniqueIdentifiers(namedValueSet).ToArray());
            return result;
        }

        /// <summary>
        /// Gets the bonds conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public object[,] GetFixedIncomeDescription(Range requestProperties)
        {
            var properties = requestProperties.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = RangeHelper.ConvertArrayToRange(ValService.GetFixedIncomeDescription(namedValueSet).ToArray());
            return result;
        }

        /// <summary>
        /// Gets the bond ISINs conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public object[,] GetFixedIncomeISINs(Range requestProperties)
        {
            var properties = requestProperties.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = RangeHelper.ConvertArrayToRange(ValService.GetFixedIncomeISINs(namedValueSet).ToArray());
            return result;
        }

        /// <summary>
        /// Gets the equities conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public object[,] GetEquityUniqueIdentifiers(Range requestProperties)
        {
            var properties = requestProperties.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = RangeHelper.ConvertArrayToRange(ValService.GetEquityUniqueIdentifiers(namedValueSet).ToArray());
            return result;
        }

        /// <summary>
        /// Gets the equities conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public object[,] GetEquityDescription(Range requestProperties)
        {
            var properties = requestProperties.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = RangeHelper.ConvertArrayToRange(ValService.GetEquityDescription(namedValueSet).ToArray());
            return result;
        }

        /// <summary>
        /// Gets the equity ISINs conforming to the properties provided.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <returns></returns>
        public object[,] GetEquityISINs(Range requestProperties)
        {
            var properties = requestProperties.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = RangeHelper.ConvertArrayToRange(ValService.GetEquityISINs(namedValueSet).ToArray());
            return result;
        }

        /// <summary>
        /// Saves the equities to files.
        /// </summary>
        /// <param name="identifierArray">An array of identifiers. This is the identifier returned by the qGetFixedIncomeISINs function. </param>
        /// <param name="directoryPath">The path to save to. </param>
        /// <param name="isShortName">If <true>isShortName</true> is true> then the id is of the form: FixedIncome.XXXX.YY.zz-zz-zzzz.
        /// Otherwise it is of the form Orion.ReferenceData.Equity.XXXX.YY.zz-zz-zzzz.</param>
        /// <returns></returns>
        public string SaveEquities(Range identifierArray, string directoryPath, bool isShortName)
        {
            var identifiers = DataRangeHelper.StripRange(identifierArray);
            var result = ValService.SaveEquities(identifiers, directoryPath, isShortName);
            return result;
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
        public object[,] GetAssetMeasureTypes()
        {
            var names = Enum.GetNames(typeof(AssetMeasureEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of valid price quote units. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of valid price quote units.</returns>
        public object[,] GetPriceQuoteUnits()
        {
            var names = Enum.GetNames(typeof(PriceQuoteUnitsEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] GetRateMetrics()
        {
            var names = Enum.GetNames(typeof(RateMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented products.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedProductTypes()
        {
            var result = RangeHelper.ConvertArrayToRange((from ProductTypeSimpleEnum value in Enum.GetValues(typeof(ProductTypeSimpleEnum))
                                                          where (value != ProductTypeSimpleEnum.Undefined) && (value != ProductTypeSimpleEnum._LAST_)
                                                          select value.ToString()).ToArray());
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented products.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedProductTaxonomyTypes()
        {
            var indices = Enum.GetValues(typeof(ProductTaxonomyEnum)).Cast<ProductTaxonomyEnum>().Select(ProductTaxonomyScheme.GetEnumString).Where(index => index != null).ToList();
            var result = RangeHelper.ConvertArrayToRange(indices);
            return result;
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
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        ///  <returns></returns>
        public object[,] ViewExpectedCashFlows(string uniqueTradeId, string reportingParty, Range metricsArray, string reportingCurrency, string market,
            DateTime valuationDate)
        {
            var metrics = DataRangeHelper.StripRange(metricsArray);
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
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="curveMapRange">The curve mapping range.</param>
        /// <returns></returns>
        public string ValueTrade(string uniqueTradeId, string reportingParty, Range metricsArray, string reportingCurrency,
            DateTime valuationDate, Range curveMapRange)//TODO change to a marketName
        {
            var curveMap = curveMapRange.Value[System.Reflection.Missing.Value] as object[,];
            List<Pair<string, string>> curves = curveMap.ToList<string, string>();
            var metrics = DataRangeHelper.StripRange(metricsArray);
            return ValService.ValueTrade(TradeSourceType.SpreadSheet, uniqueTradeId, reportingParty, metrics, reportingCurrency, valuationDate, curves);
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
        public string ValueTradeFromMarket(string uniqueTradeId, string reportingParty, Range metricsArray, string reportingCurrency, string market,
            DateTime valuationDate)
        {
            var metrics = DataRangeHelper.StripRange(metricsArray);
            return ValService.ValueTradeFromMarket(uniqueTradeId, reportingParty, metrics, reportingCurrency, market, valuationDate);
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
        ///  <param name="marketsArray">The markets as a vertical range.</param>
        /// <returns></returns>
        public string ValueTradeFromMarkets(String valuationPortfolioId, string uniqueTradeId, string reportingParty, Range metricsArray, string reportingCurrency, Range marketsArray,
            DateTime valuationDate)
        {
            var metrics = DataRangeHelper.StripRange(metricsArray);
            var markets = DataRangeHelper.StripRange(marketsArray);
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
        ///  <param name="curveMapRange">The curve mapping range.</param>
        /// <returns></returns>
        public double GetNPVWithSpecifiedCurves(string uniqueTradeId, string reportingParty, string reportingCurrency, DateTime valuationDate, Range curveMapRange)//Use a range mapping area
        {
            var curveMap = curveMapRange.Value[System.Reflection.Missing.Value] as object[,];
            List<Pair<string, string>> curves = curveMap.ToList<string, string>();
            return ValService.GetNPVWithSpecifiedCurves(uniqueTradeId, reportingParty, reportingCurrency, valuationDate, curves);
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
        ///  <param name="curveMapRange">The curve mapping range.</param>
        /// <returns></returns>
        public double GetParRateWithSpecifiedCurves(string uniqueTradeId, string reportingParty, DateTime valuationDate, Range curveMapRange)
        {
            var curveMap = curveMapRange.Value[System.Reflection.Missing.Value] as object[,];
            List<Pair<string, string>> curves = curveMap.ToList<string, string>();
            return ValService.GetParRateWithSpecifiedCurves(uniqueTradeId, reportingParty, valuationDate, curves);
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
        /// <param name="dayCount">THe daycount basis. Must be a valid type.</param>
        /// <returns></returns>
        public string CreateTermDeposit(string tradeId, bool isLenderBase, string lenderParty,
            string borrowerParty, DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount)
        {
            var result = ValService.CreateTermDeposit(tradeId, isLenderBase, lenderParty, borrowerParty, tradeDate, startDate, maturityDate, currency, notionalAmount, fixedRate, dayCount);
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
        /// <param name="dayCount">THe daycount basis. Must be a valid type.</param>
        /// <param name="properties2DRange">The properties2DRange. This must contain: 
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateSimpleDepositWithProperties(string tradeId,
            DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount,
            Range properties2DRange)
        {
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            var props = properties.ToNamedValueSet();
            props.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_TermDeposit));
            var tradeIdentifier = new TradeIdentifier(props);
            var trade = TermDepositPricer.CreateSimpleTermDepositTrade(tradeIdentifier.Id, ProductTypeSimpleEnum.TermDeposit.ToString(), tradeDate, startDate,
                maturityDate, currency, Convert.ToDecimal(notionalAmount), Convert.ToDecimal(fixedRate), dayCount);
            var identifier = NameSpace + "." + tradeIdentifier.UniqueIdentifier;
            Engine.Cache.SaveObject(trade, identifier, props);
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
        /// <param name="dayCount">THe daycount basis. Must be a valid type.</param>
        /// <param name="properties2DRange">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateDepositWithProperties(DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount,
            Range properties2DRange)
        {
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            var props = properties.ToNamedValueSet();
            props.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_TermDeposit));
            return ValService.CreateDepositWithProperties(tradeDate, startDate, maturityDate, currency, notionalAmount, fixedRate, dayCount, props);
        }


        ///// <summary>
        ///// Views a trade that has already been created.
        ///// </summary>
        ///// <param name="tradeId">The unique identifier.</param>
        ///// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        //public object[,] ViewDepositCashFlows(string tradeId, string reportingParty)//TODO May need to calculate thia first to get the function working!
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
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="discountCurveId">The discountCurve identifier.</param>
        /// <returns></returns>
        public string ValueTermDeposit(string tradeId, string reportingParty, Range metricsArray, string reportingCurrency,
            DateTime valuationDate, string discountCurveId)
        {
            var metrics = DataRangeHelper.StripRange(metricsArray);
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
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="discountCurveId">The discountCurve identifier.</param>
        /// <returns></returns>
        public string TestValueTermDeposit(string tradeId, string reportingParty, Range metricsArray, string reportingCurrency,
            DateTime valuationDate, string discountCurveId)
        {
            var metrics = DataRangeHelper.StripRange(metricsArray);
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
        public string CreateEquityTrade(string tradeId, bool isParty1Buyer, string party1, string party2, string platform, DateTime tradeDate, DateTime effectiveDate, Decimal numberOfShares,
            Decimal unitPrice, String unitPriceCurrency, string equityIdentifier, string tradingBook)
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
        /// <param name="properties2DRange">The properties range</param>
        /// <returns></returns>
        public string CreateEquityTradeWithProperties(string tradeId, bool isParty1Buyer, string party1, string party2, DateTime tradeDate, DateTime effectiveDate, Decimal numberOfShares,
            Decimal unitPrice, String unitPriceCurrency, string equityIdentifier, Range properties2DRange)
        {
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
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
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.EquityTransaction.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.equityTransaction.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Equity_OrdinaryShares));
            namedValueSet.Set(TradeProp.TradeDate, tradeDate);
            namedValueSet.Set(TradeProp.EffectiveDate, effectiveDate);
            namedValueSet.Set(TradeProp.MaturityDate, effectiveDate);
            namedValueSet.Set(TradeProp.TradeId, tradeId);
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            namedValueSet.Set(EquityProp.ReferenceEquity, equityIdentifier);
            return ValService.CreateEquityTransactionWithProperties(tradeId, isParty1Buyer, tradeDate, numberOfShares, unitPrice, unitPriceCurrency, equityIdentifier, namedValueSet);
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
            properties.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Property_Residential));
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
        /// <param name="properties2DRange">The properties range</param>
        /// <returns></returns>
        public string CreatePropertyTradeWithProperties(string tradeId, bool isParty1Buyer, string party1, string party2, DateTime tradeDate, DateTime effectiveDate, Decimal purchaseAmount,
            DateTime paymentDate, String propertyType, String currency, string propertyIdentifier, string tradingBook, Range properties2DRange)
        {
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
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
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.EquityTransaction.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.equityTransaction.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Equity_OrdinaryShares));
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
        /// <param name="properties2DRange">A trade properties range. This should include the following:
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
            DateTime leaseExpiryDate, string referencePropertyIdentifier, string description, Range properties2DRange)
        {
            // string reviewFrequency, DateTime nextReviewDate, decimal reviewChange,
            //string leaseType, string shopNumber, string unitsOfArea,
            //decimal upfrontAmount, DateTime paymentDate, 
            var properties = properties2DRange?.Value[System.Reflection.Missing.Value] as object[,];
            var namedValueSet = new NamedValueSet();
            if (properties != null)
            {
                properties = (object[,]) DataRangeHelper.TrimNulls(properties);
                namedValueSet = properties.ToNamedValueSet();
            }
            //Get the values required from the range data.
            var upfrontAmount = namedValueSet.GetValue(LeaseProp.UpfrontAmount, 0.0m);
            var leaseType = namedValueSet.GetValue(LeaseProp.LeaseType, "unspecified");
            var shopNumber = namedValueSet.GetValue(LeaseProp.ShopNumber, "unspecified");
            var area = namedValueSet.GetValue(LeaseProp.Area, 0.0m);
            var unitsOfArea = namedValueSet.GetValue(LeaseProp.UnitsOfArea, "sqm");
            var reviewFrequency = namedValueSet.GetValue(LeaseProp.ReviewFrequency, "1Y");
            var nextReviewDate = namedValueSet.GetValue(LeaseProp.NextReviewDate, leaseStartDate.AddYears(1));
            var reviewChange = namedValueSet.GetValue(LeaseProp.ReviewChange, 0.0m);
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
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.EquityTransaction.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.equityTransaction.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Equity_OrdinaryShares));
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

        #endregion

        #region Bond Transaction

        /// <summary>
        /// Creates a bond transaction
        /// </summary>
        /// <param name="tradeId">THe trade identifier </param>
        /// <param name="isParty1Buyer">Is party1 the bond buyer. If not then it is the seller. </param>
        /// <param name="platform">The execution platform.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="priceHeaderArray">THe price header range. This must contain either:
        /// DirtyPrice OR CleanPrice and Accruals.</param>
        /// <param name="priceDataArray">The price data array. These are all decimal values.</param>
        /// <param name="bondIdentifier">The bond identifier. Currently assumed to be of the form:  Orion.ReferenceData.FixedIncome.Corp.ANZ.Fixed.5,25.01-16-14 </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="notional">The notional in the trade currency, which is assumed to be same currency as the coupon. </param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="tradingBook">The trading book.</param>
        /// <returns></returns>
        public string CreateBondTrade(string tradeId, bool isParty1Buyer, string party1, string party2, string platform, DateTime tradeDate, DateTime effectiveDate, Decimal notional,
            Range priceHeaderArray, Range priceDataArray, string bondIdentifier, string tradingBook)
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
            var pricesHeader = DataRangeHelper.StripRange(priceHeaderArray);
            var pricesData = DataRangeHelper.StripDecimalRange(priceDataArray);
            if (pricesHeader.Count != pricesData.Count) return "Price data not correct!";
            var nvs = new NamedValueSet();
            var index = 0;
            foreach (var item in pricesHeader)
            {
                nvs.Set(item, pricesData[index]);
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
        /// <param name="priceHeaderArray">THe price header range. This must contian either:
        /// DirtyPrice OR CleanPrice and Accruals.</param>
        /// <param name="priceDataArray">The price data array. These are all decimal values.</param>
        /// <param name="bondIdentifier">The bond identifier. Currently assumed to be of the form:  Orion.ReferenceData.FixedIncome.Corp.ANZ.Fixed.5,25.01-16-14 </param>
        /// <param name="effectiveDate">The date when the bond is paid for.</param>
        /// <param name="notional">The notional in the trade currency, which is assumed to be same currency as the coupon. </param>
        /// <param name="party1">Party1, the first party.</param>
        /// <param name="party2">Party2, the second party.</param>
        /// <param name="properties2DRange">The properties range.</param>
        /// <returns></returns>
        public string CreateBondTradeWithProperties(string tradeId, bool isParty1Buyer, string party1, string party2, DateTime tradeDate, DateTime effectiveDate, Decimal notional,
            Range priceHeaderArray, Range priceDataArray, string bondIdentifier, Range properties2DRange)
        {
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
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
            var pricesHeader = DataRangeHelper.StripRange(priceHeaderArray);
            var pricesData = DataRangeHelper.StripDecimalRange(priceDataArray);
            if (pricesHeader.Count != pricesData.Count) return "Price data not correct!";
            var nvs = new NamedValueSet();
            var index = 0;
            foreach (var item in pricesHeader)
            {
                nvs.Set(item, pricesData[index]);
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
            DateTime tradeDate, DateTime effectiveDate, int numberOfContracts, Double price, string futuresIdentifier, string tradingBook)
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
        /// <param name="properties2DRange">The properties range.</param>
        /// <returns></returns>
        public string CreateIRFutureTradeWithProperties(string tradeId, bool isParty1Buyer, string party1, 
            string party2, DateTime tradeDate, DateTime effectiveDate, int numberOfContracts,
            Decimal purchasePrice, string futuresIdentifier, Range properties2DRange)
        {
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
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
        /// <param name="properties2DRange">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <param name="businessDayCalendar">The calendar. </param>
        /// <returns></returns>
        public string CreateBulletWithProperties(DateTime tradeDate, DateTime paymentDate,
            string businessDayCalendar, string businessDayAdjustments, string currency, double amount, Range properties2DRange)
        {
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            var props = properties.ToNamedValueSet();
            var isPayerBase = props.GetValue<Boolean>("PayerIsBase", true);
            props.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.Cash_Payment));
            return ValService.CreateBulletPaymentWithProperties(tradeDate, paymentDate, isPayerBase, businessDayCalendar, businessDayAdjustments, currency, amount, props);
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
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate)
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
        /// <param name="properties2DRange">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxSpotWithProperties(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, Range properties2DRange)
        {
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            var props = properties.ToNamedValueSet();
            return ValService.CreateFxSpotWithProperties(tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, props);
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
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, Decimal forwardRate, [Optional] object forwardPoints)
        {
            decimal? result = null;
            if (!(forwardPoints is System.Reflection.Missing))
            {
                if (forwardPoints is Range r2) result = Convert.ToDecimal(r2.Value2);
            }
            return ValService.CreateFxForward(tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, forwardRate, result, "SpreadSheet");
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
        /// <param name="properties2DRange">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxForwardWithProperties(string tradeId, bool isPayerBase, DateTime tradeDate, string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, string quoteBasis, DateTime valueDate, Decimal spotRate, Decimal forwardRate, Range properties2DRange, 
            [Optional] object forwardPoints)
        {
            decimal? result = null;
            if (!(forwardPoints is System.Reflection.Missing))
            {
                if (forwardPoints is Range r2) result = Convert.ToDecimal(r2.Value2);
            }
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            var props = properties.ToNamedValueSet();
            return ValService.CreateFxForwardWithProperties(tradeId, isPayerBase, tradeDate, exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference,
            exchangeCurrency1Amount, exchangeCurrency1, exchangeCurrency2, quoteBasis, valueDate, spotRate, forwardRate, result, props);
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
            DateTime startValueDate, DateTime forwardValueDate, Decimal startRate, Decimal forwardRate, [Optional] object forwardPoints)
        {
            decimal? result = null;
            if (!(forwardPoints is System.Reflection.Missing))
            {
                if (forwardPoints is Range r2) result = Convert.ToDecimal(r2.Value2);
            }
            return ValService.CreateFxSwap(tradeId, isCurrency1LenderBase, tradeDate, currency1Lender, currency2Lender,
            currency1Amount, currency1, currency2, quoteBasis, startValueDate, forwardValueDate, startRate, forwardRate, result, "SpreadSheet");
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
        /// <param name="properties2DRange">The properties2DRange. This must contain:
        /// TradeId - the trade Id w/o the trade type.
        /// Party1 - the lender party 
        /// Party2 - the borrower party.
        /// This is used to determine the base direction in calculations</param>
        /// <returns></returns>
        public string CreateFxSwapWithProperties(string tradeId, bool isCurrency1LenderBase, DateTime tradeDate, string currency1Lender, string currency2Lender,
            decimal currency1Amount, string currency1, string currency2, string quoteBasis,
            DateTime startValueDate, DateTime forwardValueDate, Decimal startRate, Decimal forwardRate, Range properties2DRange, [Optional] object forwardPoints)
        {
            decimal? result = null;
            if (!(forwardPoints is System.Reflection.Missing))
            {
                if (forwardPoints is Range r2) result = Convert.ToDecimal(r2.Value2);
            }
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            var props = properties.ToNamedValueSet();
            props.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.ForeignExchange_Swap));
            return ValService.CreateFxSwapWithProperties(tradeId, isCurrency1LenderBase, tradeDate, currency1Lender, currency2Lender,
            currency1Amount, currency1, currency2, quoteBasis, startValueDate, forwardValueDate, startRate, forwardRate, result, props);
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
        /// <param name="expiryBusinessCenter">The expiry business centyer. </param>
        /// <param name="currency">The underlying currency.</param>
        /// <param name="expiryDate">The expiry date. </param>
        /// <param name="propertiesRange">The properties. </param>
        /// <param name="amount">The amount of currency.</param>
        /// <returns></returns>
        public string CreateVanillaFxOptionWithProperties(string tradeId, DateTime tradeDate,
        bool isBuyerBase, string buyerPartyReference, string sellerPartyReference, bool isCall, string currencyPair,
        Decimal strikePrice, decimal amount, string currency, DateTime expiryDate, string expiryBusinessCenter,
        string strikeQuoteBasis, DateTime valueDate, Range propertiesRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
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
        /// <param name="propertiesRange">THe properties. </param>
        public string CreateVanillaFxOption(Range fxOptionParametersRange, Range propertiesRange)
        {
            var leg1Values = fxOptionParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<FxOptionParametersRange>(ArrayHelper.RangeToMatrix(leg1Values));
            //Properties
            //var propertiesR = propertiesRange as Excel.Range;
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = ValService.CreateVanillaFxOption(leg1ParamRange, namedValueSet);
            return result;
        }

        #endregion

        #region Forward Rate Agreement Functions

        /// <summary>
        /// returns an array of rates
        /// </summary>
        /// <returns></returns>
        public object[,] GetFraEquivalentRates(Range propertiesRange, Range curveDataRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var curveData = curveDataRange.Value[System.Reflection.Missing.Value] as object[,];
            var tempData = ArrayHelper.RangeToMatrix(curveData);
            curveData = (object[,])DataRangeHelper.TrimNulls(tempData);
            int strCol = FraSolver.FindHeader(curveData, "Instrument");
            var instruments = FraSolver.GetObjects<string>(curveData, strCol);
            int rateCol = FraSolver.FindHeader(curveData, "Rate");
            var rates = FraSolver.GetObjects<double>(curveData, rateCol);
            var newRates = rates.Select(Convert.ToDecimal).ToList();
            int fraCol = FraSolver.FindHeader(curveData, "Guess");
            var fraGuesses = FraSolver.GetObjects<object>(curveData, fraCol);
            var fraEquivalents = ValService.CalculateFraEquivalents(namedValueSet, instruments, newRates, fraGuesses);
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
        /// <param name="indexTenor">The inindex tenor - this is relative to the base date as well!</param>
        /// <param name="discountingType">The discounting type: ISDA or AFMA.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="floatingRateIndex">The [Optional] floating rate index. If not providsed the default will be used.</param>
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
        /// <param name="fraInputRangeAsObject">An Nx2 range in Excel, with the first column a valid list of properties for a Fra.
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
        public string CreateFraTrade(Range fraInputRangeAsObject)
        {
            var values = fraInputRangeAsObject.Value[System.Reflection.Missing.Value] as object[,];
            values = (object[,])DataRangeHelper.TrimNulls(values);
            var fraInputRange = RangeHelper.Convert2DArrayToClass<FraInputRange2>(ArrayHelper.RangeToMatrix(values));
            var result = ValService.CreateFraTrade(fraInputRange, TradeSourceType.SpreadSheet);
            return result;
        }

        ///<summary>
        ///  Creates a fra trade and caches it locally.
        ///</summary>
        ///<param name="fraInputRangeAsObject">An Nx2 range in Excel, with the first column a valid list of properties for a Fra.
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
        ///<param name="propertiesRange">A list of properties, including mandatory ones.</param>
        ///<returns>The Id the valuation report is cached under.</returns>
        public string CreateFraTradeWithProperties(Range fraInputRangeAsObject, Range propertiesRange)
        {
            var values = fraInputRangeAsObject.Value[System.Reflection.Missing.Value] as object[,];
            values = (object[,])DataRangeHelper.TrimNulls(values);
            var fraInputRange = RangeHelper.Convert2DArrayToClass<FraInputRange2>(ArrayHelper.RangeToMatrix(values));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
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
        /// <param name="propertiesRange">A list of properties, including mandatory ones: trade date, trade id, currency and notional</param>
        /// <returns></returns>
        public string VanillaIRSwap(string tradeId, DateTime tradeDate, DateTime effectiveDate, string maturityTenor,
            string adjustedType, string currency, decimal notionalAmount, bool isParty1Base,
            Range fixedLeg, Range floatingLeg, Range propertiesRange)
        {
            var leg1Values = fixedLeg.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegSimpleRange>(ArrayHelper.RangeToMatrix(leg1Values));
            var leg2Values = floatingLeg.Value[System.Reflection.Missing.Value] as object[,];
            leg2Values = (object[,])DataRangeHelper.TrimNulls(leg2Values);
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegSimpleRange>(ArrayHelper.RangeToMatrix(leg2Values));
            //Properties
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var type = EnumHelper.Parse<AdjustedType>(adjustedType);
            return ValService.CreateIRSwap(tradeId, tradeDate, type, isParty1Base, effectiveDate, currency, notionalAmount, maturityTenor,
            leg1ParamRange, leg2ParamRange, namedValueSet);
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
            Range leg1ParametersRange, Range leg2ParametersRange)
        {
            var leg1Values = leg1ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg1Values));
            var leg2Values = leg2ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg2Values = (object[,])DataRangeHelper.TrimNulls(leg2Values);
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg2Values));
            return ValService.CreateInterestRateSwap(tradeId, tradeDate, isParty1Base, party1, party2,
            leg1ParamRange, leg2ParamRange);
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
        ///<param name="propertiesRange">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateInterestRateSwapWithProperties(Range leg1ParametersRange, Range leg2ParametersRange, Range propertiesRange)
        {
            var leg1Values = leg1ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg1Values));
            var leg2Values = leg2ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg2Values = (object[,])DataRangeHelper.TrimNulls(leg2Values);
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg2Values));
            //Properties
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
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
            namedValueSet.Set(TradeProp.EffectiveDate, leg1ParamRange.EffectiveDate);
            namedValueSet.Set(TradeProp.MaturityDate, leg1ParamRange.MaturityDate);
            namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
            namedValueSet.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            namedValueSet.Set(EnvironmentProp.SourceSystem, sourceSystem);
            namedValueSet.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            //Set the market curves.
            var market1 = new SwapLegEnvironment();
            if (leg1ParamRange.DiscountCurve != null)
            {
                market1.AddPricingStructure("DiscountCurve", Engine.GetCurve(leg1ParamRange.DiscountCurve, false));
            }
            if (leg1ParamRange.ForecastCurve != null)
            {
                market1.AddPricingStructure("ForecastCurve", Engine.GetCurve(leg1ParamRange.ForecastCurve, false));
            }
            var market2 = new SwapLegEnvironment();
            if (leg2ParamRange.DiscountCurve != null)
            {
                market2.AddPricingStructure("DiscountCurve", Engine.GetCurve(leg2ParamRange.DiscountCurve, false));
            }
            if (leg2ParamRange.ForecastCurve!=null)
            {
                market2.AddPricingStructure("ForecastCurve", Engine.GetCurve(leg2ParamRange.ForecastCurve, false));
            }
            //Create the swap.
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(Engine.Logger, Engine.Cache, Engine.NameSpace, leg1ParamRange, null, leg2ParamRange, null, null, null, null, market1, market2, valuationDate);
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
        ///<param name="legParametersRangeObject">Contains all the parameter information required by SwapLegParametersRange.
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
        ///<param name="valuationRangeObject">Contains all the parameter information required by ValuationRange, 
        /// notably the valuation date.</param>
        ///<returns>A list of detailed cash flow items</returns>
        public object[,] GetSwapLegDetailedCashflowsWithNotionalSchedule(Range legParametersRangeObject,
                                                                                        Range notionalValueItems,
                                                                                        Range valuationRangeObject)
        {
            //To object[,]
            var legs = legParametersRangeObject.Value[System.Reflection.Missing.Value] as object[,];
            var valuation = valuationRangeObject.Value[System.Reflection.Missing.Value] as object[,];
            var legParametersRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(legs));
            List<DateTimeDoubleRangeItem> datesAndNotionals = null;
            if (notionalValueItems.Value[System.Reflection.Missing.Value] is object[,] notional)
            {
                datesAndNotionals = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DateTimeDoubleRangeItem>(notional);
            }
            var valuationRange = RangeHelper.Convert2DArrayToClass<ValuationRange>(ArrayHelper.RangeToMatrix(valuation));
            var cashflows = ValService.GetSwapLegDetailedCashflowsWithNotionalSchedule(legParametersRange, datesAndNotionals, valuationRange);
            object[,] resultAsArray = ObjectToArrayOfPropertiesConverter.ConvertObjectToHorizontalArrayRange(cashflows);
            return resultAsArray;
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
        public object[,] CreateIRSwapPrincipalExchanges(Range legParametersRange, Range notionalValueItems,
                                                                              Range valuationRange)
        {
            var legValues = legParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            legValues = (object[,])DataRangeHelper.TrimNulls(legValues);
            var legParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(legValues));
            var valuation = valuationRange.Value[System.Reflection.Missing.Value] as object[,];
            valuation = (object[,])DataRangeHelper.TrimNulls(valuation);
            var valRange = RangeHelper.Convert2DArrayToClass<ValuationRange>(ArrayHelper.RangeToMatrix(valuation));
            var notionals = notionalValueItems.Value[System.Reflection.Missing.Value] as object[,];
            notionals = (object[,])DataRangeHelper.TrimNulls(notionals);
            var notionalArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DateTimeDoubleRangeItem>(ArrayHelper.RangeToMatrix(notionals));
            var irpricer = new InterestRateSwapPricer();
            var result = irpricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, NameSpace, legParamRange, notionalArray, valRange);
            object[,] resultAsArray = ObjectToArrayOfPropertiesConverter.ConvertObjectToHorizontalArrayRange(result);
            return resultAsArray;
        }

        /// <summary>
        ///  This function builds a swap and then saves all the trade details into a file.
        /// </summary>
        /// <param name="propertiesRange">THe properties range. </param>
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
        /// <param name="leg1AdditionalPaymentListArray">Any Leg1 additional cash flows.</param>
        /// <param name="leg2AdditionalPaymentListArray">Any Leg2 additional cash flows.</param>
        /// <returns>The identifier string for this event.</returns>
        public string BuildSwap(
            Range propertiesRange,
            Range leg1ParametersRange,
            Range leg2ParametersRange,
            Range leg1DetailedCashflowsListArray,
            Range leg2DetailedCashflowsListArray,
            Range leg1PrincipalExchangeCashflowListArray,
            Range leg2PrincipalExchangeCashflowListArray,
            Range leg1AdditionalPaymentListArray,
            Range leg2AdditionalPaymentListArray)
        {
            //Properties
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            //Transformation of data.
            var leg1 = leg1ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg1));
            var leg2 = leg2ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg2));
            var leg1DetailedCashflows = leg1DetailedCashflowsListArray.Value[System.Reflection.Missing.Value] as object[,];
            var leg1CashflowsListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputCashflowRangeItem>(leg1DetailedCashflows);
            var leg2DetailedCashflows = leg2DetailedCashflowsListArray.Value[System.Reflection.Missing.Value] as object[,];
            var leg2CashflowsListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputCashflowRangeItem>(leg2DetailedCashflows);
            var leg1PrincipalExchangeCashflow = leg1PrincipalExchangeCashflowListArray.Value[System.Reflection.Missing.Value] as object[,];
            var leg1PECashflowListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputPrincipalExchangeCashflowRangeItem>(leg1PrincipalExchangeCashflow);
            var leg2PrincipalExchangeCashflow = leg1PrincipalExchangeCashflowListArray.Value[System.Reflection.Missing.Value] as object[,];
            var leg2PECashflowListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputPrincipalExchangeCashflowRangeItem>(leg2PrincipalExchangeCashflow);
            var leg1AddPaymentListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AdditionalPaymentRangeItem>(ArrayHelper.RangeToMatrix(leg1AdditionalPaymentListArray.Value[System.Reflection.Missing.Value] as object[,]));
            var leg2AddPaymentListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AdditionalPaymentRangeItem>(ArrayHelper.RangeToMatrix(leg2AdditionalPaymentListArray.Value[System.Reflection.Missing.Value] as object[,]));
            var result = ValService.CreateSwapFromCashflows(Engine.Logger, Engine.Cache, NameSpace, leg1ParamRange, leg2ParamRange,
                leg1CashflowsListArray, leg2CashflowsListArray, leg1PECashflowListArray, leg2PECashflowListArray, leg1AddPaymentListArray, leg2AddPaymentListArray, namedValueSet);
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="propertiesRange">The properties, which contain all the valuation items. </param>
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
        public Object[,] CreateSwapPrice(
            Range propertiesRange,
            Range leg1,
            Range leg2,
            Range leg1DetailedCashflows,
            Range leg2DetailedCashflows,
            Range leg1PrincipalExchangeCashflow,
            Range leg2PrincipalExchangeCashflow,
            Range leg1AdditionalPayment,
            Range leg2AdditionalPayment)
        {
            //To classes
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var valRange = new ValuationRange
            {
                ValuationDate = namedValueSet.GetValue<DateTime>("ValuationDate", true),
            };
            var reportingParty = namedValueSet.GetValue<string>("ReportingParty", true);
            var party1 = namedValueSet.GetValue<string>("Party1", true);
            var baseParty = reportingParty == party1 ? "Party1" : "Party2";
            valRange.BaseParty = baseParty;
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg1.Value[System.Reflection.Missing.Value] as object[,]));
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg2.Value[System.Reflection.Missing.Value] as object[,]));
            var leg1CashflowsListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputCashflowRangeItem>(ArrayHelper.RangeToMatrix(leg1DetailedCashflows.Value[System.Reflection.Missing.Value] as object[,]));
            var leg2CashflowsListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputCashflowRangeItem>(ArrayHelper.RangeToMatrix(leg2DetailedCashflows.Value[System.Reflection.Missing.Value] as object[,]));
            var leg1PECashflowListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputPrincipalExchangeCashflowRangeItem>(ArrayHelper.RangeToMatrix(leg1PrincipalExchangeCashflow.Value[System.Reflection.Missing.Value] as object[,]));
            var leg2PECashflowListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputPrincipalExchangeCashflowRangeItem>(ArrayHelper.RangeToMatrix(leg2PrincipalExchangeCashflow.Value[System.Reflection.Missing.Value] as object[,]));
            var leg1AddPaymentListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AdditionalPaymentRangeItem>(ArrayHelper.RangeToMatrix(leg1AdditionalPayment.Value[System.Reflection.Missing.Value] as object[,]));
            var leg2AddPaymentListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AdditionalPaymentRangeItem>(ArrayHelper.RangeToMatrix(leg2AdditionalPayment.Value[System.Reflection.Missing.Value] as object[,]));
            var irSwap = new InterestRateSwapPricer();
            //Get the curves and create a swap market environment.
            var leg1DiscountCurve = (RateCurve)Engine.GetCurve(leg1ParamRange.DiscountCurve, false);
            var leg2DiscountCurve = (RateCurve)Engine.GetCurve(leg2ParamRange.DiscountCurve, false);
            var result = irSwap.GetPrice(Engine.Logger, Engine.Cache, NameSpace, valRange, leg1ParamRange, leg1DiscountCurve, leg2ParamRange, leg2DiscountCurve,
                leg1CashflowsListArray, leg2CashflowsListArray, leg1PECashflowListArray,
                leg2PECashflowListArray, leg1AddPaymentListArray, leg2AddPaymentListArray);
            object[,] resultAsArray = ObjectToArrayOfPropertiesConverter.ConvertObjectToHorizontalArrayRange(result);
            return resultAsArray;
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
        /// <param name="propertiesRange">A list of properties, including mandatory ones: trade date, trade id, currency and notional</param>
        /// <returns></returns>
        public string XccySwap(string tradeId, DateTime tradeDate, DateTime effectiveDate, string maturityTenor,
            string adjustedType, bool isParty1Base,
            Range fixedLeg, Range floatingLeg, Range propertiesRange)
        {
            var leg1Values = fixedLeg.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SimpleXccySwapLeg>(ArrayHelper.RangeToMatrix(leg1Values));
            var leg2Values = floatingLeg.Value[System.Reflection.Missing.Value] as object[,];
            leg2Values = (object[,])DataRangeHelper.TrimNulls(leg2Values);
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SimpleXccySwapLeg>(ArrayHelper.RangeToMatrix(leg2Values));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var type = EnumHelper.Parse<AdjustedType>(adjustedType);
            return ValService.CreateXccySwap(tradeId, tradeDate, type, isParty1Base, effectiveDate, maturityTenor,
            leg1ParamRange, leg2ParamRange, namedValueSet);
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
            Range leg1ParametersRange, Range leg2ParametersRange)
        {
            object[,] leg1Values = leg1ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg1Values));
            object[,] leg2Values = leg2ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg2Values = (object[,])DataRangeHelper.TrimNulls(leg2Values);
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg2Values));
            return ValService.CreateCrossCurrencySwap(productTaxonomy, tradeId, tradeDate, isParty1Base, party1, party2, TradeSourceType.SpreadSheet,
            leg1ParamRange, leg2ParamRange);
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
        ///<param name="propertiesRange">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateXccySwapWithProperties(Range leg1ParametersRange, Range leg2ParametersRange, Range propertiesRange)
        {
            var leg1Values = leg1ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg1Values));
            var leg2Values = leg2ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg2Values = (object[,])DataRangeHelper.TrimNulls(leg2Values);
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg2Values));
            //Properties
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
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
            namedValueSet.Set(TradeProp.EffectiveDate, leg1ParamRange.EffectiveDate);
            namedValueSet.Set(TradeProp.MaturityDate, leg1ParamRange.MaturityDate);
            namedValueSet.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
            namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            namedValueSet.Set(EnvironmentProp.SourceSystem, sourceSystem);
            namedValueSet.Set(EnvironmentProp.Schema, FpML5R3NameSpaces.ReportingSchema);
            //Set the market curves.
            var market1 = new SwapLegEnvironment();
            if (leg1ParamRange.DiscountCurve != null)
            {
                market1.AddPricingStructure("DiscountCurve", Engine.GetCurve(leg1ParamRange.DiscountCurve, false));
            }
            if (leg1ParamRange.ForecastCurve != null)
            {
                market1.AddPricingStructure("ForecastCurve", Engine.GetCurve(leg1ParamRange.ForecastCurve, false));
            }
            var market2 = new SwapLegEnvironment();
            if (leg2ParamRange.DiscountCurve != null)
            {
                market2.AddPricingStructure("DiscountCurve", Engine.GetCurve(leg2ParamRange.DiscountCurve, false));
            }
            if (leg2ParamRange.ForecastCurve != null)
            {
                market2.AddPricingStructure("ForecastCurve", Engine.GetCurve(leg2ParamRange.ForecastCurve, false));
            }
            //Create the swap.
            var swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(Engine.Logger, Engine.Cache, Engine.NameSpace, leg1ParamRange, null, leg2ParamRange, null, null, null, null, market1, market2, valuationDate);
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
        public object[,] CreateXccySwapPrincipalExchanges(Range legParametersRange, Range notionalValueItems,
                                                                              Range valuationRange)
        {
            object[,] legValues = legParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            legValues = (object[,])DataRangeHelper.TrimNulls(legValues);
            var legParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(legValues));
            object[,] valuation = valuationRange.Value[System.Reflection.Missing.Value] as object[,];
            valuation = (object[,])DataRangeHelper.TrimNulls(valuation);
            var valRange = RangeHelper.Convert2DArrayToClass<ValuationRange>(ArrayHelper.RangeToMatrix(valuation));
            object[,] notionals = notionalValueItems.Value[System.Reflection.Missing.Value] as object[,];
            notionals = (object[,])DataRangeHelper.TrimNulls(notionals);
            var notionalArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DateTimeDoubleRangeItem>(ArrayHelper.RangeToMatrix(notionals));
            var xccyRricer = new CrossCurrencySwapPricer();
            var result = xccyRricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, NameSpace, legParamRange, notionalArray, valRange);
            object[,] resultAsArray = ObjectToArrayOfPropertiesConverter.ConvertObjectToHorizontalArrayRange(result);
            return resultAsArray;
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
        /// <param name="propertiesRange">A list of properties, including mandatory ones: trade date, trade id, currency and notional</param>
        /// <returns></returns>
        public string SimpleIRSwaption(string tradeId, DateTime tradeDate, DateTime effectiveDate, string maturityTenor,
            string adjustedType, string currency, decimal notionalAmount, bool isParty1Base,
            Range fixedLeg, Range floatingLeg, Range swaptionTerms, Range propertiesRange)
        {
            var leg1Values = fixedLeg.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegSimpleRange>(ArrayHelper.RangeToMatrix(leg1Values));
            //var floatingLeg = floatingLeg1 as Excel.Range;
            var leg2Values = floatingLeg.Value[System.Reflection.Missing.Value] as object[,];
            leg2Values = (object[,])DataRangeHelper.TrimNulls(leg2Values);
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegSimpleRange>(ArrayHelper.RangeToMatrix(leg2Values));
            object[,] terms = swaptionTerms.Value[System.Reflection.Missing.Value] as object[,];
            var swaptionTermsRange = RangeHelper.Convert2DArrayToClass<SwaptionParametersRange>(ArrayHelper.RangeToMatrix(terms));
            //Properties
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var type = EnumHelper.Parse<AdjustedType>(adjustedType);
            return ValService.CreateSimpleIRSwaption(tradeId, tradeDate, type, isParty1Base, effectiveDate, currency, notionalAmount, maturityTenor,
            leg1ParamRange, leg2ParamRange, swaptionTermsRange, namedValueSet);
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
        /// <param name="propertiesRange"></param>
        /// <returns>The identified for the swaption..</returns>
        public string CreateSwaptionTrade(
            Range swaptionTerms,
            Range leg1ParametersRange, 
            Range leg2ParametersRange, 
            Range propertiesRange
            )
        {
            //To object[,]
            var terms = swaptionTerms.Value[System.Reflection.Missing.Value] as object[,];
            var swaptionTermsRange = RangeHelper.Convert2DArrayToClass<SwaptionParametersRange>(ArrayHelper.RangeToMatrix(terms));
            var leg1Values = leg1ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg1Values));
            var leg2Values = leg2ParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg2Values = (object[,])DataRangeHelper.TrimNulls(leg2Values);
            var leg2ParamRange = RangeHelper.Convert2DArrayToClass<SwapLegParametersRange>(ArrayHelper.RangeToMatrix(leg2Values));
            //Properties
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(TradeProp.TradingBookId, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.InterestRateSwaption.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.swaption.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_Option_Swaption));
            namedValueSet.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            namedValueSet.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            return ValService.CreateInterestRateSwaptionWithProperties(swaptionTermsRange, leg1ParamRange, leg2ParamRange, namedValueSet);
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
        /// <param name="propertiesRange">A list of properties, including mandatory ones: trade date, trade id, currency and notional</param>
        /// <returns></returns>
        public string CapFloor(string tradeId, DateTime tradeDate, DateTime effectiveDate, string maturityTenor,
            string adjustedType, string currency, decimal notionalAmount, decimal strike, bool isParty1Base,
            Range capLeg, Range propertiesRange)
        {
            var leg1Values = capLeg.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<CapFloorLegSimpleRange>(ArrayHelper.RangeToMatrix(leg1Values));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var type = EnumHelper.Parse<AdjustedType>(adjustedType);
            return ValService.CreateCapFloor(tradeId, tradeDate, type, isParty1Base, effectiveDate, currency, notionalAmount, maturityTenor, strike, 
            leg1ParamRange, namedValueSet);
        }

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
        /// <para>DiscountCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>ForecastCurve:	Orion.Market.Live.RateCurve.AUD-BBR-BBSW-1M</para>
        /// <para>DiscountingType:	Standard</para>
        /// <para>BaseParty:	CBA</para>
        /// <para> CapFloorType CapOrFloor</para>
        /// <para>  StrikeRate</para>
        /// <para>  VolatilitySurface</para>
        /// </param>
        public string CreateCapFloor(string tradeId, DateTime tradeDate, bool isParty1Base, string party1, string party2, //bool isParty1Leg1Payer,
            Range capfloorParametersRange)
        {
            var leg1Values = capfloorParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<CapFloorLegParametersRange>(ArrayHelper.RangeToMatrix(leg1Values));
            return ValService.CreateCapFloor(tradeId, tradeDate, isParty1Base, party1, party2,
            leg1ParamRange);
        }

        ///<summary>
        /// Gets the principal exchanges in the defined swap.
        ///</summary>
        ///<param name="capfloorParametersRange">An Nx2 range in Excel, with the first column a valid list of properties for a swap leg.
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
        ///<param name="propertiesRange">A list of properties, including mandatory ones: trade date, trade id, maturity date, valuation date</param>
        public string CreateCapFloorWithProperties(Range capfloorParametersRange, Range propertiesRange)
        {
            var leg1Values = capfloorParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            leg1Values = (object[,])DataRangeHelper.TrimNulls(leg1Values);
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<CapFloorLegParametersRange>(ArrayHelper.RangeToMatrix(leg1Values));
            //Properties
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var sourceSystem = namedValueSet.GetValue(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            namedValueSet.Set(TradeProp.TradingBookId, TradeState.Test.ToString());
            namedValueSet.Set(TradeProp.TradingBookName, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.ProductType, ProductTypeSimpleEnum.CapFloor.ToString());
            namedValueSet.Set(TradeProp.TradeType, ItemChoiceType15.capFloor.ToString());
            namedValueSet.Set(TradeProp.ProductTaxonomy, ProductTaxonomyScheme.GetEnumString(ProductTaxonomyEnum.InterestRate_CapFloor));
            namedValueSet.Set(TradeProp.TradeSource, TradeSourceType.SpreadSheet);
            namedValueSet.Set(EnvironmentProp.SourceSystem, TradeSourceType.SpreadSheet);
            namedValueSet.Set(TradeProp.EffectiveDate, leg1ParamRange.EffectiveDate);
            namedValueSet.Set(TradeProp.MaturityDate, leg1ParamRange.MaturityDate);
            namedValueSet.Set(TradeProp.BaseParty, TradeProp.Party1);
            namedValueSet.Set(TradeProp.TradeState, TradeState.Pricing.ToString());
            namedValueSet.Set(TradeProp.CounterPartyName, TradeProp.Party2);
            namedValueSet.Set(TradeProp.AsAtDate, DateTime.Today);
            var valuationDate = namedValueSet.GetValue<DateTime>(TradeProp.ValuationDate, true);
            var tradeDate = namedValueSet.GetValue<DateTime>(TradeProp.TradeDate, true);
            var tradeId = namedValueSet.GetValue<string>(TradeProp.TradeId, true);
            //Set the market curves.
            var market1 = new SwapLegEnvironment();
            if (leg1ParamRange.DiscountCurve != null)
            {
                market1.AddPricingStructure("DiscountCurve", Engine.GetCurve(leg1ParamRange.DiscountCurve, false));
            }
            if (leg1ParamRange.ForecastCurve != null)
            {
                market1.AddPricingStructure("ForecastCurve", Engine.GetCurve(leg1ParamRange.ForecastCurve, false));
            }
            if (leg1ParamRange.VolatilitySurface != null)
            {
                market1.AddPricingStructure("VolatilitySurface", Engine.GetCurve(leg1ParamRange.VolatilitySurface, false));
            }
            //Create the cap.
            var capfloor = CapFloorGenerator.GenerateDefinitionCashflowsAmounts(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, leg1ParamRange, null, null, null, market1, valuationDate);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetCapFloor(trade, capfloor);
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
        ///<param name="propertiesRange"> </param>
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
        ///<returns>The identifer string for this event.</returns>
        public string BuildCapFloor(
            Range propertiesRange,
            Range capFloorParametersRange,
            Range capFloorDetailedCashflowsListArray,
            Range capFloorPrincipalExchangeCashflowListArray,
            Range capFloorAdditionalCashflowListArray,
            Range feePaymentList)
        {
            //Properties
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            //Transformation of data.
            var leg1 = capFloorParametersRange.Value[System.Reflection.Missing.Value] as object[,];
            var leg1ParamRange = RangeHelper.Convert2DArrayToClass<CapFloorLegParametersRange>(ArrayHelper.RangeToMatrix(leg1));
            var leg1DetailedCashflows = capFloorDetailedCashflowsListArray.Value[System.Reflection.Missing.Value] as object[,];
            var leg1CashflowsListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputCashflowRangeItem>(leg1DetailedCashflows);
            var leg1PrincipalExchangeCashflow = capFloorPrincipalExchangeCashflowListArray.Value[System.Reflection.Missing.Value] as object[,];
            var leg1PECashflowListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InputPrincipalExchangeCashflowRangeItem>(leg1PrincipalExchangeCashflow);
            var leg1AddPaymentList = capFloorAdditionalCashflowListArray.Value[System.Reflection.Missing.Value] as object[,];
            var leg1AdditionalListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AdditionalPaymentRangeItem>(leg1AddPaymentList);
            var leg1Fees = feePaymentList.Value[System.Reflection.Missing.Value] as object[,];
            var leg1FeesListArray = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<FeePaymentRangeItem>(leg1Fees);          
            //var isParty1Base = namedValueSet.GetValue<bool>("IsParty1Base", false);
            var result = ValService.CreateCapFloorFromCashflows(Engine.Logger, Engine.Cache, NameSpace, leg1ParamRange, leg1CashflowsListArray, leg1PECashflowListArray, leg1AdditionalListArray, leg1FeesListArray, namedValueSet);
            return result;
        }

        #endregion

        #region BillSwap Pricer

        ///<summary>
        ///</summary>
        ///<param name="billSwapTerms"></param>
        ///<returns></returns>
        public object[,] BuildSwapDates(Range billSwapTerms)
        {
            var terms = billSwapTerms.Value[System.Reflection.Missing.Value] as object[,];
            var billSwapPricer = new BillSwapPricer();
            var billSwapPricerDatesRange = RangeHelper.Convert2DArrayToClass<BillSwapPricerDatesRange>(DataRangeHelper.RangeToMatrix(terms));
            List<DateTime> dates = billSwapPricer.BuildDates(Engine.Logger, Engine.Cache, NameSpace, billSwapPricerDatesRange, null);
            var result = dates.Select(dts => new DateTimeRangeItem { Value = dts }).ToList();
            object[,] resultAsArray = ObjectToArrayOfPropertiesConverter.ConvertObjectToHorizontalArrayRange(result);
            return resultAsArray;
        }

        ///<summary>
        ///</summary>
        ///<param name="dateAndNotional"></param>
        ///<param name="dayCounterAsString"></param>
        ///<param name="curveId"></param>
        ///<returns></returns>
        public double GetSimpleYield(Range dateAndNotional, string dayCounterAsString, string curveId)
        {
            var dates = dateAndNotional.Value[System.Reflection.Missing.Value] as object[,];
            List<BillSwapPricerDateNotional> datesAndNotionals =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<BillSwapPricerDateNotional>(dates);
            var billSwapPricer = new BillSwapPricer();
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCounterAsString);
            List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(datesAndNotionals, dayCounter, rc);
            double annYield = billSwapPricer.GetSimpleYield(forwardRates, dayCounter, rc);
            return annYield;
        }

        ///<summary>
        ///</summary>
        ///<param name="dateAndNotional"></param>
        ///<param name="dayCounterAsString"></param>
        ///<param name="curveId"></param>
        ///<returns></returns>
        public double GetAnnualCompoundingYield(Range dateAndNotional, string dayCounterAsString, string curveId)
        {
            var dates = dateAndNotional.Value[System.Reflection.Missing.Value] as object[,];
            List<BillSwapPricerDateNotional> datesAndNotionals =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<BillSwapPricerDateNotional>(dates);
            var billSwapPricer = new BillSwapPricer();
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCounterAsString);
            List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(datesAndNotionals, dayCounter, rc);
            double annYield = billSwapPricer.GetAnnualYield(forwardRates, dayCounter, rc);
            return annYield;
        }

        ///<summary>
        ///</summary>
        ///<param name="dateAndNotional"></param>
        ///<param name="dayCounterAsString"></param>
        ///<param name="curveId"></param>
        ///<returns></returns>
        public object[,] CalculatePurchaseCost( Range dateAndNotional, string dayCounterAsString, string curveId)
        {
            var dates = dateAndNotional.Value[System.Reflection.Missing.Value] as object[,];
            List<BillSwapPricerDateNotional> datesAndNotionals =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<BillSwapPricerDateNotional>(dates);
            var billSwapPricer = new BillSwapPricer();
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCounterAsString);
            List<BillSwapPricerCashflowRow> forwardRates = billSwapPricer.PopulateForwardRates(datesAndNotionals, dayCounter, rc);
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
            Range payTerms, Range payRolls,
            Range receiveTerms, Range receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue)
        {
            var pay = payTerms.Value[System.Reflection.Missing.Value] as object[,];
            var payrolls = payRolls.Value[System.Reflection.Missing.Value] as object[,];
            var receive = receiveTerms.Value[System.Reflection.Missing.Value] as object[,];
            var recRolls = receiveRolls.Value[System.Reflection.Missing.Value] as object[,];
            var payTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(pay));
            var receiveTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(receive));
            List<AmortisingResultItem> payRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(payrolls);
            List<AmortisingResultItem> receiveRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(recRolls);
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            double fixedRate = BillsSwapPricer2.CalculateFixedRate(valuationDate, floatMargin, payTermsFormat, payRollsFormat, receiveTermsFormat,
                receiveRollsFormat, rc, bulletPaymentDate, bulletPaymentValue);
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
            Range payTerms, Range payRolls,
            Range receiveTerms, Range receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue)
        {
            var pay = payTerms.Value[System.Reflection.Missing.Value] as object[,];
            var payrolls = payRolls.Value[System.Reflection.Missing.Value] as object[,];
            var receive = receiveTerms.Value[System.Reflection.Missing.Value] as object[,];
            var recRolls = receiveRolls.Value[System.Reflection.Missing.Value] as object[,];
            var payTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(pay));
            var receiveTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(receive));
            List<AmortisingResultItem> payRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(payrolls);
            List<AmortisingResultItem> receiveRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(recRolls);
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            double fixedSidePV = BillsSwapPricer2.CalculateFixedSidePV(valuationDate, floatMargin, fixedRate,
                payTermsFormat, payRollsFormat, receiveTermsFormat, receiveRollsFormat, rc, bulletPaymentDate, bulletPaymentValue);
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
            Range payTerms, Range payRolls,
            Range receiveTerms, Range receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue,
            string curveInstrumentId, double perturbationAmount)
        {
            var pay = payTerms.Value[System.Reflection.Missing.Value] as object[,];
            var payrolls = payRolls.Value[System.Reflection.Missing.Value] as object[,];
            var receive = receiveTerms.Value[System.Reflection.Missing.Value] as object[,];
            var recrolls = receiveRolls.Value[System.Reflection.Missing.Value] as object[,];
            var payTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(pay));
            var receiveTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(receive));
            List<AmortisingResultItem> payRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(payrolls);
            List<AmortisingResultItem> receiveRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(recrolls);
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            var sensitivity = BillsSwapPricer2.CalculateFixedSideSensitivity(valuationDate,
                floatMargin, fixedRate, payTermsFormat, payRollsFormat, receiveTermsFormat, receiveRollsFormat, rc,
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
            Range payTerms, Range payRolls,
            Range receiveTerms, Range receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue,
            Range listInstrumentIdAndQuotes,
            Range listPerturbations)
        {
            var pay = payTerms.Value[System.Reflection.Missing.Value] as object[,];
            var payrolls = payRolls.Value[System.Reflection.Missing.Value] as object[,];
            var receive = receiveTerms.Value[System.Reflection.Missing.Value] as object[,];
            var recRolls = receiveRolls.Value[System.Reflection.Missing.Value] as object[,];
            var payTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(pay));
            var instruments = listInstrumentIdAndQuotes.Value[System.Reflection.Missing.Value] as object[,];
            var perturbations = listPerturbations.Value[System.Reflection.Missing.Value] as object[,];
            var receiveTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(receive));
            List<AmortisingResultItem> payRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(payrolls);
            List<AmortisingResultItem> receiveRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(recRolls);
            List<InstrumentIdAndQuoteRangeItem> listInstrumentIdAndQuotesFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InstrumentIdAndQuoteRangeItem>(instruments);
            List<DoubleRangeItem> listPerturbationsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DoubleRangeItem>(perturbations);
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            List<DoubleRangeItem> sensitivity = BillsSwapPricer2.CalculateFixedSideSensitivity2(valuationDate, floatMargin,
                fixedRate, payTermsFormat, payRollsFormat, receiveTermsFormat, receiveRollsFormat, rc, bulletPaymentDate,
                bulletPaymentValue, listInstrumentIdAndQuotesFormat, listPerturbationsFormat);
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
            Range payTerms, Range payRolls,
            Range receiveTerms, Range receiveRolls,
            string curveId, DateTime bulletPaymentDate, double bulletPaymentValue,
            Range listInstrumentIdAndQuotes,
            Range listPerturbations, string filterByInstruments)
        {
            var pay = payTerms.Value[System.Reflection.Missing.Value] as object[,];
            var payrolls = payRolls.Value[System.Reflection.Missing.Value] as object[,];
            var receive = receiveTerms.Value[System.Reflection.Missing.Value] as object[,];
            var recRolls = receiveRolls.Value[System.Reflection.Missing.Value] as object[,];
            var instruments = listInstrumentIdAndQuotes.Value[System.Reflection.Missing.Value] as object[,];
            var perturbations = listPerturbations.Value[System.Reflection.Missing.Value] as object[,];
            var payTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(pay));
            var receiveTermsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(receive));
            List<AmortisingResultItem> payRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(payrolls);
            List<AmortisingResultItem> receiveRollsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(recRolls);
            List<InstrumentIdAndQuoteRangeItem> listInstrumentIdAndQuotesFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<InstrumentIdAndQuoteRangeItem>(instruments);
            List<DoubleRangeItem> listPerturbationsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DoubleRangeItem>(perturbations);
            var rc = (RateCurve)Engine.GetCurve(curveId, false);
            var sensitivity = BillsSwapPricer2.CalculateFixedSideDelta(valuationDate, floatMargin, fixedRate,
                                                                       payTermsFormat, payRollsFormat, receiveTermsFormat,
                                                                       receiveRollsFormat, rc, bulletPaymentDate,
                                                                       bulletPaymentValue, listInstrumentIdAndQuotesFormat, listPerturbationsFormat, filterByInstruments);
            return sensitivity;
        }

        ///<summary>
        ///</summary>
        ///<param name="cashflowsSchedule"></param>
        ///<param name="terms"></param>
        ///<returns></returns>
        public double GetEffectiveFrequency(Range cashflowsSchedule, Range terms)
        {
            var cash = cashflowsSchedule.Value[System.Reflection.Missing.Value] as object[,];
            var t = terms.Value[System.Reflection.Missing.Value] as object[,];
            var termsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(t));
            List<AmortisingResultItem> cashflowsScheduleFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(cash);
            double effectiveFrequency = BillsSwapPricer2.GetEffectiveFrequency(cashflowsScheduleFormat, termsFormat);
            return effectiveFrequency;
        }

        ///<summary>
        ///</summary>
        ///<param name="termsRange"></param>
        ///<param name="metaScheduleDefinitionRange"></param>
        ///<returns></returns>
        public object[,] GenerateCashflowSchedule(Range termsRange, Range metaScheduleDefinitionRange)
        {
            var termsR = termsRange.Value[System.Reflection.Missing.Value] as object[,];
            var meta = metaScheduleDefinitionRange.Value[System.Reflection.Missing.Value] as object[,];
            var terms = (object[,])DataRangeHelper.TrimNulls(termsR);
            var termsFormat = RangeHelper.Convert2DArrayToClass<BillsSwapPricer2TermsRange>(ArrayHelper.RangeToMatrix(terms));
            List<MetaScheduleRangeItem> metaScheduleDefinitionRangeFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<MetaScheduleRangeItem>(meta);
            var cashflowSchedule = BillsSwapPricer2.GenerateCashflowSchedule(termsFormat, metaScheduleDefinitionRangeFormat, null);
            object[,] result = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(cashflowSchedule);
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="cfItems"></param>
        ///<param name="amortSchedule"></param>
        ///<returns></returns>
        public object[,] GenerateAmortisationSchedule(Range cfItems, Range amortSchedule)
        {
            var termsR = cfItems.Value[System.Reflection.Missing.Value] as object[,];
            var amort = amortSchedule.Value[System.Reflection.Missing.Value] as object[,];
            List<AmortisingResultItem> cfItemsFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingResultItem>(termsR);
            List<AmortisingScheduleItem> amortScheduleFormat =
                ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<AmortisingScheduleItem>(amort);
            var amortizationSchedule = BillsSwapPricer2.GenerateAmortisationSchedule(cfItemsFormat, amortScheduleFormat);
            object[,] result = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(amortizationSchedule);
            return result;
        }

        #endregion
    }
}
