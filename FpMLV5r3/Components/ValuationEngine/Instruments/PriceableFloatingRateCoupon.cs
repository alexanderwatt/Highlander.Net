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
using System.Linq;
using System.Collections.Generic;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Reporting.V5r3;
using Highlander.Constants;
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.Reporting.Models.V5r3.Rates;
using Highlander.Reporting.Models.V5r3.Rates.Coupons;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;

#endregion

namespace Highlander.ValuationEngine.V5r3.Instruments
{
    [Serializable]
    public class PriceableFloatingRateCoupon : PriceableRateCoupon, IPriceableFloatingRateCoupon<IRateCouponParameters, IRateInstrumentResults>
    {
        #region Member Fields

        //Fixing
        public DateTime AdjustedFixingDate { get; set; }

        /// <summary>
        /// Start of the accrual period.
        /// </summary>
        public DateTime ForwardStartDate { get; protected set; }

        /// <summary>
        /// End of the accrual period.
        /// </summary>
        public DateTime ForwardEndDate { get; protected set; }

        /// <summary>
        /// Gets the base rate. USed for nettable FRA type cash flows..
        /// </summary>
        /// <value>The base rate.</value>
        public Decimal BaseRate { get; protected set; }

        /// <summary>
        /// Flag where: ForwardEndDate = forecastRateInterpolation ? AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);  
        /// </summary>
        public Boolean ForecastRateInterpolation { get; set; }

        #endregion

        #region Public Fields

        /// <summary>
        /// Gets the FixingCalendar.
        /// </summary>
        /// <value>The Fixing Calendar.</value>
        public IBusinessCalendar FixingCalendar { get; set; }

        /// <summary>
        /// The observed rate has been specified [true];
        /// </summary>
        public bool ObservedRateSpecified => RateObservation.observedRateSpecified;

        /// <summary>
        /// Gets or sets a value indicating whether [use observed rate].
        /// </summary>
        /// <value><c>true</c> if [use observed rate]; otherwise, <c>false</c>.</value>
        public bool UseObservedRate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires reset].
        /// </summary>
        /// <value><c>true</c> if [requires reset]; otherwise, <c>false</c>.</value>
        public bool RequiresReset { get; set; }

        /// <summary>
        /// Gets the rate observation.
        /// </summary>
        /// <value>The rate observation.</value>
        public RateObservation RateObservation { get; set; }

        /// <summary>
        /// Gets the forecast rate index.
        /// </summary>
        /// <value>The forecast rate index.</value>
        public ForecastRateIndex ForecastRateIndex { get; set;}

        /// <summary>
        /// Gets the fixing date offset.
        /// </summary>
        /// <value>The fixing date offset.</value>
        public RelativeDateOffset FixingDateRelativeOffset { get; set; }

        /// <summary>
        /// Gets the rate observation specified.
        /// </summary>
        /// <value>The rate observation specified.</value>
        public bool RateObservationSpecified => RateObservation.observedRateSpecified;

        /// <summary>
        /// Gets the observed rate.
        /// </summary>
        /// <value>The observed rate.</value>
        public decimal? ObservedRate => RateObservation.observedRateSpecified ? RateObservation.observedRate : (decimal?)null;

        /// <summary>
        /// Gets the name of the forward curve.
        /// </summary>
        /// <value>The name of the forward curve.</value>
        public string ForecastCurveName { get; protected set; }

        /// <summary>
        /// Gets the reset date.
        /// </summary>
        /// <value>The reset date.</value>
        public DateTime ResetDate => AdjustedFixingDate;

        /// <summary>
        /// Gets the reset relative to.
        /// </summary>
        /// <value>The reset relative to.</value>
        public ResetRelativeToEnum ResetRelativeTo { get; set; }

        /// <summary>
        /// Gets the name of the rate index.
        /// </summary>
        /// <value>The name of the rate index.</value>
        public string RateIndexName => ForecastRateIndex.floatingRateIndex.Value;

        /// <summary>
        /// Gets the rate index tenor.
        /// </summary>
        /// <value>The rate index tenor.</value>
        public string RateIndexTenor => ForecastRateIndex.indexTenor.ToString();

        #endregion

        #region Constructors

