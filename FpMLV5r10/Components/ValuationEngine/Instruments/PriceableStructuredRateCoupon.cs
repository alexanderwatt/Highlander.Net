using System;
using National.QRSC.FpML.V47;
using National.QRSC.AnalyticModels.Rates;
using National.QRSC.AnalyticModels.Rates.Coupons;
using National.QRSC.ModelFramework.Instruments;
using National.QRSC.ModelFramework.MarketEnvironments;

namespace Orion.ValuationEngine.Instruments.Rates.Coupons
{
    [Serializable]
    public class PriceableStructuredRateCoupon : PriceableFloatingRateCoupon
    {
        #region Public Fields

        public RateIndex UnderlyingRateIndex { get; set; }

        public RelativeDateOffset ResetLagOffset { get; set; }

        //Calculation.
        public DateTime CalculationStartDate { get; set; }

        //Calculation.
        public DateTime CalculationEndDate { get; set; }

        public DayCountFraction CalculationDayCountFraction { get; set; }

        //THe covexity adjustment.
        public Boolean ConvexityAdjustment { get; set; }

        #endregion

        #region Constructors

        public PriceableStructuredRateCoupon()
        {
            PriceableCouponType = CouponType.StructuredRate;
            AnalyticsModel = new StructuredRateCouponAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateCoupon"/> class. 
        /// This is only used for Xibor type structured coupons with convexity adjustments and only
        /// in the same currency i.e. there is no quanto effects.
        /// </summary>
        /// <param name="cashlfowId">The stream id.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="calculationDayCountFraction">The daycountfraction used for the calculation period i.e. the index calculation.</param>
        /// <param name="margin">The margin.</param>
        /// <param name="observedRate">The observed Rate. If this is not null, then it is used.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="adjustedFixingDate">The adjusted fixing date.</param>
        /// <param name="calculationStartDate">The calculation start date.</param>
        /// <param name="calculationEndDate">The calculation end date.</param>
        /// <param name="accrualDayCountfraction">Type of dayCountfraction use for the the accrual period.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="forecastRateIndex">The forecastrateindex.</param>
        /// <param name="discountingType">The swap discounting type.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        /// <param name="calculationStartDate"></param>
        /// then it is assumed that there is no fradiscounting</param>
        public PriceableStructuredRateCoupon
            (
            string cashlfowId
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , DateTime adjustedFixingDate
            , DateTime calculationStartDate
            , DateTime calculationEndDate
            , DayCountFraction accrualDayCountfraction
            , DayCountFraction calculationDayCountFraction
            , Decimal margin
            , Decimal? observedRate
            , Money notionalAmount
            , DateTime paymentDate
            , ForecastRateIndex forecastRateIndex
            , DiscountingTypeEnum? discountingType
            , Decimal? discountRate
            , FraDiscountingEnum? fraDiscounting) 
            : base(
                cashlfowId
                , accrualStartDate
                , accrualEndDate
                , adjustedFixingDate
                , accrualDayCountfraction
                , margin
                , observedRate
                , notionalAmount
                , paymentDate
                , forecastRateIndex
                , discountingType
                , discountRate
                , fraDiscounting
                )
        {
            CalculationStartDate = calculationStartDate;
            CalculationEndDate = calculationEndDate;
            CalculationDayCountFraction = calculationDayCountFraction;
            PriceableCouponType = CouponType.StructuredRate;
            AnalyticsModel = new StructuredRateCouponAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateCoupon"/> class.
        /// </summary>
        /// <param name="cashlfowId">The stream id.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="adjustCalculationDatesIndicator">if set to <c>true</c> [adjust calculation dates indicator].</param>
        /// <param name="accrualBusinessCenters">The accrual business centers.</param>
        /// <param name="underlyingRateIndex">Te underlying rate index can be Xibor or Swap.</param>
        /// <param name="resetLagOffset">This allows for non-standard lags to be used in convexity adjustment calculations.</param>
        /// <param name="margin">The margin.</param>
        /// <param name="observedRate">The observed Rate.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="accrualDayCountfraction">Type of day Countfraction used for the acrual period.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="accrualRollConvention">The accrual roll convention.</param>
        /// <param name="resetRelativeTo">reset relative to?</param>
        /// <param name="fixingDateRelativeOffset">The fixing date offset.</param>
        /// <param name="forecastRateIndex">The forecastrateindex.</param>
        /// <param name="discountingType">The swap discounting type.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        /// <param name="underlyingRateIndex"></param>
        /// then it is assumed that there is no fradiscounting</param>
        public PriceableStructuredRateCoupon
            (
            string cashlfowId
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , Boolean adjustCalculationDatesIndicator
            , BusinessCenters accrualBusinessCenters
            , BusinessDayConventionEnum accrualRollConvention
            , DayCountFraction accrualDayCountfraction
            , ResetRelativeToEnum resetRelativeTo
            , RelativeDateOffset fixingDateRelativeOffset
            , RateIndex underlyingRateIndex
            , RelativeDateOffset resetLagOffset
            , Decimal margin
            , Decimal? observedRate
            , Money notionalAmount
            , AdjustableDate paymentDate
            , ForecastRateIndex forecastRateIndex
            , DiscountingTypeEnum? discountingType
            , Decimal? discountRate
            , FraDiscountingEnum? fraDiscounting) 
            : base(
                cashlfowId
                ,  accrualStartDate
                ,  accrualEndDate
                ,  adjustCalculationDatesIndicator
                ,  accrualBusinessCenters
                ,  accrualRollConvention
                , accrualDayCountfraction
                , resetRelativeTo
                , fixingDateRelativeOffset
                , margin
                ,  observedRate
                ,  notionalAmount
                ,  paymentDate
                , forecastRateIndex
                ,  discountingType
                ,  discountRate
                ,  fraDiscounting
                )
        {
            UnderlyingRateIndex = underlyingRateIndex;
            ResetLagOffset = resetLagOffset;
            PriceableCouponType = CouponType.StructuredRate;
            AnalyticsModel = new StructuredRateCouponAnalytic();
        }


        public PriceableStructuredRateCoupon
            (string uniqueId
             , DateTime accrualStartDate
             , DateTime accrualEndDate
             , Boolean adjustCalculationDatesIndicator
             , AdjustableDate paymentDate
             , Money notionalAmount
             , ResetRelativeToEnum resetRelativeTo
             , RelativeDateOffset fixingDateRelativeOffset
             , RateIndex underlyingRateIndex
             , RelativeDateOffset resetLagOffset
             , Decimal margin
             , Calculation calculation
             , ForecastRateIndex forecastRateIndex
            )
            : base
                (uniqueId
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
                )
        {
            UnderlyingRateIndex = underlyingRateIndex;
            ResetLagOffset = resetLagOffset;
            PriceableCouponType = CouponType.StructuredRate;
            AnalyticsModel = new StructuredRateCouponAnalytic();
        }

        #endregion

        #region Metrics for Valuation

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        override public AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;

            RequiresReset = modelData.ValuationDate > ResetDate;
            IsRealised = HasBeenRealised(ModelData.ValuationDate);

            if (AnalyticsModel == null)
            {
                AnalyticsModel = new StructuredRateCouponAnalytic();
            }
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);

            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            if (metrics.Contains(RateInstrumentMetrics.DiscountFactorAtMaturity))
            {
                metrics.RemoveAll(metricItem => metricItem != RateInstrumentMetrics.DiscountFactorAtMaturity);
            }

            var metricsToEvaluate = metrics.ToArray();
            if (metricsToEvaluate.Length > 0)
            {
                //May need to adapt for fra discounting.
                var isDiscounted = (GetDiscountingTypeEnum() ==
                                    DiscountingTypeEnum.Standard
                                        ? true
                                        : false);
                var reportingCurrency = ModelData.ReportingCurrency == null
                                            ? PaymentCurrency.Value
                                            : ModelData.ReportingCurrency.Value;

                IRateCouponParameters analyticModelParameters = new RateCouponParameters {
                                                                                             Currency = PaymentCurrency.Value,
                                                                                             ReportingCurrency = reportingCurrency,
                                                                                             IsDiscounted = isDiscounted,
                                                                                             IsRealised = IsRealised,
                                                                                             HasReset = RequiresReset,
                                                                                             NotionalAmount = NotionalAmount.amount,
                                                                                             Spread = Margin,
                                                                                             Rate = GetRate(),
                                                                                             DiscountRate = GetRate(),
                                                                                             YearFraction = CouponYearFraction};

                // Curve Related
                var streamMarket = (ISwapLegEnvironment)modelData.MarketEnvironment;
                var discountCurve = streamMarket.GetDiscountRateCurve();
                discountCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                var fwdCurve = streamMarket.GetForecastRateCurve();
                fwdCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                DiscountCurveName = discountCurve.GetPricingStructureId().UniqueIdentifier;
                ForecastCurveName = fwdCurve.GetPricingStructureId().UniqueIdentifier;

                //Volatility must be added for the structuredanalytic, which can handle convexity adjustments.

                //Calculating the appropriate index using an assetcontroller. THe config will need to be set up
                //for both Xibor and Swap indices.

                //Setting the parameters
                analyticModelParameters.StartDiscountFactor = (decimal)fwdCurve.GetDiscountFactor(ModelData.ValuationDate, AccrualStartDate);
                analyticModelParameters.EndDiscountFactor = (decimal)fwdCurve.GetDiscountFactor(ModelData.ValuationDate, AccrualEndDate);
                analyticModelParameters.PaymentDiscountFactor = (decimal)discountCurve.GetDiscountFactor(ModelData.ValuationDate, PaymentDate);
                analyticModelParameters.CurveYearFraction = GetPaymentYearFraction(ModelData.ValuationDate, PaymentDate);

                // Bucketed Delta
                if (BucketedDates.Count > 1)
                {
                    analyticModelParameters.PeriodAsTimesPerYear = GetPaymentYearFraction(BucketedDates[0], BucketedDates[1]);
                    analyticModelParameters.BucketedDiscountFactors = GetBucketedDiscountFactors(discountCurve, ModelData.ValuationDate, BucketedDates);
                }

                IRateInstrumentResults analyticResults = AnalyticsModel.Calculate<IRateInstrumentResults, RateInstrumentResults>(analyticModelParameters, metricsToEvaluate);
                UpdateResults(metricsToEvaluate, analyticResults);

                // store inputs and results from this run
                AnalyticModelParameters = analyticModelParameters;
                CalculationPerfomedIndicator = true;

            }
            AssetValuation valuation = GetValue(CalculationResults);
            valuation.id = Id;
            return valuation;
        }


