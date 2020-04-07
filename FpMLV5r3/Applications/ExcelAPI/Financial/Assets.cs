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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Highlander.CalendarEngine.V5r3;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.CurveEngine.V5r3;
using Highlander.CurveEngine.V5r3.Assets.Helpers;
using Highlander.CurveEngine.V5r3.Assets.Rates.Swaps;
using Highlander.CurveEngine.V5r3.Factory;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.Markets;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.Assets;
using Highlander.Reporting.Models.V5r3.Commodities;
using Highlander.Reporting.Models.V5r3.Equity;
using Highlander.Reporting.Models.V5r3.ForeignExchange;
using Highlander.Reporting.Models.V5r3.Futures;
using Highlander.Reporting.Models.V5r3.Property;
using Highlander.Reporting.Models.V5r3.Property.Lease;
using Highlander.Reporting.Models.V5r3.Rates.Options;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.ValuationEngine.V5r3;
using HLV5r3.Helpers;
using Microsoft.Win32;
using ApplicationHelper = HLV5r3.Helpers.ApplicationHelper;
using Excel = Microsoft.Office.Interop.Excel;
using RuntimeEnvironment = HLV5r3.Runtime.RuntimeEnvironment;

#endregion

namespace HLV5r3.Financial
{
    /// <summary>
    /// Creates and manages all priceable assets.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("4959FA02-DD3A-4709-8375-EEA0EC45A239")]
    public partial class Cache
    {
        #region Fields

        public CalendarEngine CalendarService;
        public CurveEngine Engine;
        public ValuationService ValService;
        public readonly RuntimeEnvironment Environment;
        public string NameSpace;

        #endregion

        #region Constructor

        public Cache()
        {
            //Be careful of using the default namespace.
            NameSpace = EnvironmentProp.DefaultNameSpace;
            Environment = new RuntimeEnvironment(NameSpace);
            Engine = new CurveEngine(Environment.LogRef.Target, Environment.Cache, NameSpace);
            ValService = new ValuationService(Environment.LogRef.Target, Environment.Cache, NameSpace);
            CalendarService = new CalendarEngine(Environment.LogRef.Target, Environment.Cache, NameSpace);
        }

        public Cache(ILogger logger, ICoreCache cache)
            : this(logger, cache, EnvironmentProp.DefaultNameSpace)
        {}

        public Cache(ILogger logger, ICoreCache cache, string nameSpace)
        {
            Engine = new CurveEngine(logger, cache, nameSpace);
            ValService = new ValuationService(logger, cache, nameSpace);
            NameSpace = nameSpace;
        }

        #endregion