        public PriceableFloatingRateCoupon()
        {
            PriceableCouponType = CouponType.FloatingRate;
            ModelIdentifier = "DualCurveCouponModel";
            BaseRate = 0.0m;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxRateCashflow"/> class.
        /// </summary>
        /// <param name="cashFlowId">The stream id.</param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="margin">The margin.</param>
        /// <param name="observedRate">The observed Rate. If this is not null, then it is used.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="adjustedFixingDate">The adjusted fixing date.</param>
        /// <param name="dayCountFraction">Type of day Count fraction.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="forecastRateIndex">The forecast rate index.</param>
        /// <param name="discountingType">The swap discounting type.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        /// then it is assumed that there is no fra discounting</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableFloatingRateCoupon
            (
            string cashFlowId
            , bool payerIsBase
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , DateTime adjustedFixingDate
            , DayCountFraction dayCountFraction
            , decimal margin
            , decimal? observedRate
            , Money notionalAmount
            , DateTime paymentDate
            , ForecastRateIndex forecastRateIndex
            , DiscountingTypeEnum? discountingType
            , decimal? discountRate
            , FraDiscountingEnum? fraDiscounting
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
            : this(
                cashFlowId
                , payerIsBase
                , accrualStartDate
                , accrualEndDate
                , adjustedFixingDate
                , dayCountFraction
                , margin
                , 0.0m
                , observedRate
                , notionalAmount
                , paymentDate
                , forecastRateIndex
                , discountingType
                , discountRate
                , fraDiscounting
                , fixingCalendar
                , paymentCalendar)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxRateCashflow"/> class.
        /// </summary>
        /// <param name="cashFlowId">The stream id.</param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="margin">The margin.</param>
        /// <param name="baseRate">The base rate for a nettable fixed/floating cash flow. </param>
        /// <param name="observedRate">The observed Rate. If this is not null, then it is used.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="adjustedFixingDate">The adjusted fixing date.</param>
        /// <param name="dayCountFraction">Type of day Count fraction.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="forecastRateIndex">The forecast rate index.</param>
        /// <param name="discountingType">The swap discounting type.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        /// then it is assumed that there is no fra discounting</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableFloatingRateCoupon
            (
            string cashFlowId
            , bool payerIsBase
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , DateTime adjustedFixingDate
            , DayCountFraction dayCountFraction
            , decimal margin
            , decimal baseRate
            , decimal? observedRate
            , Money notionalAmount
            , DateTime paymentDate
            , ForecastRateIndex forecastRateIndex
            , DiscountingTypeEnum? discountingType
            , decimal? discountRate
            , FraDiscountingEnum? fraDiscounting
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar) 
            : base(
                cashFlowId
                ,  CouponType.FloatingRate
                ,  payerIsBase
                ,  accrualStartDate
                ,  accrualEndDate
                ,  notionalAmount
                ,  dayCountFraction
                ,  observedRate
                ,  paymentDate
                ,  discountingType
                ,  discountRate
                ,  fraDiscounting
                , paymentCalendar)
        {
            BaseRate = baseRate;
            FixingCalendar = fixingCalendar;
            ModelIdentifier = "DualCurveCouponModel";
            Id = cashFlowId;
            ForecastRateIndex = forecastRateIndex;
            Margin = margin;
            AdjustedFixingDate = adjustedFixingDate;
            RateObservation = RateObservationHelper.Parse(AdjustedFixingDate, observedRate, "1");
            SetRateObservation(RateObservation, ResetDate);
            ForecastCurveName = CurveNameHelpers.GetForecastCurveName(forecastRateIndex);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxRateCashflow"/> class.
        /// </summary>
        /// <param name="cashFlowId">The stream id.</param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="adjustAccrualDatesIndicator">if set to <c>true</c> [adjust calculation dates indicator].</param>
        /// <param name="accrualBusinessCenters">The accrual business centers.</param>
        /// <param name="margin">The margin.</param>
        /// <param name="observedRate">The observed Rate.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="dayCountFraction">Type of day Count fraction.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="accrualRollConvention">The accrual roll convention.</param>
        /// <param name="resetRelativeTo">reset relative to?</param>
        /// <param name="fixingDateRelativeOffset">The fixing date offset.</param>
        /// <param name="forecastRateIndex">The forecast rate index.</param>
        /// <param name="discountingType">The swap discounting type.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        /// then it is assumed that there is no fra discounting</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableFloatingRateCoupon
            (
            string cashFlowId
            , bool payerIsBase
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , bool adjustAccrualDatesIndicator
            , BusinessCenters accrualBusinessCenters
            , BusinessDayConventionEnum accrualRollConvention
            , DayCountFraction dayCountFraction
            , ResetRelativeToEnum resetRelativeTo
            , RelativeDateOffset fixingDateRelativeOffset
            , decimal margin
            , decimal? observedRate
            , Money notionalAmount
            , AdjustableOrAdjustedDate paymentDate
            , ForecastRateIndex forecastRateIndex
            , DiscountingTypeEnum? discountingType
            , decimal? discountRate
            , FraDiscountingEnum? fraDiscounting
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
            : this(
                cashFlowId
                , payerIsBase
                , accrualStartDate
                , accrualEndDate
                , adjustAccrualDatesIndicator
                , accrualBusinessCenters
                , accrualRollConvention
                , dayCountFraction
                , resetRelativeTo
                , fixingDateRelativeOffset
                , margin
                , 0.0m
                ,  observedRate
                , notionalAmount
                , paymentDate
                , forecastRateIndex
                , discountingType
                , discountRate
                , fraDiscounting
                , fixingCalendar
                , paymentCalendar)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxRateCashflow"/> class.
        /// </summary>
        /// <param name="cashflowId">The stream id.</param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="adjustAccrualDatesIndicator">if set to <c>true</c> [adjust calculation dates indicator].</param>
        /// <param name="accrualBusinessCenters">The accrual business centers.</param>
        /// <param name="margin">The margin.</param>
        /// <param name="baseRate">The base rate for a nettable fixed/floating cash flow. </param>
        /// <param name="observedRate">The observed Rate.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="dayCountFraction">Type of day Count fraction.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="accrualRollConvention">The accrual roll convention.</param>
        /// <param name="resetRelativeTo">reset relative to?</param>
        /// <param name="fixingDateRelativeOffset">The fixing date offset.</param>
        /// <param name="forecastRateIndex">The forecast rate index.</param>
        /// <param name="discountingType">The swap discounting type.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        /// then it is assumed that there is no fra discounting</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableFloatingRateCoupon
            (
            string cashflowId
            , bool payerIsBase
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , bool adjustAccrualDatesIndicator
            , BusinessCenters accrualBusinessCenters
            , BusinessDayConventionEnum accrualRollConvention
            , DayCountFraction dayCountFraction
            , ResetRelativeToEnum resetRelativeTo
            , RelativeDateOffset fixingDateRelativeOffset
            , decimal margin
            , decimal baseRate
            , decimal? observedRate
            , Money notionalAmount
            , AdjustableOrAdjustedDate paymentDate
            , ForecastRateIndex forecastRateIndex
            , DiscountingTypeEnum? discountingType
            , decimal? discountRate
            , FraDiscountingEnum? fraDiscounting
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar) 
            : base(
                cashflowId
                ,  CouponType.FloatingRate
                ,  payerIsBase
                ,  accrualStartDate
                ,  accrualEndDate
                ,  adjustAccrualDatesIndicator
                ,  accrualBusinessCenters
                ,  accrualRollConvention
                ,  dayCountFraction
                ,  observedRate
                ,  notionalAmount
                ,  paymentDate
                ,  discountingType
                ,  discountRate
                ,  fraDiscounting
                ,  paymentCalendar)
        {
            BaseRate = baseRate;
            FixingCalendar = fixingCalendar;
            ModelIdentifier = "DualCurveCouponModel";
            Id = cashflowId;
            ForwardStartDate = AccrualStartDate;       
            ForecastRateIndex = forecastRateIndex;
            FixingDateRelativeOffset = fixingDateRelativeOffset;
            ResetRelativeTo = resetRelativeTo;
            Margin = margin;
            AdjustedFixingDate = GetResetDate(resetRelativeTo, fixingDateRelativeOffset);
            if (observedRate != null) RateObservation = RateObservationHelper.Parse(AdjustedFixingDate, (decimal)observedRate, "1");
            SetRateObservation(RateObservation, ResetDate);
            ForecastCurveName = CurveNameHelpers.GetForecastCurveName(forecastRateIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="payerIsBase"></param>
        /// <param name="accrualStartDate"></param>
        /// <param name="accrualEndDate"></param>
        /// <param name="adjustCalculationDatesIndicator"></param>
        /// <param name="paymentDate"></param>
        /// <param name="notionalAmount"></param>
        /// <param name="resetRelativeTo"></param>
        /// <param name="fixingDateRelativeOffset"></param>
        /// <param name="margin"></param>
        /// <param name="calculation"></param>
        /// <param name="forecastRateIndex"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        public PriceableFloatingRateCoupon
            (string uniqueId
            , bool payerIsBase
             , DateTime accrualStartDate
             , DateTime accrualEndDate
             , bool adjustCalculationDatesIndicator
             , AdjustableOrAdjustedDate paymentDate
             , Money notionalAmount
             , ResetRelativeToEnum resetRelativeTo
             , RelativeDateOffset fixingDateRelativeOffset
             , decimal margin
             , Calculation calculation
             , ForecastRateIndex forecastRateIndex
             , IBusinessCalendar fixingCalendar
             , IBusinessCalendar paymentCalendar)
            : this
                (uniqueId
                 , payerIsBase
                 , accrualStartDate
                 , accrualEndDate
                 , adjustCalculationDatesIndicator
                 , paymentDate
                 , notionalAmount
                 , resetRelativeTo
                 , fixingDateRelativeOffset
                 , margin
                 , 0.0m
                 , calculation
                 , forecastRateIndex
                 , fixingCalendar
                 , paymentCalendar)
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="payerIsBase"></param>
        /// <param name="accrualStartDate"></param>
        /// <param name="accrualEndDate"></param>
        /// <param name="adjustCalculationDatesIndicator"></param>
        /// <param name="paymentDate"></param>
        /// <param name="notionalAmount"></param>
        /// <param name="resetRelativeTo"></param>
        /// <param name="fixingDateRelativeOffset"></param>
        /// <param name="margin"></param>
        /// <param name="baseRate"> </param>
        /// <param name="calculation"></param>
        /// <param name="forecastRateIndex"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        public PriceableFloatingRateCoupon
            (string uniqueId
            , bool payerIsBase
             , DateTime accrualStartDate
             , DateTime accrualEndDate
             , bool adjustCalculationDatesIndicator
             , AdjustableOrAdjustedDate paymentDate
             , Money notionalAmount
             , ResetRelativeToEnum resetRelativeTo
             , RelativeDateOffset fixingDateRelativeOffset
             , decimal margin
             , decimal baseRate
             , Calculation calculation
             , ForecastRateIndex forecastRateIndex
             , IBusinessCalendar fixingCalendar
             , IBusinessCalendar paymentCalendar)
            : base
                (uniqueId
                 , CouponType.FloatingRate
                 , payerIsBase
                 , accrualStartDate
                 , accrualEndDate
                 , adjustCalculationDatesIndicator
                 , BusinessDayAdjustmentsHelper.Create(fixingDateRelativeOffset.businessDayConvention, fixingDateRelativeOffset.businessCenters)
                 , calculation
                 , notionalAmount
                 , paymentDate
                 , paymentCalendar)
        {
            BaseRate = baseRate;
            FixingCalendar = fixingCalendar;
            ModelIdentifier = "DualCurveCouponModel";
            ForwardStartDate = AccrualStartDate;
            ForwardEndDate = AccrualEndDate;
            ForecastRateIndex = forecastRateIndex;
            FixingDateRelativeOffset = fixingDateRelativeOffset;
            ResetRelativeTo = resetRelativeTo;
            Margin = margin;
            var floatingRateDefinition = (FloatingRateDefinition)calculation.Items[0];
            RateObservation = floatingRateDefinition.rateObservation[0];
            AdjustedFixingDate = GetResetDate(resetRelativeTo, fixingDateRelativeOffset);
            SetRateObservation(RateObservation, ResetDate);
            ForecastCurveName = CurveNameHelpers.GetForecastCurveName(forecastRateIndex);
        }

        #endregion

        #region Metrics for Valuation

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override AssetValuation Calculate(IInstrumentControllerData modelData)            
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            AnalyticsModel = new FloatingRateCouponAnalytic();
            RequiresReset = modelData.ValuationDate > ResetDate;
            IsRealised = HasBeenRealised(ModelData.ValuationDate);
            //Make sure there are some bucket dates even if not set previously.
            if (BucketedDates.Length < 1)
            {
                UpdateBucketingInterval(ModelData.ValuationDate, PeriodHelper.Parse(CDefaultBucketingInterval));
            } 
            //Add the extra metrics required
            var quotes = ModelData.AssetValuation.quote.ToList();
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.BreakEvenRate.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.BreakEvenRate.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.AccrualFactor.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.AccrualFactor.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.FloatingNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.FloatingNPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.NPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.NPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.RiskNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.RiskNPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.LocalCurrencyNPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyExpectedValue.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.LocalCurrencyExpectedValue.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            //Check if risk calc are required.
            bool delta1PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta1PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta1PDH.ToString()) != null;
            //Check if risk calc are required.
            bool delta0PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta0PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta0PDH.ToString()) != null;
            ModelData.AssetValuation.quote = quotes.ToArray();
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            IFxCurve fxCurve = null;
            IRateCurve discountCurve = null;
            IRateCurve forecastCurve = null;
            //// Determine if DFAM has been requested - if so that is all we evaluate - every other metric is ignored
            //if (metrics.Contains(InstrumentMetrics.DiscountFactorAtMaturity))
            //{
            //    metrics.RemoveAll(metricItem => metricItem != InstrumentMetrics.DiscountFactorAtMaturity);
            //}
            //Set the forecast rate dates. The ForecastRateInterpolation should have been set.
            ForwardStartDate = AccrualStartDate;
            ForwardEndDate = ForecastRateInterpolation ? AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(FixingCalendar, ForecastRateIndex.indexTenor.Add(ForwardStartDate), AccrualBusinessDayAdjustments);             
            //var metricsToEvaluate = metrics.ToArray();
            if (metrics.Count > 0)
            {
                YearFractionToCashFlowPayment = GetPaymentYearFraction(ModelData.ValuationDate, PaymentDate);
                var reportingCurrency = ModelData.ReportingCurrency == null ? PaymentCurrency.Value : ModelData.ReportingCurrency.Value;
                var amount = NotionalAmount.amount;
                IRateCouponParameters analyticModelParameters = new RateCouponParameters {
                                                                                             Multiplier = Multiplier,
                                                                                             ValuationDate = modelData.ValuationDate,
                                                                                             PaymentDate = PaymentDate,
                                                                                             Currency = PaymentCurrency.Value,
                                                                                             ReportingCurrency = reportingCurrency,
                                                                                             DiscountType = DiscountType,
                                                                                             IsRealised = IsRealised,
                                                                                             HasReset = RequiresReset,
                                                                                             NotionalAmount = amount,
                                                                                             Spread = Margin,
                                                                                             YearFraction = CouponYearFraction,
                                                                                             CurveYearFraction = YearFractionToCashFlowPayment
                                                                                         };
                decimal? discountRate = null;
                // Curve Related
                if (modelData.MarketEnvironment is ISwapLegEnvironment environment)
                {
                    var streamMarket = environment;
                    discountCurve = streamMarket.GetDiscountRateCurve();
                    discountCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    forecastCurve = streamMarket.GetForecastRateCurve();
                    forecastCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    DiscountCurveName = discountCurve.GetPricingStructureId().UniqueIdentifier;
                    analyticModelParameters.DiscountCurve = discountCurve;
                    ForecastCurveName = forecastCurve.GetPricingStructureId().UniqueIdentifier;
                    analyticModelParameters.ForecastCurve = forecastCurve;
                    // Bucketed Delta
                    if (BucketedDates.Length > 1)
                    {
                        analyticModelParameters.PeriodAsTimesPerYear = GetPaymentYearFraction(BucketedDates[0],
                                                                                              BucketedDates[1]);
                        analyticModelParameters.BucketedDiscountFactors = GetBucketedDiscountFactors(discountCurve,
                                                                                                     ModelData.
                                                                                                         ValuationDate,
                                                                                                     BucketedDates);
                    }
                    //Check for currency.
                    if (ModelData.ReportingCurrency != null)
                    {
                        if (ModelData.ReportingCurrency.Value != PaymentCurrency.Value)
                        {
                            fxCurve = streamMarket.GetReportingCurrencyFxCurve();
                            fxCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                            analyticModelParameters.ReportingCurrencyFxCurve = fxCurve;
                        }
                    }
                    AnalyticModelParameters = analyticModelParameters;
                }
                else if (modelData.MarketEnvironment.GetType() == typeof(MarketEnvironment))
                {
                    var market = (MarketEnvironment)modelData.MarketEnvironment;
                    discountCurve = (IRateCurve)market.SearchForPricingStructureType(DiscountCurveName);
                    discountCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    forecastCurve = (IRateCurve)market.SearchForPricingStructureType(ForecastCurveName);
                    forecastCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    analyticModelParameters.DiscountCurve = discountCurve;
                    analyticModelParameters.ForecastCurve = forecastCurve;
                    // Bucketed Delta
                    if (BucketedDates.Length > 1)
                    {
                        analyticModelParameters.PeriodAsTimesPerYear = GetPaymentYearFraction(BucketedDates[0],
                                                                                              BucketedDates[1]);
                        analyticModelParameters.BucketedDiscountFactors = GetBucketedDiscountFactors(discountCurve,
                                                                                                     ModelData.
                                                                                                         ValuationDate,
                                                                                                     BucketedDates);
                    }
                    if (delta1PDH)
                    {
                        var riskMarket = market.SearchForPerturbedPricingStructures(DiscountCurveName, "delta1PDH");
                        analyticModelParameters.Delta1PDHCurves = riskMarket;
                        analyticModelParameters.Delta1PDHPerturbation = 10;
                    }
                    if (delta0PDH)
                    {
                        var riskMarket = market.SearchForPerturbedPricingStructures(ForecastCurveName, "delta0PDH");
                        analyticModelParameters.Delta0PDHCurves = riskMarket;
                        analyticModelParameters.Delta0PDHPerturbation = 10;
                    }
                    //Check for currency.
                    if (ModelData.ReportingCurrency != null)
                    {
                        if (ModelData.ReportingCurrency.Value != PaymentCurrency.Value)
                        {
                            string curveName = MarketEnvironmentHelper.ResolveFxCurveNames(PaymentCurrency.Value, modelData.ReportingCurrency.Value);
                            fxCurve = (IFxCurve)market.SearchForPricingStructureType(curveName);
                            fxCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                            analyticModelParameters.ReportingCurrencyFxCurve = fxCurve;
                        }
                    }
                    AnalyticModelParameters = analyticModelParameters;
                }
                //Set the base rate. Default is zero
                if (AnalyticModelParameters != null) AnalyticModelParameters.BaseRate = BaseRate;
                if (UseObservedRate)
                {
                    AnalyticsModel = new FixedRateCouponAnalytic(ModelData.ValuationDate, AccrualStartDate, AccrualEndDate, PaymentDate, Rate,
                    analyticModelParameters.YearFraction, DiscountType, fxCurve, discountCurve, forecastCurve);
                    if (Rate != null) analyticModelParameters.Rate = (decimal)Rate;
                }
                else
                {
                    if (Rate != null) discountRate = Rate;
                    if (DiscountRate != null)
                    {
                        discountRate = DiscountRate;
                    }
                    AnalyticsModel = new FloatingRateCouponAnalytic(ModelData.ValuationDate, AccrualStartDate,
                                                                        AccrualEndDate, PaymentDate, discountRate,
                                                                        analyticModelParameters.YearFraction,
                                                                        DiscountType, fxCurve, discountCurve,
                                                                        forecastCurve);
                }
                CalculationResults = AnalyticsModel.Calculate<IRateInstrumentResults, RateInstrumentResults>(AnalyticModelParameters, metrics.ToArray());
                CalculationPerformedIndicator = true;
                PaymentDiscountFactor = ((FixedRateCouponAnalytic)AnalyticsModel).PaymentDiscountFactor;
                if (!UseObservedRate)
                {
                    Rate = CalculationResults.BreakEvenRate;
                }
                ForecastAmount = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyExpectedValue, PaymentAmount.currency);
                NPV = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyNPV, PaymentAmount.currency);
            }
            AssetValuation valuation = GetValue(CalculationResults, modelData.ValuationDate);
            valuation.id = Id;
            return valuation;
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
        /// Updates the name of the forward curve.
        /// </summary>
        /// <param name="newCurveName">New name of the curve.</param>
        public void UpdateForwardCurveName(string newCurveName)
        {
            ForecastCurveName = newCurveName;
        }

        #endregion

        #region Payment

        /// <summary>
        /// Gets the reset date.
        /// </summary>
        /// <param name="resetRelativeTo">The reset relative to.</param>
        /// <returns></returns>
        /// <param name="fixingDateRelativeOffset"></param>
        private DateTime GetResetDate(ResetRelativeToEnum resetRelativeTo, RelativeDateOffset fixingDateRelativeOffset)
        {
            var resetDate = AccrualStartDate;
            switch (resetRelativeTo)
            {
                case ResetRelativeToEnum.CalculationPeriodEndDate:
                    resetDate = AccrualEndDate;
                    break;
            }

            var interval = (Period)fixingDateRelativeOffset;
            if (fixingDateRelativeOffset != null)
            {
                var adjustment = BusinessDayAdjustmentsHelper.Create(fixingDateRelativeOffset.businessDayConvention, BusinessCentersHelper.BusinessCentersString(fixingDateRelativeOffset.businessCenters.businessCenter));
                resetDate = AdjustedDateHelper.ToAdjustedDate(FixingCalendar, resetDate, adjustment, OffsetHelper.FromInterval(interval, fixingDateRelativeOffset.dayType));
            }
            return resetDate;
        }

        /// <summary>
        /// Sets the rate observation.
        /// </summary>
        /// <param name="rateObservation">The rate observation.</param>
        /// <param name="resetDate">The reset date.</param>
        private void SetRateObservation(RateObservation rateObservation, DateTime resetDate)
        {
            if (rateObservation != null)
            {
                if (rateObservation.observedRateSpecified)
                {
                    Rate = rateObservation.observedRate;
                    UseObservedRate = true;
                }

                if (!rateObservation.adjustedFixingDateSpecified)
                {
                    rateObservation.adjustedFixingDate = resetDate;
                    rateObservation.adjustedFixingDateSpecified = true;
                }
            }
        }

        #endregion

        #region Instance Helpers

        /// <summary>
        /// Accrued amount at the given date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public override Money GetAccruedAmount(DateTime date)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Forecast amount at the given date.
        /// </summary>
        /// <returns></returns>
        public override Money GetForecastAmount()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <returns></returns>
        public override decimal GetRate()//IRateCurve forecastCurve
        {
            return 0.0m;// GetRate(ForwardStartDate, ForwardEndDate, forecastCurve, ModelData.ValuationDate);
        }

        /// <summary>
        /// Gets the forward rate.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="forwardDate">The forward date.</param>
        /// <param name="forecastCurve">The forecast curve.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        protected decimal GetRate(DateTime startDate, DateTime forwardDate, IRateCurve forecastCurve, DateTime valuationDate)
        {
            if (Rate == null) return GetForwardRate(startDate, forwardDate, forecastCurve, valuationDate);
            if (RequiresReset || ObservedRateSpecified || UseObservedRate)
            {
                return (Decimal)Rate;
            }
            return GetForwardRate(startDate, forwardDate, forecastCurve, valuationDate);           
        }

        #endregion;

        #region FpML Representation

        /// <summary>
        /// Builds the calculation period.
        /// </summary>
        /// <returns></returns>
        protected override CalculationPeriod BuildCalculationPeriod()
        {
            CalculationPeriod cp = base.BuildCalculationPeriod();
            //Set the floating rate definition
            FloatingRateDefinition floatingRateDefinition = FloatingRateDefinitionHelper.CreateSimple(ForecastRateIndex.floatingRateIndex, ForecastRateIndex.indexTenor, AdjustedFixingDate, GetRate(), Margin);
            cp.Item1 = floatingRateDefinition;
            if (floatingRateDefinition.calculatedRateSpecified)
            {
                cp.forecastRate = floatingRateDefinition.calculatedRate;
                cp.forecastRateSpecified = true;
            }
            if (CalculationPerformedIndicator)
            {
                cp.forecastAmount = MoneyHelper.GetAmount(CalculationResults.ExpectedValue, NotionalAmount.currency.Value);
            }
            return cp;
        }

        #endregion

        #region Static Helpers

        public static ResetRelativeToEnum GetResetRelativeToStartDate()
        {
            return ResetRelativeToEnum.CalculationPeriodStartDate;
        }

        #endregion;

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return null;
        }

        #endregion
    }
}