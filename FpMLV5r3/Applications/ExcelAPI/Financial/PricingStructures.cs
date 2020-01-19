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

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Constants;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Metadata.Common;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.NamedValues;
using Highlander.ValuationEngine.V5r3.Helpers;
using HLV5r3.Helpers;
using HLV5r3.Impl;
using FxCurve = Highlander.CurveEngine.V5r3.PricingStructures.Curves.FxCurve;
using RateCurve = Highlander.CurveEngine.V5r3.PricingStructures.Curves.RateCurve;
using Excel = Microsoft.Office.Interop.Excel;

#endregion

namespace HLV5r3.Financial
{
    /// <summary>
    /// Caches all the curves created.
    /// </summary>
    public partial class Cache
    {
        //#region Pedersen Calibration

        //#region Set the curves with Identifiers

        ///// <summary>
        ///// Sets the discount factors to use
        ///// </summary>
        ///// <param name="rateCurveId"></param>
        ///// <returns></returns>
        //public string PedersenSetDiscountFactors(String rateCurveId)
        //{
        //    return Engine.GetCurve(rateCurveId, false) is IRateCurve rateCurve ? Engine.Pedersen.SetDiscountFactors(rateCurve) : String.Format("Discount factors were not set.");
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="strike"></param>
        ///// <param name="volSurfaceIdentifier"></param>
        ///// <returns></returns>
        //public string PedersenSetCapletVolatilities(Double strike, String volSurfaceIdentifier)
        //{
        //    return Engine.GetCurve(volSurfaceIdentifier, false) is IStrikeVolatilitySurface volSurface ? Engine.Pedersen.SetCapletImpliedVolatility(strike, volSurface) : null;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="volSurfaceIdentifier"></param>
        ///// <returns></returns>
        //public string PedersenSetSwaptionVolatilities(String volSurfaceIdentifier)
        //{
        //    if (Engine.GetCurve(volSurfaceIdentifier, false) is IVolatilitySurface volSurface)
        //    {
        //        Pair<PricingStructure, PricingStructureValuation> fpMLPair = volSurface.GetFpMLData();
        //        var volObj = PricingStructureHelper.FpMLPairTo2DArray(fpMLPair);
        //        return Engine.Pedersen.SetSwaptionImpliedVolatility(volObj);
        //    }
        //    return null;
        //}

        //#endregion

        //#region Set the curves with Ranges

        ///// <summary>
        ///// Setting discount function from a range in the spreadsheet
        ///// * Requires actual data
        ///// </summary>
        ///// <param name="discountFactorArray">THe discount factors by tenor.</param>
        ///// <returns></returns>
        //public string PedersenSetDiscountFactorRange(Excel.Range discountFactorArray)
        //{
        //    var r1 = DataRangeHelper.StripDoubleRange(discountFactorArray);
        //    return Engine.Pedersen.SetDiscountFactors(r1);
        //}

        ///// <summary>
        ///// Converts an Excel 2 column range of monthly tenors and volatilities.
        ///// </summary>
        ///// <param name="capletVolArray">the caplet volatilities by tenor.</param>
        ///// <returns></returns>
        //public string PedersenSetCapletRange(Excel.Range capletVolArray)
        //{
        //    var r1 = DataRangeHelper.StripDoubleRange(capletVolArray);
        //    return Engine.Pedersen.SetCapletImpliedVolatility(r1);
        //}

        ///// <summary>
        ///// Converts an Excel swaption matrix range of monthly tenors and volatilities.
        ///// </summary>
        ///// <param name="range">The matrix of volatilities and tenors.</param>
        ///// <returns></returns>
        //public string PedersenSetSwaptionRange(Excel.Range range)
        //{
        //    var r1 = new Utility.Utilities().ConvertRangeTo2DArray(range);
        //    return Engine.Pedersen.SetSwaptionImpliedVolatility(r1);
        //}

        ///// <summary>
        ///// Sets the correlation matrix as a range in Excel.
        ///// </summary>
        ///// <param name="range">The correlation matrix.</param>
        ///// <returns></returns>
        //public object PedersenSetCorrelation(Excel.Range range)
        //{
        //    var r1 = new Utility.Utilities().ConvertRangeTo2DArray(range);
        //    return Engine.Pedersen.SetCorrelation(r1);
        //}

        //#endregion

        //#region Calibration

        ///// <summary>
        ///// Performs the actual calibration with a setting range.
        ///// All the curves must be set, using the setter functions.
        ///// </summary>
        ///// <param name="range"></param>
        ///// <returns></returns>
        //public object PedersenCalibration(Excel.Range range)
        //{
        //    var r1 = new Utility.Utilities().ConvertRangeTo2DArray(range);
        //    return Engine.Pedersen.Calibration(r1);
        //}

        ///// <summary>
        ///// WIP stub. Still using dummy vol and correlation.
        ///// </summary>
        ///// <param name="rateCurveIdentifier">RateCurve Id</param>
        ///// <returns></returns>
        //public object[,] PedersenCalibration1(string rateCurveIdentifier)
        //{

        //    var result = Engine.PedersenCalibration(rateCurveIdentifier);
        //    return result;
        //}

        ///// <summary>
        ///// WIP stub. Still using dummy vol and correlation.
        ///// </summary>
        ///// <param name="rateCurveIdentifier">RateCurve Id</param>
        ///// <param name="capletCurveIdentifier">The caplet volatility curve.</param>
        ///// <param name="strike">The strike of the caplet volatility curve.</param>
        ///// <param name="swaptionVolSurfaceIdentifier">The swaption volatility surface identifier.</param>
        ///// <param name="correlationRange">The correlation matrix as a range.</param>
        ///// <returns></returns>
        //public object[,] PedersenCalibration2(string rateCurveIdentifier, String capletCurveIdentifier, Double strike, 
        //    String swaptionVolSurfaceIdentifier, Excel.Range correlationRange)
        //{
        //    Engine.PedersenSetDiscountFactors(rateCurveIdentifier);
        //    Engine.PedersenSetCapletImpliedVolatility(strike, capletCurveIdentifier);
        //    Engine.PedersenSetSwaptionImpliedVolatility(swaptionVolSurfaceIdentifier);
        //    var r1 = new Utility.Utilities().ConvertRangeTo2DArray(correlationRange);
        //    Engine.Pedersen.SetCorrelation(r1);
        //    var result = Engine.PedersenCalibration(rateCurveIdentifier);
        //    return result;
        //}

        //#endregion

        //#region Investigative Functions

        ///// <summary>
        ///// Show the correlation data.
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenShowCorrelation()
        //{
        //    return Engine.PedersenShowCorrelation();
        //}

        ///// <summary>
        ///// Displays post-Calibration result summary.
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenCalSummary()
        //{
        //    return Engine.Pedersen.CalSummary();
        //}

        ///// <summary>
        ///// Displays post-Calibration vol surface (multi-factored)
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenCalVol()
        //{
        //    return Engine.Pedersen.CalVol();
        //}

        ///// <summary>
        ///// Displays post-Calibration vol surface (vol sizes)
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenCalVolNorm()
        //{
        //    return Engine.Pedersen.CalVolNorm();
        //}

        //#endregion

        //#region Simulation

        //public object PedersenSimulation(Excel.Range range)
        //{
        //    var r1 = new Utility.Utilities().ConvertRangeTo2DArray(range);
        //    return Engine.Pedersen.Simulation(r1);
        //}

        //public object PedersenSimSummary()
        //{
        //    return Engine.Pedersen.SimSummary();
        //}

        ///// <summary>
        ///// Gets the debug information
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenDebugOutput()
        //{
        //    return Engine.Pedersen.DebugOutput();
        //}

        //#endregion

        //#endregion

        #region Curve Building and Property Functions

        /// <summary>
        /// The equities market data that is updated real-time and is distributed. 
        /// This will become redundant once I work out how to use the market data service and eventually bond curves will be a type of discount curve.
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should include: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdArray">A list of instrumentIds to update of the form Equity.Ticker.PricingSource e.g. Equity.ANZ.AU</param>
        /// <param name="marketDataQuoteArray">The quotes can be: P(Price).</param>
        /// <param name="quotesRange">The actual quote range consistent with the quote list.</param> 
        /// <returns></returns>
        public string UpdateEquitiesMarkets(Excel.Range propertiesAs2DRange,
                                              Excel.Range instrumentIdArray, Excel.Range marketDataQuoteArray,
                                              Excel.Range quotesRange)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> instrumentIdList = DataRangeHelper.StripRange(instrumentIdArray);
            List<string> marketDataQuoteList = DataRangeHelper.StripRange(marketDataQuoteArray);
            var quotesMatrix = DataRangeHelper.ToMatrix<Double>(quotesRange);
            var result = ValService.UpdateSecuritiesMarkets(namedValueSet, instrumentIdList, marketDataQuoteList, quotesMatrix);
            return result;
        }

