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
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Utilities.Logging;
using Highlander.Reporting.V5r3;
using Highlander.Constants;
using Highlander.Reporting.Analytics.V5r3.Utilities;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Reporting.ModelFramework.V5r3.Instruments.Lease;
using Highlander.Reporting.Models.V5r3.Property.Lease;
using Highlander.ValuationEngine.V5r3.Factory;

#endregion

namespace Highlander.ValuationEngine.V5r3.Instruments
{
    [Serializable]
    public class PriceableLeasePaymentStream : InstrumentControllerBase, IPriceableLeasePaymentStream<ILeaseStreamParameters, ILeaseStreamInstrumentResults>, IPriceableInstrumentController<List<Payment>>
    {
        #region Member Fields

        // Analytics
        public IModelAnalytic<ILeaseStreamParameters, LeaseInstrumentMetrics> AnalyticsModel { get; set; }

        protected const string CModelIdentifier = "LeasePaymentStream";
        //protected const string CDefaultBucketingInterval = "3M";

        /// <summary>
        /// The bond identifier for discounting.
        /// </summary>
        public string LeaseId { get; set; }
        /// <summary>
        /// THe bond curve to use for valuation.
        /// </summary>
        public string LeaseCurveName { get; set; }


        //// BucketedCouponDates
        //public Period BucketingInterval { get; set; }
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();
        //public DateTime[] BucketedDates { get; set; }

        public List<DateTime> ReviewDates { get; set; }

        // Children
        public List<PriceablePayment> Payments { get; set; }

        #endregion

        #region Public Fields

        /// <summary>
        /// Gets the currency.
        /// </summary>
        /// <value>The currency.</value>
        public string Currency { get; protected set; }

