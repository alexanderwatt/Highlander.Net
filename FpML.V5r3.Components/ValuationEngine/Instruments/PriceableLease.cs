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
using Orion.Constants;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.Analytics.Helpers;
using Orion.Analytics.Utilities;
using Orion.Models.Rates.Stream;
using Orion.ValuationEngine.Generators;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Instruments.InterestRates;
using Orion.CalendarEngine.Helpers;
using Orion.ModelFramework.Instruments.Lease;
using Orion.Models.Property.Lease;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableLeaseTransaction : InstrumentControllerBase, IPriceableLeaseTransaction<ILeaseAssetParameters, ILeaseAssetResults>, IPriceableInstrumentController<LeaseTransaction>
    {
        #region Member Fields

        // Analytics
        public IModelAnalytic<ILeaseAssetParameters, LeaseAssetResults> AnalyticsModel { get; set; }

        protected const string CModelIdentifier = "PaymentStream";
        //protected const string CDefaultBucketingInterval = "3M";

        // Requirements for pricing
        public Currency Currency { get; set; }

        public string BasePartyCurveName { get; set; }

        public string CounterPartyCurveName { get; set; }

        public string DiscountCurveName { get; set; }

        public bool IsDiscounted { get; set; }

        // Requirements for pricing
        public string ReportingCurrencyFxCurveName { get; set; }

        //// BucketedCouponDates
        //public Period BucketingInterval { get; set; }
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();
        //public DateTime[] BucketedDates { get; set; }

        // THe payments
        public List<PriceablePayment> Payments { get; set; }


        #endregion

        #region Public Fields

        /// <summary>
        /// 
        /// </summary>
        public bool PayerIsBaseParty { get; set; }

        /// <summary>
        /// Gets the payer.
        /// </summary>
        /// <value>The payer.</value>
        public string Payer { get; protected set; }

        /// <summary>
        /// Gets the receiver.
        /// </summary>
        /// <value>The receiver.</value>
        public string Receiver { get; protected set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public ILeaseAssetParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ILeaseAssetResults CalculationResults { get; protected set; }


        ///// <summary>
        ///// Gets the priceable coupons.
        ///// </summary>
        ///// <value>The priceable coupons.</value>
        //public List<InstrumentControllerBase> Payments
        //{
        //    get 
        //    {
        //        List<InstrumentControllerBase> result = null;
        //        if (Payments != null && Payments.Count > 0)
        //        {
        //            result = new List<InstrumentControllerBase>(Payments); 
        //        }
        //        return result;
        //    }
        //}

        ///// <summary>
        ///// Gets the stream payment dates.
        ///// </summary>
        ///// <value>The stream payment dates.</value>
        //public IList<DateTime> StreamPaymentDates
        //{
        //    get
        //    {
        //        var datesList = Coupons.Select(coupon => coupon.PaymentDate).ToList();
        //        datesList.Sort();
        //        return datesList;
        //    }
        //}

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public PriceableLease()
        {
            Multiplier = 1.0m;
            AnalyticsModel = new LeaseAssetAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableInterestRateStream"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace.</param>
        /// <param name="swapId">The swap Id.</param>
        /// <param name="payerPartyReference">The payer party reference.</param>
        /// <param name="receiverPartyReference">The receiver party reference.</param>
        /// <param name="payerIsBase">The flag for whether the payer reference is the base party.</param>
        /// : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableLease
            (
            ILogger logger
            , ICoreCache cache
            , String nameSpace
            , string swapId
            , string payerPartyReference
            , string receiverPartyReference 
            , bool payerIsBase
            , IBusinessCalendar paymentCalendar)
        {
            Multiplier = 1.0m;
            Payer = payerPartyReference;
            Receiver = receiverPartyReference;
            PayerIsBaseParty = payerIsBase;
            PaymentCurrencies = new List<string>();
            AnalyticsModel = new StructuredStreamAnalytic();
            Id = BuildId(swapId, CouponStreamType);
            //Get the currency.
            var currency = XsdClassesFieldResolver.CalculationGetNotionalSchedule(Calculation);
            Currency = currency.notionalStepSchedule.currency;
            if (!PaymentCurrencies.Contains(Currency.Value))
            {
                PaymentCurrencies.Add(Currency.Value);
            }
            //The calendars
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, PaymentDates.paymentDatesAdjustments.businessCenters, nameSpace);
            }
            //Set the default discount curve name.
            DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency.Value, true);
            RiskMaturityDate = LastDate();
            logger.LogInfo("Stream built");
        }

        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public Lease Build()
        {
            var lease = new Lease
            {
                                  // cashflows = BuildCashflows(),
            };
            
            return lease;
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
                AnalyticsModel = new StructuredStreamAnalytic();
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
            var couponValuation = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            var childControllerValuations = new List<AssetValuation> {couponValuation};
            if (Exchanges != null && Exchanges.Count > 0)
            {
                foreach (var exchange in Exchanges)
                {
                    exchange.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    exchange.BucketedDates = BucketedDates;
                    exchange.Multiplier = Multiplier;
                }
                // Roll-Up and merge child valuations into parent Valuation
                var childPrincipalControllers = new List<InstrumentControllerBase>(Exchanges.ToArray());
                var childPrincipalValuations = EvaluateChildMetrics(childPrincipalControllers, modelData, Metrics);
                var principalValuation = AssetValuationHelper.AggregateMetrics(childPrincipalValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
                childControllerValuations.Add(principalValuation);
            }
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (streamControllerMetrics.Count > 0)
            {
                var reportingCurrency = ModelData.ReportingCurrency == null ? Currency.Value : ModelData.ReportingCurrency.Value;
                var notionals = GetCouponNotionals();
                var accrualFactors = GetCouponAccrualFactors();
                var discountFactors = GetPaymentDiscountFactors();
                var floatingNPV = AggregateMetric(InstrumentMetrics.FloatingNPV, childControllerValuations);
                var accrualFactor = AggregateMetric(InstrumentMetrics.AccrualFactor, childControllerValuations);
                //TODO need to  set the notional amount and the weighting. Also amortisation??
                IStructuredStreamParameters analyticModelParameters = new StructuredStreamParameters
                                                                          {   Multiplier = Multiplier,
                                                                              IsDiscounted = IsDiscounted,
                                                                              CouponNotionals = notionals,
                                                                              Currency = Currency.Value,
                                                                              ReportingCurrency = reportingCurrency,
                                                                              AccrualFactor = accrualFactor,
                                                                              FloatingNPV = floatingNPV,
                                                                              NPV = AggregateMetric(InstrumentMetrics.NPV, childControllerValuations),
                                                                              CouponYearFractions = accrualFactors,
                                                                              PaymentDiscountFactors = discountFactors,
                                                                              TargetNPV = floatingNPV
                                                                          };
                CalculationResults = AnalyticsModel.Calculate<IStreamInstrumentResults, StreamInstrumentResults>(analyticModelParameters, streamControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var streamControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                streamValuation = AssetValuationHelper.UpdateValuation(streamControllerValuation,
                                                                       childControllerValuations, ConvertMetrics(streamControllerMetrics), new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
                AnalyticModelParameters = analyticModelParameters;
            }
            else
            {
                streamValuation = AssetValuationHelper.AggregateMetrics(childControllerValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
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

        //public void AddCoupon(PriceableRateCoupon newFlow)
        //{
        //    Coupons.Add(newFlow);
        //}

        //public void AddPrincipalExchange(PriceablePrincipalExchange newFlow)
        //{
        //    Exchanges.Add(newFlow);
        //}

        ///// <summary>
        ///// Updates the name of the discount curve.
        ///// </summary>
        ///// <param name="newCurveName">New name of the curve.</param>
        //public void UpdateDiscountCurveName(string newCurveName)
        //{
        //    foreach (PriceableRateCoupon coupon in Coupons)
        //    {
        //        coupon.UpdateDiscountCurveName(newCurveName);
        //    }
        //    DiscountCurveName = newCurveName;
        //}

        #endregion

        #region Helper Methods

        //public decimal[] GetCouponNotionals()
        //{
        //    var result = new List<decimal>();
        //    Coupons.ForEach(cashflow => result.Add(cashflow.Notional));
        //    return result.ToArray();
        //}

        //public decimal[] GetCouponAccrualFactors()
        //{
        //    var result = new List<decimal>();
        //    Coupons.ForEach(cashflow => result.Add(cashflow.CouponYearFraction));
        //    return result.ToArray();
        //}

        //public decimal[] GetPaymentDiscountFactors()
        //{
        //    var result = new List<decimal>();
        //    Coupons.ForEach(cashflow => result.Add(cashflow.PaymentDiscountFactor));
        //    return result.ToArray();
        //}

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

        protected static Cashflows BuildCashflow(InterestRateStream stream, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var cashflows = stream.cashflows;
            if (stream.cashflows == null || stream.cashflows.cashflowsMatchParameters == false)
            {
                cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, fixingCalendar, paymentCalendar);
            }
            return cashflows;
        }

        protected static string BuildId(string leaseId)//, string payerPartyReference)
        {
            const string cUnknown = "UNKNOWN";
            string leaseIdentifier = string.IsNullOrEmpty(leaseId) ? cUnknown : leaseId;
            //return string.Format("{0}.{1}.{2}", swapIdentifier, couponStreamType, payerPartyReference);
            return $"{leaseIdentifier}";
        }

        //public DateTime LastDate()
        //{
        //    if (LastExchangeDate() == null) return LastCouponDate();
        //    var lastExchangeDate = LastExchangeDate();
        //    return lastExchangeDate != null && LastCouponDate() <= (DateTime)lastExchangeDate ? (DateTime)lastExchangeDate : LastCouponDate();
        //}

        ////It is assumed that all coupons are ordered and there is at least one.
        //public DateTime LastCouponDate()
        //{
        //    return Coupons[Coupons.Count - 1].AccrualEndDate;
        //}

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

        /////<summary>
        ///// Gets all the child controllers.
        /////</summary>
        /////<returns></returns>
        //public override IList<InstrumentControllerBase> GetChildren()
        //{
        //    var children = Coupons.Cast<InstrumentControllerBase>().ToList();
        //    if (Exchanges!=null)
        //    {
        //        children.AddRange(Exchanges);
        //    }
        //    return children;
        //}

        #endregion

    }
}