        /// <summary>
        /// The securities market data that is updated real-time and is distributed. 
        /// This will become redundant once I work out how to use the market data service and eventually bond curves will be a type of discount curve.
        /// </summary>
        /// <param name="propertiesAs2DRange">The market properties. This should include: MarketName, BaseDate, Currency etc.</param>
        /// <param name="instrumentIdArray">A list of instrumentIds to update of the form Corp.Ticker.CouponType.Coupon.Maturity e.g. Corp.ANZ.Fixed.5,25.01-16-14</param>
        /// <param name="marketDataQuoteArray">The quotes can be: DP(DirtyPrice), CP(CleanPrice), ASW (AssetSwapSpread),	DS (DiscountSpread), YTM (YieldToMaturity).</param>
        /// <param name="quotesRange">The actual quote range consistent with the quote list.</param> 
        /// <returns></returns>
        public string UpdateSecuritiesMarkets(Excel.Range propertiesAs2DRange,
                                              Excel.Range instrumentIdArray, Excel.Range marketDataQuoteArray,
                                              Excel.Range quotesRange)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> instrumentIdList = DataRangeHelper.StripRange(instrumentIdArray);
            List<string> marketDataQuoteList = DataRangeHelper.StripRange(marketDataQuoteArray);
            var quotesMatrix = DataRangeHelper.ToMatrix<Double>(quotesRange);
            var result = ValService.UpdateSecuritiesMarkets(namedValueSet, instrumentIdList, marketDataQuoteList, quotesMatrix);
            return result;
        }

        /// <summary>
        /// Publishes the QuotedAssetSet and refreshed the curve.
        /// THe market data must be in the correct format.
        /// </summary>
        /// <remarks>
        /// <see cref="AssetTypesEnum"></see>
        /// </remarks>
        /// <param name="curveIdentifier">The curve identifier</param>
        /// <param name="assetIdentifierArray">The identifiers. 
        /// These must be valid asset identifiers for the pricing structure type to be bootstrapped.
        /// </param>
        /// <param name="valuesArray">The market quotes</param>
        /// <param name="additionalArray">The additional data. This will be volatilities, in the case of IRFutures, or spreads for all other assets.</param>
        /// <returns></returns>
        public string RefreshPricingStructure(String curveIdentifier, Excel.Range assetIdentifierArray, Excel.Range valuesArray, Excel.Range additionalArray)
        {
            List<string> unqInstruments = DataRangeHelper.StripRange(assetIdentifierArray);
            List<decimal> unqRates = DataRangeHelper.StripDecimalRange(valuesArray);
            List<decimal> unqMeasureTypes = DataRangeHelper.StripDecimalRange(additionalArray);
            var result = ValService.RefreshPricingStructure(curveIdentifier, unqInstruments.ToArray(), unqRates.ToArray(), unqMeasureTypes.ToArray());
            return result;
        }

        /// <summary>
        /// Publishes the QuotedAssetSet.
        /// A quoted asset set is used for storing market data, to be consumed by a curve.
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
        /// <param name="propertiesAs2DRange">The properties: 
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
        /// <param name="assetIdentifierArray">The identifiers. 
        /// These must be valid asset identifiers for the pricing structure type to be bootstrapped.
        /// </param>
        /// <param name="valuesArray">The market quotes</param>
        /// <param name="additionalArray">The additional data. This will be volatilities, in the case of IRFutures, or spreads for all other assets.</param>
        /// <returns></returns>
        public string CreateQuotedAssetSet(Excel.Range propertiesAs2DRange, Excel.Range assetIdentifierArray, Excel.Range valuesArray, Excel.Range additionalArray)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> unqInstruments = DataRangeHelper.StripRange(assetIdentifierArray);
            List<decimal> unqRates = DataRangeHelper.StripDecimalRange(valuesArray);
            List<decimal> unqMeasureTypes = DataRangeHelper.StripDecimalRange(additionalArray);
            var result = ValService.CreateQuotedAssetSet(namedValueSet, unqInstruments.ToArray(), unqRates.ToArray(), unqMeasureTypes.ToArray());
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="currency"></param>
        /// <param name="paymentFrequency"></param>
        /// <param name="businessDayConvention"></param>
        /// <param name="businessCentersAsString"></param>
        /// <returns></returns>
        public string CreateLeaseConfiguration(string id, string currency, string paymentFrequency, string businessDayConvention, string businessCentersAsString)
        {
            var result = Engine.CreateLeaseConfiguration(id, currency, paymentFrequency, businessDayConvention,
                businessCentersAsString);
            return result;
        }

        ///  <summary>
        ///  Examples of values are:
        ///  <Property name = "Tolerance" > 1E-10</Property >
        ///  <Property name="Bootstrapper">SimpleRateBootstrapper</Property>
        ///  <Property name = "BootstrapperInterpolation" > LinearRateInterpolation</Property >
        ///  <Property name="CurveInterpolation">LinearInterpolation</Property>
        ///  <Property name = "UnderlyingCurve" > ZeroCurve</Property >
        ///  <Property name="CompoundingFrequency">Continuous</Property>
        ///  <Property name = "ExtrapolationPermitted" >true</Property >
        ///  <Property name="DayCounter">ACT/365.FIXED</Property>
        ///  </summary>
        ///  <param name="algorithmName">The name of the algorithm E.g Cubic</param>
        ///  <param name="pricingStructureType">RateCurve, RateSpreadCurve etc.</param>
        ///  <param name="tolerance">A decimal value.</param>
        ///  <param name="bootstrapper">E.g. FastBootstrapper</param>
        ///  <param name="bootstrapperInterpolation">LogLinearInterpolation</param>
        ///  <param name="curveInterpolation">LogLinearInterpolation</param>
        ///  <param name="underlyingCurve">DiscountFactorCurve or ZeroCurve</param>
        ///  <param name="compoundingFrequency">Continuous, Quarterly etc</param>
        ///  <param name="extrapolation">true</param>
        ///  <param name="dayCounter">Typically ACT/365.FIXED</param>
        ///  <returns></returns>
        public string CreateAlgorithm(string algorithmName, string pricingStructureType, string tolerance,
            string bootstrapper, string bootstrapperInterpolation, string curveInterpolation,
            string underlyingCurve, string compoundingFrequency, string extrapolation, string dayCounter)
        {
            var result = Engine.CreateAlgorithm(algorithmName, pricingStructureType, tolerance, bootstrapper, 
                bootstrapperInterpolation, curveInterpolation,  underlyingCurve, compoundingFrequency, extrapolation, dayCounter);
            return result;
        }

        /// <summary>
        /// Publishes the QuotedAssetSet.
        /// A quoted asset set is used for storing market data, to be consumed by a curve.
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
        /// <param name="propertiesAs2DRange">The properties: 
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
        /// <param name="assetIdentifierArray">The identifiers. 
        /// These must be valid asset identifiers for the pricing structure type to be bootstrapped.
        /// </param>
        /// <param name="valuesArray">The market quotes. These will be rates when creating a RateCurve.</param>
        /// <param name="assetMeasuresArray">The measure type of the value. This would normally be MarketQuote.
        /// However, for a IRFuture a LogNormalVolatility may be required.</param>
        /// <param name="priceQuoteUnitsArray">The price quote units. the most common example of this is DecimalRate.</param>
        /// <returns></returns>
        public string CreateQuotedAssetSetWithUnits(Excel.Range propertiesAs2DRange, Excel.Range assetIdentifierArray,
            Excel.Range valuesArray, Excel.Range assetMeasuresArray, Excel.Range priceQuoteUnitsArray)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> unqInstruments = DataRangeHelper.StripRange(assetIdentifierArray);
            List<decimal> unqRates = DataRangeHelper.StripDecimalRange(valuesArray);
            List<string> unqMeasureTypes = DataRangeHelper.StripRange(assetMeasuresArray);
            List<string> unqPriceQuoteUnits = DataRangeHelper.StripRange(priceQuoteUnitsArray);
            var result = ValService.CreateQuotedAssetSetWithUnits(namedValueSet, unqInstruments.ToArray(), unqRates.ToArray(), unqMeasureTypes.ToArray(), unqPriceQuoteUnits.ToArray());
            return result;
        }

        /// <summary>
        /// Publishes the pricing structure configuration.
        /// A set of properties is associated with the cached market and these properties 
        /// are also associated with the published pricing structure.
        /// </summary>
        /// <param name="assetIdentifierArray">The asset Identifiers as an array.</param>
        /// <param name="propertiesAs2DRange">The properties as NX2 range..</param>
        /// <param name="valuesArray">The values.</param>
        /// <param name="assetMeasuresArray">The measures for each asset as an array.</param>
        /// <param name="priceQuoteUnitsArray">The quotes for each measure as an array.</param>
        /// <param name="includeMarketQuoteValues">A flag to be able to set the values to null.</param>
        /// <returns>This identifier is the key to use when requesting the cached pricing structure.</returns>
        public string CreatePricingStructureProperties(Excel.Range propertiesAs2DRange, Excel.Range assetIdentifierArray,
            Excel.Range valuesArray, Excel.Range assetMeasuresArray, Excel.Range priceQuoteUnitsArray, bool includeMarketQuoteValues)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> unqInstruments = DataRangeHelper.StripRange(assetIdentifierArray);
            List<decimal> unqRates = DataRangeHelper.StripDecimalRange(valuesArray);
            List<string> unqMeasureTypes = DataRangeHelper.StripRange(assetMeasuresArray);
            List<string> unqPriceQuoteUnits = DataRangeHelper.StripRange(priceQuoteUnitsArray);
            var result = ValService.CreatePricingStructureProperties(namedValueSet, unqInstruments.ToArray(), unqRates.ToArray(), unqMeasureTypes.ToArray(), unqPriceQuoteUnits.ToArray(), includeMarketQuoteValues);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented pricing structures.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Pricing Structure Types</term>
        ///         <description>A list of the various pricing structure types that are available.</description>
        ///     </listheader>
        ///     <item>
        ///          <term>RateCurve</term>
        ///          <description>A rate curve pricing structure types built from valid priceable assets.</description>
        ///     </item>
        ///     <item>
        ///          <term>ClearedRateCurve</term>
        ///          <description>A cleared rate curve pricing structure types built from valid priceable assets.</description>
        ///     </item>
        ///     <item>
        ///          <term>RateSpreadCurve</term>
        ///          <description>A rate spread curve pricing structure types built from valid priceable assets.</description>
        ///     </item>
        ///     <item>
        ///          <term>DiscountCurve</term>
        ///          <description>A discount curve pricing structure types built from valid priceable assets.</description>
        ///     </item>
        ///     <item>
        ///          <term>InflationCurve</term>
        ///          <description>An inflation curve pricing structure types built from valid priceable assets.</description>
        ///     </item>
        ///     <item>
        ///          <term>FxCurve</term>
        ///          <description>A rate spread curve pricing structure types built from valid priceable assets.</description>
        ///     </item>
        ///     <item>
        ///          <term>FxVolatilityMatrix</term>
        ///          <description>An fx vol surface pricing structure types using a valid algorithm.</description>
        ///     </item>
        ///     <item>
        ///          <term>SurvivalProbabilityCurve</term>
        ///          <description>A survival curve pricing structure types built from valid priceable assets.</description>
        ///     </item>
        ///     <item>
        ///          <term>CommodityCurve</term>
        ///          <description>A commodity curve pricing structure types built from valid priceable assets.</description>
        ///     </item>
        ///         ///     <item>
        ///          <term>CommoditySpreadCurve</term>
        ///          <description>A commodity curve pricing structure types built from valid priceable assets.</description>
        ///     </item>
        ///     <item>
        ///          <term>CommodityVolatilityMatrix</term>
        ///          <description>A commodity vol surface pricing structure types using a valid algorithm.</description>
        ///     </item>
        ///     <item>
        ///          <term>RateVolatilityMatrix</term>
        ///          <description>A rate vol surface pricing structure types using a valid algorithm.</description>
        ///     </item>
        ///     <item>
        ///          <term>RateATMVolatilityMatrix</term>
        ///          <description>A rate atm vol surface pricing structure using a valid algorithm.</description>
        ///     </item>
        ///     <item>
        ///          <term>RateVolatilityCube</term>
        ///          <description>A rate vol cube pricing structure types using a valid algorithm.</description>
        ///     </item>
        ///     <item>
        ///          <term>LPMCapFloorCurve</term>
        ///          <description>An lpm vol surface pricing structure using a valid algorithm.</description>
        ///     </item>
        ///     <item>
        ///          <term>LPMSwaptionCurve</term>
        ///          <description>An lpm vol surface pricing structure types using a valid algorithm.</description>
        ///     </item>
        ///     <item>
        ///          <term>VolatilitySurface</term>
        ///          <description>A vol surface pricing structure types using a valid algorithm.</description>
        ///     </item> 
        ///     <item>
        ///          <term>VolatilityCube</term>
        ///          <description>A vol cube pricing structure types using a valid algorithm.</description>
        ///     </item>  
        ///     <item>
        ///          <term>VolatilitySurface2</term>
        ///          <description>A vol surface pricing structure types using a valid algorithm.</description>
        ///     </item>
        ///     <item>
        ///          <term>EquityVolatilityMatrix</term>
        ///          <description>A vol surface pricing structure types using standard models.</description>
        ///     </item> 
        ///     <item>
        ///          <term>EquityWingVolatilityMatrix</term>
        ///          <description>A vol surface pricing structure types using the wing vol model.</description>
        ///     </item> 
        /// </list> 
        /// </summary>
        /// <see cref="PricingStructureTypeEnum">PricingStructureTypeEnum</see>
        /// <remarks>
        /// Valid PricingStructureTypes are:
        /// </remarks>       
        /// <returns>A range object listing all the pricing structure types.</returns>
        public object[,] SupportedPricingStructureTypes()
        {
            var names = Enum.GetNames(typeof(PricingStructureTypeEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// Creates the specified curve type.
        /// </summary>
        /// <param name="propertiesAs2DRange">The properties as a 2D range.</param>
        /// <param name="instrumentsAsArray">A vertical range of instruments.</param>
        /// <param name="ratesAsArray">A vertical range of the adjusted rates.</param>
        /// <param name="discountFactorDatesAsArray">The discount factors dates range.</param>
        /// <param name="discountFactorsAsArray">The discount factors range.</param>
        /// <returns>The curve identifier as a handle.</returns>
        public string CreateFincadRateCurve(Excel.Range propertiesAs2DRange, Excel.Range instrumentsAsArray, Excel.Range ratesAsArray,
            Excel.Range discountFactorDatesAsArray, Excel.Range discountFactorsAsArray)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> unqInstruments = DataRangeHelper.StripRange(instrumentsAsArray);
            List<decimal> unqRates = DataRangeHelper.StripDecimalRange(ratesAsArray);
            List<DateTime> unqDiscountFactorDates = DataRangeHelper.StripDateTimeRange(discountFactorDatesAsArray);
            List<decimal> unqDiscountFactors = DataRangeHelper.StripDecimalRange(discountFactorsAsArray);          
            var curve = Engine.CreateFincadRateCurve(namedValueSet, unqInstruments.ToArray(), unqRates.ToArray(), unqDiscountFactorDates.ToArray(), unqDiscountFactors.ToArray());
            Engine.SaveCurve(curve);
            return curve.GetPricingStructureId().UniqueIdentifier;
        }

        /// <summary>
        /// Creates the rate curve with algorithm inputs.
        /// </summary>
        /// <param name="propertiesRange">The properties as a 2D range.</param>
        /// <param name="valuesRange">A vertical range of instruments and values.</param>
        /// <param name="algorithmPropertiesRange">The algorithm properties to build with.</param>
        /// <returns>The curve identifier as a handle.</returns>
        public string CreateTestCurve(Excel.Range propertiesRange, Excel.Range valuesRange, Excel.Range algorithmPropertiesRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            NamedValueSet structureProperties = properties.ToNamedValueSet();
            var algorithmProperties = algorithmPropertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            NamedValueSet algorithm = algorithmProperties.ToNamedValueSet();
            //Algorithm helper function.
            var temp = AlgorithmHelper.CreateAlgorithm(algorithm);
            structureProperties.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            var values = DataRangeHelper.ToMatrix(valuesRange);
            values = (object[,])DataRangeHelper.TrimNulls(values);
            IPricingStructure pricingStructure = Engine.CreatePricingStructure(structureProperties, values, temp);
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
            return structureId;
        }

        /// <summary>
        /// Creates the specified curve type.
        /// </summary>
        /// <param name="propertiesAs2DRange">The properties as a 2D range.</param>
        /// <param name="instrumentsAsArray">A vertical range of instruments.</param>
        /// <param name="ratesAsArray">A vertical range of the adjusted rates.</param>
        /// <param name="measureTypesAsArray">The measure types range.</param>
        /// <param name="priceQuoteUnitsAsArray">The price quote units range.</param>
        /// <returns>The curve identifier as a handle.</returns>
        public string CreateRateCurveWithUnits(Excel.Range propertiesAs2DRange, Excel.Range instrumentsAsArray, Excel.Range ratesAsArray,
            Excel.Range measureTypesAsArray, Excel.Range priceQuoteUnitsAsArray)
        {
            var properties = propertiesAs2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            List<string> unqInstruments = DataRangeHelper.StripRange(instrumentsAsArray);
            List<decimal> unqRates = DataRangeHelper.StripDecimalRange(ratesAsArray);
            List<string> unqMeasureTypes = DataRangeHelper.StripRange(measureTypesAsArray);
            List<string> unqPriceQuoteUnits = DataRangeHelper.StripRange(priceQuoteUnitsAsArray);
            var curve = Engine.CreateCurve(namedValueSet, unqInstruments.ToArray(), unqRates.ToArray(), unqMeasureTypes.ToArray(), unqPriceQuoteUnits.ToArray(), null, null);
            Engine.SaveCurve(curve);
            return curve.GetPricingStructureId().UniqueIdentifier;
        }

        /// <summary>
        /// Gets the assets and values from a configuration.
        /// </summary>
        /// <param name="pricingStructureId">Configuration for a pricing structure.</param>
        /// <returns></returns>
        public object GetPricingStructureConfiguration(string pricingStructureId)
        {
            var market = Engine.Cache.LoadObject<Market>(Engine.NameSpace + "." + pricingStructureId);
            if (market != null)
            {
                QuotedAssetSet instrumentSet = null;
                if (market.Items1[0] is YieldCurveValuation yieldCurveValuation)
                {
                    instrumentSet = yieldCurveValuation.inputs;
                }
                else
                {
                    if (market.Items1[0] is FxCurveValuation fxCurveValuation)
                    {
                        instrumentSet = fxCurveValuation.spotRate;
                    }
                }
                //TODO Other types of curve
                if (instrumentSet != null)
                {
                    var result = new object[instrumentSet.instrumentSet.Items.Length,2];
                    var index = 0;
                    foreach (var asset in instrumentSet.assetQuote)
                    {
                        result[index, 0] = instrumentSet.instrumentSet.Items[index].id;
                        var value =
                            MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote",
                                                                         new List<BasicQuotation>(asset.quote)).value;
                        //Can't handle Spread type measures yet.
                        result[index, 1] = value;
                        index++;
                    }
                    return result;
                }
            }
            return "No configuration found!";
        }

        /// <summary>
        /// Gets the assets and values for the AdvancedCurve.
        /// </summary>
        /// <param name="pricingStructureId">This must be an AdvancedRateCurve.</param>
        /// <returns></returns>
        public object GetQuotedAssetSet(string pricingStructureId)
        {
            var pricingStructure = (ICurve)Engine.GetCurve(pricingStructureId, false);
            var assetSet = pricingStructure.GetQuotedAssetSet().instrumentSet;
            var result = new object[assetSet.Items.Length, 2];
            var index = 0;
            foreach (var asset in pricingStructure.GetQuotedAssetSet().assetQuote)
            {
                result[index, 0] = assetSet.Items[index].id;
                var value = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", new List<BasicQuotation>(asset.quote)).value;
                //Can't handle Spread type measures yet.
                result[index, 1] = value;
                index++;
            }
            return result;
        }

        /// <summary>
        /// Gets the number of days and the zero rates for the AdvancedCurve.
        /// </summary>
        /// <param name="pricingStructureId">This must be an AdvancedRateCurve.</param>
        /// <param name="baseDate">The base date for rate calculation.</param>
        /// <param name="compounding">The compounding frequency for the zero rate. Can take: Continuous, Daily, Quarterly,
        /// Semi-Annual,SemiAnnual,Semi and Annual</param>
        /// <returns>A range of days and zero rates.</returns>
        public object[,] GetDaysAndRates(string pricingStructureId, DateTime baseDate, string compounding)
        {
            if (string.IsNullOrEmpty(compounding))
            {
                compounding = "Continuous";
            }
            var curve = (IRateCurve)Engine.GetCurve(pricingStructureId, false);
            IDictionary<int, Double> data = curve.GetDaysAndZeroRates(baseDate, compounding);
            object[,] results = ArrayHelper.ConvertDictionaryTo2DArray(data);
            return results;
        }

        /// <summary>
        /// Gets the number of days and the zero rates for the AdvancedCurve.
        /// </summary>
        /// <param name="pricingStructureId">This must be an AdvancedRateCurve.</param>
        /// <param name="baseDate">The base date for rate calculation.</param>
        /// <param name="compounding">The compounding frequency for the zero rate. Can take: Continuous, Daily, Quarterly,
        /// Semi-Annual,SemiAnnual,Semi and Annual</param>
        /// <returns>A range of days and zero rates.</returns>
        public object[,] GetDaysAndZeroRates(string pricingStructureId, DateTime baseDate, string compounding)
        {
            var pricingStructure = (IRateCurve)Engine.GetCurve(pricingStructureId, false);
            var curve = pricingStructure.GetTermCurve().point;
            var points = new object[curve.Length, 2];
            var index = 0;
            foreach (var point in curve)
            {
                var time = ((DateTime)point.term.Items[0] - baseDate).Days;//This is risky...assumes an ordered term curve.
                if (time == 0)
                {
                    var tempTime = ((DateTime)curve[index + 1].term.Items[0] - baseDate).Days;
                    var tempDF = (double)curve[index + 1].mid;
                    points[index, 0] = time;
                    points[index, 1] = RateAnalytics.DiscountFactorToZeroRate(tempDF, tempTime / 365d, compounding);
                }
                else
                {
                    points[index, 0] = time;
                    points[index, 1] = RateAnalytics.DiscountFactorToZeroRate((double)curve[index].mid, time / 365.0, compounding);
                }
                index++;
            }
            return points;
        }

        /// <summary>
        /// Gets the term curve.
        /// </summary>
        /// <param name="pricingStructureId">This must be an AdvancedRateCurve.</param>
        /// <returns></returns>
        public object[,] GetTermCurve(string pricingStructureId)
        {
            var pricingStructure = (ICurve)Engine.GetCurve(pricingStructureId, false);
            var curve = pricingStructure.GetTermCurve().point;
            var points = new object[curve.Length, 2];
            var index = 0;
            foreach (var point in curve)
            {
                var time = (DateTime)point.term.Items[0];
                points[index, 0] = time;
                points[index, 1] = point.mid;
                index++;
            }
            return points;
        }

        /// <summary>
        /// Gets the value assuming the curve base date.
        /// </summary>
        /// <param name="pricingStructureId">The pricing structure identifier.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetValue(string pricingStructureId, DateTime targetDate)
        {
            return Engine.GetValue(pricingStructureId, targetDate);
        }

        /// <summary>
        /// Gets the value assuming the curve base date.
        /// </summary>
        /// <param name="pricingStructureId">The pricing structure identifier.</param>
        /// <param name="targetDatesAsArray">The target dates. THe default base date for this is the system date.</param>
        /// <returns></returns>
        public object[,] GetValues(string pricingStructureId, Excel.Range targetDatesAsArray)
        {
            List<DateTime> unqTargetDates = DataRangeHelper.StripDateTimeRange(targetDatesAsArray);
            var values = Engine.GetValues(pricingStructureId, unqTargetDates.ToArray());
            var result = RangeHelper.ConvertArrayToRange(values);
            return result;
        }

        /// <summary>
        /// Gets the value from a base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetValueFromBase(string pricingStructureId, DateTime baseDate,
                                      DateTime targetDate)
        {
            return Engine.GetValue(pricingStructureId, baseDate,
                                       targetDate);
        }

        /// <summary>
        /// Gets the value from a base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDatesAsArray">The target dates.</param>
        /// <returns></returns>
        public object[,] GetValuesFromBase(string pricingStructureId, DateTime baseDate, Excel.Range targetDatesAsArray)
        {
            List<DateTime> unqtargetDates = DataRangeHelper.StripDateTimeRange(targetDatesAsArray);
            var values = Engine.GetValues(pricingStructureId, baseDate, unqtargetDates.ToArray());
            var result = RangeHelper.ConvertArrayToRange(values);
            return result;
        }

        /// <summary>
        /// Gets the value from a base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="valuationDate">The valuation date. This can be different to the base date of the pricing structure.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetHorizonValue(string pricingStructureId, DateTime valuationDate, DateTime targetDate)
        {
            return Engine.GetHorizonValue(pricingStructureId, valuationDate, targetDate);
        }

        /// <summary>
        /// Gets the volatility for the base date and strike provided.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="date">The base date.</param>
        /// <param name="strike">The strike.</param>
        /// <returns></returns>
        public double GetSurfaceValue(string curveId, DateTime date, Double strike)
        {
            return GetTenorStrikeValue(curveId, date, strike);
        }

        /// <summary>
        /// Gets the volatility for the base date and strike provided.
        /// </summary>
        /// <param name="volCurveId">The vol curve id.</param>
        /// <returns></returns>
        public object[,] GetSurface(string volCurveId)
        {
            var value = Engine.GetSurfaceValues(volCurveId);
            return value;
        }

        /// <summary>
        /// Gets the volatility for the base date, strike and tenor provided.
        /// </summary>
        /// <param name="curveId">The curveId.</param>
        /// <param name="expiryTerm">The expiry date, as a tenor.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="tenor">The tenor.</param>
        /// <returns></returns>
        public double GetCubeValue(string curveId, string expiryTerm, Double strike, string tenor)
        {
            var value = Engine.GetCubeValue(curveId, expiryTerm, strike, tenor);
            return value;
        }
        
        /// <summary>
        /// Gets the value assuming the vol surface base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="strike">The strike value required.</param>
        /// <returns></returns>
        public double GetTenorStrikeValue(string pricingStructureId, DateTime targetDate, Double strike)
        {
            var value = Engine.GetTenorStrikeValue(pricingStructureId, targetDate, strike);
            return value;
        }

        /// <summary>
        /// Gets the value assuming the vol surface base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="term">The term of the underlying required.</param>
        /// <returns>The interpolated value.</returns>

        public double GetExpiryDateTenorValue(string pricingStructureId, DateTime baseDate, DateTime targetDate, String term)
        {
            var value = Engine.GetExpiryDateTenorValue(pricingStructureId, baseDate, targetDate, term);
            return value;
        }

        /// <summary>
        /// Gets the value assuming the vol surface base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="expiryTerm">The expiry Term.</param>
        /// <param name="tenorTerm">The term of the underlying required.</param>
        /// <returns>The interpolated value.</returns>
        public double GetExpiryTermTenorValue(string pricingStructureId, String expiryTerm, String tenorTerm)
        {
            var value = Engine.GetExpiryTermTenorValue(pricingStructureId, expiryTerm, tenorTerm);
            return value;
        }

        /// <summary>
        /// Gets the interpolated value from a strike volatility surface.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="expirydate">The expiry date.</param>
        /// <param name="strike">The strike.</param>
        /// <returns>The interpolated value.</returns>
        public double GetExpiryDateStrikeValue(string pricingStructureId, DateTime baseDate, DateTime expirydate, Double strike)
        {
            var value = Engine.GetExpiryDateStrikeValue(pricingStructureId, baseDate, expirydate, strike);
            return value;
        }

        /// <summary>
        /// Gets the interpolated value from a strike volatility surface.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="expiryTerm">The expiry term.</param>
        /// <param name="strike">The strike.</param>
        /// <returns>The interpolated value.</returns>
        public double GetExpiryTermStrikeValue(string pricingStructureId, String expiryTerm, Double strike)
        {
            var value = Engine.GetExpiryTermStrikeValue(pricingStructureId, expiryTerm, strike);
            return value;
        }

        /// <summary>
        /// Gets the interpolated value from a strike volatility surface.
        /// </summary>
        /// <param name="pricingStructureId">The pricing structure identifier.</param>
        /// <param name="expiryTermsAsArray">The expiry terms.</param>
        /// <param name="strikesAsArray">The strikes.</param>
        /// <returns>The interpolated value.</returns>
        public object[,] GetExpiryTermStrikeValues(string pricingStructureId, Excel.Range expiryTermsAsArray, Excel.Range strikesAsArray)
        {
            List<string> unqExpiryTerms = DataRangeHelper.StripRange(expiryTermsAsArray);
            List<double> unqStrikes = DataRangeHelper.StripDoubleRange(strikesAsArray);
            var value = Engine.GetExpiryTermStrikeValues(pricingStructureId, unqExpiryTerms, unqStrikes);
            return value;
        }

        ///<summary>
        /// Creates the specified curve type.
        ///</summary>
        ///<param name="structurePropertiesRange">The properties range. This must include all mandatory properties.</param>
        ///<param name="valuesRange">The values to be used for bootstrapping.</param>
        ///<returns>A handle to a bootstrapped pricing structure.</returns>
        ///<exception cref="NotImplementedException"></exception>
        public string CreatePricingStructure(Excel.Range structurePropertiesRange, Excel.Range valuesRange)
        {
            var properties = structurePropertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            var values = valuesRange.Value[System.Reflection.Missing.Value] as object[,];
            NamedValueSet structureProperties = properties.ToNamedValueSet();
            structureProperties.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            values = (object[,])DataRangeHelper.TrimNulls(values);
            IPricingStructure pricingStructure = Engine.CreatePricingStructure(structureProperties, values);
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
            return structureId;
        }

        /// <summary>
        /// Creates a new Ratio Discount Curve by adjusting an existing Rate Curve.
        /// The returned curve's points are ratios of the discounts from the original curve
        /// and the new solved curve which was created using the supplied spreads.
        /// </summary>
        /// <param name="baseCurveId">The ID of the base (foreign) zero rate curve the new curve is based on.</param>
        /// <param name="quoteCurveId">The ID of the quote (local) zero rate curve, used for creating synthetic deposits.</param>
        /// <param name="fxCurveId">The ID of the FX curve, used for creating synthetic deposits.</param>
        /// <param name="propertiesRange">The properties of the new curve.</param>
        /// <param name="spreadRange">The adjustments required.</param>
        /// <returns>The ID of the newly created curve.</returns>
        public string CreateXccySpreadCurve(string baseCurveId, string quoteCurveId,
            string fxCurveId, Excel.Range propertiesRange, Excel.Range spreadRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            var values = spreadRange.Value[System.Reflection.Missing.Value] as object[,];
            // convert the properties to what we need
            List<Pair<string, double>> spreads = values.ToList<string, double>();
            string[] instruments = spreads.Select(a => a.First).ToArray();
            decimal[] rates = spreads.Select(a => (decimal)a.Second).ToArray();
            var baseCurve = (RateCurve)Engine.GetCurve(baseCurveId, false);
            var quoteCurve = (RateCurve)Engine.GetCurve(quoteCurveId, false);
            var fxCurve = (FxCurve)Engine.GetCurve(fxCurveId, false);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            // get the new curve
            var newCurve = Engine.CreateXccySpreadCurve(properties.ToNamedValueSet(), baseCurve,
                quoteCurve, fxCurve, instruments, rates, null, null);
            // Save the new curve to the cache
            string discountCurveId = newCurve.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(newCurve);
            // return the ID of the new curve
            return discountCurveId;
        }

        /// <summary>
        /// Creates an adjusted Rate Curve.
        /// </summary>
        /// <param name="propertiesRange">A properties range. All mandatory properties must be included.</param>
        /// <param name="spreadRange">THe range of instruments and spreads to apply.</param>
        /// <returns>The ID of the newly created curve.</returns>
        public string CreateAdjustedRateCurve(Excel.Range propertiesRange, Excel.Range spreadRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            var values = spreadRange.Value[System.Reflection.Missing.Value] as object[,];
            var props = properties.ToNamedValueSet();
            props.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            var referenceCurveId = props.GetValue<string>(CurveProp.ReferenceCurveUniqueId, null);
            var baseCurve = (RateCurve)Engine.GetCurve(referenceCurveId, false);
            List<Pair<string, double>> spreads = values.ToList<string, double>();
            var curve = Engine.CreateAdjustedRateCurve(baseCurve, props, spreads, null, null);
            Engine.SaveCurve(curve);
            return curve.GetPricingStructureId().UniqueIdentifier;
        }

        /// <summary>
        /// Creates a Curve using the generic range format.
        /// </summary>
        /// <remarks>
        /// When creating a rate curve, the function CreateCurve uses the rate inputs to create 
        /// a table of dates and discount factors which is then interpolated to get 
        /// the discount factor on a particular date. 
        /// It also stores the sensitivities of each discount factor to the rate inputs.
        /// The discount factor on the deal date is always set to one. Although yield curves 
        /// are often built with the discount factor set to one on the spot rather than the 
        /// deal date this can lead to confusing results when we consider multi-currency 
        /// transactions where the spot dates are different for each currency. If the “deal date” 
        /// is not a good business day in a particular currency, then the calculations are 
        /// done from the next good business day. Orion makes a distinction between the 
        /// deal date as input, and the deal date after possible adjustment for a good business day, 
        /// the latter being referred to as the “anchor date”. The discount factor is always 
        /// one on the anchor date (hence the name), but for convenience, the discount factor 
        /// is set to one back to deal date as well.
        /// <para>
        /// The curve is can be built from cash instruments, interest-rate futures, FRAs and swaps. 
        /// The first three are zero-coupon instruments whose level implies a ratio of 
        /// discount factors for the start and end dates of the instrument given by
        /// </para>
        /// <para>
        /// df/ds = 1 / (1 + R*a)
        /// </para>
        /// <para>
        /// where R is the cash rate, FRA level or futures price given as a yield, and
        /// </para>
        /// <para>
        /// a = 1/100 DCF(Ds, De, DC)  
        /// </para> 
        /// <para> 
        /// i.e. the day count fraction between the start date of the instrument Ds and 
        /// the end date Df using the given day count basis DC.
        /// </para> 
        /// <para>
        /// NB: Price and bill interest-rate futures have the feature that the margin 
        /// payments/receipts are not exactly the differences in the PV of the positions, 
        /// being calculated instead according to some formula that takes no account 
        /// of discounting. This results in the yield of the future contract (100 minus the price) 
        /// not being exactly the same as the equivalent FRA. No account of this is taken by 
        /// the yield curve builder, as apart from anything else, dealing with this correctly 
        /// would require a full yield curve model. It is anticipated that the problem would 
        /// be dealt with by a future price adjustment before the yield curve builder is called.
        /// </para> 
        /// <para>
        /// Interest-rate swap inputs are treated using a standard “bootstrapping” algorithm. 
        /// It is assumed that the swaps all have the same start date, coupon frequency, 
        /// business day convention, roll day and day count, so their coupon dates will 
        /// coincide where applicable. Swaps which mature on or before the end of the cash/futures 
        /// domain are ignored. After that the cash/futures part of the curve will be used to 
        /// value early swap coupons and the remainder used to generate new discount factors. 
        /// The floating payments of a swap can be replicated by a floating rate investment 
        /// of the notional amount, so the floating payment stream is then equivalent to the 
        /// notional amount being deposited at the start and repaid at the end. Equating the 
        /// present value of the floating and fixed legs of one unit of the swap, we then get 
        /// </para> 
        /// <para>
        /// d0 - dn = Sum (1, n, ai-1 * Sn * di)
        /// </para>  
        /// <para>
        /// where  Sn is the swap level of the swap of maturity n coupon periods, and aij are the 
        /// accrual fractions defined by 
        /// </para>
        /// <para>
        /// aij = 1/100 * DCF(Di, Dj, SwapsDC)
        /// </para> 
        /// <para>     	
        /// where Di are the swap coupon dates.
        /// </para>
        /// <para>
        /// The bootstrapping algorithm requires swap levels for all maturities spaced by the 
        /// coupon period until the last swap, i.e. there can be no gaps. The means for 
        /// filling the gaps that occur in real life is to linearly interpolate between 
        /// the nearest swaps on the basis of coupon number. In the special case of finding 
        /// missing swap levels between the cash/futures regime and the known swaps regime, 
        /// we need to imply a swap rate from the cash/futures and then treat this as a 
        /// real swap input for the purpose of interpolation. If a single period swap is 
        /// not input and cannot be implied from the cash and futures then the yield curve building will fail.
        /// When the yield curve building succeeds, a table of dates and discount factors (grid points) 
        /// is created. A zero-coupon instrument (i.e. a depo, future or a FRA) will add one 
        /// or two discount factor points, or it will be ignored. Any instrument maturing in 
        /// the domain of a future will be ignored. Another rule is that multiple instruments 
        /// may not mature on the same date: preference being given in the order future, FRA, 
        /// swap and depo, and then, if the instruments are of the same type, in the order 
        /// later start date, earlier start date. The instruments are then listed in order of end date, 
        /// and the bootstrapping algorithm applied. If the start date of each instrument in 
        /// turn lies outside the yield curve constructed so far, then the discount factor at 
        /// the end date is set to a level that causes the implied (interpolated) discount factor 
        /// at the start to reproduce the rate level correctly. Thus each zero-coupon instrument 
        /// that is used always adds exactly one grid point, at the end date. If the instrument 
        /// is the first to be added, then the earlier discount factor is obtained by maintaining 
        /// the continuously compounded rate at a constant level up till the maturity of the instrument
        /// </para>
        /// If a date falls between grid dates then linear interpolation of the continuously 
        /// compounded rate is performed, i.e. the rate ri defined by  
        /// <para>
        /// di = exp(ri * ti)   
        ///  </para> 
        /// gets linearly interpolated. 
        /// <para>
        /// Thus for a date D such that Di less than or equals D less than or equals Di+1, 
        /// the discount factor d is given by  
        ///  </para> 
        /// ln d / (D - D0) = lambda * ln di / (D - D0) + (1-lambda) * ln di+1 / (D - D0)
        ///  <para>     
        /// where D0 is the deal date and lambda is the interpolation factor defined by
        /// </para> 
        /// lambda = (Di+1 - D) / (Di+1 - Di)
        /// <para>
        /// An alternative interpolation scheme is the linear log DF. 
        /// In this case, intermediate discount factors are given by
        /// </para>
        /// ln d = lambda * di + (1+lambda) * ln di+1
        /// </remarks>
        /// <param name="propertiesRange">This range of properties must include:
        /// BaseDate
        /// PricingStructureType
        /// MarketName</param>
        /// <param name="valuesRange">The values range should be made up of 4 columns:
        /// Column 1 - the instrumentId.
        /// Column 2 - the instrument value.
        /// Column 3 - the measure type.
        /// Column 4 - the price quote units.
        /// </param>
        /// <returns>The ID of the newly created curve.</returns>
        public string CreateCurve(Excel.Range propertiesRange, Excel.Range valuesRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            //var values = valuesRange.get_Value(System.Reflection.Missing.Value) as object[,];
            NamedValueSet structureProperties = properties.ToNamedValueSet();
            structureProperties.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            var values = DataRangeHelper.ToMatrix(valuesRange);
            values = (object[,])DataRangeHelper.TrimNulls(values);
            //Check which type of range is it.
            int lbx = values.GetLowerBound(1);
            int ubx = values.GetUpperBound(1);
            IPricingStructure pricingStructure = (ubx - lbx) < 3 ? Engine.CreatePricingStructure(structureProperties, values) : Engine.CreateGenericPricingStructure(structureProperties, values);          
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
            return structureId;
        }

        /// <summary>
        /// Creates a collection of curves, where the instrument array is the same for each curve.
        /// </summary>
        /// <param name="propertiesRange">The curve properties. The base date and build date time will be updated for each curve.</param>
        /// <param name="headersRange">A horizontal N array or instruments to use in curve construction.</param>
        /// <param name="valuesRange">An N X M matrix od instrument values: M curves and N instruments</param>
        /// <returns></returns>
        public string CreateCurves(Excel.Range propertiesRange, Excel.Range headersRange, Excel.Range valuesRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            NamedValueSet structureProperties = properties.ToNamedValueSet();
            structureProperties.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            var values = DataRangeHelper.ToMatrix(valuesRange);
            values = (object[,])DataRangeHelper.TrimNulls(values);
            var headers = DataRangeHelper.StripRange(headersRange);
            var pricingStructures = Engine.CreatePricingStructures(structureProperties, headers, values);
            foreach (var pricingStructure in pricingStructures)
            {
                Engine.SaveCurve(pricingStructure);
            }
            return "Curves created.";
        }

        ///<summary>
        /// Creates the specified curve type.
        ///</summary>
        ///<param name="baseRateCurve">The base curve.</param>
        ///<param name="perturbationBasisPoints">The basis points the perturb. </param>
        ///<returns>A range od dentifiers. Each id is a handle to a curve in memory.</returns>
        ///<exception cref="NotImplementedException"></exception>
        public object[,] CreateBasicRateCurveRiskSet(string baseRateCurve, decimal perturbationBasisPoints)
        {
            var baseCurve = (RateCurve)Engine.GetCurve(baseRateCurve, false);
            var riskCurves = Engine.CreateRateCurveRiskSet(baseCurve, perturbationBasisPoints);
            var ids = new List<string>();
            foreach (var curve in riskCurves)
            {
                Engine.SaveCurve(curve);
                ids.Add(curve.GetPricingStructureId().UniqueIdentifier);
            }
            var values = RangeHelper.ConvertArrayToRange(ids);
            return values;
        }

        ///<summary>
        /// Creates the specified rate spread curve type.
        ///</summary>
        ///<param name="propertiesRange">THe properties. This must include a ReferenceCurveId.</param>
        ///<param name="instrumentsAsArray">The instruments to transform the refernce curve into.</param>
        ///<param name="spreadsAsArray">THe spreads to apply.</param>
        ///<returns>An identifier that can be uses as a handle.</returns>
        ///<exception cref="NotImplementedException"></exception>
        [Obsolete("Use CreateCurve()")]
        public string CreateRateSpreadCurve(Excel.Range propertiesRange, Excel.Range instrumentsAsArray, Excel.Range spreadsAsArray)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            List<string> unqinstruments = DataRangeHelper.StripRange(instrumentsAsArray);
            List<decimal> unqspreads = DataRangeHelper.StripDecimalRange(spreadsAsArray);
            var pricingStructure = Engine.CreateCurve(namedValueSet, unqinstruments.ToArray(), unqspreads.ToArray(), null, null, null);
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
            return structureId;
        }

        ///<summary>
        /// Creates the specified rate spread curve type.
        ///</summary>
        ///<param name="propertiesRange">THe properties. This must include a ReferenceCurveId.</param>
        ///<param name="instrumentsAsArray">The instruments to transform the reference curve into.</param>
        ///<param name="spreadsAsArray">THe spreads to apply.</param>
        ///<returns>An identifier that can be uses as a handle.</returns>
        ///<exception cref="NotImplementedException"></exception>
        [Obsolete("Use CreateCurve()")]
        public string CreateRateBasisCurve(Excel.Range propertiesRange, Excel.Range instrumentsAsArray, Excel.Range spreadsAsArray)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            List<string> unqinstruments = DataRangeHelper.StripRange(instrumentsAsArray);
            List<decimal> unqspreads = DataRangeHelper.StripDecimalRange(spreadsAsArray);
            var pricingStructure = Engine.CreateCurve(namedValueSet, unqinstruments.ToArray(), unqspreads.ToArray(), null, null, null);
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
            return structureId;
        }

        ///<summary>
        /// Creates the specified rate spread curve type.
        ///</summary>
        ///<param name="propertiesRange">Tfe properties. This must include a ReferenceCurveId and the ReferenceFxCurveId.</param>
        ///<param name="instrumentsAsArray">The xccy basis instruments to use: e.g. AUD-XccyBasisSwap-1Y.</param>
        ///<param name="spreadsAsArray">THe spreads to apply.</param>
        ///<returns>An identifier that can be uses as a handle.</returns>
        ///<exception cref="NotImplementedException"></exception>
        [Obsolete("Use CreateCurve()")]
        public string CreateRateXccyCurve(Excel.Range propertiesRange, Excel.Range instrumentsAsArray, Excel.Range spreadsAsArray)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            List<string> unqinstruments = DataRangeHelper.StripRange(instrumentsAsArray);
            List<decimal> unqspreads = DataRangeHelper.StripDecimalRange(spreadsAsArray);
            var pricingStructure = Engine.CreateCurve(namedValueSet, unqinstruments.ToArray(), unqspreads.ToArray(), null, null, null);
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
            return structureId;
        }

        ///<summary>
        /// Creates the specified rate spread curve type.
        ///</summary>
        ///<param name="propertiesRange">The properties. This must include a ReferenceCurveId.
        /// This must include:
        /// 1) The fxCurve.
        /// 2) The reference rate curve.
        /// 3) The is base curve flag for the rate curve provided.</param>
        ///<returns>An identifier that can be uses as a handle.</returns>
        public string CreateRateCurveFromFxCurve(Excel.Range propertiesRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            var referenceFxCurveId = namedValueSet.GetValue<string>(CurveProp.ReferenceFxCurveUniqueId, null);
            var fxCurve = (FxCurve)Engine.GetCurve(referenceFxCurveId, false);
            var referenceRateCurveId = namedValueSet.GetValue<string>(CurveProp.ReferenceCurveUniqueId, null);
            var baseCurve = (RateCurve)Engine.GetCurve(referenceRateCurveId, false);
            var isRefCurveCurrency1 = namedValueSet.GetValue<bool>("Currency1RateCurve", false);
            //Old version
            //var fxCurveId = propertiesRange.GetValue<string>("BaseFxCurve");
            //var rateCurveId = propertiesRange.GetValue<string>("ReferenceRateCurve");
            //var isCurrency1RateCurve = propertiesRange.GetValue<bool>("Currency1RateCurve");
            var pricingStructure = fxCurve.GenerateRateCurve(Engine.Logger, Engine.Cache, NameSpace, baseCurve, isRefCurveCurrency1, namedValueSet, null, null);
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
            return structureId;
        }

        ///<summary>
        /// Creates the specified curve type.
        ///</summary>
        ///<param name="propertiesRange">The properties. This must include a ReferenceCurveId.
        /// This must include:
        /// 1) The fxCurve.
        /// 2) The reference rate curve.
        /// 3) The is base curve flag for the rate curve provided.</param>
        ///<param name="instrumentsAsArray">The rnage of instruements.</param>
        ///<param name="valuesAsArray">The range of instrument values.</param>
        ///<param name="additionalAsArray">The range of additional data. This can be a spread or a volatilitiy, if a future.</param>
        ///<returns>An identifier that can be uses as a handle.</returns>
        ///<exception cref="NotImplementedException"></exception>
        [Obsolete("Use CreateCurve()")]
        public string CreateCurve_Old(Excel.Range propertiesRange, Excel.Range instrumentsAsArray, Excel.Range valuesAsArray, Excel.Range additionalAsArray)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            List<string> unqInstruments = DataRangeHelper.StripRange(instrumentsAsArray);
            List<decimal> unqValues = DataRangeHelper.StripDecimalRange(valuesAsArray);
            List<decimal> unqAdditional = DataRangeHelper.StripDecimalRange(additionalAsArray);
            var pricingStructure = Engine.CreateCurve(namedValueSet, unqInstruments.ToArray(), unqValues.ToArray(), unqAdditional.ToArray(), null, null);
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
            return structureId;
        }


        /// <summary>
        /// Creates the volatility cube.
        /// </summary>
        /// <param name="propertiesRange">The properties.</param>
        /// <param name="strikesArray">The strike array.</param>
        /// <param name="dataRange">The data range.</param>
        /// <returns>An identifier that can be uses as a handle.</returns>
        public string CreateVolatilityCube(Excel.Range propertiesRange, Excel.Range strikesArray, Excel.Range dataRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var data = RangeFunctions.AsMatrix<object>(dataRange);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            var strikes = DataRangeHelper.StripDecimalRange(strikesArray);
            string[] expiries = DataRangeHelper.ExtractExpiries(data).Distinct().ToArray();
            string[] tenors = DataRangeHelper.ExtractTenors(data).Distinct().ToArray();
            decimal[,] vols = DataRangeHelper.ConvertToDecimalArray(data, 2);
            var pricingStructure = Engine.CreateVolatilityCube(namedValueSet, expiries, tenors, vols, strikes.ToArray());
            string structureId = pricingStructure.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(pricingStructure);
            return structureId;
        }

        ///<summary>
        /// Creates the specified curve type.
        ///</summary>
        ///<param name="propertiesRange">A property collection.</param>
        ///<param name="tenorOrStrikesArray">An array of tenors or strikes.</param>
        ///<param name="dataRange">The appropriate data range.</param>
        ///<returns>A list of identifiers that can be uses as handles.</returns>
        ///<exception cref="NotImplementedException"></exception>
        public object[,] CreateVolatilitySurfaceCollection(Excel.Range propertiesRange, Excel.Range tenorOrStrikesArray, Excel.Range dataRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            var data = RangeFunctions.AsMatrix<object>(dataRange);
            var tenorOrStrikes = DataRangeHelper.StripRange(tenorOrStrikesArray);
            var result = CreateVolatilitySurfaceCollection(namedValueSet, data, tenorOrStrikes.ToArray());
            return RangeHelper.ConvertArrayToRange(result);
        }


        ///<summary>
        /// Strips a surface: expiry by strike, from a flattened cube, excluding the strike headers.
        ///</summary>
        ///<param name="propertyRange">The property range.</param>
        ///<param name="dataRange">The input data range.</param>
        ///<param name="strikeArray">The strike array.</param>
        ///<returns></returns>
        private String[] CreateVolatilitySurfaceCollection(NamedValueSet propertyRange, object[,] dataRange, String[] strikeArray)//TODO This only works with a single tenor. Extend to include more...
        {
            var strikes = ConvertStringArrayToDoubleArray(strikeArray);
            var expiries = DataRangeHelper.ExtractExpiries(dataRange);
            var uniqueExpiries = expiries.Distinct().ToArray();
            var tenors = DataRangeHelper.ExtractTenors(dataRange);
            var uniqueTenors = tenors.Distinct().ToArray();
            var ids = new List<string>();
            var instrument = propertyRange.GetString("Instrument", true);           
            foreach (var tenor in uniqueTenors)
            {
                var tempPropertyRange = propertyRange.Clone();
                var data = DataRangeHelper.GetTenorSurfaceFromCube2(dataRange, tenor, uniqueTenors.Length, strikes);
                var temp = instrument + "-" + tenor;
                tempPropertyRange.Set("Instrument", temp);
                tempPropertyRange.Set("CurveName", temp);
                var pricingStructure = Engine.CreateVolatilitySurface(tempPropertyRange, uniqueExpiries, strikeArray, data);
                Engine.SaveCurve(pricingStructure);
                ids.Add(pricingStructure.GetPricingStructureId().UniqueIdentifier);
            }
            return ids.ToArray();
        }

        private static double[] ConvertStringArrayToDoubleArray(string[] strikes)
        {
            var result = new double[strikes.Length];
            for (var i = 0; i < strikes.Length; i++)
            {
                result[i] = Convert.ToDouble(strikes[i]);
            }
            return result;
        }

        ///<summary>
        /// Creates the specified curve type.
        ///</summary>
        ///<param name="propertiesRange">A property collection.</param>
        ///<param name="expiriesAsArray">A range of expiries.</param>
        ///<param name="tenorOrStrikesAsArray">An array of tenors or strikes.</param>
        ///<param name="volatilitiesAsRange">A range of volatilities.</param>
        ///<param name="forwardsAsArray">A range of forwards.</param>
        ///<returns>An identifier handle for the curve.</returns>
        ///<exception cref="NotImplementedException"></exception>
        public string CreateEquityWingVolatilitySurface(Excel.Range propertiesRange, Excel.Range expiriesAsArray, Excel.Range tenorOrStrikesAsArray,
            Excel.Range volatilitiesAsRange, Excel.Range forwardsAsArray)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var volatilities = volatilitiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
            var volatilitiesAsDoubles = RangeHelper.RangeToDoubleMatrix(volatilities);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            var expiries = DataRangeHelper.StripRange(expiriesAsArray);
            var tenorOrStrikes = DataRangeHelper.StripRange(tenorOrStrikesAsArray);
            var forwards = DataRangeHelper.StripDoubleRange(forwardsAsArray);
            var pricingStructure = Engine.CreateVolatilitySurface(namedValueSet, expiries.ToArray(), tenorOrStrikes.ToArray(), volatilitiesAsDoubles, forwards.ToArray());
            Engine.SaveCurve(pricingStructure);
            return pricingStructure.GetPricingStructureId().UniqueIdentifier;
        }

        ///<summary>
        /// Creates the specified curve type.
        ///</summary>
        ///<param name="propertiesRange">A property collection.</param>
        ///<param name="expiriesAsArray">A range of expiries.</param>
        ///<param name="tenorOrStrikesAsArray">An array of tenors or strikes.</param>
        ///<param name="volatilitiesAsRange">A range of volatilities.</param>
        ///<returns>An identifier handle for the curve.</returns>
        ///<exception cref="NotImplementedException"></exception>
        public string CreateVolatilitySurfaceWithProperties(Excel.Range propertiesRange, Excel.Range expiriesAsArray, 
            Excel.Range tenorOrStrikesAsArray, Excel.Range volatilitiesAsRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var volatilities = volatilitiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
            var volatilitiesAsDoubles = RangeHelper.RangeToDoubleMatrix(volatilities);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(EnvironmentProp.SourceSystem, CurveCalculationProp.Spreadsheet.ToString());
            var expiries = DataRangeHelper.StripRange(expiriesAsArray);
            var tenorOrStrikes = DataRangeHelper.StripRange(tenorOrStrikesAsArray);
            var pricingStructure = Engine.CreateVolatilitySurface(namedValueSet, expiries.ToArray(), tenorOrStrikes.ToArray(), volatilitiesAsDoubles);
            Engine.SaveCurve(pricingStructure);
            return pricingStructure.GetPricingStructureId().UniqueIdentifier;
        }

        /// <summary>
        /// Saves the pricing structure to the database.
        /// </summary>
        /// <param name="pricingStructureId">The pricing structure identifier.</param>
        /// <param name="expiryDate">The length of time to persist the structure.</param>
        /// <returns>A message confirming the action.</returns>
        public string SavePricingStructure(String pricingStructureId, DateTime expiryDate)
        {
            var curve = Engine.GetCurve(pricingStructureId, false);
            Engine.SaveCurve(curve);
            return curve.GetPricingStructureId().UniqueIdentifier;
        }

        /// <summary>
        /// Saves the pricing structures to the database.
        /// </summary>
        /// <param name="pricingStructureIdsAsArray">The range of  pricing structures.</param>
        /// <param name="expiryDate">The length of time to persist the structure.</param>
        /// <returns>A list identifiers that can be uses as handles.</returns>
        public object[,] SavePricingStructures(Excel.Range pricingStructureIdsAsArray, DateTime expiryDate)
        {
            var pricingStructureIds = DataRangeHelper.StripRange(pricingStructureIdsAsArray);
            var result = new List<string>();
            foreach (var curveId in pricingStructureIds)
            {
                var curve = Engine.GetCurve(curveId, false);
                Engine.SaveCurve(curve);
                result.Add(curve.GetPricingStructureId().UniqueIdentifier);
            }
            return RangeHelper.ConvertArrayToRange(result);
        }

        ///// <summary>
        ///// Saves the pricing structure to the database.
        ///// </summary>
        ///// <param name="name">The name of the pricing structure.</param>
        ///// <returns>A list identifiers that can be uses as handles.</returns>
        //public string LoadCurve(String name)
        //{
        //    return Engine.LoadInterestRateCurve(name);
        //}

        /// <summary>
        /// Caches the pricing structure to the database.
        /// </summary>
        /// <param name="requestProperties">The request Properties.</param>
        /// <param name="numberToReturn">The number To Return.</param>
        /// <returns>A list identifiers that can be uses as handles.</returns>
        public object[,] GetPricingStructures(Excel.Range requestProperties, int numberToReturn)
        {
            var properties = requestProperties.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            if (requestProperties == null) throw new ArgumentNullException(nameof(requestProperties));
            var namedValueSet = properties.ToNamedValueSet();
            var loadedObjects = Engine.GetPricingStructures(namedValueSet, numberToReturn);
            //var index = 0;
            return RangeHelper.ConvertArrayToRange(loadedObjects.Select(element => element.GetPricingStructureId().UniqueIdentifier).ToArray());
        }

        /// <summary>
        /// Displays data relevant to the provided pricing structure.
        /// </summary>
        /// <param name="uniqueName">The uniqueId.</param>
        /// <returns>The range of information for that curve.</returns>
        public object[,] DisplayPricingStructureProperties(String uniqueName)
        {
            var curve = Engine.GetCurve(uniqueName, false);
            var props = curve.GetPricingStructureId().Properties.ToDictionary();
            object[,] results = ArrayHelper.ConvertDictionaryTo2DArray(props);
            return results;
        }

        /// <summary>
        /// Lists all pricing structures that satisfy the set of properties provided.
        /// </summary>
        /// <param name="propertiesRange">The Properties.</param>
        /// <returns>A list identifiers that can be uses as handles.</returns>
        public object[,] ListUpTo100PricingStructures(Excel.Range propertiesRange)
        {
            var result = GetPricingStructures(propertiesRange, 100);
            return result;
        }

        /// <summary>
        /// Displays data for that particular curve.
        /// </summary>
        /// <param name="pricingStructureId">The pricing structure identifier of the curve to expose.</param>
        /// <returns>The range of information for that curve.</returns>
        public object[,] DisplayCurve(string pricingStructureId)
        {
            IPricingStructure pricingStructure = Engine.GetCurve(pricingStructureId, false);
            Pair<PricingStructure, PricingStructureValuation> fpMLPair = pricingStructure.GetFpMLData();
            return PricingStructureHelper.FpMLPairTo2DArray(fpMLPair);
        }

        #endregion

        #region LPM and SABR PPD Functions

        /// <summary>
        /// Publishes the LPM cap floor vol matrix.
        /// </summary>
        /// <param name="structurePropertiesRange">The structure properties range.</param>
        /// <param name="publishPropertiesRange">The publish properties range.</param>
        /// <param name="valuesRange">The values range.</param>
        /// <param name="rateCurveFiltersRange">The rate curve filters range.</param>
        /// <returns></returns>
        public string PublishLpmCapFloorVolMatrix(Excel.Range structurePropertiesRange, Excel.Range publishPropertiesRange, Excel.Range valuesRange, Excel.Range rateCurveFiltersRange)
        {
            var properties = structurePropertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            var publishProperties = publishPropertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            var values = valuesRange.Value[System.Reflection.Missing.Value] as object[,];
            var rateCurveFilters = rateCurveFiltersRange.Value[System.Reflection.Missing.Value] as object[,];
            var result = ValService.PublishLpmSwaptionVolMatrix(properties, publishProperties, values, rateCurveFilters);
            return result;
        }

        /// <summary>
        /// Publishes the LPM swaption vol matrix.
        /// </summary>
        /// <param name="structurePropertiesRange">The structure properties range.</param>
        /// <param name="publishPropertiesRange">The publish properties range.</param>
        /// <param name="valuesRange">The values range.</param>
        /// <param name="rateCurveFiltersRange">The rate curve filters range.</param>
        /// <returns></returns>
        public string PublishLpmSwaptionVolMatrix(Excel.Range structurePropertiesRange, Excel.Range publishPropertiesRange, Excel.Range valuesRange, Excel.Range rateCurveFiltersRange)
        {
            var properties = structurePropertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            var publishProperties = publishPropertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            var values = valuesRange.Value[System.Reflection.Missing.Value] as object[,];
            var rateCurveFilters = rateCurveFiltersRange.Value[System.Reflection.Missing.Value] as object[,];
            var result = ValService.PublishLpmSwaptionVolMatrix(properties, publishProperties, values, rateCurveFilters);
            return result;
        }

        #endregion

        #region Fincad Functions


        ///<summary>
        /// This function emulates the fincad swap pricing function. In addition though Orion curves are refenced.
        /// These curves must be cached.
        ///</summary>
        ///<param name="valueDate">The value date of the swap.</param>
        ///<param name="effectiveDate">The effective date of the swap.</param>
        ///<param name="terminationDate">The termination date of the swap.</param>
        ///<param name="interpolationMethod">The interpolation to use.</param>
        ///<param name="margineAboveFloatingRate">The margin on the floating leg.</param>
        ///<param name="resetRate">The rest rate for the last reset.</param>
        ///<param name="directionDateGenerationPayLeg">The date generation logic: Forward or Backward.</param>
        ///<param name="cashFlowFrequencyPayLeg">The frequency of pay leg payment.</param>
        ///<param name="accrualMethodPayLeg">The accrual method for the pay leg.</param>
        ///<param name="holidaysPayLeg">The holiday calendars to use on the pay leg.</param>
        ///<param name="discountFactorCurvePayLeg">The discount factor collection as a Orion cure reference for the pay leg.</param>
        ///<param name="directionDateGenerationRecLeg">The date generation logic: Forward or Backward.</param>
        ///<param name="cashFlowFrequencyRecLeg">The frequency of receive leg payment.</param>
        ///<param name="accrualMethodRecLeg">The accrual method for the receive leg.</param>
        ///<param name="holidaysRecLeg">The holiday calendars to use on the receive leg.</param>
        ///<param name="discountFactorCurveRecLeg">The discount factor collection as a Orion cure reference for the receive leg.</param>
        ///<returns>The par swap rate is returned.</returns>
        public double GetFincadSwapParRate
            (
            DateTime valueDate,
            DateTime effectiveDate,
            DateTime terminationDate,
            string interpolationMethod, //1 is linear on forward rates => make sure that the right curve is provided ...
            double margineAboveFloatingRate,// use 0 initially
            double resetRate,
            int directionDateGenerationPayLeg,
            string cashFlowFrequencyPayLeg,
            string accrualMethodPayLeg,
            string holidaysPayLeg,
            string discountFactorCurvePayLeg,
            int directionDateGenerationRecLeg,
            string cashFlowFrequencyRecLeg,
            string accrualMethodRecLeg,
            string holidaysRecLeg,
            string discountFactorCurveRecLeg
            )
        {
            var result = new Functions();
            var curve1 = (IRateCurve)Engine.GetCurve(discountFactorCurvePayLeg, false);
            var curve2 = (IRateCurve)Engine.GetCurve(discountFactorCurveRecLeg, false);
            return result.GetSwapParRateWithoutCurves(Engine.Logger, Engine.Cache, NameSpace,
                valueDate, effectiveDate, terminationDate,
                interpolationMethod,
                margineAboveFloatingRate, resetRate,
                directionDateGenerationPayLeg,
                cashFlowFrequencyPayLeg,
                accrualMethodPayLeg,
                holidaysPayLeg,
                curve1,
                directionDateGenerationRecLeg,
                cashFlowFrequencyRecLeg,
                accrualMethodRecLeg,
                holidaysRecLeg,
                curve2);
        }

        /// <summary>
        ///  This function emulates the fincad swap pricing function.
        /// </summary>
        /// <param name="swapInputRangeAsObject">
        /// ValueDate: The value date of the swap.
        /// EffectiveDate: The effective date of the swap.
        /// TerminationDate: The termination date of the swap.
        /// InterpolationMethod>The interpolation to use.
        /// MargineAboveFloatingRate>The margin on the floating leg.
        /// ResetRate: The rest rate for the last reset.
        /// DirectionDateGenerationPayLeg: The date generation logic: Forward or Backward.
        /// CashFlowFrequencyPayLeg: The frequency of pay leg payment.
        /// AccrualMethodPayLeg: The accrual method for the pay leg.
        /// HolidaysPayLeg: The holiday calendars to use on the pay leg.
        /// DirectionDateGenerationRecLeg: The date generation logic: Forward or Backward.
        /// CashFlowFrequencyRecLeg: The frequency of receive leg payment.
        /// AccrualMethodRecLeg: The accrual method for the receive leg.
        /// HolidaysRecLeg: The holiday calendars to use on the receive leg.</param>
        /// <param name="discountFactorCurvePayLegAsObject">The discount factor collection for the pay leg.</param>
        /// <param name="discountFactorCurveRecLegAsObject">The discount factor collection for the receive leg.</param>
        /// <returns>The par swap rate is returned.</returns>
        /// <returns></returns>
        public double FincadSwapCpn2
        (
            Excel.Range swapInputRangeAsObject,
            Excel.Range discountFactorCurvePayLegAsObject,
            Excel.Range discountFactorCurveRecLegAsObject
        )
        {
            var discountFactorCurvePayLeg = discountFactorCurvePayLegAsObject.Value[System.Reflection.Missing.Value] as object[,];
            var discountFactorCurveRecLeg = discountFactorCurveRecLegAsObject.Value[System.Reflection.Missing.Value] as object[,];
            var values = swapInputRangeAsObject.Value[System.Reflection.Missing.Value] as object[,];
            values = (object[,])DataRangeHelper.TrimNulls(values);
            var swapInputRange = RangeHelper.Convert2DArrayToClass<FincadSwapInputRange>(ArrayHelper.RangeToMatrix(values));
            var result = new Functions();
            return result.GetSwapParRate(Engine.Logger, Engine.Cache, NameSpace,
                swapInputRange.ValueDate,
                swapInputRange.EffectiveDate,
                swapInputRange.TerminationDate,
                swapInputRange.InterpolationMethod,
                swapInputRange.MargineAboveFloatingRate,
                swapInputRange.ResetRate,
                swapInputRange.DirectionDateGenerationPayLeg,
                swapInputRange.CashFlowFrequencyPayLeg,
                swapInputRange.AccrualMethodPayLeg,
                swapInputRange.HolidaysPayLeg,
                discountFactorCurvePayLeg,
                swapInputRange.DirectionDateGenerationRecLeg,
                swapInputRange.CashFlowFrequencyRecLeg,
                swapInputRange.AccrualMethodRecLeg,
                swapInputRange.HolidaysRecLeg,
                discountFactorCurveRecLeg);
        }

        ///<summary>
        /// This function emulates the fincad swap pricing function.
        ///</summary>
        ///<param name="valueDate">The value date of the swap.</param>
        ///<param name="effectiveDate">The effective date of the swap.</param>
        ///<param name="terminationDate">The termination date of the swap.</param>
        ///<param name="interpolationMethod">The interpolation to use.</param>
        ///<param name="margineAboveFloatingRate">The margin on the floating leg.</param>
        ///<param name="resetRate">The rest rate for the last reset.</param>
        ///<param name="directionDateGenerationPayLeg">The date generation logic: Forward or Backward.</param>
        ///<param name="cashFlowFrequencyPayLeg">The frequency of pay leg payment.</param>
        ///<param name="accrualMethodPayLeg">The accrual method for the pay leg.</param>
        ///<param name="holidaysPayLeg">The holiday calendars to use on the pay leg.</param>
        ///<param name="discountFactorCurvePayLegAsObject">The discount factor collection for the pay leg.</param>
        ///<param name="directionDateGenerationRecLeg">The date generation logic: Forward or Backward.</param>
        ///<param name="cashFlowFrequencyRecLeg">The frequency of receive leg payment.</param>
        ///<param name="accrualMethodRecLeg">The accrual method for the receive leg.</param>
        ///<param name="holidaysRecLeg">The holiday calendars to use on the receive leg.</param>
        ///<param name="discountFactorCurveRecLegAsObject">The discount factor collection for the receive leg.</param>
        ///<returns>The par swap rate is returned.</returns>
        ///<returns></returns>
        public double FincadSwapCpn
            (
            DateTime valueDate,
            DateTime effectiveDate,
            DateTime terminationDate,
            string interpolationMethod, //1 is linear on forward rates => make sure that the right curve is provided ...
            double margineAboveFloatingRate,// use 0 initially
            double resetRate,
            int directionDateGenerationPayLeg,
            string cashFlowFrequencyPayLeg,
            string accrualMethodPayLeg,
            string holidaysPayLeg,
            Excel.Range discountFactorCurvePayLegAsObject,
            int directionDateGenerationRecLeg,
            string cashFlowFrequencyRecLeg,
            string accrualMethodRecLeg,
            string holidaysRecLeg,
            Excel.Range discountFactorCurveRecLegAsObject
            )
        {
            var discountFactorCurvePayLeg = discountFactorCurvePayLegAsObject.Value[System.Reflection.Missing.Value] as object[,];
            var discountFactorCurveRecLeg = discountFactorCurveRecLegAsObject.Value[System.Reflection.Missing.Value] as object[,];
            var result = new Functions();
            return result.GetSwapParRate(Engine.Logger, Engine.Cache, NameSpace,
                valueDate, effectiveDate, terminationDate,
                interpolationMethod,
                margineAboveFloatingRate, resetRate,
                directionDateGenerationPayLeg,
                cashFlowFrequencyPayLeg,
                accrualMethodPayLeg,
                holidaysPayLeg,
                discountFactorCurvePayLeg,
                directionDateGenerationRecLeg,
                cashFlowFrequencyRecLeg,
                accrualMethodRecLeg,
                holidaysRecLeg,
                discountFactorCurveRecLeg);
        }

        ///<summary>
        /// This function emulates the fincad cashflow generation function.
        ///</summary>
        ///<param name="valueDate">The value date of the swap.</param>
        ///<param name="effectiveDate">The effective date of the swap.</param>
        ///<param name="terminationDate">The termination date of the swap.</param>
        ///<param name="interpolationMethod">The interpolation to use.</param>
        ///<param name="margineAboveFloatingRate">The margin on the floating leg.</param>
        ///<param name="resetRate">The rest rate for the last reset.</param>
        ///<param name="notional">The notional of the swap.</param>
        ///<param name="directionDateGenerationPayLeg">The date generation logic: Forward or Backward.</param>
        ///<param name="cashFlowFrequencyPayLeg">The frequency of pay leg payment.</param>
        ///<param name="accrualMethodPayLeg">The accrual method for the pay leg.</param>
        ///<param name="holidaysPayLeg">The holiday calendars to use on the pay leg.</param>
        ///<param name="discountFactorCurvePayLegAsObject">The discount factor collection for the pay leg.</param>
        ///<param name="directionDateGenerationRecLeg">The date generation logic: Forward or Backward.</param>
        ///<param name="cashFlowFrequencyRecLeg">The frequency of receive leg payment.</param>
        ///<param name="accrualMethodRecLeg">The accrual method for the receive leg.</param>
        ///<param name="holidaysRecLeg">The holiday calendars to use on the receive leg.</param>
        ///<param name="discountFactorCurveRecLegAsObject">The discount factor collection for the receive leg.</param>
        ///<param name="layout">A parameter clarifying the actual layout of the cashflows generated.</param>
        ///<returns>The collection of cashflows.</returns>
        public object[,] FincadSwapCouponSchedule
            (
            DateTime valueDate,
            DateTime effectiveDate,
            DateTime terminationDate,
            string interpolationMethod, //1 is linear on forward rates => make sure that the right curve is provided ...
            double margineAboveFloatingRate,// use 0 initially
            double resetRate,
            decimal notional,
            int directionDateGenerationPayLeg,
            string cashFlowFrequencyPayLeg,
            string accrualMethodPayLeg,
            string holidaysPayLeg,
            Excel.Range discountFactorCurvePayLegAsObject,
            int directionDateGenerationRecLeg,
            string cashFlowFrequencyRecLeg,
            string accrualMethodRecLeg,
            string holidaysRecLeg,
            Excel.Range discountFactorCurveRecLegAsObject,
            double layout
            )
        {
            var discountFactorCurvePayLeg = discountFactorCurvePayLegAsObject.Value[System.Reflection.Missing.Value] as object[,];
            var discountFactorCurveRecLeg = discountFactorCurveRecLegAsObject.Value[System.Reflection.Missing.Value] as object[,];
            var result = new Functions();
            return result.GetSwapCashflows(Engine.Logger, Engine.Cache, NameSpace,
                valueDate, effectiveDate, terminationDate,
                interpolationMethod,
                margineAboveFloatingRate, resetRate,
                notional,
                directionDateGenerationPayLeg,
                cashFlowFrequencyPayLeg,
                accrualMethodPayLeg,
                holidaysPayLeg,
                discountFactorCurvePayLeg,
                directionDateGenerationRecLeg,
                cashFlowFrequencyRecLeg,
                accrualMethodRecLeg,
                holidaysRecLeg,
                discountFactorCurveRecLeg,
                Convert.ToInt32(layout));
        }

        ///<summary>
        /// This function emulates the fincad cashflow generation function. 
        /// However the curves are referenced Orion rate curves.
        ///</summary>
        ///<param name="valueDate">The value date of the swap.</param>
        ///<param name="effectiveDate">The effective date of the swap.</param>
        ///<param name="terminationDate">The termination date of the swap.</param>
        ///<param name="interpolationMethod">The interpolation to use.</param>
        ///<param name="margineAboveFloatingRate">The margin on the floating leg.</param>
        ///<param name="resetRate">The rest rate for the last reset.</param>
        ///<param name="notional">The notional of the swap.</param>
        ///<param name="directionDateGenerationPayLeg">The date generation logic: Forward or Backward.</param>
        ///<param name="cashFlowFrequencyPayLeg">The frequency of pay leg payment.</param>
        ///<param name="accrualMethodPayLeg">The accrual method for the pay leg.</param>
        ///<param name="holidaysPayLeg">The holiday calendars to use on the pay leg.</param>
        ///<param name="discountFactorCurvePayLeg">The referenced rate curve for the pay leg.</param>
        ///<param name="directionDateGenerationRecLeg">The date generation logic: Forward or Backward.</param>
        ///<param name="cashFlowFrequencyRecLeg">The frequency of receive leg payment.</param>
        ///<param name="accrualMethodRecLeg">The accrual method for the receive leg.</param>
        ///<param name="holidaysRecLeg">The holiday calendars to use on the receive leg.</param>
        ///<param name="discountFactorCurveRecLeg">The referenced rate curve for the receive leg.</param>
        ///<param name="layout">A parameter clarifying the actual layout of the cashflows generated.</param>
        ///<returns>The collection of cashflows.</returns>
        public object[,] GetSwapCashflows
            (
            DateTime valueDate,
            DateTime effectiveDate,
            DateTime terminationDate,
            string interpolationMethod, //1 is linear on forward rates => make sure that the right curve is provided ...
            double margineAboveFloatingRate,// use 0 initially
            double resetRate,
            decimal notional,
            int directionDateGenerationPayLeg,
            string cashFlowFrequencyPayLeg,
            string accrualMethodPayLeg,
            string holidaysPayLeg,
            string discountFactorCurvePayLeg,
            int directionDateGenerationRecLeg,
            string cashFlowFrequencyRecLeg,
            string accrualMethodRecLeg,
            string holidaysRecLeg,
            string discountFactorCurveRecLeg,
            double layout
            )
        {
            var result = new Functions();
            var curve1 = (IRateCurve)Engine.GetCurve(discountFactorCurvePayLeg, false);
            var curve2 = (IRateCurve)Engine.GetCurve(discountFactorCurveRecLeg, false);
            return result.GetSwapCashflowsWithoutCurves(Engine.Logger, Engine.Cache, NameSpace,
                valueDate, effectiveDate, terminationDate,
                interpolationMethod,
                margineAboveFloatingRate, resetRate,
                notional,
                directionDateGenerationPayLeg,
                cashFlowFrequencyPayLeg,
                accrualMethodPayLeg,
                holidaysPayLeg,
                curve1,
                directionDateGenerationRecLeg,
                cashFlowFrequencyRecLeg,
                accrualMethodRecLeg,
                holidaysRecLeg,
                curve2,
                Convert.ToInt32(layout));
        }

        ///<summary>
        /// This function emulates the fincad futures convexity function.
        ///</summary>
        ///<param name="valueDate">The value date.</param>
        ///<param name="effectiveDate">The effective date.</param>
        ///<param name="terminationDate">The termination date.</param>
        ///<param name="volatility">The volatility.</param>
        ///<param name="manReversionConstant">A mean reversion constant.</param>
        ///<returns>The convexity adjustment.</returns>
        public double FincadEDFuturesConvexityAdjHW
            (
            DateTime valueDate,
            DateTime effectiveDate,
            DateTime terminationDate,
            double volatility,
            double manReversionConstant
            )
        {
            return FuturesAnalytics.HullWhiteConvexityAdjustment(valueDate, effectiveDate, terminationDate,
                                                    volatility,
                                                    manReversionConstant);
        }

        #endregion
    }
}