        /// <summary>
        /// Gets the issuer.
        /// </summary>
        /// <value>The seller.</value>
        public string Tenant { get; protected set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public ILeaseStreamParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ILeaseStreamInstrumentResults CalculationResults { get; protected set; }

        /// <summary>
        /// Gets the priceable coupons.
        /// </summary>
        /// <value>The priceable coupons.</value>
        public List<InstrumentControllerBase> PriceablePayments
        {
            get 
            {
                List<InstrumentControllerBase> result = null;
                if (Payments != null && Payments.Count > 0)
                {
                    result = new List<InstrumentControllerBase>(Payments); 
                }
                return result;
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
                var datesList = Payments.Select(payment => payment.AdjustedPaymentDate.Value).ToList();
                datesList.Sort();
                return datesList;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public PriceableLeasePaymentStream()
        {
            Multiplier = 1.0m;
            AnalyticsModel = new LeaseStreamAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableLeasePaymentStream"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace.</param>
        /// <param name="leaseId">The lease Id.</param>
        /// <param name="paymentConvention">The payment roll conventions</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="payerIsBase">Is the payer/tenant the base party?</param>
        /// <param name="tradeDate">The trade date is used to set the base date for future coupon generation.</param>
        /// <param name="lease">The lease details.</param>
        /// <param name="payerPartyReference">The payer/tenant reference.</param>
        /// <param name="receiverPartyReference">The receiver reference.</param>
        public PriceableLeasePaymentStream
            (
            ILogger logger
            , ICoreCache cache
            , string nameSpace
            , string leaseId
            , string payerPartyReference
            , string receiverPartyReference
            , bool payerIsBase 
            , DateTime tradeDate
            , Lease lease
            , BusinessDayAdjustments paymentConvention
            , IBusinessCalendar paymentCalendar)
        {
            LeaseId = leaseId;
            Multiplier = 1.0m;
            PaymentCurrencies = new List<string>();
            AnalyticsModel = new LeaseStreamAnalytic();
            Id = BuildId(leaseId);
            Currency = lease.currency.Value;
            //Get the currency.
            if (!PaymentCurrencies.Contains(lease.currency.Value))
            {
                PaymentCurrencies.Add(lease.currency.Value);
            }
            //The calendars
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, paymentConvention.businessCenters, nameSpace);
            }
            //Set the default discount curve name.
            LeaseCurveName = CurveNameHelpers.GetDiscountCurveName(Currency, true);
            //LeaseCurveName = CurveNameHelpers.GetLeaseCurveName(Currency, leaseId);
            Payments = PriceableInstrumentsFactory.CreatePriceableLeasePayments(tradeDate, payerPartyReference, receiverPartyReference, payerIsBase, lease,
                paymentCalendar);
            UpdateDiscountCurveName(LeaseCurveName);
            UpdatePaymentIds();
            //RiskMaturityDate = ;
            logger.LogInfo("Lease Payment Stream built");
        }

        #endregion

        #region Payments

        /// <summary>
        /// Gets the stream dates.
        /// </summary>
        /// <returns></returns>
        public DateTime[] GetStreamDates()
        {
            var paymentDates = new List<DateTime>();
            foreach (var payment in Payments)
            {
                paymentDates.Add(payment.PaymentDate);
            }
            paymentDates.Sort();
            return paymentDates.ToArray();
        }

        /// <summary>
        /// Gets the bucketed payment dates.
        /// </summary>
        /// <param name="bucketInterval">The bucket interval.</param>
        /// <returns></returns>
        protected IDictionary<string, DateTime[]> GetBucketedPaymentDates(Period bucketInterval)
        {
            IDictionary<string, DateTime[]> bucketCouponDates = new Dictionary<string, DateTime[]>();
            var bucketDates = new List<DateTime>();
            foreach (var payment in Payments)
            {
                var priceablePayment = payment;
                bucketDates.Add(priceablePayment.PaymentDate);
                bucketDates = RemoveDuplicates(bucketDates);
                bucketCouponDates.Add(payment.Id, bucketDates.ToArray());
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
            if (Payments.Count > 1)
            {
                bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out _, out _));
            }
            return bucketDates.ToArray();
        }

        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds the cashflows.
        /// </summary>
        /// <returns></returns>
        public List<Payment> Build()
        {
            //TODO what about the cash flow match parameter?
            var payments = Payments.Select(priceablePayment => priceablePayment.Build()).ToList();
            return payments;
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
                AnalyticsModel = new LeaseStreamAnalytic();
            }
            var streamControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation streamValuation;
            // 2. Now evaluate only the child specific metrics (if any)
            foreach (var payment in Payments)
            {
                payment.PricingStructureEvolutionType = PricingStructureEvolutionType;
                payment.BucketedDates = BucketedDates;
                payment.Multiplier = Multiplier;
            }
            var childControllers = new List<InstrumentControllerBase>(Payments.ToArray());
            //Now the stream analytics can be completed.
            var childValuations = EvaluateChildMetrics(childControllers, modelData, Metrics);
            var couponValuation = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);
            var childControllerValuations = new List<AssetValuation> {couponValuation};
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (streamControllerMetrics.Count > 0)
            {
                var amounts = GetPaymentAmounts();
                var discountFactors = GetPaymentDiscountFactors();
                //TODO need to  set the notional amount and the weighting. Also amortisation??
                ILeaseStreamParameters analyticModelParameters = new LeaseStreamParameters
                {
                    Multiplier = Multiplier,
                    Amounts = amounts,
                    NPV = AggregateMetric(InstrumentMetrics.NPV, childControllerValuations),
                    PaymentDiscountFactors = discountFactors
                };
                CalculationResults = AnalyticsModel.Calculate<ILeaseStreamInstrumentResults, LeaseStreamInstrumentResults>(analyticModelParameters, streamControllerMetrics.ToArray());
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
        protected decimal AggregateMetric(InstrumentMetrics metric, List<AssetValuation> childValuations)
        {
            var result = 0.0m;
            if (childValuations != null && childValuations.Count > 0)
            {
                result = Aggregator.SumDecimals(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))).ToArray());
            }
            return result;
        }

        #endregion

        #region Helper Methods

        public decimal[] GetPaymentAmounts()
        {
            var result = new List<decimal>();
            Payments.ForEach(cashflow => result.Add(cashflow.ForecastAmount.amount));
            return result.ToArray();
        }

        public decimal[] GetPaymentDiscountFactors()
        {
            var result = new List<decimal>();
            Payments.ForEach(cashflow => result.Add(cashflow.PaymentDiscountFactor));
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

        public void UpdateDiscountCurveName(string name)
        {
            foreach (var payment in Payments)
            {
                payment.DiscountCurveName = name;
            }
        }
        protected void  UpdatePaymentIds()
        {
            var index = 1;
            foreach (var payment in Payments)
            {
                payment.Id = Id + "." + payment.CashflowType.Value + "." + index;
                index++;
            }
        }

        protected static string BuildId(string swapId)//, string payerPartyReference)
        {
            const string cUnknown = "UNKNOWN";
            string swapIdentifier = string.IsNullOrEmpty(swapId) ? cUnknown : swapId;
            //return string.Format("{0}.{1}.{2}", swapIdentifier, couponStreamType, payerPartyReference);
            return $"{swapIdentifier}";
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
        public DateTime LastPaymentDate()
        {
            return Payments[Payments.Count - 1].PaymentDate;
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
            var children = Payments.Cast<InstrumentControllerBase>().ToList();
            return children;
        }

        #endregion

    }
}