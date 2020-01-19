using System;
using System.Collections.Generic;
using National.QRSC.AnalyticModels.Rates;
using National.QRSC.AnalyticModels.Rates.Coupons;
using National.QRSC.FpML.V47;
using National.QRSC.ModelFramework.Instruments;
using National.QRSC.ModelFramework.MarketEnvironments;

namespace Orion.ValuationEngine.Instruments.Rates.Coupons
{
    [Serializable]
    public class PriceableComplexRateCoupon : PriceableFloatingRateCoupon
    {
        #region Member Fields

        //Fixing
        public List<PriceableStructuredRateCoupon> CalculationPeriods { get; set; }

        #endregion

        #region Public Fields

        ///// <summary>
        ///// Gets the reset date.
        ///// </summary>
        ///// <value>The reset date.</value>
        //public DateTime[] ResetDates
        //{
        //    get { return AdjustedFixingDates; }
        //}

        /// <summary>
        /// Gets the margin.
        /// </summary>
        /// <value>The margin.</value>
        public decimal[] Margins
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Constructors

        public PriceableComplexRateCoupon()
        {
            PriceableCouponType = CouponType.ComplexRate;
            AnalyticsModel = new FloatingRateCouponAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateCoupon"/> class.
        /// </summary>
        /// <param name="cashlfowId">The stream id.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="margin">The margin.</param>
        /// <param name="observedRate">The observed Rate. If this is not null, then it is used.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="adjustedFixingDate">The adjusted fixing date.</param>
        /// <param name="dayCountfraction">Type of day Countfraction.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="forecastRateIndex">The forecastrateindex.</param>
        /// <param name="discountingType">The swap discounting type.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        /// then it is assumed that there is no fradiscounting</param>
        public PriceableComplexRateCoupon
            (
            string cashlfowId
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , DateTime adjustedFixingDate
            , DayCountFraction dayCountfraction
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
                ,  accrualStartDate
                ,  accrualEndDate
                ,  adjustedFixingDate
                ,  dayCountfraction
                ,  margin
                , observedRate
                , notionalAmount
                , paymentDate
                , forecastRateIndex
                , discountingType
                , discountRate
                , fraDiscounting
                )
        {
        }

        // <summary>
        // Initializes a new instance of the <see cref="PriceableRateCoupon"/> class.
        // </summary>
        // <param name="cashlfowId">The stream id.</param>
        // <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        // <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        // <param name="adjustCalculationDatesIndicator">if set to <c>true</c> [adjust calculation dates indicator].</param>
        // <param name="accrualBusinessCenters">The accrual business centers.</param>
        // <param name="margin">The margin.</param>
        // <param name="observedRate">The observed Rate.</param>
        // <param name="notionalAmount">The notional amount.</param>
        // <param name="dayCountfraction">Type of day Countfraction.</param>
        // <param name="paymentDate">The payment date.</param>
        // <param name="accrualRollConvention">The accrual roll convention.</param>
        // <param name="resetRelativeTo">reset relative to?</param>
        // <param name="fixingDateRelativeOffset">The fixing date offset.</param>
        // <param name="forecastRateIndex">The forecastrateindex.</param>
        // <param name="discountingType">The swap discounting type.</param>
        // <param name="discountRate">The discount rate.</param>
        // <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        // then it is assumed that there is no fradiscounting</param>
        //public PriceableComplexRateCoupon
        //    (
        //    string cashlfowId
        //    , DateTime accrualStartDate
        //    , DateTime accrualEndDate
        //    , Boolean adjustCalculationDatesIndicator
        //    , BusinessCenters accrualBusinessCenters
        //    , BusinessDayConventionEnum accrualRollConvention
        //    , DayCountFraction dayCountfraction
        //    , ResetRelativeToEnum resetRelativeTo
        //    , RelativeDateOffset fixingDateRelativeOffset
        //    , Decimal margin
        //    , Decimal? observedRate
        //    , Money notionalAmount
        //    , AdjustableDate paymentDate
        //    , ForecastRateIndex forecastRateIndex
        //    , DiscountingTypeEnum? discountingType
        //    , Decimal? discountRate
        //    , FraDiscountingEnum? fraDiscounting) 
        //    : base(
        //     cashlfowId
        //    ,  CouponType.FloatingRate
        //    ,  accrualStartDate
        //    ,  accrualEndDate
        //    ,  adjustCalculationDatesIndicator
        //    ,  accrualBusinessCenters
        //    ,  accrualRollConvention
        //    ,  dayCountfraction
        //    ,  observedRate
        //    ,  notionalAmount
        //    ,  paymentDate
        //    ,  discountingType
        //    ,  discountRate
        //    ,  fraDiscounting
        //    )
        //{
        //}


        //public PriceableComplexRateCoupon
        //    (string uniqueId
        //     , DateTime accrualStartDate
        //     , DateTime accrualEndDate
        //     , Boolean adjustCalculationDatesIndicator
        //     , AdjustableDate paymentDate
        //     , Money notionalAmount
        //     , ResetRelativeToEnum resetRelativeTo
        //     , RelativeDateOffset fixingDateRelativeOffset
        //     , Decimal margin
        //     , Calculation calculation
        //     , ForecastRateIndex forecastRateIndex
        //    )
        //    : base
        //        (uniqueId
        //    , CouponType.FloatingRate
        //    , accrualStartDate
        //    , accrualEndDate
        //    , adjustCalculationDatesIndicator
        //    , BusinessDayAdjustmentsHelper.Create(fixingDateRelativeOffset.businessDayConvention, fixingDateRelativeOffset.businessCenters)
        //    , calculation
        //    , notionalAmount
        //    , paymentDate
        //        )
        //{
        //}

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
                AnalyticsModel = new FloatingRateCouponAnalytic();
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

        #region Payment


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
        public override decimal GetRate()
        {
            return GetRate(AccrualStartDate, AccrualEndDate, ModelData.ValuationDate);
        }

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

        #region Static Helpers

        #endregion;

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