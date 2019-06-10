/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Schedulers;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.Models.Assets;
using MarketQuoteHelper = Orion.CurveEngine.Helpers.MarketQuoteHelper;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableSimpleIRSwap : PriceableSwapRateAsset
    {
        #region Properties

        /// <summary>
        /// Gets the index of the underlying rate.
        /// </summary>
        /// <value>The index of the underlying rate.</value>
        public RateIndex UnderlyingRateIndex { get; protected set; }

        /// <summary>
        /// Gets the discounting type.
        /// </summary>
        /// <value>The discounting type.</value>
        public DiscountingTypeEnum? DiscountingType => Calculation.discounting?.discountingType;

        #endregion

        #region Constructors - Single leg logic

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="discountingType">The discounting type.</param>
        /// <param name="effectiveDate">The base date.</param>
        /// <param name="tenor">The maturity tenor.</param>
        /// <param name="fxdDayFraction">The fixed leg day fraction.</param>
        /// <param name="businessCenters">The payment business centers.</param>
        /// <param name="businessDayConvention">The payment business day convention.</param>
        /// <param name="fxdFrequency">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="currency">THe currency.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableSimpleIRSwap(string id, DateTime baseDate, string currency,
                                     decimal amount, DiscountingTypeEnum? discountingType,
                                     DateTime effectiveDate, string tenor, string fxdDayFraction,
                                     string businessCenters, string businessDayConvention, string fxdFrequency,
                                     RateIndex underlyingRateIndex, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : this(baseDate,  SimpleIrsHelper.Parse(id, currency, fxdDayFraction, tenor, fxdFrequency, id), effectiveDate,
                   CalculationFactory.CreateFixed(fixedRate.value, MoneyHelper.GetAmount(amount, currency),
                                                  DayCountFractionHelper.Parse(fxdDayFraction), discountingType),
                   BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters), underlyingRateIndex, fixingCalendar, paymentCalendar, fixedRate) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="simpleIRSwap"></param>
        /// <param name="spotDateOffset"></param>
        /// <param name="calculation"></param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableSimpleIRSwap(DateTime baseDate, SimpleIRSwap simpleIRSwap, RelativeDateOffset spotDateOffset, 
                                    Calculation calculation, BusinessDayAdjustments businessDayAdjustments, RateIndex underlyingRateIndex,
                                      IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, businessDayAdjustments, calculation, fixedRate)
        {
            Id = simpleIRSwap.id;
            FixingDateOffset = spotDateOffset;
            SimpleIRSwap = simpleIRSwap;
            UnderlyingRateIndex = underlyingRateIndex;
            //TODO: This needs to be modified to use backwards roll.
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, FixingDateOffset);
            List<DateTime> unadjustedDateSchedule = DateScheduler.GetUnadjustedDateSchedule(AdjustedStartDate, SimpleIRSwap.term, SimpleIRSwap.paymentFrequency);
            AdjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDateSchedule, BusinessDayAdjustments.businessDayConvention, paymentCalendar);
            AdjustedStartDate = AdjustedPeriodDates[0];
            RiskMaturityDate = AdjustedPeriodDates[AdjustedPeriodDates.Count - 1];
            Weightings = CreateWeightings(CDefaultWeightingValue, AdjustedPeriodDates.Count - 1);
            YearFractions = GetYearFractions();
            ModelIdentifier = DiscountingType == null ? "SimpleSwapAsset" : "SimpleDiscountSwapAsset";
            var timeToMaturity = Actual365.Instance.YearFraction(baseDate, RiskMaturityDate);
            EndDiscountFactor = (decimal)System.Math.Exp(-(double)fixedRate.value * timeToMaturity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct;</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableSimpleIRSwap(DateTime baseDate, SimpleIRSwapNodeStruct nodeStruct,
                                     IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, nodeStruct.DateAdjustments, nodeStruct.Calculation, fixedRate)
        {
            Id = nodeStruct.SimpleIRSwap.id;
            FixingDateOffset = nodeStruct.SpotDate;
            SimpleIRSwap = nodeStruct.SimpleIRSwap;
            UnderlyingRateIndex = nodeStruct.UnderlyingRateIndex;
            //TODO: This needs to be modified to use backwards roll.
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, FixingDateOffset);
            List<DateTime> unadjustedDateSchedule = DateScheduler.GetUnadjustedDateSchedule(AdjustedStartDate, SimpleIRSwap.term, SimpleIRSwap.paymentFrequency);
            AdjustedPeriodDates =
                AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDateSchedule, BusinessDayAdjustments.businessDayConvention, paymentCalendar);         
            RiskMaturityDate = AdjustedPeriodDates[AdjustedPeriodDates.Count - 1];
            Weightings = CreateWeightings(CDefaultWeightingValue, AdjustedPeriodDates.Count - 1);
            YearFractions = GetYearFractions();
            ModelIdentifier = DiscountingType == null ? "SimpleSwapAsset" : "SimpleDiscountSwapAsset";
            var timeToMaturity = Actual365.Instance.YearFraction(baseDate, RiskMaturityDate);
            EndDiscountFactor = (decimal)System.Math.Exp(-(double)fixedRate.value * timeToMaturity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="simpleIRSwap"></param>
        /// <param name="spotDate"></param>
        /// <param name="calculation"></param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableSimpleIRSwap(DateTime baseDate, SimpleIRSwap simpleIRSwap,
                                     DateTime spotDate, Calculation calculation,
                                     BusinessDayAdjustments businessDayAdjustments, RateIndex underlyingRateIndex,
                                     IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, businessDayAdjustments, calculation, fixedRate)
        {
            Id = simpleIRSwap.id;
            SimpleIRSwap = simpleIRSwap;
            UnderlyingRateIndex = underlyingRateIndex;
            AdjustedStartDate = spotDate;
            List<DateTime> unadjustedDateSchedule = DateScheduler.GetUnadjustedDateSchedule(AdjustedStartDate, SimpleIRSwap.term, SimpleIRSwap.paymentFrequency);
            AdjustedPeriodDates =
                AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDateSchedule, BusinessDayAdjustments.businessDayConvention, paymentCalendar);
            RiskMaturityDate = AdjustedPeriodDates[AdjustedPeriodDates.Count - 1];
            Weightings = CreateWeightings(CDefaultWeightingValue, AdjustedPeriodDates.Count - 1);
            YearFractions = GetYearFractions();
            ModelIdentifier = DiscountingType == null ? "SimpleSwapAsset" : "SimpleDiscountSwapAsset";
            var timeToMaturity = Actual365.Instance.YearFraction(baseDate, RiskMaturityDate);
            EndDiscountFactor = (decimal)System.Math.Exp(-(double)fixedRate.value * timeToMaturity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="simpleIRSwap">THe FpML swap.</param>
        /// <param name="spotDate">The spot date.</param>
        /// <param name="calculation">A calculation.</param>
        /// <param name="stringRollConvention">The roll convention.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableSimpleIRSwap(DateTime baseDate, SimpleIRSwap simpleIRSwap,
                                     DateTime spotDate, Calculation calculation, String stringRollConvention,
                                     BusinessDayAdjustments businessDayAdjustments, RateIndex underlyingRateIndex,
                                     IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, businessDayAdjustments, calculation, fixedRate)
        {
            Id = simpleIRSwap.id;
            SimpleIRSwap = simpleIRSwap;
            UnderlyingRateIndex = underlyingRateIndex;
            AdjustedStartDate = spotDate;
            List<DateTime> unadjustedDateSchedule = DateScheduler.GetUnadjustedDateSchedule(AdjustedStartDate, SimpleIRSwap.term, SimpleIRSwap.paymentFrequency);
            AdjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDateSchedule, BusinessDayAdjustments.businessDayConvention, paymentCalendar);
            RiskMaturityDate = AdjustedPeriodDates[AdjustedPeriodDates.Count - 1];
            Weightings = CreateWeightings(CDefaultWeightingValue, AdjustedPeriodDates.Count - 1);
            YearFractions = GetYearFractions();
            ModelIdentifier = DiscountingType == null ? "SimpleSwapAsset" : "SimpleDiscountSwapAsset";
            var timeToMaturity = Actual365.Instance.YearFraction(baseDate, RiskMaturityDate);
            EndDiscountFactor = (decimal)System.Math.Exp(-(double)fixedRate.value * timeToMaturity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="identifier">The asset identifier.</param>
        /// <param name="adjustedDates">The dates.</param>
        /// <param name="notionalWeights">The notionals, weighted by the first notional.</param>
        /// <param name="calculation">A calculation.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableSimpleIRSwap(DateTime baseDate, string identifier, DateTime[] adjustedDates, Decimal[] notionalWeights,
                                     Calculation calculation, BusinessDayAdjustments businessDayAdjustments, RateIndex underlyingRateIndex,
                                     BasicQuotation fixedRate)
            : base(baseDate, businessDayAdjustments, calculation, fixedRate)
        {
            Id = identifier;
            SimpleIRSwap = new SimpleIRSwap
                               {
                                   dayCountFraction = calculation.dayCountFraction
                               };
            UnderlyingRateIndex = underlyingRateIndex;
            RiskMaturityDate = adjustedDates[adjustedDates.Length - 1];
            AdjustedPeriodDates = new List<DateTime>(adjustedDates);
            AdjustedStartDate = AdjustedPeriodDates[0];
            Weightings = notionalWeights;
            YearFractions = GetYearFractions();
            ModelIdentifier = DiscountingType == null ? "SimpleSwapAsset" : "SimpleDiscountSwapAsset";
            var timeToMaturity = Actual365.Instance.YearFraction(baseDate, RiskMaturityDate);
            EndDiscountFactor = (decimal)System.Math.Exp(-(double)fixedRate.value * timeToMaturity);
        }

        #endregion

        #region Overrides of PriceableRateSpreadAssetController

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                switch (ModelIdentifier)
                {
                    case "SimpleSwapAsset":
                        AnalyticsModel = new SimpleSwapAssetAnalytic();
                        break;
                    case "SimpleDiscountSwapAsset":
                        AnalyticsModel = new SimpleDiscountSwapAssetAnalytic();
                        break;
                }
            }
            ISwapAssetParameters analyticModelParameters = new IRSwapAssetParameters
            {
                NotionalAmount = Notional,
                //2. Get the discount factors
                DiscountFactors =
                    GetDiscountFactors(interpolatedSpace,
                                       AdjustedPeriodDates.ToArray(),
                                       BaseDate),
                //3. Get the respective year fractions
                YearFractions = YearFractions,
                Rate =
                    MarketQuoteHelper.NormalisePriceUnits(
                    FixedRate, "DecimalRate").value
            };
            //4. Get the Weightings
            analyticModelParameters.Weightings =
                CreateWeightings(CDefaultWeightingValue, analyticModelParameters.DiscountFactors.Length - 1);
            AnalyticResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            if (AnalyticsModel != null)
                AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote + Spread.value;
        }

        #endregion
    }
}