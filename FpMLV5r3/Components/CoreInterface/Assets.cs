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
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
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

#endregion

namespace Highlander.Core.Interface.V5r3
{
    /// <summary>
    /// Creates and manages all pricing functionality.
    /// </summary>
    public partial class PricingCache
    {
        #region Fields

        public CalendarEngine.V5r3.CalendarEngine CalendarService;
        public CurveEngine.V5r3.CurveEngine Engine;
        public ValuationService ValService;
        public readonly RuntimeEnvironment Environment;
        public string NameSpace;

        #endregion

        #region Constructor

        public PricingCache() : this(EnvironmentProp.DefaultNameSpace, true)
        {}

        public PricingCache(string nameSpace, bool loadData)
        {
            NameSpace = nameSpace ?? EnvironmentProp.DefaultNameSpace;
            //This environment now loads default data
            //TODO make the configuration data specific to the namespace!
            Environment = new RuntimeEnvironment(NameSpace, loadData);
            Engine = new CurveEngine.V5r3.CurveEngine(Environment.LogRef.Target, Environment.Cache, NameSpace);
            ValService = new ValuationService(Environment.LogRef.Target, Environment.Cache, NameSpace);
            CalendarService = new CalendarEngine.V5r3.CalendarEngine(Environment.LogRef.Target, Environment.Cache, NameSpace);
        }

        public PricingCache(ILogger logger, ICoreCache cache)
            : this(logger, cache, EnvironmentProp.DefaultNameSpace)
        {}

        public PricingCache(ILogger logger, ICoreCache cache, string nameSpace)
        {
            Engine = new CurveEngine.V5r3.CurveEngine(logger, cache, nameSpace);
            ValService = new ValuationService(logger, cache, nameSpace);
            NameSpace = nameSpace;
        }

        #endregion

        #region Information Functions

        public string CurrentNameSpace()
        {
            return Engine.NameSpace;
        }

        /// <summary>
        /// Displays the properties.
        /// </summary>
        public List<Pair<string, string>> DisplayPropertyData(string name)
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
            return nvs.ToArray().Select(element => new Pair<string, string>(element.Name, element.ValueString)).ToList();
        }

        /// <summary>
        /// Displays the properties.
        /// </summary>
        public List<Pair<string, string>> DisplayPropertyData(NamedValueSet properties)
        {
            return properties.ToArray().Select(element => new Pair<string, string>(element.Name, element.ValueString)).ToList();
        }

        /// <summary>
        /// A function to return the list of valid asset measures. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of asset measure types.</returns>
        public List<string> SupportedExchangeTradedTypes()
        {
            return Enum.GetNames(typeof(ExchangeContractTypeEnum)).ToList();
        }

        /// <summary>
        /// A function to return the list of valid asset measures. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of asset measure types.</returns>
        public List<string> SupportedAssetMeasureTypes()
        {
            return Enum.GetNames(typeof(AssetMeasureEnum)).ToList();
        }

        /// <summary>
        /// A function to return the list of valid price quote units. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of valid price quote units.</returns>
        public List<string> SupportedPriceQuoteUnits()
        {
            return Enum.GetNames(typeof(PriceQuoteUnitsEnum)).ToList();
        }