        #region Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type type)
        {
            Registry.ClassesRoot.CreateSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"));
            var key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
            key?.SetValue("", System.Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type type)
        {
            Registry.ClassesRoot.DeleteSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"), false);
        }

        #endregion

        #region Information Functions

        public String CurrentNameSpace()
        {
            return Engine.NameSpace;
        }

        /// <summary>
        /// Saves the pricing structure to the database.
        /// </summary>
        public object[,] DisplayPropertyData(string name)
        {
            //load object to storage.
            var loadedObjects = Engine.Cache.LoadItem(null, NameSpace + "." + name);
            if (loadedObjects == null)
            {
                var message = $"The search using this name '{name}' yielded no results.";
                throw new ApplicationException(message);
            }
            // if there's more than one object returned - use the most recent one
            //
            var nvs = loadedObjects.AppProps;
            var result = new object[nvs.Count, 2];
            var index = 0;
            foreach (var element in nvs.ToArray())
            {
                //create and set the pricing structure
                result[index, 0] = element.Name;
                result[index, 1] = element.ValueString;
                index++;
            }
            return result;
        }

        /// <summary>
        /// A function to return the list of valid asset measures. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of asset measure types.</returns>
        public object[,] SupportedExchangeTradedTypes()
        {
            var names = Enum.GetNames(typeof(ExchangeContractTypeEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of valid asset measures. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of asset measure types.</returns>
        public object[,] SupportedAssetMeasureTypes()
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
        public object[,] SupportedPriceQuoteUnits()
        {
            var names = Enum.GetNames(typeof(PriceQuoteUnitsEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of valid bond price quotes. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of valid price quote units.</returns>
        public object[,] SupportedBondPriceQuotes()
        {
            var names = Enum.GetNames(typeof(BondPriceEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented bond assets. 
        /// Each bond type has different static data information.
        /// <para>AGB,</para> 
        /// <para>BOBL,</para> 
        /// <para>Bund,</para> 
        /// <para>Gilt,</para> 
        /// <para>USTreas,</para> 
        /// <para>JGB</para>
        /// </summary>
        /// <remarks>
        /// The configuration information for these bonds is stored in the database.
        /// The main differences relate to the settlement day convention used in each 
        /// domicile where the particular bonds are traded.
        /// </remarks>
        /// <returns>A vertical range object, containing the list of valid bond types.</returns>
        public object[,] SupportedBondTypes()
        {
            var names = Enum.GetNames(typeof(BondTypesEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented bond assets. 
        /// Each bond type has different static data information.
        /// <para>Govt,</para> 
        /// <para>Corp,</para> 
        /// <para>Mtge,</para> 
        /// <para>Equity,</para> 
        /// <para>Comdty,</para> 
        /// <para>Pfd</para>
        /// <para>Index,</para> 
        /// <para>Curncy,</para> 
        /// <para>Muni</para>
        /// <para>Mmkt</para>
        /// </summary>
        /// <returns>A vertical range object, containing the list of valid bond types.</returns>
        public object[,] SupportedMarketSectors()
        {
            var names = Enum.GetNames(typeof(MarketSectorEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented bond assets. 
        /// Each bond type has different static data information.
        /// <para>Senior,</para> 
        /// <para>SubTier1,</para> 
        /// <para>SubUpperTier2,</para> 
        /// <para>SubLowerTier2,</para> 
        /// <para>SubTier3,</para> 
        /// </summary>
        /// <returns>A vertical range object, containing the list of valid bond types.</returns>
        public object[,] SupportedCreditSeniority()
        {
            var names = Enum.GetNames(typeof(CreditSeniorityEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented bond assets. 
        /// Each bond type has different static data information.
        /// <para>Fixed,</para> 
        /// <para>Float,</para> 
        /// <para>Struct,</para> 
        /// </summary>
        /// <returns>A vertical range object, containing the list of valid bond types.</returns>
        public object[,] SupportedCouponTypes()
        {
            var names = Enum.GetNames(typeof(CouponTypeEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented assets. 
        /// Each asset type has different static data information.
        /// <para>BasisSwap,</para>
        /// <para>Bond,</para>
        /// <para>Deposit,</para> 
        /// <para>XccyDepo,</para> 
        /// <para>IRFuture,</para> 
        /// <para>IRFutureOption,</para> 
        /// <para>Caplet,</para> 
        /// <para>Floorlet,</para> 
        /// <para>BillCaplet,</para> 
        /// <para>BillFloorlet,</para> 
        /// <para>FxSpot,</para> 
        /// <para>FxForward,</para> 
        /// <para>FxFuture,</para> 
        /// <para>CommoditySpot,</para> 
        /// <para>CommodityForward,</para> 
        /// <para>CommodityFuture,</para> 
        /// <para>BankBill,</para> 
        /// <para>SimpleFra, </para>
        /// <para>Fra,</para> 
        /// <para>SpreadFra, </para>
        /// <para>BillFra,</para> 
        /// <para>SimpleIRSwap,</para> 
        /// <para>IRSwap,</para> 
        /// <para>XccySwap,</para> 
        /// <para>IRCap,</para> 
        /// <para>Xibor,</para> 
        /// <para>OIS,</para> 
        /// <para>CPIndex,</para> 
        /// <para>CPISwap,</para> 
        /// <para>SimpleCPISwap,</para> 
        /// <para>ZCCPISwap,</para> 
        /// <para>ZeroRate</para>
        /// </summary>
        /// <remarks>
        /// The configuration information for these assets is stored in the database.
        /// The main differences relate to the spot date conventions used in each 
        /// asset. Other data relates to default information about the particular asset.
        /// </remarks>
        /// <returns>A vertical range object, containing the list of valid asset types.</returns>
        public object[,] SupportedAssets()
        {
            var names = Enum.GetNames(typeof(AssetTypesEnum));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of valid indices, which may or may not have been implemented.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedFloatingRates()
        {
            //var names = Enum.GetNames(typeof(FloatingRateIndexEnum));
            var indices = Enum.GetValues(typeof (FloatingRateIndexEnum)).Cast<FloatingRateIndexEnum>().Select(FloatingRateIndexScheme.GetEnumString).Where(index => index != null).ToList();
            var result = RangeHelper.ConvertArrayToRange(indices);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedEquityMetrics()
        {
            var names = Enum.GetNames(typeof(EquityMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedBondMetrics()
        {
            var names = Enum.GetNames(typeof(BondMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedPropertyMetrics()
        {
            var names = Enum.GetNames(typeof(PropertyMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedLeaseMetrics()
        {
            var names = Enum.GetNames(typeof(LeaseMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedFuturesMetrics()
        {
            var names = Enum.GetNames(typeof(FuturesMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedRateMetrics()
        {
            var names = Enum.GetNames(typeof(RateMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedFxMetrics()
        {
            var names = Enum.GetNames(typeof(FxMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedCommodityMetrics()
        {
            var names = Enum.GetNames(typeof(CommodityMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public object[,] SupportedRateOptionMetrics()
        {
            var names = Enum.GetNames(typeof(RateOptionMetrics));
            var result = RangeHelper.ConvertArrayToRange(names);
            return result;
        }

        #endregion

        #region Assets and Instruments

        /// <summary>
        /// Load Asset Config from the XML in the database
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="asset"></param>
        /// <param name="maturityTenor"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public object[,] GetAssetConfigurationData(string currency, string asset, string maturityTenor, [Optional] string variant)
        {
            var result = Engine.GetAssetConfigurationData(currency, asset, maturityTenor, variant);
            return result;
        }
    
        ///<summary>
        /// Calculates the partial differential hedge for a swap.
        ///</summary>
        ///<param name="curveId">The curve id. This must have been created already.</param>
        ///<param name="currency">The configuration currency.
        /// This is used to get all default values and must exist in the cache.</param>
        ///<param name="baseDate">The base date.</param>
        ///<param name="fixedRate">THe fixed rate.</param>
        ///<param name="businessDayConvention">The businessDayConvention.</param>
        ///<param name="notionalWeightsArray">The notional weights.</param>
        ///<param name="fixedLegDayCount">The dayCount.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="datesAsArray">The roll dates.</param>
        ///<param name="businessCentersAsString">Te business centers.</param>
        ///<returns>The PDH array.</returns>
        public object[,] GetSwapPDH(string curveId, string currency, DateTime baseDate,
            decimal notional, Excel.Range datesAsArray, Excel.Range notionalWeightsArray, string fixedLegDayCount, decimal fixedRate,
            string businessDayConvention, string businessCentersAsString)
        {
            var result = new List<decimal>();
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            bav.quote = new[] { quote };
            //Map the ranges.
            var rollDates = DataRangeHelper.StripDateTimeRange(datesAsArray);
            var weights = DataRangeHelper.StripDecimalRange(notionalWeightsArray);
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention,
                                                                             businessCentersAsString);
            //create the swap.
            var swap = (PriceableSwapRateAsset)PriceableAssetFactory.CreateInterestRateSwap("Local", baseDate, MoneyHelper.GetAmount(notional, currency), rollDates.ToArray(),
                                                                    weights.ToArray(), fixedLegDayCount, businessDayAdjustments, bav);
            //Get the curve.
            var rateCurve = (CurveBase)Engine.GetCurve(curveId, false);//This requires an IPricingStructure to be cached.
            //Create the interpolator and get the pdh.
            var valuation = swap.CalculatePDH(Engine.Logger, Engine.Cache, rateCurve, null, null);
            if (valuation != null)
            {
                result.AddRange(valuation);
            }
            return RangeHelper.ConvertArrayToRange(result);
        }

        /// <summary>
        /// Calculates the bond asset swap metric using the curve provided.
        /// </summary>
        /// <param name="identifier">The identifier of the bond.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="exDivDate">The next ex div date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="amount">The notional amount.</param>
        /// <param name="coupon">The coupon.</param>
        /// <param name="frequency">The coupon frequency.</param>
        /// <param name="issuerName">The issuer.</param>
        /// <param name="businessCenters">The business centers.</param>
        /// <param name="ytm">The yield to maturity.</param>
        /// <param name="curveId">The curve to use for calculation</param>
        /// <param name="dayCount">The dayCount convention.</param>
        /// <param name="settlementDate">The settlement date.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <returns>The asset swap level.</returns>
        public decimal GetBondAssetSwapSpread(string identifier, DateTime baseDate, DateTime settlementDate, DateTime exDivDate, DateTime maturityDate,
            decimal amount, decimal coupon, string dayCount, string frequency, string issuerName, string rollConvention, string businessCenters, decimal ytm, string curveId)
        {
            //Get the curve.
            var rateCurve = (IRateCurve)Engine.GetCurve(curveId, false);
            //Calculate the asset swap level.
            var asw = Engine.GetBondAssetSwapSpread(identifier, baseDate, settlementDate, exDivDate, MoneyHelper.GetAmount(amount)
                , coupon, maturityDate, dayCount, frequency, issuerName, rollConvention, businessCenters, rateCurve, ytm, null);
            return asw;
        }

        ///<summary>
        /// This function is a simple was to create a swap that has sequential roll dates and is adjusted, 
        /// with no spread on the floating side.
        /// The function will return a series of metrics for that swap, depending on what is requested.
        ///</summary>
        /// <remarks>
        /// The assumption is that only one curve is required for valuation, as the floating leg always 
        /// values to zero when including principal exchanges.
        /// </remarks>
        ///<param name="curveId">The curve id. This must have been created already.</param>
        ///<param name="currency">The configuration currency.
        /// This is used to get all default values and must exist in the cache.</param>
        ///<param name="baseDate">The base date.</param>
        ///<param name="fixedRate">The fixed rate.</param>
        ///<param name="businessDayConvention">The businessDayConvention.</param>
        ///<param name="notionalWeightsAsArray">The notional weights.</param>
        ///<param name="fixedLegDayCount">The daycount.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="businessCentersAsString">The business centers.</param>
        ///<param name="metric">The metric.</param>
        ///<param name="datesAsArray">The dates.</param>
        ///<returns></returns>
        public decimal GetStructuredSwapMetric(string curveId, string currency, DateTime baseDate,
            decimal notional, Excel.Range datesAsArray, Excel.Range notionalWeightsAsArray, string fixedLegDayCount, decimal fixedRate,
            string businessDayConvention, string businessCentersAsString, string metric)
        {
            //Map the ranges.
            var rollDates = DataRangeHelper.StripDateTimeRange(datesAsArray);
            var weights = DataRangeHelper.StripDecimalRange(notionalWeightsAsArray);
            var result = Engine.GetStructuredSwapMetric(curveId, currency, baseDate, 
                notional, rollDates, weights, fixedLegDayCount, fixedRate, businessDayConvention, businessCentersAsString, metric);
            return result;
        }

        ///<summary>
        /// This function is a simple was to create a cap that has sequential roll dates and is adjusted, 
        /// This function does however allow different conventions on the floating side and a non-zero spread.
        /// The function will return a series of metrics for that cap, depending on what is requested.
        ///</summary>
        /// <remarks>
        /// The assumption is that only one curve is required for valuation, as the floating leg always 
        /// values to zero when including principal exchanges.
        /// </remarks>
        ///<param name="rollBackward">If tru, roll back from the maturity date. Otherwise roll forward from the effective date.</param>
        ///<param name="paymentBusinessDayConvention">The payment businessDayConvention.</param>
        ///<param name="strike">The strike rate.</param>
        ///<param name="lastResetsAsArray">An array of last Reset rates. This may be null.</param>
        ///<param name="includeStubFlag">Include the first reset flag.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="resetPeriod">The reset period e.g. -2D.</param>
        ///<param name="paymentBusinessCentersAsString">The payment business centers.</param>
        ///<param name="resetBusinessCentersAsString">The rest business days.</param>
        ///<param name="metricsAsArray">The metrics as an array.</param>
        ///<param name="effectiveDate">The roll dates with the maturity date included..</param>
        ///<param name="resetBusinessDayConvention">The reset business day convention.</param>
        ///<param name="cashFlowDetail">A detail cashflow flag - mainly for debugging.</param>
        /// <param name="propertiesRange">Contains all the required properties.</param>
        ///<returns>The value of the metric requested.</returns>
        public object[,] GetIRCapFloorMetrics(
            DateTime effectiveDate, double notional, double strike, Excel.Range lastResetsAsArray, bool includeStubFlag, 
            bool rollBackward, string paymentBusinessDayConvention, string paymentBusinessCentersAsString,
            string resetBusinessDayConvention, string resetBusinessCentersAsString,
            string resetPeriod, Excel.Range metricsAsArray, bool cashFlowDetail, Excel.Range propertiesRange)
        {
            if (paymentBusinessDayConvention == null) throw new ArgumentNullException(nameof(paymentBusinessDayConvention));
            if (propertiesRange == null) throw new ArgumentNullException(nameof(propertiesRange));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var forecastCurveId = namedValueSet.GetString("ForecastCurve", true);
            var discountCurveId = namedValueSet.GetString("DiscountCurve", true);
            var volCurveId = namedValueSet.GetString("VolatilitySurface", true);
            var baseDate = namedValueSet.GetValue<DateTime>("BaseDate", true);
            var isCap = namedValueSet.GetValue<bool>("IsCap", true);
            var currency = namedValueSet.GetString("Currency", true);
            var indexTenor = namedValueSet.GetString("IndexTenor", true);
            var dayCount = namedValueSet.GetString("FloatingLegDayCount", true);
            var maturityTerm = namedValueSet.GetString("Term", true);
            var floatingRateIndex = namedValueSet.GetString("FloatingRateIndex", true);
            var result = new Dictionary<string, decimal>();
            var metricsArray = DataRangeHelper.StripRange(metricsAsArray);
            //Map the ranges.
            var lastResets = DataRangeHelper.StripDoubleRange(lastResetsAsArray);
            //Remove any empty cell values.          
            List<Double> resetRates = null;
            if (lastResets.Count > 0)
            {
                resetRates = lastResets;
            }
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(paymentBusinessDayConvention,
                                                                             paymentBusinessCentersAsString);
            //Create the RelativeDateOffset.
            var resetOffset = RelativeDateOffsetHelper.Create(resetPeriod, DayTypeEnum.Business,
                                                              resetBusinessDayConvention, resetBusinessCentersAsString, "StartDate");
            //Create the ForecastRateIndex.
            var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex, indexTenor);
            //create the cap/floor.
            var capfloor = isCap
                               ? PriceableAssetFactory.CreateIRCap(Engine.Logger, Engine.Cache, NameSpace, baseDate, effectiveDate, maturityTerm, currency,
                    indexTenor, includeStubFlag, notional, strike, rollBackward, resetRates,
                                                                     resetOffset, businessDayAdjustments, dayCount, forecastRate, null, null, namedValueSet)
                               : PriceableAssetFactory.CreateIRFloor(Engine.Logger, Engine.Cache, NameSpace, baseDate, effectiveDate, maturityTerm, currency,
                    indexTenor, includeStubFlag, notional, strike, rollBackward, resetRates,
                                                                     resetOffset, businessDayAdjustments, dayCount, forecastRate, null, null, namedValueSet);
            //Get the AssetControllerData.
            var market = new SwapLegEnvironment();
            var bavMetric = new BasicAssetValuation
                                {
                                    quote =
                                        metricsArray.Select(metric => BasicQuotationHelper.Create(0.0m, metric)).ToArray
                                        ()
                                };
            //Set the quote metrics.
            //Get the curve.
            var forecastcurve = (IRateCurve)Engine.GetCurve(forecastCurveId, false);
            var discountcurve = (IRateCurve)Engine.GetCurve(discountCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), forecastcurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), discountcurve);
            var volcurve = (IVolatilitySurface)Engine.GetCurve(volCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastIndexVolatilitySurface.ToString(), volcurve);
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = capfloor.Calculate(assetControllerData);
            if (valuation == null) return ArrayHelper.ConvertDictionaryTo2DArray(result);
            //Check to see if details are required.
            if (cashFlowDetail)
            {
                foreach (var metricQuote in valuation.quote)
                {
                    if (metricQuote.informationSource != null)
                    {
                        result.Add(metricQuote.measureType.Value + metricQuote.informationSource[0].rateSource.Value, metricQuote.value);
                    }
                    else
                    {
                        result.Add(metricQuote.measureType.Value, metricQuote.value);
                    }
                }
                return ArrayHelper.ConvertDictionaryTo2DArray(result);
            }
            //Return a scalar sum if details are not required.
            result = MetricsHelper.AggregateMetrics(valuation, new List<string>(metricsArray));
            return ArrayHelper.ConvertDictionaryTo2DArray(result);
        }

        /// <summary>
        ///  This function is a simple was to create a cap that has sequential roll dates and is adjusted, 
        ///  This function does however allow different conventions on the floating side and a non-zero spread.
        ///  The function will return a series of metrics for that cap, depending on what is requested.
        /// </summary>
        ///  <remarks>
        ///  The assumption is that only one curve is required for valuation, as the floating leg always 
        ///  values to zero when including principal exchanges.
        ///  </remarks>
        /// <param name="paymentBusinessDayConvention">The payment businessDayConvention.</param>
        /// <param name="strikesAsArray">The strike array.</param>
        /// <param name="resetsAsArray">An array of rest rates. This may be null.</param>
        /// <param name="notionalsAsArray">The notionals.</param>
        /// <param name="resetPeriod">The reset period e.g. -2D.</param>
        /// <param name="paymentBusinessCentersAsString">The payment business centers.</param>
        /// <param name="resetBusinessCentersAsString">The rest business days.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="rollAndMaturityDatesAsArray">The roll dates with the maturity date included.</param>
        /// <param name="resetBusinessDayConvention">The reset business day convention.</param>
        /// <param name="propertiesRange">Contains all the required properties.</param>
        /// <returns>The value of the metric requested.</returns>
        public object[,] GetIRCapFloorMetric(Excel.Range rollAndMaturityDatesAsArray, Excel.Range notionalsAsArray,
            Excel.Range strikesAsArray, Excel.Range resetsAsArray, string paymentBusinessDayConvention, string paymentBusinessCentersAsString,
            string resetBusinessDayConvention, string resetBusinessCentersAsString, string resetPeriod, string metric, Excel.Range propertiesRange)
        {
            if (propertiesRange == null) throw new ArgumentNullException(nameof(propertiesRange));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var forecastCurveId = namedValueSet.GetString("ForecastCurve", true);
            var discountCurveId = namedValueSet.GetString("DiscountCurve", true);
            var volCurveId = namedValueSet.GetString("VolatilitySurface", true);
            var baseDate = namedValueSet.GetValue<DateTime>("BaseDate", true);
            var isCap = namedValueSet.GetValue<bool>("IsCap", true);
            var currency = namedValueSet.GetString("Currency", true);
            var indexTenor = namedValueSet.GetString("IndexTenor", true);
            var dayCount = namedValueSet.GetString("FloatingLegDayCount", true);
            var floatingRateIndex = namedValueSet.GetString("FloatingRateIndex", true);
            if (paymentBusinessDayConvention == null) throw new ArgumentNullException(nameof(paymentBusinessDayConvention));
            var result = new Dictionary<string, decimal>();
            //Map the ranges.
            var rollDates = DataRangeHelper.StripDateTimeRange(rollAndMaturityDatesAsArray);
            var weights = DataRangeHelper.StripDoubleRange(notionalsAsArray);
            var strikeRates = DataRangeHelper.StripDoubleRange(strikesAsArray);
            var resets = DataRangeHelper.StripDoubleRange(resetsAsArray);
            //Remove any empty cell values.          
            List<Double> resetRates = null;
            if (resets.Count > 0)
            {
                resetRates = resets;
            }
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(paymentBusinessDayConvention,
                                                                             paymentBusinessCentersAsString);
            //Create the RelativeDateOffset.
            var resetOffset = RelativeDateOffsetHelper.Create(resetPeriod, DayTypeEnum.Business,
                                                              resetBusinessDayConvention, resetBusinessCentersAsString, "StartDate");
            //Create the ForecastRateIndex.
            var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex, indexTenor);
            //create the cap/floor.
            var capfloor = isCap ? PriceableAssetFactory.CreateIRCap(Engine.Logger, Engine.Cache, NameSpace, "Local", baseDate, currency, rollDates,
                                                                    weights, strikeRates, resetRates, resetOffset,
                                                                    businessDayAdjustments, dayCount, forecastRate, null, namedValueSet)
                                                                    : PriceableAssetFactory.CreateIRFloor(Engine.Logger, Engine.Cache, NameSpace, "Local", baseDate, currency, rollDates,
                                                                    weights, strikeRates, resetRates, resetOffset,
                                                                    businessDayAdjustments, dayCount, forecastRate, null, namedValueSet);
            //Get the AssetControllerData.
            var market = new SwapLegEnvironment();
            var bavMetric = new BasicAssetValuation();
            var quoteMetric = BasicQuotationHelper.Create(0.0m, metric);
            bavMetric.quote = new[] { quoteMetric };
            //Get the curve.
            var forecastcurve = (IRateCurve)Engine.GetCurve(forecastCurveId, false);
            var discountcurve = (IRateCurve)Engine.GetCurve(discountCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), forecastcurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), discountcurve);
            var volcurve = (IVolatilitySurface)Engine.GetCurve(volCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastIndexVolatilitySurface.ToString(), volcurve);
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = capfloor.Calculate(assetControllerData);
            if (valuation != null)
            {
                foreach (var metricQuote in valuation.quote)
                {
                    if (metricQuote.informationSource != null)
                    {
                        result.Add(metricQuote.measureType.Value + metricQuote.informationSource[0].rateSource.Value, metricQuote.value);
                    }
                    else
                    {
                        result.Add(metricQuote.measureType.Value, metricQuote.value);
                    }
                }
            }
            object[,] results = ArrayHelper.ConvertDictionaryTo2DArray(result);
            return results;
        }

        ///<summary>
        /// This function is a simple was to create a cap that has sequential roll dates and is adjusted, 
        /// This function does however allow different conventions on the floating side and a non-zero spread.
        /// The function will return the pdh for the forecast and discount curve together.
        ///</summary>
        /// <remarks>
        /// The assumption is that only one curve is required for valuation, as the floating leg always 
        /// values to zero when including principal exchanges.
        /// </remarks>
        ///<param name="paymentBusinessDayConvention">The payment businessDayConvention.</param>
        ///<param name="strikesAsArray">The strike array.</param>
        ///<param name="resetsAsArray">An array of rest rates. This may be null.</param>
        ///<param name="notionalsAsArray">The notionals.</param>
        ///<param name="resetPeriod">The reset period e.g. -2D.</param>
        ///<param name="paymentBusinessCentersAsString">The payment business centers.</param>
        ///<param name="resetBusinessCentersAsString">The rest business days.</param>
        ///<param name="rollAndMaturityDatesAsArray">The roll dates with the maturity date included..</param>
        ///<param name="resetBusinessDayConvention">The reset business day convention.</param>
        /// <param name="propertiesRange">Contains all the required properties.</param>
        ///<returns>The rate curve pdh.</returns>
        public object[,] GetIRCapFloorPDH(Excel.Range rollAndMaturityDatesAsArray, Excel.Range notionalsAsArray,
            Excel.Range strikesAsArray, Excel.Range resetsAsArray, string paymentBusinessDayConvention, string paymentBusinessCentersAsString,
            string resetBusinessDayConvention, string resetBusinessCentersAsString, string resetPeriod, Excel.Range propertiesRange)
        {
            if (propertiesRange == null) throw new ArgumentNullException(nameof(propertiesRange));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var forecastCurveId = namedValueSet.GetString("ForecastCurve", true);
            var discountCurveId = namedValueSet.GetString("DiscountCurve", true);
            var volCurveId = namedValueSet.GetString("VolatilitySurface", true);
            var baseDate = namedValueSet.GetValue<DateTime>("BaseDate", true);
            var curveToPerturb = namedValueSet.GetString("CurveToPerturb", true);
            var curvePerturbation = EnumHelper.Parse<CurvePerturbation>(curveToPerturb);
            var isCap = namedValueSet.GetValue<bool>("IsCap", true);
            var currency = namedValueSet.GetString("Currency", true);
            var indexTenor = namedValueSet.GetString("IndexTenor", true);
            var dayCount = namedValueSet.GetString("FloatingLegDayCount", true);
            var floatingRateIndex = namedValueSet.GetString("FloatingRateIndex", true);
            if (paymentBusinessDayConvention == null) throw new ArgumentNullException(nameof(paymentBusinessDayConvention));
            //Map the ranges.
            var rollDates = DataRangeHelper.StripDateTimeRange(rollAndMaturityDatesAsArray);
            var weights = DataRangeHelper.StripDoubleRange(notionalsAsArray);
            var strikeRates = DataRangeHelper.StripDoubleRange(strikesAsArray);
            var resets = DataRangeHelper.StripDoubleRange(resetsAsArray);
            //Remove any empty cell values.          
            List<Double> resetRates = null;
            if (resets.Count > 0)
            {
                resetRates = resets;
            }
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(paymentBusinessDayConvention,
                                                                             paymentBusinessCentersAsString);
            //Create the RelativeDateOffset.
            var resetOffset = RelativeDateOffsetHelper.Create(resetPeriod, DayTypeEnum.Business,
                                                              resetBusinessDayConvention, resetBusinessCentersAsString, "StartDate");
            //Create the ForecastRateIndex.
            var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex, indexTenor);
            //create the cap/floor.
            var capfloor = isCap ? PriceableAssetFactory.CreateIRCap(Engine.Logger, Engine.Cache, NameSpace, "Local", baseDate, currency, new List<DateTime>(rollDates),
                                                                    new List<double>(weights), new List<double>(strikeRates), resetRates, resetOffset,
                                                                    businessDayAdjustments, dayCount, forecastRate, null, namedValueSet)
                                                                    : PriceableAssetFactory.CreateIRFloor(Engine.Logger, Engine.Cache, NameSpace, "Local", baseDate, currency, new List<DateTime>(rollDates),
                                                                    new List<double>(weights), new List<double>(strikeRates), resetRates, resetOffset,
                                                                    businessDayAdjustments, dayCount, forecastRate, null, namedValueSet);
            //Get the curve.
            var market = new SwapLegEnvironment();
            var forecastcurve = (IRateCurve)Engine.GetCurve(forecastCurveId, false);
            var discountcurve = (IRateCurve)Engine.GetCurve(discountCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), forecastcurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), discountcurve);
            var volcurve = (IVolatilitySurface)Engine.GetCurve(volCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastIndexVolatilitySurface.ToString(), volcurve);
            //Create the interpolator and get the pdh.
            var valuation = capfloor.CalculateRatePDH(baseDate, discountcurve, forecastcurve, volcurve, curvePerturbation);
            object[,] results = ArrayHelper.ConvertDictionaryTo2DArray(valuation);
            return results;
        }

        ///<summary>
        /// This function is a simple was to create a swap that has sequential roll dates and is adjusted, 
        /// This function does however allow different conventions on the floating side and a non-zero spread.
        /// The function will return a series of metrics for that swap, depending on what is requested.
        ///</summary>
        /// <remarks>
        /// The assumption is that only one curve is required for valuation, as the floating leg always 
        /// values to zero when including principal exchanges.
        /// </remarks>
        ///<param name="fixedNotionalsAsArray">The fixed notionals.</param>
        ///<param name="metric">The metric.</param>
        ///<param name="fixedRollDatesAsArray">The fixed roll dates, including the maturity date.</param>
        ///<param name="floatingRollDatesAsArray">The floating roll dates. Includes the maturity date</param>
        ///<param name="floatingNotionalsAsArray">The floating notionals.</param>
        ///<param name="floatingResetRatesAsArray">The floating leg rest rate array.</param>
        ///<param name="propertiesRange">The properties associated with the asset.</param>
        ///<returns>The value of the metric requested.</returns>
        public decimal GetIRSwapMetric(Excel.Range fixedRollDatesAsArray, Excel.Range fixedNotionalsAsArray, 
            Excel.Range floatingRollDatesAsArray, Excel.Range floatingNotionalsAsArray, 
            Excel.Range floatingResetRatesAsArray, string metric, Excel.Range propertiesRange)
        {
            if (propertiesRange == null) throw new ArgumentNullException(nameof(propertiesRange));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var forecastCurveId = namedValueSet.GetString("ForecastCurve", true);
            var discountCurveId = namedValueSet.GetString("DiscountCurve", true);
            var fixedRate = namedValueSet.GetValue<decimal>("Rate", true);
            var floatingSpread = namedValueSet.GetValue<decimal>("Spread", true);
            var baseDate = namedValueSet.GetValue<DateTime>("BaseDate", true);
            var businessDayConvention = namedValueSet.GetString("BusinessDayConvention", true);
            var businessCentersAsString = namedValueSet.GetString("BusinessCentersAsString", true);
            var result = 0.0m;
            //Map the ranges.
            var rollDates = DataRangeHelper.StripDateTimeRange(fixedRollDatesAsArray);
            var weights = DataRangeHelper.StripDecimalRange(fixedNotionalsAsArray);
            var floatingDates = DataRangeHelper.StripDateTimeRange(floatingRollDatesAsArray);
            var floatingWeights = DataRangeHelper.StripDecimalRange(floatingNotionalsAsArray);
            var floatingResets = DataRangeHelper.StripDecimalRange(floatingResetRatesAsArray);
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            var spreadQuote = BasicQuotationHelper.Create(floatingSpread, "Spread", "DecimalRate");
            bav.quote = new[] { quote, spreadQuote };
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention,
                                                                             businessCentersAsString);
            //create the swap.
            var swap = PriceableAssetFactory.CreateIRSwap(rollDates,
                                                                    weights,
                                                                    floatingDates,
                                                                    floatingWeights,
                                                                    floatingResets,
                                                                    businessDayAdjustments, bav, namedValueSet);
            //Get the AssetControllerData.
            var market = new SwapLegEnvironment();
            var bavMetric = new BasicAssetValuation();
            var quoteMetric = BasicQuotationHelper.Create(0.0m, metric);
            bavMetric.quote = new[] { quoteMetric };
            //Get the curve.
            var discountCurve = (IRateCurve)Engine.GetCurve(discountCurveId, false);
            var forecastCurve = (IRateCurve)Engine.GetCurve(forecastCurveId, false);
            market.AddPricingStructure("DiscountCurve", discountCurve);
            market.AddPricingStructure("ForecastCurve", forecastCurve);
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = swap.Calculate(assetControllerData);
            if (valuation != null)
            {
                result = valuation.quote[0].value;
            }
            return result;
        }

        ///<summary>
        /// This function is a simple was to create a swap that has sequential roll dates and is adjusted, 
        /// This function does however allow different conventions on the floating side and a non-zero spread.
        /// The function will return a set of metrics for that swap, depending on what is requested.
        ///</summary>
        /// <remarks>
        /// The assumption is that only one curve is required for valuation, as the floating leg always 
        /// values to zero when including principal exchanges.
        /// </remarks>
        ///<param name="fixedRate">The fixed rate.</param>
        ///<param name="fixedNotionalsAsArray">The fixed notionals.</param>
        ///<param name="floatingSpread">The spread on the floating leg.</param>
        ///<param name="metricsAsArray">The metrics.</param>
        ///<param name="fixedRollDatesAsArray">The fixed dates.</param>
        ///<param name="floatingRollDatesAsArray">The floating dates.</param>
        ///<param name="floatingNotionalsAsArray">The floating leg notionals.</param>
        ///<param name="floatingResetRatesAsArray">The floating rest rates array.</param>
        ///<param name="floatingLegDayCount">The floating leg daycount.</param>
        ///<param name="propertiesRange">The properties associated with the asset.</param>
        ///<returns>The metrics requested.</returns>
        public object[,] GetIRSwapMetrics( Excel.Range fixedRollDatesAsArray, Excel.Range fixedNotionalsAsArray,
            Decimal fixedRate, Excel.Range floatingRollDatesAsArray, Excel.Range floatingNotionalsAsArray, 
            Excel.Range floatingResetRatesAsArray, String floatingLegDayCount, Decimal floatingSpread, 
            Excel.Range metricsAsArray, Excel.Range propertiesRange)
        {
            if (propertiesRange == null) throw new ArgumentNullException(nameof(propertiesRange));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var baseDate = namedValueSet.GetValue<DateTime>("BaseDate", true);
            var businessDayConvention = namedValueSet.GetString("BusinessDayConvention", true);
            var businessCentersAsString = namedValueSet.GetString("BusinessCentersAsString", true);
            //Map the ranges.
            var rollDates = DataRangeHelper.StripDateTimeRange(fixedRollDatesAsArray);
            var weights = DataRangeHelper.StripDecimalRange(fixedNotionalsAsArray);
            var floatingDates = DataRangeHelper.StripDateTimeRange(floatingRollDatesAsArray);
            var floatingweights = DataRangeHelper.StripDecimalRange(floatingNotionalsAsArray);
            var floatingResets = DataRangeHelper.StripDecimalRange(floatingResetRatesAsArray);
            var riskMetrics = DataRangeHelper.StripRange(metricsAsArray);
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            var spreadquote = BasicQuotationHelper.Create(floatingSpread, "Spread", "DecimalRate");
            bav.quote = new[] { quote, spreadquote };
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention,
                                                                             businessCentersAsString);
            //create the swap.
            var swap = PriceableAssetFactory.CreateIRSwap(rollDates,
                                                                    weights,
                                                                    floatingDates,
                                                                    floatingweights,
                                                                    floatingResets,
                                                                    businessDayAdjustments, bav, namedValueSet);
            //Get the AssetControllerData.
            var market = new SimpleRateMarketEnvironment();
            var bavMetric = new BasicAssetValuation
                                {
                                    quote =
                                        riskMetrics.Select(metric => BasicQuotationHelper.Create(0.0m, metric)).ToArray()
                                };
            //Get the curve.
            var curveId = namedValueSet.GetString("DiscountCurve", true);
            var forecastCurveId = namedValueSet.GetString("ForecastCurve", false);
            var ratecurve = (IRateCurve)Engine.GetCurve(curveId, false);
            market.AddPricingStructure("DiscountCurve", ratecurve);
            if (forecastCurveId != null)
            {
                market.AddPricingStructure("DiscountCurve", ratecurve);
            }
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = swap.Calculate(assetControllerData);
            var result = valuation.quote.ToDictionary(val => val.measureType.Value, val => val.value);
            object[,] results = ArrayHelper.ConvertDictionaryTo2DArray(result);
            return results;
        }

        ///<summary>
        /// This function is a simple was to create a cap that has sequential roll dates and is adjusted, 
        /// This function does however allow different conventions on the floating side and a non-zero spread.
        /// The function will return a series of metrics for that swap, depending on what is requested.
        ///</summary>
        /// <remarks>
        /// The assumption is that only one curve is required for valuation, as the floating leg always 
        /// values to zero when including principal exchanges.
        /// </remarks>
        ///<param name="rollBackward">If tru, roll back from the maturity date. Otherwise roll forward from the effective date.</param>
        ///<param name="floatingResetRatesAsArray">An array of last Reset rates. This may be null.</param>
        ///<param name="includeStubFlag">Include the first reset flag.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="resetPeriod">The reset period e.g. -2D.</param>
        ///<param name="resetBusinessCentersAsString">The rest business days.</param>
        ///<param name="effectiveDate">The roll dates with the maturity date included..</param>
        ///<param name="resetBusinessDayConvention">The reset business day convention.</param>
        ///<param name="propertiesRange">The properties associated with the asset.</param>
        ///<returns>The value of the metric requested.</returns>
        public String CreateIRSwap(DateTime effectiveDate,
            Double notional, Excel.Range floatingResetRatesAsArray,
            Boolean includeStubFlag, Boolean rollBackward,
            string resetBusinessDayConvention, string resetBusinessCentersAsString,
            string resetPeriod, Excel.Range propertiesRange)
        {
            if (propertiesRange == null) throw new ArgumentNullException(nameof(propertiesRange));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var fixedRate = namedValueSet.GetValue<decimal>("Rate", true);
            var floatingSpread = namedValueSet.GetValue<decimal>("Spread", true);
            var businessDayConvention = namedValueSet.GetString("BusinessDayConvention", true);
            var businessCentersAsString = namedValueSet.GetString("BusinessCentersAsString", true);
            var floatingResets = DataRangeHelper.StripDecimalRange(floatingResetRatesAsArray);
            //Create the quotations
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            var spreadQuote = BasicQuotationHelper.Create(floatingSpread, "Spread", "DecimalRate");
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention,
                businessCentersAsString);
            //Create the RelativeDateOffset.
            var resetOffset = RelativeDateOffsetHelper.Create(resetPeriod, DayTypeEnum.Business,
                                                              resetBusinessDayConvention, resetBusinessCentersAsString, "StartDate");
            //create the swap.
            var irSwap = PriceableAssetFactory.CreateIRSwap(Engine.Logger, Engine.Cache, NameSpace, effectiveDate, notional,
                businessDayAdjustments, resetOffset, floatingResets, null, null, quote, spreadQuote, namedValueSet);
            var id = Engine.SetIRSwap(irSwap, namedValueSet);
            return id;
        }

        ///<summary>
        /// This function is a simple was to create a cap that has sequential roll dates and is adjusted, 
        /// This function does however allow different conventions on the floating side and a non-zero spread.
        /// The function will return a series of metrics for that swap, depending on what is requested.
        ///</summary>
        /// <remarks>
        /// The assumption is that only one curve is required for valuation, as the floating leg always 
        /// values to zero when including principal exchanges.
        /// </remarks>
        ///<param name="rollBackward">If tru, roll back from the maturity date. Otherwise roll forward from the effective date.</param>
        ///<param name="paymentBusinessDayConvention">The payment businessDayConvention.</param>
        ///<param name="floatingResetRatesAsArray">An array of last Reset rates. This may be null.</param>
        ///<param name="includeStubFlag">Include the first reset flag.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="resetPeriod">The reset period e.g. -2D.</param>
        ///<param name="paymentBusinessCentersAsString">The payment business centers.</param>
        ///<param name="resetBusinessCentersAsString">The rest business days.</param>
        ///<param name="metricsAsArray">The metrics as an array.</param>
        ///<param name="effectiveDate">The roll dates with the maturity date included..</param>
        ///<param name="resetBusinessDayConvention">The reset business day convention.</param>
        /// <param name="propertiesRange">The properties associated with the asset.</param>
        ///<returns>The value of the metric requested.</returns>
        public object[,] GetIRSwapMetrics2(DateTime effectiveDate, double notional, Excel.Range floatingResetRatesAsArray,
            bool includeStubFlag, bool rollBackward, string paymentBusinessDayConvention,
            string paymentBusinessCentersAsString, string resetBusinessDayConvention, string resetBusinessCentersAsString,
            string resetPeriod, Excel.Range metricsAsArray, Excel.Range propertiesRange)
        {
            if (paymentBusinessDayConvention == null) throw new ArgumentNullException(nameof(paymentBusinessDayConvention));
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var baseDate = namedValueSet.GetValue<DateTime>("BaseDate", true);
            var result = new Dictionary<string, decimal>();
            var resets = DataRangeHelper.StripDecimalRange(floatingResetRatesAsArray);
            var metrics = DataRangeHelper.StripRange(metricsAsArray);
            //Create the quotations
            var fixedRate = namedValueSet.GetValue<decimal>("Rate", true);
            var floatingSpread = namedValueSet.GetValue<decimal>("Spread", true);
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            var spreadQuote = BasicQuotationHelper.Create(floatingSpread, "Spread", "DecimalRate");
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(paymentBusinessDayConvention,
                                                                             paymentBusinessCentersAsString);
            //Create the RelativeDateOffset.
            var resetOffset = RelativeDateOffsetHelper.Create(resetPeriod, DayTypeEnum.Business,
                                                              resetBusinessDayConvention, resetBusinessCentersAsString, "StartDate");
            //create the swap.
            var irSwap = PriceableAssetFactory.CreateIRSwap(Engine.Logger, Engine.Cache, NameSpace, effectiveDate, notional,
                businessDayAdjustments, resetOffset, resets, null, null, quote, spreadQuote, namedValueSet);
            //Get the AssetControllerData.
            var market = new SwapLegEnvironment();
            var bavMetric = new BasicAssetValuation
                                {
                                    quote =
                                        metrics.Select(metric => BasicQuotationHelper.Create(0.0m, metric)).ToArray
                                        ()
                                };
            //Set the quote metrics.
            //Get the curve.
            var curveId = namedValueSet.GetString("DiscountCurve", true);
            var forecastCurveId = namedValueSet.GetString("ForecastCurve", false);
            var rateCurve = (IRateCurve)Engine.GetCurve(curveId, false);
            market.AddPricingStructure("DiscountCurve", rateCurve);
            if (forecastCurveId != null)
            {
                market.AddPricingStructure("DiscountCurve", rateCurve);
            }
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), rateCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), (IRateCurve)rateCurve.Clone());
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = irSwap.Calculate(assetControllerData);
            if (valuation == null) return ArrayHelper.ConvertDictionaryTo2DArray(result);
            //Return a scalar sum if details are not required.
            result = MetricsHelper.AggregateMetrics(valuation, metrics);
            return ArrayHelper.ConvertDictionaryTo2DArray(result);
        }

        /// <summary>
        ///  This function is a simple was to create a swap that has sequential roll dates and is adjusted, 
        ///  but does allow different conventions on the floating side and a non-zero spread.
        /// </summary>
        ///  <remarks>
        ///  The assumption is that only one curve is required for valuation, as the floating leg always 
        ///  values to zero when including principal exchanges.
        ///  The function will return the partial differential hedge for that swap. This will be a spectrum based 
        ///  perturbation rather than a cumulative based interpolation. Fort this reason, the sum of the partials 
        ///  will not equal a parallel shift delta.
        ///  </remarks>
        /// <param name="fixedNotionalsAsArray">The fixed notional weights.</param>
        /// <param name="fixedRollDatesAsArray">The fixed dates.</param>
        /// <param name="floatingResetRatesAsArray">The floating rest rates array.</param>
        /// <param name="floatingRollDatesAsArray">The floating dates.</param>
        /// <param name="floatingNotionalsAsArray">The floating leg notional weights.</param>
        /// <param name="propertiesRange">The properties Range: currency, baseDate and isDiscounted.</param>
        /// <returns>The VERTICAL ARRAY OF PARTIAL DERIVATIVES, WITH RESPECT TO EACH ASSET IN THE REFERENCED CURVE.</returns>
        public object[,] GetIRSwapPDH(Excel.Range fixedRollDatesAsArray, 
            Excel.Range fixedNotionalsAsArray, Excel.Range floatingRollDatesAsArray, 
            Excel.Range floatingNotionalsAsArray, Excel.Range floatingResetRatesAsArray, 
            Excel.Range propertiesRange)
        {
            var properties = propertiesRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var businessDayConvention = namedValueSet.GetString("BusinessDayConvention", true);
            var businessCentersAsString = namedValueSet.GetString("BusinessCentersAsString", true);
            var curveToPerturb = namedValueSet.GetString("CurveToPerturb", true);
            var forecastCurveId = namedValueSet.GetString("ForecastCurve", true);
            var discountCurveId = namedValueSet.GetString("DiscountCurve", true);
            var fixedRate = namedValueSet.GetValue<decimal>("Rate", true);
            var floatingSpread = namedValueSet.GetValue<decimal>("Spread", true);
            var curvePerturbation = EnumHelper.Parse<CurvePerturbation>(curveToPerturb);
            //Map the ranges.
            var rollDates = DataRangeHelper.StripDateTimeRange(fixedRollDatesAsArray);
            var weights = DataRangeHelper.StripDecimalRange(fixedNotionalsAsArray);
            var floatingDates = DataRangeHelper.StripDateTimeRange(floatingRollDatesAsArray);
            var floatingWeights = DataRangeHelper.StripDecimalRange(floatingNotionalsAsArray);
            var floatingResets = DataRangeHelper.StripDecimalRange(floatingResetRatesAsArray);
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            var spreadQuote = BasicQuotationHelper.Create(floatingSpread, "Spread", "DecimalRate");
            bav.quote = new[] { quote, spreadQuote };
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCentersAsString);
            //create the swap.
            var swap = PriceableAssetFactory.CreateIRSwap(rollDates,
                                                                    weights,
                                                                    floatingDates,
                                                                    floatingWeights,
                                                                    floatingResets,
                                                                    businessDayAdjustments, bav, namedValueSet);
            //Get the curve.
            var forecastcurve = (CurveBase)Engine.GetCurve(forecastCurveId, false);
            var discountcurve = (CurveBase)Engine.GetCurve(discountCurveId, false);
            var valuation = swap.CalculatePDH(discountcurve, forecastcurve, curvePerturbation);
            //This requires an IPricingStructure to be cached.
            //Create the interpolator and get the pdh.          
            object[,] results = ArrayHelper.ConvertDictionaryTo2DArray(valuation);
            return results;
        }

        ///<summary>
        /// Calculates the implied quote for a swap.
        ///</summary>
        ///<param name="curveId">The curve id. This must have been created already.</param>
        ///<param name="currency">The configuration currency.
        /// This is used to get all default values and must exist in the cache.</param>
        ///<param name="baseDate">The base date.</param>
        ///<param name="spotDate">The spot date.</param>
        ///<param name="fixedLegDayCount">The daycount.</param>
        ///<param name="term">The term of the swap.</param>
        ///<param name="paymentFrequency">The payment frequency.</param>
        ///<param name="rollConvention">The roll convention. This can include EOM, but is normally the roll day.</param>
        ///<returns></returns>
        public decimal GetSwapImpliedQuote(string curveId, string currency, DateTime baseDate, DateTime spotDate, string fixedLegDayCount,
            string term, string paymentFrequency, string rollConvention)
        {
            var impliedQuote = Engine.GetSwapImpliedQuote(curveId, currency, baseDate, spotDate, fixedLegDayCount, term, paymentFrequency, rollConvention);
            return impliedQuote;
        }


        /// <summary>
        /// Creates the lease asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="grossAmount">The gross amount.</param>
        /// <param name="stepUp">The step up.</param>
        /// <returns></returns>
        public string CreateSimpleLease(string assetIdentifier, DateTime baseDate, double grossAmount, double stepUp)
        {
            NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForLeaseAssets(NameSpace, assetIdentifier, baseDate, grossAmount, stepUp);
            return Engine.CreateLocalAsset(Convert.ToDecimal(grossAmount), 0.0m, namedValueSet);
        }

        /// <summary>
        /// Creates the lease asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="grossAmount">The gross amount.</param>
        /// <param name="stepUp">The step up.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string CreateSimpleLeaseWithProperties(string assetIdentifier, DateTime baseDate, double grossAmount, double stepUp, object[,] properties)
        {
            NamedValueSet nvs = PriceableAssetFactory.BuildPropertiesForLeaseAssets(NameSpace, assetIdentifier, baseDate, grossAmount, stepUp);
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            var namedValueSet = new NamedValueSet(properties);
            namedValueSet = CurveHelper.CombinePropertySetsClone(namedValueSet, nvs);
            return Engine.CreateLocalAsset(Convert.ToDecimal(grossAmount), 0.0m, namedValueSet);
        }

        /// <summary>
        /// Creates the assets.
        /// </summary>
        /// <param name="assetIdentifiersAsArray">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="grossAmountsAsArray">The gross amounts.</param>
        /// <param name="stepUpsAsArray">The coupons.</param>
        /// <returns></returns>
        public object[,] CreateSimpleLeases(Excel.Range assetIdentifiersAsArray, DateTime baseDate, Excel.Range grossAmountsAsArray,
            Excel.Range stepUpsAsArray)
        {
            //Map the ranges.
            var amounts = DataRangeHelper.StripDoubleRange(grossAmountsAsArray);
            var stepUps = DataRangeHelper.StripDoubleRange(stepUpsAsArray);
            var assetIdentifiers = DataRangeHelper.StripRange(assetIdentifiersAsArray);
            var rateAssets = Engine.CreateLocalLeases(assetIdentifiers, baseDate, amounts, stepUps);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates the bond asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="coupon">The coupon.</param>
        /// <param name="ytm">The yield to maturity.</param>
        /// <returns></returns>
        public string CreateSimpleBond(string assetIdentifier, DateTime baseDate, DateTime maturityDate, decimal coupon, decimal ytm)
        {
            NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForBondAssets(NameSpace, assetIdentifier, baseDate, coupon, maturityDate);
            return Engine.CreateLocalAsset(ytm, 0.0m, namedValueSet);
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="coupon">The coupon.</param>
        /// <param name="ytm">The yield to maturity.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string CreateSimpleBondWithProperties(string assetIdentifier, DateTime baseDate, DateTime maturityDate, decimal coupon, decimal ytm, object[,] properties)
        {
            NamedValueSet nvs = PriceableAssetFactory.BuildPropertiesForBondAssets(NameSpace, assetIdentifier, baseDate, coupon, maturityDate);
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            var namedValueSet = new NamedValueSet(properties);
            namedValueSet = CurveHelper.CombinePropertySetsClone(namedValueSet, nvs);
            return Engine.CreateLocalAsset(ytm, 0.0m, namedValueSet);
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifiersAsArray">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="maturityDatesAsArray">The maturity dates.</param>
        /// <param name="couponsAsArray">The coupons.</param>
        /// <param name="ytmsAsArray">The yield to maturities.</param>
        /// <returns></returns>
        public object[,] CreateSimpleBonds(Excel.Range assetIdentifiersAsArray, DateTime baseDate, Excel.Range maturityDatesAsArray,
                                                  Excel.Range couponsAsArray, Excel.Range ytmsAsArray)
        {
            //Map the ranges.
            var maturityDates = DataRangeHelper.StripDateTimeRange(maturityDatesAsArray);
            var coupons = DataRangeHelper.StripDecimalRange(couponsAsArray);
            var ytms = DataRangeHelper.StripDecimalRange(ytmsAsArray);
            var assetIdentifiers = DataRangeHelper.StripRange(assetIdentifiersAsArray);
            var rateAssets = Engine.CreateLocalBonds(assetIdentifiers, baseDate, maturityDates, coupons, ytms);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifiersAsArray">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="maturityDatesAsArray">The maturity dates.</param>
        /// <param name="couponsAsArray">The coupons.</param>
        /// <param name="ytmsAsArray">The yield to maturities.</param>
        /// <param name="properties2DRange">The properties.</param>
        /// <returns></returns>
        public object[,] CreateSimpleBondsWithProperties(Excel.Range assetIdentifiersAsArray, DateTime baseDate, Excel.Range maturityDatesAsArray,
                                                  Excel.Range couponsAsArray, Excel.Range ytmsAsArray, Excel.Range properties2DRange)
        {
            //Map the ranges.
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            //var props = properties.ToNamedValueSet();//BaseFxCurve
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var maturityDates = DataRangeHelper.StripDateTimeRange(maturityDatesAsArray);
            var coupons = DataRangeHelper.StripDecimalRange(couponsAsArray);
            var ytms = DataRangeHelper.StripDecimalRange(ytmsAsArray);
            var assetIdentifiers = DataRangeHelper.StripRange(assetIdentifiersAsArray);
            var rateAssets = Engine.CreateLocalBonds(assetIdentifiers, baseDate, maturityDates, coupons, ytms, namedValueSet);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="coupon">The coupon.</param>
        /// <param name="frequency">The coupon frequency.</param>
        /// <param name="ytm">The yield to maturity.</param>
        /// <param name="properties2DRange">The properties.</param>
        /// <param name="daycount">The daycount convention.</param>
        /// <returns></returns>
        public string CreateBondWithDetail(string assetIdentifier, DateTime baseDate, DateTime maturityDate,
                                                  Decimal coupon, String daycount, String frequency, Decimal ytm, Excel.Range properties2DRange)
        {
            if (properties2DRange == null) throw new ArgumentNullException(nameof(properties2DRange));
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = Engine.CreateLocalBond(assetIdentifier, baseDate, maturityDate, coupon, daycount, frequency, ytm, namedValueSet);
            return result;
        }

        /// <summary>
        /// Creates a bond in the data store.
        /// </summary>
        /// <param name="instrumentId">The instrument identifier.</param>
        /// <param name="clearanceSystem">A valid clearance system.</param>
        /// <param name="couponType">A valid coupon type: Fixed, Float or Struct as defined by the CouponTypeEnum.</param>
        /// <param name="couponRate">The coupon rate as a number i.e. 5.25. If the bond is a floater, then the rate is the spread.</param>
        /// <param name="currency">A valid currency.</param>
        /// <param name="faceAmount">The face amount. Normally 100.</param>
        /// <param name="maturityDate">The maturity date. Formatted if possible as MM/DD/YY</param>
        /// <param name="paymentFrequency">A valid payment frequency.</param>
        /// <param name="dayCountFraction">Defined by the DayCountFractionEnum types.</param>
        /// <param name="creditSeniority">The credit seniority. Of the form: CreditSeniorityEnum</param>
        /// <param name="description">The issuer description. e.g. BHP 5.3 12/23/18</param>
        /// <param name="exchangeId">The exchange identifier. These are sorted by reference data.</param>
        /// <param name="issuerName">The issuer name, but as a ticker.</param>
        /// <param name="propertyHeaders">An array property headers. This includes:
        /// BondTypeEnum
        /// MarketSectorEnum
        /// </param>
        /// <param name="propertyValues">An array of property values. All strings. </param>
        /// <returns></returns>
        public string CreateBond(string instrumentId, string clearanceSystem, string couponType,
            decimal couponRate, string currency, double faceAmount, DateTime maturityDate,
            string paymentFrequency, string dayCountFraction, string creditSeniority, string description, 
            string exchangeId, string issuerName, Excel.Range propertyHeaders, Excel.Range propertyValues)
        {
            var headers = DataRangeHelper.StripRange(propertyHeaders);
            var values = DataRangeHelper.StripRange(propertyValues);
            if (headers.Count != values.Count) return "The arrays are not of equal length!";
            var properties = new NamedValueSet();
            var index = 0;
            foreach(var header in headers)
            {
                properties.Set(header, values[index]);
                index++;
            }
            var result = ValService.CreateBond(instrumentId, clearanceSystem, couponType, couponRate, currency,
                                               faceAmount, maturityDate, paymentFrequency, dayCountFraction,
                                               creditSeniority, description, exchangeId, issuerName, properties);
            return result;
        }

        /// <summary>
        /// Retrieves and saves bonds to a directory.
        /// </summary>
        /// <param name="identifierArray">The identifier array to retrieve.</param>
        /// <param name="directoryPath">The path to save the files into.</param>
        /// <param name="isShortName">If <true>isShortName</true> is true> then the id is of the form: FixedIncome.XXXX.YY.zz-zz-zzzz.
        /// Otherwise it is of the form Orion.ReferenceData.FixedIncome.XXXX.YY.zz-zz-zzzz.</param>
        /// <returns></returns>
        public string SaveBonds(Excel.Range identifierArray, string directoryPath, bool isShortName)
        {
            var identifiers = DataRangeHelper.StripRange(identifierArray);
            var result = ValService.SaveBonds(identifiers, directoryPath, isShortName);
            return result;
        }

        /// <summary>
        /// Creates the bond assets.
        /// </summary>
        /// <param name="assetIdentifiersAsArray">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="maturityDatesAsArray">The maturity dates.</param>
        /// <param name="couponsAsArray">The coupons.</param>
        /// <param name="frequenciesAsArray">The coupon frequencies.</param>
        /// <param name="daycountsAsArray">The daycount conventions.</param>
        /// <param name="ytmsAsArray">The yield to maturities.</param>
        /// <param name="properties2DRange">The properties.</param>
        /// <returns></returns>
        public object[,] CreateBondsWithDetails(Excel.Range assetIdentifiersAsArray, DateTime baseDate, Excel.Range maturityDatesAsArray,
        Excel.Range couponsAsArray, Excel.Range daycountsAsArray, Excel.Range frequenciesAsArray, Excel.Range ytmsAsArray, Excel.Range properties2DRange)
        {
            //Map the ranges.
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var maturityDates = DataRangeHelper.StripDateTimeRange(maturityDatesAsArray);
            var coupons = DataRangeHelper.StripDecimalRange(couponsAsArray);
            var ytms = DataRangeHelper.StripDecimalRange(ytmsAsArray);
            var assetIdentifiers = DataRangeHelper.StripRange(assetIdentifiersAsArray);
            var frequencys = DataRangeHelper.StripRange(frequenciesAsArray);
            var daycounts = DataRangeHelper.StripRange(daycountsAsArray);
            var rateAssets = Engine.CreateLocalBonds(assetIdentifiers, baseDate, maturityDates, coupons, daycounts, frequencys, ytms, namedValueSet);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates a equity in the data store.
        /// </summary>
        /// <param name="instrumentId">The instrument identifier.</param>
        /// <param name="clearanceSystem">A valid clearance system.</param>
        /// <param name="currency">A valid currency.</param>
        /// <param name="assetId">The asset identifier. e.g. BHP 5.3 12/23/18</param>
        /// <param name="description">The issuer description.</param>
        /// <param name="exchangeId">The exchange identifier. These are sorted by reference data.</param>
        /// <param name="issuerName">The issuer name, but as a ticker.</param>
        /// <param name="propertyHeaders">An array property headers. This includes:
        /// BondTypeEnum
        /// MarketSectorEnum
        /// </param>
        /// <param name="propertyValues">An array of property values. All strings. </param>
        /// <returns></returns>
        public string CreateEquity(string instrumentId, string clearanceSystem, string currency, string assetId,
            string description, string exchangeId, string issuerName, Excel.Range propertyHeaders, Excel.Range propertyValues)
        {
            var headers = DataRangeHelper.StripRange(propertyHeaders);
            var values = DataRangeHelper.StripRange(propertyValues);
            if (headers.Count != values.Count) return "The arrays are not of equal length!";
            var properties = new NamedValueSet();
            var index = 0;
            foreach (var header in headers)
            {
                properties.Set(header, values[index]);
                index++;
            }
            var result = ValService.CreateEquity(instrumentId, clearanceSystem,  currency, assetId, description, exchangeId, issuerName, properties);
            return result;
        }

        /// <summary>
        /// Creates a property in the data store.
        /// </summary>
        /// <param name="propertyId">The property identifier.</param>
        /// <param name="streetIdentifier">A street Identifier.</param>
        /// <param name="streetName">A street Name.</param>
        /// <param name="suburb">The suburb</param>
        /// <param name="propertyType">THe property type: residential, commercial, investment etc</param>
        /// <param name="numParking">The number of car parking spots.</param>
        /// <param name="currency">THe currency.</param>
        /// <param name="description">The issuer description.</param>
        /// <param name="propertyHeaders">An array property headers.</param>
        /// <param name="propertyValues">An array of property values. All strings. </param>
        /// <param name="city">The city</param>
        /// <param name="postalCode">The postal code. This could be a number or a string.</param>
        /// <param name="state">The state</param>
        /// <param name="country">The country</param>
        /// <param name="numBedrooms">The number of bedrooms.</param>
        /// <param name="numBathrooms">The number of bathrooms</param>
        /// <returns></returns>
        public string CreateProperty(string propertyId, string propertyType, string streetIdentifier, string streetName, 
            string suburb, string city, string postalCode, string state, string country, string numBedrooms, string numBathrooms, string numParking,
            string currency, string description, Excel.Range propertyHeaders, Excel.Range propertyValues)
        {
            var headers = DataRangeHelper.StripRange(propertyHeaders);
            var values = DataRangeHelper.StripRange(propertyValues);
            if (headers.Count != values.Count) return "The arrays are not of equal length!";
            var properties = new NamedValueSet();
            var index = 0;
            foreach (var header in headers)
            {
                properties.Set(header, values[index]);
                index++;
            }
            var property = EnumHelper.Parse<PropertyType>(propertyType);
            var result = ValService.CreatePropertyAsset(propertyId, property, streetIdentifier, streetName, suburb, city, postalCode, state, country,
                numBedrooms, numBathrooms, numParking, currency, description, properties);
            return result;
        }

        /// <summary>
        /// Creates the surface volatility asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="volatility">The volatility.</param>
        /// <param name="properties2DRange">The properties.</param>
        /// <returns></returns>
        public string CreateSurfaceAsset(string assetIdentifier, DateTime baseDate, decimal strike,
                                                decimal volatility, Excel.Range properties2DRange)
        {
            //Map the ranges.
            var properties = properties2DRange.Value[System.Reflection.Missing.Value] as object[,];
            //var props = properties.ToNamedValueSet();//BaseFxCurve
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = Engine.CreateLocalSurfaceAsset(assetIdentifier, baseDate, volatility, strike, namedValueSet);
            return result;
        }

        /// <summary>
        /// Creates the surface volatility asset.
        /// </summary>
        /// <param name="assetIdentifiersAsArray">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="strikesAsArray">The strikes.</param>
        /// <param name="volatilityRange">The volatility matrix.</param>
        /// <param name="propertiesAsRange">The properties.</param>
        /// <returns></returns>
        public object[,] CreateSurfaceAssets(Excel.Range assetIdentifiersAsArray, DateTime baseDate, Excel.Range strikesAsArray,
                                                    Excel.Range volatilityRange, Excel.Range propertiesAsRange)
        {
            if (propertiesAsRange == null) throw new ArgumentNullException(nameof(propertiesAsRange));
            var properties = propertiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var strikes = DataRangeHelper.StripDecimalRange(strikesAsArray);
            var assetIdentifiers = DataRangeHelper.StripRange(assetIdentifiersAsArray);
            var volatility = volatilityRange.Value[System.Reflection.Missing.Value] as object[,];
            var result = Engine.CreateLocalSurfaceAssets(assetIdentifiers, baseDate, DataRangeHelper.RangeToMatrix<decimal>(volatility), strikes, namedValueSet);
            return ArrayHelper.RangeToMatrix<object>(result);
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="adjustedRate">The adjusted rates.</param>
        /// <param name="additionalValue">The additional values.</param>
        /// <returns></returns>
        public string CreateAsset(string assetIdentifier, DateTime baseDate, double adjustedRate,
                                         double additionalValue)
        {
            NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForAssets(Engine.NameSpace, assetIdentifier, baseDate);
            var result = Engine.CreateLocalAsset(Convert.ToDecimal(adjustedRate), Convert.ToDecimal(additionalValue), namedValueSet);
            return result;
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="propertiesAsRange">The properties. Must include AssetId, BaseDate and AssetType.
        /// May contain Notional.</param>
        /// <param name="adjustedRate">The adjusted rates.</param>
        /// <param name="additionalValue">The additional values.</param>
        /// <returns></returns>
        public string CreateAssetWithProperties(double adjustedRate, double additionalValue,
                                         Excel.Range propertiesAsRange)
        {
            if (propertiesAsRange == null) throw new ArgumentNullException(nameof(propertiesAsRange));
            var properties = propertiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var result = Engine.CreateLocalAsset(Convert.ToDecimal(adjustedRate), Convert.ToDecimal(additionalValue), namedValueSet);
            return result;
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="valuesAsArray">The adjusted rates.</param>
        /// <param name="measureTypesAsArray">The measure types.</param>
        /// <param name="priceQuoteUnitsAsArray">The price quote units.</param>
        /// <returns></returns>
        public string CreateAssetWithUnits(string assetIdentifier, DateTime baseDate, Excel.Range valuesAsArray,
                                                  Excel.Range measureTypesAsArray, Excel.Range priceQuoteUnitsAsArray)
        {
            NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForAssets(NameSpace, assetIdentifier, baseDate);
            var values = DataRangeHelper.StripDecimalRange(valuesAsArray);
            var measureTypes = DataRangeHelper.StripRange(measureTypesAsArray);
            var priceQuoteUnits = DataRangeHelper.StripRange(priceQuoteUnitsAsArray);
            var result = Engine.CreateLocalAsset(values, measureTypes, priceQuoteUnits, namedValueSet);
            return result;
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="valuesAsArray">The adjusted rates.</param>
        /// <param name="propertiesAsRange">The properties. Must include AssetId, BaseDate and AssetType.</param>
        /// <param name="measureTypesAsArray">The measure types.</param>
        /// <param name="priceQuoteUnitsAsArray">The price quote units.</param>
        /// <returns></returns>
        public string CreateAssetWithUnitsAndProperties(Excel.Range valuesAsArray,
                                                  Excel.Range measureTypesAsArray, Excel.Range priceQuoteUnitsAsArray, Excel.Range propertiesAsRange)
        {
            if (propertiesAsRange == null) throw new ArgumentNullException(nameof(propertiesAsRange));
            var properties = propertiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var values = DataRangeHelper.StripDecimalRange(valuesAsArray);
            var measureTypes = DataRangeHelper.StripRange(measureTypesAsArray);
            var priceQuoteUnits = DataRangeHelper.StripRange(priceQuoteUnitsAsArray);
            var result = Engine.CreateLocalAsset(values, measureTypes, priceQuoteUnits, namedValueSet);
            return result;
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifiersAsArray">The asset identifier.</param>
        /// <param name="adjustedRatesAsArray">The adjusted rates.</param>
        /// <param name="propertiesAsRange">The properties.</param>
        /// <param name="measureTypesAsArray">The measure types.</param>
        /// <param name="priceQuoteUnitsAsArray">The price quote units.</param>
        /// <returns></returns>
        public object[,] CreateAssetsWithUnits(Excel.Range assetIdentifiersAsArray, Excel.Range adjustedRatesAsArray,
                                                      Excel.Range measureTypesAsArray, Excel.Range priceQuoteUnitsAsArray,
                                                      Excel.Range propertiesAsRange)
        {
            if (propertiesAsRange == null) throw new ArgumentNullException(nameof(propertiesAsRange));
            var properties = propertiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            var instrumentsArray = DataRangeHelper.StripRange(assetIdentifiersAsArray);
            var rates = DataRangeHelper.StripDecimalRange(adjustedRatesAsArray);
            var measureTypes = DataRangeHelper.StripRange(measureTypesAsArray);
            var priceQuoteUnits = DataRangeHelper.StripRange(priceQuoteUnitsAsArray);
            var rateAssets = Engine.CreateLocalAssets(instrumentsArray, rates, measureTypes, priceQuoteUnits, namedValueSet);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates the assets.
        /// </summary>
        /// <param name="assetIdentifiersAsArray">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="adjustedRatesAsArray">The adjusted rates.</param>
        /// <param name="additionalAsArray">The additional values.</param>
        /// <returns></returns>
        public object[,] CreateAssets(Excel.Range assetIdentifiersAsArray, DateTime baseDate, Excel.Range adjustedRatesAsArray,
                                             Excel.Range additionalAsArray)
        {
            var instrumentsArray = DataRangeHelper.StripRange(assetIdentifiersAsArray);
            var rates = DataRangeHelper.StripDecimalRange(adjustedRatesAsArray);
            var additionalData = DataRangeHelper.StripDecimalRange(additionalAsArray);
            var rateAssets = Engine.CreateLocalAssets(instrumentsArray, baseDate, rates, additionalData);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates the assets.
        /// </summary>
        /// <param name="assetIdentifiersAsArray">The asset identifier.</param>
        /// <param name="adjustedRatesAsArray">The adjusted rates.</param>
        /// <param name="additionalAsArray">The additional values.</param>
        /// <param name="propertiesAsRange">The properties.</param>
        /// <returns></returns>
        public object[,] CreateAssetsWithProperties(Excel.Range assetIdentifiersAsArray, Excel.Range adjustedRatesAsArray,
            Excel.Range additionalAsArray, Excel.Range propertiesAsRange)
        {
            if (propertiesAsRange == null) throw new ArgumentNullException(nameof(propertiesAsRange));
            var properties = propertiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();  
            var instrumentsArray = DataRangeHelper.StripRange(assetIdentifiersAsArray);
            var rates = DataRangeHelper.StripDecimalRange(adjustedRatesAsArray);
            var additionalData = DataRangeHelper.StripDecimalRange(additionalAsArray);
            var rateAssets = Engine.CreateLocalAssets(instrumentsArray, rates, additionalData, namedValueSet);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates the assets.
        /// </summary>
        /// <param name="assetIdentifiersAsArray">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="adjustedRatesAsArray">The adjusted rates.</param>
        /// <param name="additionalAsArray">The additional values.</param>
        /// <param name="propertiesAsRange">The properties.</param>
        /// <returns></returns>
        public object[,] CreateAssetsWithBaseDate(Excel.Range assetIdentifiersAsArray, DateTime baseDate, Excel.Range adjustedRatesAsArray, 
            Excel.Range additionalAsArray, Excel.Range propertiesAsRange)
        {
            if (propertiesAsRange == null) throw new ArgumentNullException(nameof(propertiesAsRange));
            var properties = propertiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            namedValueSet.Set(CurveProp.BaseDate, baseDate);
            var instrumentsArray = DataRangeHelper.StripRange(assetIdentifiersAsArray);
            var rates = DataRangeHelper.StripDecimalRange(adjustedRatesAsArray);
            var additionalData = DataRangeHelper.StripDecimalRange(additionalAsArray);
            var rateAssets = Engine.CreateLocalAssets(instrumentsArray, rates, additionalData, namedValueSet);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKey">The asset reference key.</param>
        /// <returns></returns>
        public decimal GetImpliedQuote(string curveId, string assetReferenceKey)
        {
            var asset = Engine.GetLocalAsset(assetReferenceKey);
            var ratecurve = (IRateCurve)Engine.GetCurve(curveId, false);
            return asset.CalculateImpliedQuote(ratecurve);
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetId">The asset id e.g. AUD-IRswap-3Y.</param>
        /// <param name="additional">Any additional data required e.g. Futures volatility.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="rate">The rate.</param>
        /// <returns>A decimal value - the break even rate.</returns>
        public decimal GetAssetImpliedQuote(string curveId, string assetId,
            decimal rate, decimal additional, DateTime baseDate)
        {
            //Create the properties.
            var properties = PriceableAssetFactory.BuildPropertiesForAssets(NameSpace, assetId, baseDate);
            //create the asset-basic asset valuation pair.
            var asset = AssetHelper.Parse(assetId, rate, additional);
            //create the priceable asset.
            var priceableAsset = Engine.CreatePriceableAsset(asset.Second, properties);
            var ratecurve = (IRateCurve)Engine.GetCurve(curveId, false);
            return priceableAsset.CalculateImpliedQuote(ratecurve);
        }

        /// <summary>
        /// Evaluates the metrics for a group of asset set.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKeysAsArray">The array of asset reference keys.</param>
        /// <returns>An array of implied quote values, one for each asset provided.</returns>
        public object GetImpliedQuotes(string curveId, Excel.Range assetReferenceKeysAsArray)
        {
            var assetKeys = DataRangeHelper.StripRange(assetReferenceKeysAsArray);
            var result = new Dictionary<string, decimal>();
            //Get the curve.
            var ratecurve = (IRateCurve)Engine.GetCurve(curveId, false);
            foreach (var assetReferenceKey in assetKeys)
            {
                var asset = Engine.GetLocalAsset(assetReferenceKey);
                result.Add(assetReferenceKey, asset.CalculateImpliedQuote(ratecurve));
            }
            return ArrayHelper.ConvertDictionaryTo2DArray(result);
        }


        /// <summary>
        /// Evaluates a metric of breakeven rates for a group of assets defined.
        /// </summary>
        /// <param name="effectiveDatesAsArray">An array of effective dates.</param>
        /// <param name="assetTenorsAsArray">The array of asset tenors.</param>
        /// <param name="propertiesAsRange">A property range. This must contain the mandatory properties:
        /// Currency, AssetType amd CurveIdentifier.</param>
        /// <returns>A matrix of implied quote values, one for each asset.</returns>
        public object[,] GetImpliedQuoteMatrix(Excel.Range effectiveDatesAsArray,
            Excel.Range assetTenorsAsArray, Excel.Range propertiesAsRange)
        {
            if (propertiesAsRange == null) throw new ArgumentNullException(nameof(propertiesAsRange));
            var properties = propertiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
            properties = (object[,])DataRangeHelper.TrimNulls(properties);
            var namedValueSet = properties.ToNamedValueSet();
            //Clean the inputs.
            var dates = DataRangeHelper.StripDateTimeRange(effectiveDatesAsArray);
            var tenors = DataRangeHelper.StripRange(assetTenorsAsArray);

            //Instantiate the result matrix.
            var result = new object[dates.Count, tenors.Count];

            //Get the mandatory data.
            var curveId = namedValueSet.GetValue<string>("CurveIdentifier");
            var currency = namedValueSet.GetValue<string>("Currency");
            var assetType = namedValueSet.GetValue<string>("AssetType");
            var ratecurve = (IRateCurve)Engine.GetCurve(curveId, false);
            for (var i = 0; i < dates.Count; i++)
            {
                for (var j = 0; j < tenors.Count; j++)
                {
                    var id = currency + "-" + assetType + "-" + tenors[j];
                    //Create the properties.
                    var assetProperties = PriceableAssetFactory.BuildPropertiesForAssets(NameSpace, id, dates[i]);
                    //create the asset-basic asset valuation pair.
                    var asset = AssetHelper.Parse(id, 0.05m, 0.0m);
                    //create the priceable asset.
                    var priceableAsset = Engine.CreatePriceableAsset(asset.Second, assetProperties);
                    result[i, j] = Convert.ToDouble(priceableAsset.CalculateImpliedQuote(ratecurve));
                }
            }
            return result;
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="metricsAsArray">The metrics.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKey">The asset reference key.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        public object[,] EvaluateMetrics(Excel.Range metricsAsArray, string curveId, string assetReferenceKey, DateTime referenceDate)
        {
            var metrics = DataRangeHelper.StripRange(metricsAsArray);
            metrics.RemoveAll(item => item == null);
            var assetReferenceKeys = new List<string> { assetReferenceKey };
            var valuations = Engine.EvaluateMetricsForAssetSet(metrics, curveId, assetReferenceKeys, referenceDate);
            return MetricsHelper.BuildEvaluationResults(assetReferenceKey, valuations.assetQuote);
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="metricsAsArray">The metrics.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKeysAsArray">The asset reference keys.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        public object[,] EvaluateMetricsForAssetSet(Excel.Range metricsAsArray, string curveId, Excel.Range assetReferenceKeysAsArray, DateTime referenceDate)
        {
            var metrics = DataRangeHelper.StripRange(metricsAsArray);
            metrics.RemoveAll(item => item == null);
            var assetReferenceKeys = DataRangeHelper.StripRange(assetReferenceKeysAsArray);
            var valuations = Engine.EvaluateMetricsForAssetSet(metrics, curveId, assetReferenceKeys, referenceDate);
            return MetricsHelper.BuildEvaluationResults(assetReferenceKeys, valuations.assetQuote);
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="metricsAsArray">The metrics.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKeysAsArray">The asset reference keys.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        public object[,] EvaluateMetricsRawData(Excel.Range metricsAsArray, string curveId,
                                                    Excel.Range assetReferenceKeysAsArray, DateTime referenceDate)
        {
            var metrics = DataRangeHelper.StripRange(metricsAsArray);
            metrics.RemoveAll(item => item == null);
            var assetReferenceKeys = DataRangeHelper.StripRange(assetReferenceKeysAsArray);
            var valuations = Engine.EvaluateMetricsForAssetSet(metrics, curveId, assetReferenceKeys, referenceDate);
            return MetricsHelper.BuildEvaluationResultsClean(metrics, assetReferenceKeys, valuations.assetQuote, assetReferenceKeys.Count);
        }

        #endregion

        #region Exchange Data

        /// <summary>
        /// Loads exchange config to the core.
        /// </summary>
        /// <param name="exchangeData">The exchange data to load.
        /// This must be consistent with the Exchange class.
        /// ID ;
        /// COUNTRY ;
        /// ISO COUNTRY CODE (ISO 3166) ;
        /// MIC ;
        /// OPERATING MIC ;
        /// O/S ;
        /// NAME-INSTITUTION DESCRIPTION ;
        /// ACRONYM ;
        /// CITY ;
        /// WEBSITE ;
        /// STATUS DATE ;
        /// STATUS ;
        /// CREATION DATE ;
        ///</param>
        /// <param name="exchangeSettlementData">THe settlement data attached to each exchange. 
        /// There should be at least 4 columns: period; daytype; businessdayconvention; businessCentre. RelativeTo is optional</param>
        /// <returns></returns>
        public string LoadExchangeConfigData(Excel.Range exchangeData, Excel.Range exchangeSettlementData)
        {
            var data = DataRangeHelper.ToMatrix<string>(exchangeData);
            var settlement = DataRangeHelper.ToMatrix<string>(exchangeSettlementData);
            return ValService.LoadExchangeConfigData(data, settlement);
        }

        /// <summary>
        /// Views exchange config to the core.
        /// </summary>
        /// <param name="exchangeId">The exchange data to load.
        /// This must be consistent with the Exchange class</param>
        /// <returns></returns>
        public object[,] ViewExchangeConfigData(string exchangeId)
        {
            return ValService.ViewExchangeConfigData(exchangeId);
        }

        #endregion
    }
}