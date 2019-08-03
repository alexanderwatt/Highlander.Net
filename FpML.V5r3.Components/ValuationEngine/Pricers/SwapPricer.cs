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
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Helpers;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.ValuationEngine.Instruments;
using FpML.V5r3.Reporting;
using Orion.Models.Rates.Swap;
using FpML.V5r3.Codes;
using Orion.Analytics.Utilities;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Instruments.InterestRates;
using Orion.ValuationEngine.Factory;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    public abstract class SwapPricer : InstrumentControllerBase, IPriceableInterestRateSwap<IIRSwapInstrumentParameters, IIRSwapInstrumentResults>, IPriceableInstrumentController<Swap> 
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether [base party paying fixed].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [base party paying fixed]; otherwise, <c>false</c>.
        /// </value>
        public bool BasePartyPayingFixed { get; set; }

        /// <summary>
        /// The type of swap: fixed/float, float/float, fixed/fixed.
        /// </summary>
        public SwapType SwapType { get; set; }

        /// <summary>
        /// Gets the effective date.
        /// </summary>
        /// <value>The effective date.</value>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets the termination date.
        /// </summary>
        /// <value>The termination date.</value>
        public DateTime TerminationDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether [adjust calculation dates indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [adjust calculation dates indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool AdjustCalculationDatesIndicator { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<IIRSwapInstrumentParameters, SwapInstrumentMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        protected const string CModelIdentifier = "Swap";
        //protected const string CDefaultBucketingInterval = "3M";

        // Requirements for pricing
        public string DiscountCurveName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ForecastCurveName { get; set; }

        //// BucketedCouponDates
        protected IDictionary<string, DateTime[]> BucketCouponDates = new Dictionary<string, DateTime[]>();

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IIRSwapInstrumentParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PriceableInterestRateStream> Legs = new List<PriceableInterestRateStream>();

        /// <summary>
        /// 
        /// </summary>
        public List<PriceablePayment> AdditionalPayments { get; protected set; }

        #endregion

        #region Constructors

        protected SwapPricer()
        {}

        protected SwapPricer(ILogger logger, ICoreCache cache, String nameSpace,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars, 
            Swap swapFpML, string basePartyReference, ProductTypeSimpleEnum productType)
            : this(logger, cache, nameSpace, legCalendars, swapFpML, basePartyReference, productType, false)
        {}

        protected SwapPricer(ILogger logger, ICoreCache cache, String nameSpace,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars,
            Swap swapFpML, string basePartyReference, 
            ProductTypeSimpleEnum productType, Boolean forecastRateInterpolation)
        {
            Multiplier = 1.0m;
            if (swapFpML == null) return;
            BusinessCentersResolver.ResolveBusinessCenters(swapFpML);
            ForecastRateInterpolation = forecastRateInterpolation;
            //Get the effective date
            AdjustableDate adjustableEffectiveDate = XsdClassesFieldResolver.CalculationPeriodDatesGetEffectiveDate(swapFpML.swapStream[0].calculationPeriodDates);
            EffectiveDate = adjustableEffectiveDate.unadjustedDate.Value;
            //We make the assumption that the termination date is the same for all legs.
            AdjustableDate adjustableTerminationDate = XsdClassesFieldResolver.CalculationPeriodDatesGetTerminationDate(swapFpML.swapStream[0].calculationPeriodDates);
            var paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, adjustableTerminationDate.dateAdjustments.businessCenters, nameSpace);
            TerminationDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, adjustableTerminationDate);
            RiskMaturityDate = TerminationDate;
            //EffectiveDate is not set;
            ProductType = productType;
            PaymentCurrencies = new List<string>();
            //Resolve the payer
            var legs = swapFpML.swapStream.Length;
            if (legs == 0) return;
            var flag = false;
            var index = 0;
            if (legCalendars != null && legCalendars.Count == legs)
            {
                flag = true;
            }
            foreach (var swapStream in swapFpML.swapStream)
            {
                bool payerIsBase = basePartyReference == swapStream.payerPartyReference.href;//TODO add in the calendar functionality.
                //Set the id of the first stream.
                PriceableInterestRateStream leg = flag ? new PriceableInterestRateStream(logger, cache, nameSpace, payerIsBase, swapStream, ForecastRateInterpolation, legCalendars[index].First, legCalendars[index].Second)
                    : new PriceableInterestRateStream(logger, cache, nameSpace, payerIsBase, swapStream, ForecastRateInterpolation, null, null);
                Legs.Add(leg);
                //Add the currencies for the trade pricer.
                if (!PaymentCurrencies.Contains(leg.Currency.Value))
                {
                    PaymentCurrencies.Add(leg.Currency.Value);
                }
                index++;
            }
            if (swapFpML.additionalPayment != null)
            {
                AdditionalPayments = PriceableInstrumentsFactory.CreatePriceablePayments(basePartyReference, swapFpML.additionalPayment, null);
                foreach (var payment in swapFpML.additionalPayment)
                {
                    if (!PaymentCurrencies.Contains(payment.paymentAmount.currency.Value))
                    {
                        PaymentCurrencies.Add(payment.paymentAmount.currency.Value);
                    }
                }
            }
        }

        #endregion

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        /// <summary>
        /// Builds this instance and returns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        public Swap Build()
        {
            var swap = new Swap
                           {
                               Items = new object[] {new ProductType {Value = ProductType.ToString()}},
                               ItemsElementName = new[] { ItemsChoiceType2.productType},
                               additionalPayment = MapToPayments(AdditionalPayments),
                               swapStream = Legs.Select(leg => leg.Build()).ToArray()
                           };
            return swap;
        }

        #endregion

        #region Implementation of IMetricsCalculation<IRateCouponParameters,IRateInstrumentResults>

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IIRSwapInstrumentResults CalculationResults { get; protected set; }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            var children = GetLegs();
            var payments = GetAdditionalPayments();
            if (children != null)
            {
                if (payments!=null)
                {
                    children.AddRange(GetAdditionalPayments());
                }
                return children;
            }
            return payments;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public List<InstrumentControllerBase> GetLegs()
        {
            return Legs != null ? new List<InstrumentControllerBase>(Legs.ToArray()) : null;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public IList<InstrumentControllerBase> GetAdditionalPayments()
        {
            return AdditionalPayments?.Cast<InstrumentControllerBase>().ToList();
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public IList<PriceableInterestRateStream> GetInstrumentControllers()
        {
            return Legs != null ? GetLegs().Cast<PriceableInterestRateStream>().ToList() : null;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public List<PriceableRateCoupon> GetCouponControllers()
        {
            return Legs.SelectMany(controller => controller.PriceableCoupons).Cast<PriceableRateCoupon>().ToList();
        }

        #endregion

        #region Static Helpers

        public Payment[] MapToPayments(List<PriceablePayment> payments)
        {
            return payments?.Select(payment => payment.Build()).ToArray();
        }

        /// <summary>
        /// Aggregates the coupon metric.
        /// </summary>
        /// <param name="childValuations">The metrics.</param>
        /// <param name="controllerMetrics">THe controller metrics</param>
        /// <returns></returns>
        protected static AssetValuation AggregateCouponMetrics(List<AssetValuation> childValuations, List<string> controllerMetrics)
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

        ///// <summary>
        ///// Aggregates the coupon metric.
        ///// </summary>
        ///// <param name="metric">The metric.</param>
        ///// <returns></returns>
        //virtual protected Decimal AggregateCouponMetric(InstrumentMetrics metric)
        //{
        //    string[] metrics = { metric.ToString() };
        //    var childValuations = GetChildValuations(GetCouponControllers().ToArray(), new List<string>(metrics), ModelData);
        //    var results = new List<decimal>();
        //    if (childValuations != null)
        //    {
        //        results.AddRange(childValuations.Select(valuation => Aggregator.SumDecimals(GetMetricResults(valuation, metric))));
        //    }
        //    decimal result = Aggregator.SumDecimals(results.ToArray());
        //    return result;
        //}

        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            var bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out _, out _));
            return bucketDates.ToArray();
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return Build();
        }

        #endregion
    }
}