        /// <summary>
        /// A function to return the list of valid bond price quotes. 
        /// Only some have been implemented.
        /// </summary>
        /// <returns>A vertical range object, containing the list of valid price quote units.</returns>
        public List<string> SupportedBondPriceQuotes()
        {
            return Enum.GetNames(typeof(BondPriceEnum)).ToList();
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
        public List<string> SupportedBondTypes()
        {
            return Enum.GetNames(typeof(BondTypesEnum)).ToList();
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
        public List<string> SupportedMarketSectors()
        {
            return Enum.GetNames(typeof(MarketSectorEnum)).ToList();
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
        public List<string> SupportedCreditSeniority()
        {
            return Enum.GetNames(typeof(CreditSeniorityEnum)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented bond assets. 
        /// Each bond type has different static data information.
        /// <para>Fixed,</para> 
        /// <para>Float,</para> 
        /// <para>Struct,</para> 
        /// </summary>
        /// <returns>A vertical range object, containing the list of valid bond types.</returns>
        public List<string> SupportedCouponTypes()
        {
            return Enum.GetNames(typeof(CouponTypeEnum)).ToList();
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
        public List<string> SupportedAssets()
        {
            return Enum.GetNames(typeof(AssetTypesEnum)).ToList();
        }

        /// <summary>
        /// A function to return the list of valid indices, which may or may not have been implemented.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedFloatingRates()
        {
            return Enum.GetNames(typeof(FloatingRateIndexEnum)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedEquityMetrics()
        {
            return Enum.GetNames(typeof(EquityMetrics)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedBondMetrics()
        {
            return Enum.GetNames(typeof(BondMetrics)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedPropertyMetrics()
        {
            return Enum.GetNames(typeof(PropertyMetrics)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedLeaseMetrics()
        {
            return Enum.GetNames(typeof(LeaseMetrics)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedFuturesMetrics()
        {
            return Enum.GetNames(typeof(FuturesMetrics)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedRateMetrics()
        {
            return Enum.GetNames(typeof(RateMetrics)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedFxMetrics()
        {
            return Enum.GetNames(typeof(FxMetrics)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedCommodityMetrics()
        {
            return Enum.GetNames(typeof(CommodityMetrics)).ToList();
        }

        /// <summary>
        /// A function to return the list of implemented metrics.
        /// </summary>
        /// <returns>A range object</returns>
        public List<string> SupportedRateOptionMetrics()
        {
            return Enum.GetNames(typeof(RateOptionMetrics)).ToList();
        }

        #endregion

        #region Assets and Instruments

        /// <summary>
        /// Load Property Data
        /// </summary>
        /// <param name="shortName"></param>
        /// <param name="propertyIdentifier"></param>
        /// <param name="propertyType"></param>
        /// <param name="city"></param>
        /// <param name="postCode"></param>
        /// <returns></returns>
        public ICoreItem GetPropertyAsset(PropertyType propertyType, string city, string shortName, string postCode, string propertyIdentifier)
        {
            return Engine.GetPropertyAsset(propertyType, city, shortName, postCode, propertyIdentifier);
        }


        /// <summary>
        /// Load Asset Config from the XML in the database
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="asset"></param>
        /// <param name="maturityTenor"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public Instrument GetAssetConfigurationData(string currency, string asset, string maturityTenor, [Optional] string variant)
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
        ///<param name="notionalWeights">The notional weights.</param>
        ///<param name="fixedLegDayCount">The dayCount.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="dates">The roll dates.</param>
        ///<param name="businessCentersAsString">Te business centers.</param>
        ///<returns>The PDH array.</returns>
        public List<decimal> GetSwapPDH(string curveId, string currency, DateTime baseDate,
            decimal notional, List<DateTime> dates, List<decimal> notionalWeights, string fixedLegDayCount, decimal fixedRate,
            string businessDayConvention, string businessCentersAsString)
        {
            var result = new List<decimal>();
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            bav.quote = new[] { quote };
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCentersAsString);
            //create the swap.
            var swap = (PriceableSwapRateAsset)PriceableAssetFactory.CreateInterestRateSwap("Local", baseDate,
                MoneyHelper.GetAmount(notional, currency), dates.ToArray(),
                notionalWeights.ToArray(), fixedLegDayCount, businessDayAdjustments, bav);
            //Get the curve.
            var rateCurve = (CurveBase)Engine.GetCurve(curveId, false);
            //Create the interpolator and get the pdh.
            var valuation = swap.CalculatePDH(Engine.Logger, Engine.Cache, rateCurve, null, null);
            if (valuation != null)
            {
                result.AddRange(valuation);
            }
            return result;
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
        /// <param name="dayCount">The day count convention.</param>
        /// <param name="settlementDate">The settlement date.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <returns>The asset swap level.</returns>
        public decimal GetBondAssetSwapSpread(string identifier, DateTime baseDate, DateTime settlementDate, DateTime exDivDate, DateTime maturityDate,
            decimal amount, decimal coupon, string dayCount, string frequency, string issuerName, string rollConvention, string businessCenters, decimal ytm, string curveId)
        {
            //Get the curve.
            var rateCurve = (IRateCurve)Engine.GetCurve(curveId, false);
            //Calculate the asset swap level.
            var asw = Engine.GetBondAssetSwapSpread(identifier, baseDate, settlementDate, exDivDate, MoneyHelper.GetAmount(amount), 
                coupon, maturityDate, dayCount, frequency, issuerName, rollConvention, businessCenters, rateCurve, ytm, null);
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
        ///<param name="notionalWeights">The notional weights.</param>
        ///<param name="fixedLegDayCount">The dayCount.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="businessCentersAsString">The business centers.</param>
        ///<param name="metric">The metric.</param>
        ///<param name="dates">The dates.</param>
        ///<returns></returns>
        public decimal GetStructuredSwapMetric(string curveId, string currency, DateTime baseDate,
            decimal notional, List<DateTime> dates, List<decimal> notionalWeights, string fixedLegDayCount, decimal fixedRate,
            string businessDayConvention, string businessCentersAsString, string metric)
        {
            var result = Engine.GetStructuredSwapMetric(curveId, currency, baseDate, 
                notional, dates, notionalWeights, fixedLegDayCount, fixedRate, businessDayConvention, businessCentersAsString, metric);
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
        ///<param name="resetRates">An array of last Reset rates. This may be null.</param>
        ///<param name="includeStubFlag">Include the first reset flag.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="resetPeriod">The reset period e.g.-2D.</param>
        ///<param name="paymentBusinessCentersAsString">The payment business centers.</param>
        ///<param name="resetBusinessCentersAsString">The rest business days.</param>
        ///<param name="metrics">The metrics as an array.</param>
        ///<param name="effectiveDate">The roll dates with the maturity date included..</param>
        ///<param name="resetBusinessDayConvention">The reset business day convention.</param>
        ///<param name="cashFlowDetail">A detail cashflow flag - mainly for debugging.</param>
        /// <param name="properties">Contains all the required properties.</param>
        ///<returns>The value of the metric requested.</returns>
        public Dictionary<string, decimal> GetIRCapFloorMetrics(
            DateTime effectiveDate, double notional, double strike, List<double> resetRates, bool includeStubFlag, 
            bool rollBackward, string paymentBusinessDayConvention, string paymentBusinessCentersAsString,
            string resetBusinessDayConvention, string resetBusinessCentersAsString,
            string resetPeriod, List<string> metrics, bool cashFlowDetail, NamedValueSet properties)
        {
            if (paymentBusinessDayConvention == null) throw new ArgumentNullException(nameof(paymentBusinessDayConvention));
            var forecastCurveId = properties.GetString("ForecastCurve", true);
            var discountCurveId = properties.GetString("DiscountCurve", true);
            var volCurveId = properties.GetString("VolatilitySurface", true);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var isCap = properties.GetValue<bool>("IsCap", true);
            var currency = properties.GetString("Currency", true);
            var indexTenor = properties.GetString("IndexTenor", true);
            var dayCount = properties.GetString("FloatingLegDayCount", true);
            var maturityTerm = properties.GetString("Term", true);
            var floatingRateIndex = properties.GetString("FloatingRateIndex", true);
            var result = new Dictionary<string, decimal>();
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(paymentBusinessDayConvention, paymentBusinessCentersAsString);
            //Create the RelativeDateOffset.
            var resetOffset = RelativeDateOffsetHelper.Create(resetPeriod, DayTypeEnum.Business,
                                                              resetBusinessDayConvention, resetBusinessCentersAsString, "StartDate");
            //Create the ForecastRateIndex.
            var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex, indexTenor);
            //create the cap/floor.
            var capFloor = isCap
                               ? PriceableAssetFactory.CreateIRCap(Engine.Logger, Engine.Cache, NameSpace, baseDate, effectiveDate, maturityTerm, currency,
                    indexTenor, includeStubFlag, notional, strike, rollBackward, resetRates,
                                                                     resetOffset, businessDayAdjustments, dayCount, forecastRate, null, null, properties)
                               : PriceableAssetFactory.CreateIRFloor(Engine.Logger, Engine.Cache, NameSpace, baseDate, effectiveDate, maturityTerm, currency,
                    indexTenor, includeStubFlag, notional, strike, rollBackward, resetRates,
                                                                     resetOffset, businessDayAdjustments, dayCount, forecastRate, null, null, properties);
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
            var forecastCurve = (IRateCurve)Engine.GetCurve(forecastCurveId, false);
            var discountCurve = (IRateCurve)Engine.GetCurve(discountCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), forecastCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), discountCurve);
            var volCurve = (IVolatilitySurface)Engine.GetCurve(volCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastIndexVolatilitySurface.ToString(), volCurve);
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = capFloor.Calculate(assetControllerData);
            if (valuation == null) return result;
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
                return result;
            }
            //Return a scalar sum if details are not required.
            result = MetricsHelper.AggregateMetrics(valuation, metrics);
            return result;
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
        /// <param name="strikeRates">The strike array.</param>
        /// <param name="resetRates">An array of rest rates. This may be null.</param>
        /// <param name="notionalWeights">The notional weights.</param>
        /// <param name="resetPeriod">The reset period e.g. -2D.</param>
        /// <param name="paymentBusinessCentersAsString">The payment business centers.</param>
        /// <param name="resetBusinessCentersAsString">The rest business days.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="rollAndMaturityDates">The roll dates with the maturity date included.</param>
        /// <param name="resetBusinessDayConvention">The reset business day convention.</param>
        /// <param name="namedValueSet">Contains all the required properties.</param>
        /// <returns>The value of the metric requested.</returns>
        public Dictionary<string, decimal> GetIRCapFloorMetric(List<DateTime> rollAndMaturityDates, List<double> notionalWeights,
            List<double> strikeRates, List<double> resetRates, string paymentBusinessDayConvention, string paymentBusinessCentersAsString,
            string resetBusinessDayConvention, string resetBusinessCentersAsString, string resetPeriod, string metric, NamedValueSet namedValueSet)
        {
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
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(paymentBusinessDayConvention, paymentBusinessCentersAsString);
            //Create the RelativeDateOffset.
            var resetOffset = RelativeDateOffsetHelper.Create(resetPeriod, DayTypeEnum.Business,
                                                              resetBusinessDayConvention, resetBusinessCentersAsString, "StartDate");
            //Create the ForecastRateIndex.
            var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex, indexTenor);
            //create the cap/floor.
            var capFloor = isCap ? PriceableAssetFactory.CreateIRCap(Engine.Logger, Engine.Cache, NameSpace, "Local", baseDate, currency, rollAndMaturityDates,
                                                                    notionalWeights, strikeRates, resetRates, resetOffset,
                                                                    businessDayAdjustments, dayCount, forecastRate, null, namedValueSet)
                                                                    : PriceableAssetFactory.CreateIRFloor(Engine.Logger, Engine.Cache, NameSpace, "Local", baseDate, currency, rollAndMaturityDates,
                                                                        notionalWeights, strikeRates, resetRates, resetOffset,
                                                                    businessDayAdjustments, dayCount, forecastRate, null, namedValueSet);
            //Get the AssetControllerData.
            var market = new SwapLegEnvironment();
            var bavMetric = new BasicAssetValuation();
            var quoteMetric = BasicQuotationHelper.Create(0.0m, metric);
            bavMetric.quote = new[] { quoteMetric };
            //Get the curve.
            var forecastCurve = (IRateCurve)Engine.GetCurve(forecastCurveId, false);
            var discountCurve = (IRateCurve)Engine.GetCurve(discountCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), forecastCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), discountCurve);
            var volCurve = (IVolatilitySurface)Engine.GetCurve(volCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastIndexVolatilitySurface.ToString(), volCurve);
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = capFloor.Calculate(assetControllerData);
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
            return result;
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
        ///<param name="strikeRates">The strike array.</param>
        ///<param name="resetRates">An array of rest rates. This may be null.</param>
        ///<param name="notionalWeights">The notional weights.</param>
        ///<param name="resetPeriod">The reset period e.g.-2D.</param>
        ///<param name="paymentBusinessCentersAsString">The payment business centers.</param>
        ///<param name="resetBusinessCentersAsString">The rest business days.</param>
        ///<param name="rollAndMaturityDates">The roll dates with the maturity date included..</param>
        ///<param name="resetBusinessDayConvention">The reset business day convention.</param>
        /// <param name="properties">Contains all the required properties.</param>
        ///<returns>The rate curve pdh.</returns>
        public IDictionary<string, double> GetIRCapFloorPDH(List<DateTime> rollAndMaturityDates, List<double> notionalWeights,
            List<double> strikeRates, List<double> resetRates, string paymentBusinessDayConvention, string paymentBusinessCentersAsString,
            string resetBusinessDayConvention, string resetBusinessCentersAsString, string resetPeriod, NamedValueSet properties)
        {
            var forecastCurveId = properties.GetString("ForecastCurve", true);
            var discountCurveId = properties.GetString("DiscountCurve", true);
            var volCurveId = properties.GetString("VolatilitySurface", true);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var curveToPerturb = properties.GetString("CurveToPerturb", true);
            var curvePerturbation = EnumHelper.Parse<CurvePerturbation>(curveToPerturb);
            var isCap = properties.GetValue<bool>("IsCap", true);
            var currency = properties.GetString("Currency", true);
            var indexTenor = properties.GetString("IndexTenor", true);
            var dayCount = properties.GetString("FloatingLegDayCount", true);
            var floatingRateIndex = properties.GetString("FloatingRateIndex", true);
            if (paymentBusinessDayConvention == null) throw new ArgumentNullException(nameof(paymentBusinessDayConvention));
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(paymentBusinessDayConvention,
                                                                             paymentBusinessCentersAsString);
            //Create the RelativeDateOffset.
            var resetOffset = RelativeDateOffsetHelper.Create(resetPeriod, DayTypeEnum.Business,
                                                              resetBusinessDayConvention, resetBusinessCentersAsString, "StartDate");
            //Create the ForecastRateIndex.
            var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex, indexTenor);
            //create the cap/floor.
            var capFloor = isCap ? PriceableAssetFactory.CreateIRCap(Engine.Logger, Engine.Cache, NameSpace, "Local", baseDate, currency, rollAndMaturityDates,
                    notionalWeights, new List<double>(strikeRates), resetRates, resetOffset,
                                                                    businessDayAdjustments, dayCount, forecastRate, null, properties)
                                                                    : PriceableAssetFactory.CreateIRFloor(Engine.Logger, Engine.Cache, NameSpace, "Local", baseDate, currency, rollAndMaturityDates,
                                                                        notionalWeights, new List<double>(strikeRates), resetRates, resetOffset,
                                                                    businessDayAdjustments, dayCount, forecastRate, null, properties);
            //Get the curve.
            var market = new SwapLegEnvironment();
            var forecastCurve = (IRateCurve)Engine.GetCurve(forecastCurveId, false);
            var discountCurve = (IRateCurve)Engine.GetCurve(discountCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), forecastCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), discountCurve);
            var volCurve = (IVolatilitySurface)Engine.GetCurve(volCurveId, false);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastIndexVolatilitySurface.ToString(), volCurve);
            //Create the interpolator and get the pdh.
            var valuation = capFloor.CalculateRatePDH(baseDate, discountCurve, forecastCurve, volCurve, curvePerturbation);
            return valuation;
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
        ///<param name="fixedNotionalWeights">The fixed notional list.</param>
        ///<param name="metric">The metric.</param>
        ///<param name="fixedRollDates">The fixed roll dates, including the maturity date.</param>
        ///<param name="floatingRollDates">The floating roll dates. Includes the maturity date</param>
        ///<param name="floatingNotionalWeights">The floating notional weights.</param>
        ///<param name="floatingResets">The floating leg rest rate array.</param>
        ///<param name="properties">The properties associated with the asset.</param>
        ///<returns>The value of the metric requested.</returns>
        public decimal GetIRSwapMetric(List<DateTime> fixedRollDates, List<decimal> fixedNotionalWeights, 
            List<DateTime> floatingRollDates, List<decimal> floatingNotionalWeights,
            List<decimal> floatingResets, string metric, NamedValueSet properties)
        {
            var forecastCurveId = properties.GetString("ForecastCurve", true);
            var discountCurveId = properties.GetString("DiscountCurve", true);
            var fixedRate = properties.GetValue<Decimal>("Rate", true);
            var floatingSpread = properties.GetValue<Decimal>("Spread", true);
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var businessDayConvention = properties.GetString("BusinessDayConvention", true);
            var businessCentersAsString = properties.GetString("BusinessCentersAsString", true);
            var result = 0.0m;
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            var spreadQuote = BasicQuotationHelper.Create(floatingSpread, "Spread", "DecimalRate");
            bav.quote = new[] { quote, spreadQuote };
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention,
                                                                             businessCentersAsString);
            //create the swap.
            var swap = PriceableAssetFactory.CreateIRSwap(fixedRollDates,
                fixedNotionalWeights, floatingRollDates, floatingNotionalWeights,
                                                                    floatingResets, businessDayAdjustments, bav, properties);
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
        ///<param name="fixedNotionalSchedule">The fixed notional schedule.</param>
        ///<param name="floatingSpread">The spread on the floating leg.</param>
        ///<param name="riskMetrics">The metrics.</param>
        ///<param name="fixedRollDates">The fixed dates.</param>
        ///<param name="floatingRollDates">The floating dates.</param>
        ///<param name="floatingNotionalSchedule">The floating leg notional schedule.</param>
        ///<param name="floatingResets">The floating rest rates array.</param>
        ///<param name="floatingLegDayCount">The floating leg dayCount.</param>
        ///<param name="properties">The properties associated with the asset.</param>
        ///<returns>The metrics requested.</returns>
        public Dictionary<string,decimal> GetIRSwapMetrics(List<DateTime> fixedRollDates, List<decimal> fixedNotionalSchedule,
            decimal fixedRate, List<DateTime> floatingRollDates, List<decimal> floatingNotionalSchedule,
            List<decimal> floatingResets, string floatingLegDayCount, decimal floatingSpread, 
            List<string> riskMetrics, NamedValueSet properties)
        {
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var businessDayConvention = properties.GetString("BusinessDayConvention", true);
            var businessCentersAsString = properties.GetString("BusinessCentersAsString", true);
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            var spreadQuote = BasicQuotationHelper.Create(floatingSpread, "Spread", "DecimalRate");
            bav.quote = new[] { quote, spreadQuote };
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention,
                                                                             businessCentersAsString);
            //create the swap.
            var swap = PriceableAssetFactory.CreateIRSwap(fixedRollDates,
                fixedNotionalSchedule, floatingRollDates,
                                                                    floatingNotionalSchedule,
                                                                    floatingResets,
                                                                    businessDayAdjustments, bav, properties);
            //Get the AssetControllerData.
            var market = new SimpleRateMarketEnvironment();
            var bavMetric = new BasicAssetValuation
                                {
                                    quote =
                                        riskMetrics.Select(metric => BasicQuotationHelper.Create(0.0m, metric)).ToArray()
                                };
            //Get the curve.
            var curveId = properties.GetString("DiscountCurve", true);
            var forecastCurveId = properties.GetString("ForecastCurve", false);
            var rateCurve = (IRateCurve)Engine.GetCurve(curveId, false);
            market.AddPricingStructure("DiscountCurve", rateCurve);
            if (forecastCurveId != null)
            {
                market.AddPricingStructure("DiscountCurve", rateCurve);
            }
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = swap.Calculate(assetControllerData);
            return valuation.quote.ToDictionary(val => val.measureType.Value, val => val.value);
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
        ///<param name="rollBackward">If true, roll back from the maturity date. Otherwise roll forward from the effective date.</param>
        ///<param name="floatingResets">A list of last Reset rates. This may be null.</param>
        ///<param name="includeStubFlag">Include the first reset flag.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="resetPeriod">The reset period e.g. -2D.</param>
        ///<param name="resetBusinessCentersAsString">The rest business days.</param>
        ///<param name="effectiveDate">The roll dates with the maturity date included..</param>
        ///<param name="resetBusinessDayConvention">The reset business day convention.</param>
        ///<param name="properties">The properties associated with the asset.</param>
        ///<returns>The value of the metric requested.</returns>
        public string CreateInterestRateSwap(DateTime effectiveDate,
            double notional, List<decimal> floatingResets,
            bool includeStubFlag, bool rollBackward,
            string resetBusinessDayConvention, string resetBusinessCentersAsString,
            string resetPeriod, NamedValueSet properties)
        {
            var fixedRate = properties.GetValue<decimal>("Rate", true);
            var floatingSpread = properties.GetValue<decimal>("Spread", true);
            var businessDayConvention = properties.GetString("BusinessDayConvention", true);
            var businessCentersAsString = properties.GetString("BusinessCentersAsString", true);
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
                businessDayAdjustments, resetOffset, floatingResets, null, null, quote, spreadQuote, properties);
            var id = Engine.SetInterestRateSwap(irSwap, properties);
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
        ///<param name="floatingResetRates">A list of last Reset rates. This may be null.</param>
        ///<param name="includeStubFlag">Include the first reset flag.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="resetPeriod">The reset period e.g. -2D.</param>
        ///<param name="paymentBusinessCentersAsString">The payment business centers.</param>
        ///<param name="resetBusinessCentersAsString">The rest business days.</param>
        ///<param name="metrics">The metrics as an array.</param>
        ///<param name="effectiveDate">The roll dates with the maturity date included..</param>
        ///<param name="resetBusinessDayConvention">The reset business day convention.</param>
        /// <param name="properties">The properties associated with the asset.</param>
        ///<returns>The value of the metric requested.</returns>
        public Dictionary<string, decimal> GetIRSwapMetrics2(DateTime effectiveDate, double notional, List<decimal> floatingResetRates,
            bool includeStubFlag, bool rollBackward, string paymentBusinessDayConvention,
            string paymentBusinessCentersAsString, string resetBusinessDayConvention, string resetBusinessCentersAsString,
            string resetPeriod, List<string> metrics, NamedValueSet properties)
        {
            if (paymentBusinessDayConvention == null) throw new ArgumentNullException(nameof(paymentBusinessDayConvention));
            var baseDate = properties.GetValue<DateTime>("BaseDate", true);
            var result = new Dictionary<string, decimal>();
            //Create the quotations
            var fixedRate = properties.GetValue<decimal>("Rate", true);
            var floatingSpread = properties.GetValue<decimal>("Spread", true);
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
                businessDayAdjustments, resetOffset, floatingResetRates, null, null, quote, spreadQuote, properties);
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
            var curveId = properties.GetString("DiscountCurve", true);
            var forecastCurveId = properties.GetString("ForecastCurve", false);
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
            return valuation == null ? result : MetricsHelper.AggregateMetrics(valuation, metrics);
            //Return a scalar sum if details are not required.
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
        /// <param name="fixedNotionalSchedule">The fixed notional weights.</param>
        /// <param name="fixedRollDates">The fixed dates.</param>
        /// <param name="floatingResets">The floating rest rates array.</param>
        /// <param name="floatingRollDates">The floating dates.</param>
        /// <param name="floatingNotionalSchedule">The floating leg notional weights.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        /// <returns>The VERTICAL ARRAY OF PARTIAL DERIVATIVES, WITH RESPECT TO EACH ASSET IN THE REFERENCED CURVE.</returns>
        public IDictionary<string,decimal> GetIRSwapPDH(List<DateTime> fixedRollDates,
            List<decimal> fixedNotionalSchedule, List<DateTime> floatingRollDates,
            List<decimal> floatingNotionalSchedule, List<decimal> floatingResets, 
            NamedValueSet properties)
        {
            var businessDayConvention = properties.GetString("BusinessDayConvention", true);
            var businessCentersAsString = properties.GetString("BusinessCentersAsString", true);
            var curveToPerturb = properties.GetString("CurveToPerturb", true);
            var forecastCurveId = properties.GetString("ForecastCurve", true);
            var discountCurveId = properties.GetString("DiscountCurve", true);
            var fixedRate = properties.GetValue<decimal>("Rate", true);
            var floatingSpread = properties.GetValue<decimal>("Spread", true);
            var curvePerturbation = EnumHelper.Parse<CurvePerturbation>(curveToPerturb);
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            var spreadQuote = BasicQuotationHelper.Create(floatingSpread, "Spread", "DecimalRate");
            bav.quote = new[] { quote, spreadQuote };
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCentersAsString);
            //create the swap.
            var swap = PriceableAssetFactory.CreateIRSwap(fixedRollDates,
                fixedNotionalSchedule,
                                                                    floatingRollDates,
                floatingNotionalSchedule,
                                                                    floatingResets,
                                                                    businessDayAdjustments, bav, properties);
            //Get the curve.
            var forecastCurve = (CurveBase)Engine.GetCurve(forecastCurveId, false);
            var discountCurve = (CurveBase)Engine.GetCurve(discountCurveId, false);
            var valuation = swap.CalculatePDH(discountCurve, forecastCurve, curvePerturbation);
            //This requires an IPricingStructure to be cached.
            //Create the interpolator and get the pdh.          
            return valuation;
        }

        ///<summary>
        /// Calculates the implied quote for a swap.
        ///</summary>
        ///<param name="curveId">The curve id. This must have been created already.</param>
        ///<param name="currency">The configuration currency.
        /// This is used to get all default values and must exist in the cache.</param>
        ///<param name="baseDate">The base date.</param>
        ///<param name="spotDate">The spot date.</param>
        ///<param name="fixedLegDayCount">The dayCount.</param>
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
        public string CreateSimpleLeaseWithProperties(string assetIdentifier, DateTime baseDate, double grossAmount, double stepUp, NamedValueSet properties)
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
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="grossAmounts">The gross amounts.</param>
        /// <param name="stepUps">The coupons.</param>
        /// <returns></returns>
        public List<string> CreateSimpleLeases(List<string> assetIdentifiers, DateTime baseDate, List<double> grossAmounts,
            List<double> stepUps)
        {
            return Engine.CreateLocalLeases(assetIdentifiers, baseDate, grossAmounts, stepUps).ToList();
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
        public string CreateSimpleBondWithProperties(string assetIdentifier, DateTime baseDate, DateTime maturityDate, decimal coupon, decimal ytm, NamedValueSet properties)
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
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="maturityDates">The maturity dates.</param>
        /// <param name="coupons">The coupons.</param>
        /// <param name="yieldToMaturities">The yield to maturities.</param>
        /// <returns></returns>
        public List<string> CreateSimpleBonds(List<string> assetIdentifiers, DateTime baseDate, List<DateTime> maturityDates,
            List<decimal> coupons, List<decimal> yieldToMaturities)
        {
            return Engine.CreateLocalBonds(assetIdentifiers, baseDate, maturityDates, coupons, yieldToMaturities).ToList();
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="maturityDates">The maturity dates.</param>
        /// <param name="coupons">The coupons.</param>
        /// <param name="yieldToMaturities">The yield to maturities.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public object[,] CreateSimpleBondsWithProperties(List<string> assetIdentifiers, DateTime baseDate, List<DateTime> maturityDates,
            List<decimal> coupons, List<decimal> yieldToMaturities, NamedValueSet properties)
        {
            var rateAssets = Engine.CreateLocalBonds(assetIdentifiers, baseDate, maturityDates, coupons, yieldToMaturities, properties);
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
        /// <param name="properties">The properties.</param>
        /// <param name="dayCount">The dayCount convention.</param>
        /// <returns></returns>
        public string CreateBondWithDetail(string assetIdentifier, DateTime baseDate, DateTime maturityDate,
                                                  decimal coupon, string dayCount, string frequency, decimal ytm, NamedValueSet properties)
        {
            var result = Engine.CreateLocalBond(assetIdentifier, baseDate, maturityDate, coupon, dayCount, frequency, ytm, properties);
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
            string exchangeId, string issuerName, List<string> propertyHeaders, List<string> propertyValues)
        {
            if (propertyHeaders.Count != propertyValues.Count) return "The arrays are not of equal length!";
            var properties = new NamedValueSet();
            var index = 0;
            foreach(var header in propertyHeaders)
            {
                properties.Set(header, propertyValues[index]);
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
        /// <param name="identifiers">The identifier array to retrieve.</param>
        /// <param name="directoryPath">The path to save the files into.</param>
        /// <param name="isShortName">If <true>isShortName</true> is true> then the id is of the form: FixedIncome.XXXX.YY.zz-zz-zzzz.
        /// Otherwise it is of the form Orion.ReferenceData.FixedIncome.XXXX.YY.zz-zz-zzzz.</param>
        /// <returns></returns>
        public string SaveBonds(List<string> identifiers, string directoryPath, bool isShortName)
        {
            var result = ValService.SaveBonds(identifiers, directoryPath, isShortName);
            return result;
        }

        /// <summary>
        /// Creates the bond assets.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="maturityDates">The maturity dates.</param>
        /// <param name="coupons">The coupons.</param>
        /// <param name="frequencies">The coupon frequencies.</param>
        /// <param name="dayCounts">The dayCount conventions.</param>
        /// <param name="yieldToMaturities">The yield to maturities.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public List<string> CreateBondsWithDetails(List<string> assetIdentifiers, DateTime baseDate, List<DateTime> maturityDates,
            List<decimal> coupons, List<string> dayCounts, List<string> frequencies, List<decimal> yieldToMaturities, NamedValueSet properties)
        {
            return Engine.CreateLocalBonds(assetIdentifiers, baseDate, maturityDates, coupons, dayCounts, frequencies, yieldToMaturities, properties).ToList();
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
            string description, string exchangeId, string issuerName, List<string> propertyHeaders, List<string> propertyValues)
        {
            if (propertyHeaders.Count != propertyValues.Count) return "The arrays are not of equal length!";
            var properties = new NamedValueSet();
            var index = 0;
            foreach (var header in propertyHeaders)
            {
                properties.Set(header, propertyValues[index]);
                index++;
            }
            var result = ValService.CreateEquity(instrumentId, clearanceSystem,  currency, assetId, description, exchangeId, issuerName, properties);
            return result;
        }

        /// <summary>
        /// Creates a property in the data store.
        /// </summary>
        /// <param name="propertyId">The property identifier.</param>
        /// <param name="propertyType">The property type: residential, commercial, investment etc</param>
        /// <param name="shortName">A short name for the property</param>
        /// <param name="streetIdentifier">A street Identifier.</param>
        /// <param name="streetName">A street Name.</param>
        /// <param name="suburb">The suburb</param>
        /// <param name="city">The city</param>
        /// <param name="postalCode">The postal code. This could be a number or a string.</param>
        /// <param name="state">The state</param>
        /// <param name="country">The country</param>
        /// <param name="numBedrooms">The number of bedrooms.</param>
        /// <param name="numBathrooms">The number of bathrooms</param>
        /// <param name="numParking">The number of car parking spots.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="description">The issuer description.</param>
        /// <param name="properties">An array of properties. </param>
        /// <returns></returns>
        public string CreatePropertyAsset(string propertyId, PropertyType propertyType, string shortName, string streetIdentifier, string streetName, 
            string suburb, string city, string postalCode, string state, string country, string numBedrooms, string numBathrooms, string numParking,
            string currency, string description, NamedValueSet properties)
        {
            var result = ValService.CreatePropertyAsset(propertyId, propertyType, shortName, streetIdentifier, streetName, suburb, city, postalCode, state, country,
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
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string CreateSurfaceAsset(string assetIdentifier, DateTime baseDate, decimal strike,
                                                decimal volatility, NamedValueSet properties)
        {
            var result = Engine.CreateLocalSurfaceAsset(assetIdentifier, baseDate, volatility, strike, properties);
            return result;
        }

        /// <summary>
        /// Creates the surface volatility asset.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volatility">The volatility matrix.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string[,] CreateSurfaceAssets(List<string> assetIdentifiers, DateTime baseDate, List<decimal> strikes,
                                                    decimal[,] volatility, NamedValueSet properties)
        {
            return Engine.CreateLocalSurfaceAssets(assetIdentifiers, baseDate, volatility, strikes, properties);
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
        /// <param name="properties">The properties. Must include AssetId, BaseDate and AssetType.
        /// May contain Notional.</param>
        /// <param name="adjustedRate">The adjusted rates.</param>
        /// <param name="additionalValue">The additional values.</param>
        /// <returns></returns>
        public string CreateAssetWithProperties(double adjustedRate, double additionalValue,
                                         NamedValueSet properties)
        {
            var result = Engine.CreateLocalAsset(Convert.ToDecimal(adjustedRate), Convert.ToDecimal(additionalValue), properties);
            return result;
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measures">The measure types.</param>
        /// <param name="priceQuoteUnits">The price quote units.</param>
        /// <returns></returns>
        public string CreateAssetWithUnits(string assetIdentifier, DateTime baseDate, List<decimal> values,
                                                  List<string> measures, List<string> priceQuoteUnits)
        {
            NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForAssets(NameSpace, assetIdentifier, baseDate);
            var result = Engine.CreateLocalAsset(values, measures, priceQuoteUnits, namedValueSet);
            return result;
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="properties">The properties. Must include AssetId, BaseDate and AssetType.</param>
        /// <param name="measureTypes">The measure types.</param>
        /// <param name="priceQuoteUnits">The price quote units.</param>
        /// <returns></returns>
        public string CreateAssetWithUnitsAndProperties(List<decimal> values,
                                                  List<string> measureTypes, List<string> priceQuoteUnits, NamedValueSet properties)
        {
            var result = Engine.CreateLocalAsset(values, measureTypes, priceQuoteUnits, properties);
            return result;
        }

        /// <summary>
        /// Creates the asset.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifier.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="measureTypes">The measure types.</param>
        /// <param name="priceQuoteUnits">The price quote units.</param>
        /// <returns></returns>
        public object[,] CreateAssetsWithUnits(List<string> assetIdentifiers, List<decimal> adjustedRates,
            List<string> measureTypes, List<string> priceQuoteUnits, NamedValueSet properties)
        {
            var rateAssets = Engine.CreateLocalAssets(assetIdentifiers, adjustedRates, measureTypes, priceQuoteUnits, properties);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates the assets.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional values.</param>
        /// <returns></returns>
        public object[,] CreateAssets(List<string> assetIdentifiers, DateTime baseDate, List<decimal> adjustedRates,
            List<decimal> additional)
        {
            var rateAssets = Engine.CreateLocalAssets(assetIdentifiers, baseDate, adjustedRates, additional);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates the assets.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifier.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional values.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public object[,] CreateAssetsWithProperties(List<string> assetIdentifiers, List<decimal> adjustedRates,
            List<decimal> additional, NamedValueSet properties)
        {
            var rateAssets = Engine.CreateLocalAssets(assetIdentifiers, adjustedRates, additional, properties);
            var result = RangeHelper.ConvertArrayToRange(rateAssets);
            return result;
        }

        /// <summary>
        /// Creates the assets.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional values.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public List<string> CreateAssetsWithBaseDate(List<string> assetIdentifiers, DateTime baseDate, List<decimal> adjustedRates,
            List<decimal> additional, NamedValueSet properties)
        {
            properties.Set(CurveProp.BaseDate, baseDate);
            return Engine.CreateLocalAssets(assetIdentifiers, adjustedRates, additional, properties);
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
            var rateCurve = (IRateCurve)Engine.GetCurve(curveId, false);
            return asset.CalculateImpliedQuote(rateCurve);
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetId">The asset id e.g. AUD-IRSwap-3Y.</param>
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
            var rateCurve = (IRateCurve)Engine.GetCurve(curveId, false);
            return priceableAsset.CalculateImpliedQuote(rateCurve);
        }

        /// <summary>
        /// Evaluates the metrics for a group of asset set.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKeys">The array of asset reference keys.</param>
        /// <returns>An array of implied quote values, one for each asset provided.</returns>
        public Dictionary<string, decimal> GetImpliedQuotes(string curveId, List<string> assetReferenceKeys)
        {
            var result = new Dictionary<string, decimal>();
            //Get the curve.
            var rateCurve = (IRateCurve)Engine.GetCurve(curveId, false);
            foreach (var assetReferenceKey in assetReferenceKeys)
            {
                var asset = Engine.GetLocalAsset(assetReferenceKey);
                result.Add(assetReferenceKey, asset.CalculateImpliedQuote(rateCurve));
            }
            return result;
        }


        /// <summary>
        /// Evaluates a metric of breakeven rates for a group of assets defined.
        /// </summary>
        /// <param name="effectiveDates">An array of effective dates.</param>
        /// <param name="assetTenors">The array of asset tenors.</param>
        /// <param name="properties">A property range. This must contain the mandatory properties:
        /// Currency, AssetType amd CurveIdentifier.</param>
        /// <returns>A matrix of implied quote values, one for each asset.</returns>
        public double[,] GetImpliedQuoteMatrix(List<DateTime> effectiveDates,
            List<string> assetTenors, NamedValueSet properties)
        {
            //Instantiate the result matrix.
            var result = new double[effectiveDates.Count, assetTenors.Count];
            //Get the mandatory data.
            var curveId = properties.GetValue<string>("CurveIdentifier");
            var currency = properties.GetValue<string>("Currency");
            var assetType = properties.GetValue<string>("AssetType");
            var rateCurve = (IRateCurve)Engine.GetCurve(curveId, false);
            for (var i = 0; i < effectiveDates.Count; i++)
            {
                for (var j = 0; j < assetTenors.Count; j++)
                {
                    var id = currency + "-" + assetType + "-" + assetTenors[j];
                    //Create the properties.
                    var assetProperties = PriceableAssetFactory.BuildPropertiesForAssets(NameSpace, id, effectiveDates[i]);
                    //create the asset-basic asset valuation pair.
                    var asset = AssetHelper.Parse(id, 0.05m, 0.0m);
                    //create the priceable asset.
                    var priceableAsset = Engine.CreatePriceableAsset(asset.Second, assetProperties);
                    result[i, j] = Convert.ToDouble(priceableAsset.CalculateImpliedQuote(rateCurve));
                }
            }
            return result;
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKey">The asset reference key.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        public QuotedAssetSet EvaluateMetrics(List<string> metrics, string curveId, string assetReferenceKey, DateTime referenceDate)
        {
            metrics.RemoveAll(item => item == null);
            var assetReferenceKeys = new List<string> { assetReferenceKey };
            return Engine.EvaluateMetricsForAssetSet(metrics, curveId, assetReferenceKeys, referenceDate);
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKeys">The asset reference keys.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        public QuotedAssetSet EvaluateMetricsForAssetSet(List<string> metrics, string curveId, List<string> assetReferenceKeys, DateTime referenceDate)
        {
            metrics.RemoveAll(item => item == null);
            return Engine.EvaluateMetricsForAssetSet(metrics, curveId, assetReferenceKeys, referenceDate);
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKeys">The asset reference keys.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        public QuotedAssetSet EvaluateMetricsRawData(List<string> metrics, string curveId,
            List<string> assetReferenceKeys, DateTime referenceDate)
        {
            metrics.RemoveAll(item => item == null);
            return Engine.EvaluateMetricsForAssetSet(metrics, curveId, assetReferenceKeys, referenceDate);
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
        /// There should be at least 4 columns: period; dayType; businessDayConvention; businessCentre. RelativeTo is optional</param>
        /// <returns></returns>
        public string LoadExchangeConfigData(string[,] exchangeData, string[,] exchangeSettlementData)
        {
            return ValService.LoadExchangeConfigData(exchangeData, exchangeSettlementData);
        }

        /// <summary>
        /// Views exchange config to the core.
        /// </summary>
        /// <param name="exchangeId">The exchange data to load.
        /// This must be consistent with the Exchange class</param>
        /// <returns></returns>
        public string[,] ViewExchangeConfigData(string exchangeId)
        {
            return ValService.ViewExchangeConfigData(exchangeId);
        }

        #endregion
    }
}