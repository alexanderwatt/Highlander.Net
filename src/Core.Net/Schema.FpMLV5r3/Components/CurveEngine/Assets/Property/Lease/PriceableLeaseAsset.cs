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
using System.Globalization;
using System.Linq;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.CalendarEngine.V5r3.Schedulers;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.Markets;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.Property.Lease;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Property.Lease
{
    /// <summary>
    /// PriceableBondAsset
    /// </summary>
    public class PriceableLeaseAsset : PriceableLeaseAssetController
    {
        private const decimal CDefaultWeightingValue = 1.0m;

        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        ///<summary>
        ///</summary>
        public string ModelIdentifier { get; set; }

        ///<summary>
        ///</summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IBusinessCalendar PaymentCalendar { get; set; }

        /// <summary>
        /// The asset swap valuation curve.
        /// </summary>
        public string LeaseDiscountCurveName { get; set; }

        /// <summary>
        /// The coupon rate. If this is a floater it would be the last reset rate.
        /// </summary>
        public decimal LeaseRate { get; set; }

        /// <summary>
        /// The instrument identifiers.
        /// </summary>
        public List<InstrumentId> InstrumentIds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected decimal[] Weightings;

        /// <summary>
        /// 
        /// </summary>
        public decimal[] ExpectedCashflows;

        /// <summary>
        /// 
        /// </summary>
        public decimal[] CashflowPVs;

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal[] YearFractions { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableLeaseAsset"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="marketQuote">The market quote.</param>
        /// <param name="lease">The lease.</param>
        public PriceableLeaseAsset(DateTime baseDate, Reporting.V5r3.Lease lease, IBusinessCalendar paymentCalendar, 
            BasicQuotation marketQuote)
        {
            PaymentCalendar = paymentCalendar;
            Multiplier = 1.0m;
            YearFractions = new[] {0.25m};
            ModelIdentifier = "LeaseAsset";
            StartAmount = lease.startGrossPrice.amount;
            MaturityDate = lease.leaseExpiryDate.Value;
            Frequency = lease.paymentFrequency;
            LeaseRate = lease.reviewChange;
            Currency = lease.currency;
            BaseDate = baseDate;
            SettlementDate = baseDate;
            PaymentBusinessDayAdjustments = lease.businessDayAdjustments;
            FirstPaymentDate = lease.startDate.Value;
            ReviewFrequency = lease.reviewFrequency;
            NextReviewDate = lease.nextReviewDate.Value;
            if (MaturityDate > BaseDate)
            {
                var rollConvention =
                    RollConventionEnumHelper.Parse(MaturityDate.Day.ToString(CultureInfo.InvariantCulture));
                UnAdjustedPeriodDates = DateScheduler.GetUnadjustedCouponDatesFromMaturityDate(SettlementDate,
                    MaturityDate,
                    Frequency,
                    rollConvention,
                    out _,
                    out var nextCouponDate);
                LastPaymentDate = UnAdjustedPeriodDates[0];
                AdjustedPeriodDates =
                    AdjustedDateScheduler.GetAdjustedDateSchedule(UnAdjustedPeriodDates,
                        PaymentBusinessDayAdjustments.businessDayConvention,
                        paymentCalendar).ToArray();
                AdjustedPeriodDates[0] = SettlementDate;
                NextPaymentDate = nextCouponDate;
            }
            RiskMaturityDate = AdjustedPeriodDates[AdjustedPeriodDates.Length - 1];
            Weightings =
                CreateWeightings(CDefaultWeightingValue, AdjustedPeriodDates.Length - 1);//, LeaseRate, 
            //TODO Need to add a credit spread to this.
            LeaseDiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency.Value, true);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new LeaseAssetAnalytic();
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so that's all we evaluate - every other metric is ignored
            var metricsToEvaluate = metrics.ToArray();
            var analyticModelParameters = new LeaseAssetParameters();
            CalculationResults = new LeaseAssetAnalytic();
            var marketEnvironment = modelData.MarketEnvironment;
            //IRateCurve rate forecast curve = null;
            IRateCurve leaseCurve = null;
            //0. Set the valuation date and recalculate the settlement date. This could mean regenerating all the coupon dates as well
            //Alternatively the lease can be recreated with a different base date = valuation date.
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                leaseCurve = (IRateCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                LeaseDiscountCurveName = leaseCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleRateMarketEnvironment))
            {
                leaseCurve = ((ISimpleRateMarketEnvironment)marketEnvironment).GetRateCurve();
                LeaseDiscountCurveName = leaseCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                leaseCurve = ((ISwapLegEnvironment)marketEnvironment).GetDiscountRateCurve();
                LeaseDiscountCurveName = leaseCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                leaseCurve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(LeaseDiscountCurveName);
            } 
            //2. Set the rate and the Multiplier
            analyticModelParameters.Multiplier = Multiplier;
            //analyticModelParameters.Quote = QuoteValue;
            analyticModelParameters.GrossAmount = StartAmount;
            analyticModelParameters.StepUp = LeaseRate;
            //3. Get the discount factors
            analyticModelParameters.PaymentDiscountFactors =
                GetDiscountFactors(leaseCurve, AdjustedPeriodDates, modelData.ValuationDate);
            //4. Get the Weightings
            analyticModelParameters.Weightings = Weightings;
            //5. Set the analytic input parameters and Calculate the respective metrics 
            AnalyticModelParameters = analyticModelParameters;
            CalculationResults =
                AnalyticsModel.Calculate<ILeaseAssetResults, LeaseAssetResults>(analyticModelParameters,
                                                                              metricsToEvaluate);
            //ExpectedCashflows = 
            //CashflowPVs = 
            return GetValue(CalculationResults);
        }

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return RiskMaturityDate;
        }

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the discount factors.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal[] GetDiscountFactors(IRateCurve discountFactorCurve, DateTime[] periodDates,
                                            DateTime valuationDate)
        {
            return periodDates.Select(periodDate => GetDiscountFactor(discountFactorCurve, periodDate, valuationDate)).ToArray();
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal GetDiscountFactor(IRateCurve discountFactorCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal) discountFactorCurve.Value(point);
            return discountFactor;
        }

        /// <summary>
        /// Creates the weightings.
        /// </summary>
        /// <param name="weightingValue">The weighting value.</param>
        /// <param name="noOfInstances">The no of instances.</param>
        /// <returns></returns>
        private static decimal[] CreateWeightings(decimal weightingValue, int noOfInstances)
        {
            var weights = new List<decimal>();
            for (var index = 0; index < noOfInstances; index++)
            {
                weights.Add(weightingValue);
            }
            return weights.ToArray();
        }

        ///<summary>
        ///</summary>
        public DateTime GetMaturityDate()
        {
            return MaturityDate;
        }

        ///<summary>
        ///</summary>
        public DateTime GetSettlementDate(DateTime baseDate, IBusinessCalendar settlementCalendar, RelativeDateOffset settlementDateOffset)
        {
            try
            {
                return settlementCalendar.Advance(baseDate, settlementDateOffset, settlementDateOffset.businessDayConvention);
            }
            catch (System.Exception)
            {
                throw new System.Exception("No settlement calendar set.");
            }          
        }

        ///<summary>
        ///</summary>
        public override Reporting.V5r3.Lease GetLease()
        {
            var lease = new Reporting.V5r3.Lease
            {
                currency = new IdentifiedCurrency { Value = Currency.Value, id = "Currency" },
                definition = null,
                description = Description,
                id = Id,
                leaseTenor = LeaseTenor,
                leaseExpiryDate = IdentifiedDateHelper.Create("MaturityDate", MaturityDate),
                paymentFrequency = Frequency,
            };
            if (InstrumentIds != null)
            {
                lease.instrumentId = InstrumentIds.ToArray();
            }
            return lease;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override DateTime GetNextLeasePaymentDate()
        {
            return NextLeasePaymentDate;
        }

        /// <summary>
        /// Gets the year fractions for dates.
        /// </summary>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="dayCountFraction">The day count fraction.</param>
        /// <returns></returns>
        public static decimal[] GetYearFractionsForDates(IList<DateTime> periodDates, DayCountFraction dayCountFraction)
        {
            var yearFractions = new List<decimal>();
            var index = 0;
            var periodDatesLastIndex = periodDates.Count - 1;
            foreach (var periodDate in periodDates)
            {
                if (index == periodDatesLastIndex)
                    break;
                var yearFraction =
                    (decimal)
                    DayCounterHelper.Parse(dayCountFraction.Value).YearFraction(periodDate,
                                                                                       periodDates[index + 1]);
                yearFractions.Add(yearFraction);
                index++;
            }
            return yearFractions.ToArray();
        }
    }
}