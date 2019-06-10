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

#region Usings

using System;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.Constants;
using FpML.V5r3.Reporting;
//using FpML.V5r3.Reporting.Extensions;
using Orion.Models.Rates;
using Orion.Models.Rates.Coupons;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableCapFloorCoupon : PriceableFloatingRateCoupon
    {
        #region Member Fields

        #endregion

        #region Public Fields

        public string VolatilitySurfaceName { get; set; }

        public decimal TimeToExpiry { get; set; }

        public bool IsCall { get; set; }

        public Decimal? CapStrike {get; set;}

        public Decimal? FloorStrike { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// The default type is a cap.
        /// </summary>
        public PriceableCapFloorCoupon()
        {
            PriceableCouponType = CouponType.Cap;
            ModelIdentifier = "DualCurveOptionModel";
            IsCall = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCapFloorCoupon"/> class.
        /// </summary>
        /// <param name="cashlFowId">The stream id.</param>
        /// <param name="buyerIsBase">The buyer is base flag.</param>
        /// <param name="capStrike">The Cap strike.</param>
        /// <param name="floorStrike">The floor strike.</param>
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
        /// <param name="fixingCalendar"> The fixingCalendar. </param>
        /// <param name="paymentCalendar"> The paymentCalendar. </param>
        public PriceableCapFloorCoupon
            (
            string cashlFowId
            , bool buyerIsBase
            , decimal? capStrike
            , decimal? floorStrike
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , DateTime adjustedFixingDate
            , DayCountFraction dayCountFraction
            , Decimal margin
            , Decimal? observedRate
            , Money notionalAmount
            , DateTime paymentDate
            , ForecastRateIndex forecastRateIndex
            , DiscountingTypeEnum? discountingType
            , Decimal? discountRate
            , FraDiscountingEnum? fraDiscounting
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar) 
            : base(
                cashlFowId
                , buyerIsBase
                ,  accrualStartDate
                ,  accrualEndDate
                ,  adjustedFixingDate
                ,  dayCountFraction
                ,  margin
                ,  observedRate
                ,  notionalAmount
                ,  paymentDate
                ,  forecastRateIndex
                ,  discountingType
                ,  discountRate
                ,  fraDiscounting
                ,  fixingCalendar
                ,  paymentCalendar)
        {
            CapStrike = capStrike;
            FloorStrike = floorStrike;
            VolatilitySurfaceName = CurveNameHelpers.GetRateVolatilityMatrixName(forecastRateIndex);
            if (capStrike != null && floorStrike == null)
            {
                PriceableCouponType = CouponType.Cap;
                ModelIdentifier = "DualCurveCapModel";
                IsCall = true;
            }
            if (floorStrike != null && capStrike == null)
            {
                PriceableCouponType = CouponType.Floor;
                ModelIdentifier = "DualCurveFloorModel";
            }
            if (floorStrike != null && capStrike != null)
            {
                PriceableCouponType = CouponType.Collar;
                ModelIdentifier = "DualCurveCollarModel";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCapFloorCoupon"/> class.
        /// </summary>
        /// <param name="cashFlowId">The stream id.</param>
        /// <param name="buyerIsBase">The buyer is base flag.</param>
        /// <param name="capStrike">The Cap strike.</param>
        /// <param name="floorStrike">The floor strike.</param>
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
        public PriceableCapFloorCoupon
            (
            string cashFlowId
            , bool buyerIsBase
            , decimal? capStrike
            , decimal? floorStrike
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , Boolean adjustAccrualDatesIndicator
            , BusinessCenters accrualBusinessCenters
            , BusinessDayConventionEnum accrualRollConvention
            , DayCountFraction dayCountFraction
            , ResetRelativeToEnum resetRelativeTo
            , RelativeDateOffset fixingDateRelativeOffset
            , Decimal margin
            , Decimal? observedRate
            , Money notionalAmount
            , AdjustableOrAdjustedDate paymentDate
            , ForecastRateIndex forecastRateIndex
            , DiscountingTypeEnum? discountingType
            , Decimal? discountRate
            , FraDiscountingEnum? fraDiscounting
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
            : base(
                cashFlowId
                , buyerIsBase
                , accrualStartDate
                , accrualEndDate
                , adjustAccrualDatesIndicator
                , accrualBusinessCenters
                , accrualRollConvention
                , dayCountFraction
                , resetRelativeTo
                , fixingDateRelativeOffset
                , margin
                , observedRate
                , notionalAmount
                , paymentDate
                , forecastRateIndex
                , discountingType
                , discountRate
                , fraDiscounting
                , fixingCalendar
                , paymentCalendar)
        {
            CapStrike = capStrike;
            FloorStrike = floorStrike;
            VolatilitySurfaceName = CurveNameHelpers.GetRateVolatilityMatrixName(forecastRateIndex);
            if (capStrike != null && floorStrike == null)
            {
                PriceableCouponType = CouponType.Cap;
                ModelIdentifier = "DualCurveCapModel";
                IsCall = true;
            }
            if (floorStrike != null && capStrike == null)
            {
                PriceableCouponType = CouponType.Floor;
                ModelIdentifier = "DualCurveFloorModel";
            }
            if (floorStrike != null && capStrike != null)
            {
                PriceableCouponType = CouponType.Collar;
                ModelIdentifier = "DualCurveCollarModel";
            }
        }
            
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCapFloorCoupon"/> class.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="buyerIsBase">The buyer is base flag.</param>
        /// <param name="capStrike">The Cap strike.</param>
        /// <param name="floorStrike">The floor strike.</param>
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
        public PriceableCapFloorCoupon
            (string uniqueId
             , bool buyerIsBase
             , decimal? capStrike
             , decimal? floorStrike
             , DateTime accrualStartDate
             , DateTime accrualEndDate
             , Boolean adjustCalculationDatesIndicator
             , AdjustableOrAdjustedDate paymentDate
             , Money notionalAmount
             , ResetRelativeToEnum resetRelativeTo
             , RelativeDateOffset fixingDateRelativeOffset
             , Decimal margin
             , Calculation calculation
             , ForecastRateIndex forecastRateIndex
             , IBusinessCalendar fixingCalendar
             , IBusinessCalendar paymentCalendar)
            : base
                (uniqueId
                 , buyerIsBase
                 , accrualStartDate
                 , accrualEndDate
                 , adjustCalculationDatesIndicator
                 , paymentDate
                 , notionalAmount
                 , resetRelativeTo
                 , fixingDateRelativeOffset
                 , margin
                 , calculation
                 , forecastRateIndex
                 , fixingCalendar
                 , paymentCalendar)
        {
            CapStrike = capStrike;
            FloorStrike = floorStrike;
            VolatilitySurfaceName = CurveNameHelpers.GetRateVolatilityMatrixName(forecastRateIndex);
            if (capStrike != null && floorStrike == null)
            {
                PriceableCouponType = CouponType.Cap;
                ModelIdentifier = "DualCurveCapModel";
                IsCall = true;
            }
            if (floorStrike != null && capStrike == null)
            {
                PriceableCouponType = CouponType.Floor;
                ModelIdentifier = "DualCurveFloorModel";
            }
            if (floorStrike != null && capStrike != null)
            {
                PriceableCouponType = CouponType.Collar;
                ModelIdentifier = "DualCurveCollarModel";
            }
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
            AnalyticsModel = new RateOptionCouponAnalytic();
            RequiresReset = modelData.ValuationDate > ResetDate;
            IsRealised = HasBeenRealised(ModelData.ValuationDate);
            TimeToExpiry = GetPaymentYearFraction(ModelData.ValuationDate, AdjustedFixingDate);
            var volatilityCurveNodeTime = GetPaymentYearFraction(ModelData.ValuationDate, AccrualStartDate);
            IFxCurve fxCurve = null;
            IRateCurve discountCurve = null;
            IRateCurve forecastCurve = null;
            IVolatilitySurface indexVolSurface = null;
            //Add the extra metrics required
            var quotes = ModelData.AssetValuation.quote.ToList();
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.BreakEvenRate.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.BreakEvenRate.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.AccrualFactor.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.AccrualFactor.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.FloatingNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.FloatingNPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.NPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.NPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.LocalCurrencyNPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyExpectedValue.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.LocalCurrencyExpectedValue.ToString(), "DecimalValue");
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
            //// Determine if DFAM has been requested - if so that is all we evaluate - every other metric is ignored
            //if (metrics.Contains(InstrumentMetrics.DiscountFactorAtMaturity))
            //{
            //    metrics.RemoveAll(metricItem => metricItem != InstrumentMetrics.DiscountFactorAtMaturity);
            //}
            //Set the forecast rate dates. The ForecastRateInterpolation should have been set.
            ForwardStartDate = AccrualStartDate;
            ForwardEndDate = ForecastRateInterpolation ? AccrualEndDate : AdjustedDateHelper.ToAdjustedDate(FixingCalendar, ForecastRateIndex.indexTenor.Add(ForwardStartDate), AccrualBusinessDayAdjustments);  
            //Set the strike
            var strike1 = 0.0m;
            var strike2 = 0.0m;
            var isCollar = false;
            if(PriceableCouponType == CouponType.Cap)
            {
                if (CapStrike != null) strike1 = (decimal)CapStrike;
            }
            if (PriceableCouponType == CouponType.Floor)
            {
                if (FloorStrike != null) strike1 = (decimal)FloorStrike;
            }
            if (PriceableCouponType == CouponType.Collar)//TODO Need to add the Floor calculation which will require a new model or extension of the current model.
            {
                if (CapStrike != null) strike1 = (decimal)CapStrike;
                if (FloorStrike != null) strike2 = (decimal)FloorStrike;
                isCollar = true;
            }
            //var metricsToEvaluate = metrics.ToArray();
            if (metrics.Count > 0)
            {
                YearFractionToCashFlowPayment = GetPaymentYearFraction(ModelData.ValuationDate, PaymentDate);
                var reportingCurrency = ModelData.ReportingCurrency == null
                                            ? PaymentCurrency.Value
                                            : ModelData.ReportingCurrency.Value;
                IRateCouponParameters analyticModelParameters = new RateCouponParameters {   Multiplier = Multiplier,
                                                                                             ValuationDate = modelData.ValuationDate,
                                                                                             PaymentDate = PaymentDate,
                                                                                             Currency = PaymentCurrency.Value,
                                                                                             ReportingCurrency = reportingCurrency,
                                                                                             DiscountType = DiscountType,
                                                                                             IsRealised = IsRealised,
                                                                                             HasReset = RequiresReset,
                                                                                             NotionalAmount = NotionalAmount.amount,
                                                                                             Spread = Margin,
                                                                                             YearFraction = CouponYearFraction,
                                                                                             CurveYearFraction = YearFractionToCashFlowPayment,
                                                                                             ExpiryYearFraction = TimeToExpiry,
                                                                                             IsCall = IsCall
                                                                                         };
                // Curve Related
                if (modelData.MarketEnvironment is ISwapLegEnvironment environment)
                {
                    var streamMarket = environment;
                    discountCurve = streamMarket.GetDiscountRateCurve();
                    discountCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    forecastCurve = streamMarket.GetForecastRateCurve();
                    forecastCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    indexVolSurface = streamMarket.GetVolatilitySurface();
                    indexVolSurface.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    DiscountCurveName = discountCurve.GetPricingStructureId().UniqueIdentifier;
                    ForecastCurveName = forecastCurve.GetPricingStructureId().UniqueIdentifier;
                    VolatilitySurfaceName = indexVolSurface.GetPricingStructureId().UniqueIdentifier;
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
                        }
                    }
                }
                else if (modelData.MarketEnvironment.GetType() == typeof(MarketEnvironment))
                {
                    var market = (MarketEnvironment)modelData.MarketEnvironment;
                    discountCurve = (IRateCurve)market.SearchForPricingStructureType(DiscountCurveName);
                    discountCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    forecastCurve = (IRateCurve)market.SearchForPricingStructureType(ForecastCurveName);
                    forecastCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    indexVolSurface = (IVolatilitySurface)market.SearchForPricingStructureType(VolatilitySurfaceName);
                    indexVolSurface.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    if (!UseObservedRate)
                    {
                        Rate = GetRate(ForwardStartDate, ForwardEndDate, forecastCurve, ModelData.ValuationDate);
                    }
                    //the rate params
                    analyticModelParameters.Rate = GetRate(ForwardStartDate, ForwardEndDate, forecastCurve, ModelData.ValuationDate);
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
                        }
                    }                   
                }
                analyticModelParameters.DiscountCurve = discountCurve;
                analyticModelParameters.ForecastCurve = forecastCurve;
                analyticModelParameters.VolatilitySurface = indexVolSurface;
                analyticModelParameters.ReportingCurrencyFxCurve = fxCurve;
                AnalyticModelParameters = analyticModelParameters;
                //Set the base rate. Default is zero
                if (AnalyticModelParameters != null) AnalyticModelParameters.BaseRate = BaseRate;
                if (UseObservedRate)
                {
                    AnalyticsModel = new FixedRateCouponAnalytic(ModelData.ValuationDate, AccrualStartDate, AccrualEndDate, PaymentDate, Rate,
                    analyticModelParameters.YearFraction, DiscountType, fxCurve, discountCurve, forecastCurve);
                    if (Rate != null) analyticModelParameters.Rate = (decimal)Rate;
                }
                if (!isCollar)
                {
                    AnalyticsModel = new RateOptionCouponAnalytic(ModelData.ValuationDate, AccrualStartDate, AccrualEndDate, PaymentDate, volatilityCurveNodeTime,
                        strike1, fxCurve, discountCurve, forecastCurve, indexVolSurface);
                }
                else
                {
                    AnalyticsModel = new RateOptionCouponAnalytic(ModelData.ValuationDate, AccrualStartDate, AccrualEndDate, PaymentDate, volatilityCurveNodeTime,
                        strike1, strike2, fxCurve, discountCurve, forecastCurve, indexVolSurface);
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

        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds the calculation period.
        /// </summary>
        /// <returns></returns>
        protected override CalculationPeriod BuildCalculationPeriod()
        {
            CalculationPeriod cp = base.BuildCalculationPeriod();
            //Set the floating rate definition
            if (FloorStrike != null)
            {
                FloatingRateDefinition floatingRateDefinition = CapStrike != null ? 
                     FloatingRateDefinitionHelper.CreateCapFloor(ForecastRateIndex.floatingRateIndex, ForecastRateIndex.indexTenor, AdjustedFixingDate, GetRate(), Margin, (decimal)CapStrike, true) 
                     : FloatingRateDefinitionHelper.CreateCapFloor(ForecastRateIndex.floatingRateIndex, ForecastRateIndex.indexTenor, AdjustedFixingDate, GetRate(), Margin, (decimal)FloorStrike, false);
                cp.Item1 = floatingRateDefinition;
                if (floatingRateDefinition.calculatedRateSpecified)
                {
                    cp.forecastRate = floatingRateDefinition.calculatedRate;
                    cp.forecastRateSpecified = true;
                }
            }
            if (CalculationPerformedIndicator)
            {
                cp.forecastAmount = ForecastAmount;
            }
            return cp;
        }

        #endregion

    }
}