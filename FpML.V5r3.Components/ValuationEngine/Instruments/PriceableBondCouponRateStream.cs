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
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Schedulers;
using Orion.Constants;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.Analytics.Utilities;
using Orion.Models.Rates;
using Orion.Models.Rates.Coupons;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Instruments.InterestRates;
using Orion.CalendarEngine.Helpers;
using Orion.Models.Rates.Bonds;
using Orion.ValuationEngine.Factory;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableBondCouponRateStream : InstrumentControllerBase, IPriceableBondCouponRateStream<IBondStreamParameters, IBondStreamInstrumentResults>, IPriceableInstrumentController<Cashflows>
    {
        #region Member Fields

        // Analytics
        public IModelAnalytic<IBondStreamParameters, BondInstrumentMetrics> AnalyticsModel { get; set; }

        protected const string CModelIdentifier = "BondCouponRateStream";
        //protected const string CDefaultBucketingInterval = "3M";

        /// <summary>
        /// The bond identifier for discounting.
        /// </summary>
        public string BondId { get; set; }
        /// <summary>
        /// THe bond curve to use for valuation.
        /// </summary>
        public string BondCurveName { get; set; }

        /// <summary>
        /// The forecast curve for floating rate note valuation.
        /// </summary>
        public string ForecastCurveName { get; set; }

        // Requirements for pricing
        public string ReportingCurrencyFxCurveName { get; set; }

        //// BucketedCouponDates
        //public Period BucketingInterval { get; set; }
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();
        //public DateTime[] BucketedDates { get; set; }

        public List<DateTime> ResetDates { get; set; }

        // Children
        public List<PriceableRateCoupon> Coupons { get; set; }

        #endregion

        #region Public Fields

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);  
        /// </summary>
        public bool ForecastRateInterpolation { get; set; }

        /// <summary>
        /// Gets the currency.
        /// </summary>
        /// <value>The currency.</value>
        public string Currency { get; protected set; }

        /// <summary>
        /// Gets the issuer.
        /// </summary>
        /// <value>The seller.</value>
        public string Issuer { get; protected set; }

        /// <summary>
        /// Gets the buyer.
        /// </summary>
        /// <value>The buyer.</value>
        public string Buyer { get; protected set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IBondStreamParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IBondStreamInstrumentResults CalculationResults { get; protected set; }

        /// <summary>
        /// Gets the coupon steam type.
        /// </summary>
        /// <value>The coupon stream type.</value>
        public CouponStreamType BondCouponStreamType { get; set; }

        /// <summary>
        /// Gets the priceable coupons.
        /// </summary>
        /// <value>The priceable coupons.</value>
        public List<InstrumentControllerBase> PriceableCoupons
        {
            get 
            {
                List<InstrumentControllerBase> result = null;
                if (Coupons != null && Coupons.Count > 0)
                {
                    result = new List<InstrumentControllerBase>(Coupons); 
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the stream start dates.
        /// </summary>
        /// <value>The stream start dates.</value>
        public IList<DateTime> StreamStartDates
        {
            get
            {
                var datesList = Coupons.Select(coupon => coupon.AccrualStartDate).ToList();
                datesList.Sort();
                return datesList;
            }
        }

        /// <summary>
        /// Gets the stream end dates.
        /// </summary>
        /// <value>The stream end dates.</value>
        public IList<DateTime> StreamEndDates
        {
            get
            {
                var datesList = Coupons.Select(coupon => coupon.AccrualEndDate).ToList();
                datesList.Sort();
                return datesList;
            }
        }

        /// <summary>
        /// Gets the stream payment dates.
        /// </summary>
        /// <value>The stream payment dates.</value>
        public IList<DateTime> StreamPaymentDates
        {
            get
            {
                var datesList = Coupons.Select(coupon => coupon.PaymentDate).ToList();
                datesList.Sort();
                return datesList;
            }
        }

        /// <summary>
        /// Gets the adjusted stream dates indicators.
        /// </summary>
        /// <value>The adjusted stream dates indicators.</value>
        public IList<bool> AdjustedStreamDatesIndicators => GetStreamAdjustedIndicators();

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public PriceableBondCouponRateStream()
        {
            Multiplier = 1.0m;
            AnalyticsModel = new BondStreamAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableBondCouponRateStream"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace.</param>
        /// <param name="bondId">The bond Id.</param>
        /// <param name="paymentConvention">The payment roll conventions</param>
        /// <param name="forecastRateInterpolation">ForwardEndDate = forecastRateInterpolation ? AccrualEndDate 
        /// : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="tradeDate">The trade date is used to set the base date for future coupon generation.</param>
        /// <param name="notionalAmount">The notional amount</param>
        /// <param name="couponType">The coupon type: fixed or floating.</param>
        /// <param name="bond">THe bond details.</param>
        public PriceableBondCouponRateStream
            (
            ILogger logger
            , ICoreCache cache
            , string nameSpace
            , string bondId
            , DateTime tradeDate
            , decimal notionalAmount
            , CouponStreamType couponType
            , Bond bond
            , BusinessDayAdjustments paymentConvention
            , bool forecastRateInterpolation
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
        {
            BondId = bondId;
            Multiplier = 1.0m;
            PaymentCurrencies = new List<string>();
            AnalyticsModel = new BondStreamAnalytic();
            BondCouponStreamType = couponType;
            Id = BuildId(bondId, BondCouponStreamType);
            Currency = bond.currency.Value;
            ForecastRateInterpolation = forecastRateInterpolation;
            //Get the currency.
            if (!PaymentCurrencies.Contains(bond.currency.Value))
            {
                PaymentCurrencies.Add(bond.currency.Value);
            }
            //The calendars
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, paymentConvention.businessCenters, nameSpace);
            }
            //Set the default discount curve name.
            BondCurveName = CurveNameHelpers.GetBondCurveName(Currency, bondId);
            //Set the forecast curve name.//TODO extend this to the other types.
            //if (BondCouponStreamType != CouponStreamType.GenericFixedRate)
            //{
            //    if (fixingCalendar == null)
            //    {
            //        fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, ResetDates.resetDatesAdjustments.businessCenters, nameSpace);
            //    }
            //    ForecastCurveName = null;
            //    //if (Calculation.Items != null)
            //    //{
            //    //    var floatingRateCalculation = Calculation.Items;
            //    //    var floatingRateIndex = (FloatingRateCalculation) floatingRateCalculation[0];
            //    //    ForecastCurveName = CurveNameHelpers.GetForecastCurveName(floatingRateIndex);
            //    //}
            //}
            //Build the coupons and principal exchanges.
            Coupons = PriceableInstrumentsFactory.CreatePriceableBondCoupons(tradeDate, bond, notionalAmount,  BondCouponStreamType,
                paymentConvention, ForecastRateInterpolation, fixingCalendar, paymentCalendar);//TODO add the stub calculation.
            UpdateCouponDiscountCurveNames();
            UpdateCouponIds();
            //RiskMaturityDate = ;
            logger.LogInfo("Bond Coupon Stream built");
        }

        #endregion

        #region Coupons

        /// <summary>
        /// Gets the stream dates.
        /// </summary>
        /// <returns></returns>
        public DateTime[] GetStreamDates()
        {
            var couponDates = new List<DateTime>();
            for (var index = 1; index < Coupons.Count; index++)
            {
                var firstCoupon = Coupons[index - 1];
                var secondCoupon = Coupons[index];
                if (!couponDates.Contains(firstCoupon.AccrualStartDate))
                {
                    couponDates.Add(firstCoupon.AccrualStartDate);
                }
                if (!couponDates.Contains(secondCoupon.AccrualStartDate))
                {
                    couponDates.Add(secondCoupon.AccrualStartDate);
                }
                if (index == Coupons.Count - 1)
                {
                    couponDates.Add(secondCoupon.AccrualEndDate);
                }
            }
            couponDates.Sort();
            return couponDates.ToArray();
        }

        /// <summary>
        /// Gets the stream adjusted indicators.
        /// </summary>
        /// <returns></returns>
        public IList<bool> GetStreamAdjustedIndicators()
        {
            var adjusted = new List<bool>();
            var lastCouponIndex = Coupons.Count - 1;
            var index = 0;
            foreach (var coupon in Coupons)
            {
                adjusted.Add(coupon.AdjustCalculationDatesIndicator);
                if (lastCouponIndex == index)
                {
                    adjusted.Add(coupon.AdjustCalculationDatesIndicator);
                }
                index++;
            }
            return adjusted;
        }

        /// <summary>
        /// Gets the bucketed coupon dates.
        /// </summary>
        /// <param name="bucketInterval">The bucket interval.</param>
        /// <returns></returns>
        protected IDictionary<string, DateTime[]> GetBucketedCouponDates(Period bucketInterval)
        {
            IDictionary<string, DateTime[]> bucketCouponDates = new Dictionary<string, DateTime[]>();
            var bucketDates = new List<DateTime>();
            foreach (IPriceableInstrumentController<PaymentCalculationPeriod> coupon in Coupons)
            {
                var priceableRateCoupon = (IPriceableRateCoupon<IRateCouponParameters, IRateInstrumentResults>) coupon;
                bucketDates.AddRange(new List<DateTime>(DateScheduler.GetUnadjustedDatesFromTerminationDate(priceableRateCoupon.UnadjustedStartDate, priceableRateCoupon.UnadjustedEndDate, BucketingInterval, RollConventionEnum.NONE, out _, out _)));
                bucketDates = RemoveDuplicates(bucketDates);
                bucketCouponDates.Add(coupon.Id, bucketDates.ToArray());
            }
            return bucketCouponDates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="bucketInterval"></param>
        /// <returns></returns>
        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            var bucketDates = new List<DateTime>();
            if (Coupons.Count > 1)
            {
                bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out _, out _));
            }
            return bucketDates.ToArray();
        }

        /// <summary>
        /// A mapping function to label a stream correctly.
        /// </summary>
        /// <param name="calculation"></param>
        /// <returns>The coupon stream type</returns>
        private static CouponStreamType CouponTypeFromCalculation(Calculation calculation)
        {
            var cpt = CouponStreamType.GenericFloatingRate;
            // assume fixed if Items is null, alex!? todo
            if ((null == calculation.Items) || (calculation.Items[0] is Schedule))
            {
                cpt = CouponStreamType.GenericFixedRate;
            }
            if (calculation.Items?[0] is FloatingRateCalculation frc)
            {
                if (frc.capRateSchedule != null && frc.floorRateSchedule == null)
                {
                    return CouponStreamType.CapRate;
                }
                if (frc.capRateSchedule == null && frc.floorRateSchedule != null)
                {
                    return CouponStreamType.FloorRate;
                }
                if (frc.capRateSchedule != null && frc.floorRateSchedule != null)
                {
                    return CouponStreamType.CollarRate;
                }
                cpt = CouponStreamType.GenericFloatingRate;
            }
            return cpt;
        }


        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds the cashflows.
        /// </summary>
        /// <returns></returns>
        public Cashflows Build()
        {
            //TODO what about the cash flow match parameter?
            var cashflows = new Cashflows
                                {
                                    paymentCalculationPeriod = Coupons.Select(priceableCoupon => priceableCoupon.Build()).ToArray()
                                    
                                };
            return cashflows;
        }

        #endregion

        #region Metrics for Valuation

        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            CalculationResults = null;
            UpdateBucketingInterval(ModelData.ValuationDate, PeriodHelper.Parse(CDefaultBucketingInterval));
            // 1. First derive the analytics to be evaluated via the stream controller model 
            // NOTE: These take precedence of the child model metrics
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new BondStreamAnalytic();
            }
            var streamControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation streamValuation;
            // 2. Now evaluate only the child specific metrics (if any)
            foreach (var coupon in Coupons)
            {
                coupon.PricingStructureEvolutionType = PricingStructureEvolutionType;
                coupon.BucketedDates = BucketedDates;
                coupon.Multiplier = Multiplier;
            }
            var childControllers = new List<InstrumentControllerBase>(Coupons.ToArray());
            //Now the stream analytics can be completed.
            var childValuations = EvaluateChildMetrics(childControllers, modelData, Metrics);
            var couponValuation = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);
            var childControllerValuations = new List<AssetValuation> {couponValuation};
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (streamControllerMetrics.Count > 0)
            {
                var reportingCurrency = ModelData.ReportingCurrency == null ? Currency : ModelData.ReportingCurrency.Value;
                var notionals = GetCouponNotionals();
                var accrualFactors = GetCouponAccrualFactors();
                var discountFactors = GetPaymentDiscountFactors();
                var floatingNPV = AggregateMetric(InstrumentMetrics.FloatingNPV, childControllerValuations);
                var accrualFactor = AggregateMetric(InstrumentMetrics.AccrualFactor, childControllerValuations);
                //TODO need to  set the notional amount and the weighting. Also amortisation??
                IBondStreamParameters analyticModelParameters = new BondStreamParameters
                {
                    Multiplier = Multiplier,
                    CouponNotionals = notionals,
                    Currency = Currency,
                    ReportingCurrency = reportingCurrency,
                    AccrualFactor = accrualFactor,
                    FloatingNPV = floatingNPV,
                    NPV = AggregateMetric(InstrumentMetrics.NPV, childControllerValuations),
                    CouponYearFractions = accrualFactors,
                    PaymentDiscountFactors = discountFactors,
                    TargetNPV = floatingNPV
                };
                CalculationResults = AnalyticsModel.Calculate<IBondStreamInstrumentResults, BondStreamInstrumentResults>(analyticModelParameters, streamControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var streamControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                streamValuation = AssetValuationHelper.UpdateValuation(streamControllerValuation,
                                                                       childControllerValuations, ConvertMetrics(streamControllerMetrics), new List<string>(Metrics), PaymentCurrencies);
                AnalyticModelParameters = analyticModelParameters;
            }
            else
            {
                streamValuation = AssetValuationHelper.AggregateMetrics(childControllerValuations, new List<string>(Metrics), PaymentCurrencies);
            }
            CalculationPerformedIndicator = true;
            streamValuation.id = Id;
            return streamValuation;
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return null;
        }

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <param name="childValuations"> </param>
        /// <returns></returns>
        protected Decimal AggregateMetric(InstrumentMetrics metric, List<AssetValuation> childValuations)
        {
            var result = 0.0m;
            if (childValuations != null && childValuations.Count > 0)
            {
                result = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
            }
            return result;
        }

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="newCurveName">New name of the curve.</param>
        public void UpdateDiscountCurveName(string newCurveName)
        {
            foreach (PriceableRateCoupon coupon in Coupons)
            {
                coupon.UpdateDiscountCurveName(newCurveName);
            }
            BondCurveName = newCurveName;
        }

        #endregion

        #region Helper Methods

        public decimal[] GetCouponNotionals()
        {
            var result = new List<decimal>();
            Coupons.ForEach(cashflow => result.Add(cashflow.Notional));
            return result.ToArray();
        }

        public decimal[] GetCouponAccrualFactors()
        {
            var result = new List<decimal>();
            Coupons.ForEach(cashflow => result.Add(cashflow.CouponYearFraction));
            return result.ToArray();
        }

        public decimal[] GetPaymentDiscountFactors()
        {
            var result = new List<decimal>();
            Coupons.ForEach(cashflow => result.Add(cashflow.PaymentDiscountFactor));
            return result.ToArray();
        }

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="childValuations">The metrics.</param>
        /// <param name="controllerMetrics">The controller metrics</param>
        /// <returns></returns>
        protected static AssetValuation AggregateMetrics(List<AssetValuation> childValuations, List<string> controllerMetrics)
        {
            var result = new AssetValuation();
            var quotes = new List<Quotation>();
            foreach (var metric in controllerMetrics)
            {
                var quote = new Quotation();
                var measure = new AssetMeasureType { Value = metric };
                quote.measureType = measure;
                quote.value = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
                quote.valueSpecified = true;
                quotes.Add(quote);
            }
            result.quote = quotes.ToArray();
            return result;
        }

        #endregion

        #region Static Helpers

        protected void UpdateCouponDiscountCurveNames()
        {
            foreach (var coupon in Coupons)
            {
                coupon.DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency, true);
            }
        }
        protected void  UpdateCouponIds()
        {
            var index = 1;
            foreach (var coupon in Coupons)
            {
                coupon.Id = Id + "." + coupon.CashflowType.Value + "." + index;
                index++;
            }
        }

        protected static string BuildId(string swapId, CouponStreamType couponStreamType)//, string payerPartyReference)
        {
            const string cUnknown = "UNKNOWN";
            string swapIdentifier = string.IsNullOrEmpty(swapId) ? cUnknown : swapId;
            //return string.Format("{0}.{1}.{2}", swapIdentifier, couponStreamType, payerPartyReference);
            return $"{swapIdentifier}.{couponStreamType}";
        }

        internal static List<IPriceableInstrumentController<PaymentCalculationPeriod>> MapCoupons(List<PriceableRateCoupon> coupons)
        {
            return coupons.Cast<IPriceableInstrumentController<PaymentCalculationPeriod>>().ToList();
        }

        internal static List<IPriceableInstrumentController<PrincipalExchange>> MapExchanges(List<PriceablePrincipalExchange> exchanges)
        {
            return exchanges.Cast<IPriceableInstrumentController<PrincipalExchange>>().ToList();
        }

        //It is assumed that all coupons are ordered and there is at least one.
        public DateTime LastCouponDate()
        {
            return Coupons[Coupons.Count - 1].AccrualEndDate;
        }

        /// <summary>
        /// Intervals the string.
        /// </summary>
        /// <param name="periodLength">Length of the period.</param>
        /// <param name="periodTimeUnit">The period time unit.</param>
        /// <returns></returns>
        protected static string IntervalString(string periodLength, PeriodEnum periodTimeUnit)
        {
            return $"{periodLength}{periodTimeUnit}";
        }

        /// <summary>
        /// Businesses the centers string.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        protected static string BusinessCentersString(List<string> businessCenters)
        {
            return BusinessCentersString(businessCenters.ToArray());
        }

        /// <summary>
        /// Businesses the centers as list.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        internal static List<string> BusinessCentersAsList(BusinessCenter[] businessCenters)
        {
            return businessCenters.Select(bc => bc.Value).ToList();
        }

        /// <summary>
        /// Businesses the centers string.
        /// </summary>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        protected static string BusinessCentersString(string[] businessCenters)
        {
            return string.Join("-", businessCenters);
        }

        /// <summary>
        /// Updates the rate.
        /// </summary>
        /// <param name="newRate">The new rate.</param>
        public void UpdateRate(Decimal newRate)
        {
            foreach (var priceableCoupon in Coupons)
            {
                if (priceableCoupon.GetType() == typeof(PriceableFixedRateCoupon))
                {
                    var coupon = (PriceableFixedRateCoupon) priceableCoupon;
                    coupon.Rate = newRate;
                }
            }
        }

        /// <summary>
        /// Removes the duplicates
        /// </summary>
        /// <param name="inputList">The input list.</param>
        /// <returns></returns>
        protected static List<T> RemoveDuplicates<T>(List<T> inputList)
        {
            var uniqueStore = new List<T>();
            foreach (T currencyValue in inputList)
            {
                if (!uniqueStore.Contains(currencyValue))
                {
                    uniqueStore.Add(currencyValue);
                }
            }
            return uniqueStore;
        }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            var children = Coupons.Cast<InstrumentControllerBase>().ToList();
            return children;
        }

        #endregion

    }
}