        #endregion

        #region Instance Helpers

        #endregion;

        #region FpML Representation

        /// <summary>
        /// Builds the calculation period.
        /// </summary>
        /// <returns></returns>
        override protected CalculationPeriod BuildCalculationPeriod()
        {
            CalculationPeriod cp = base.BuildCalculationPeriod();

            FloatingRateDefinition floatingRateDefinition = FloatingRateDefinitionHelper.CreateSimple(ForecastRateIndex.floatingRateIndex, ForecastRateIndex.indexTenor, AdjustedFixingDate, GetRate(), Margin);
            cp.Item1 = floatingRateDefinition;

            if (floatingRateDefinition.calculatedRateSpecified)
            {
                cp.forecastRate = floatingRateDefinition.calculatedRate;
                cp.forecastRateSpecified = true;
            }
            cp.forecastAmount = MoneyHelper.GetAmount(CalculationResults.ExpectedValue, NotionalAmount.currency.Value);
            return cp;
        }

        #endregion

        #region Implementation of IPriceableFixedRateCoupon<IRateCouponParameters,IRateInstrumentResults>

        ///// <summary>
        ///// Gets the rate.
        ///// </summary>
        ///// <value>The rate.</value>
        //public new decimal Rate
        //{
        //    get { return (Decimal)base.Rate; }
        //    set { Rate = value; }
        //}

        #endregion

    }
}