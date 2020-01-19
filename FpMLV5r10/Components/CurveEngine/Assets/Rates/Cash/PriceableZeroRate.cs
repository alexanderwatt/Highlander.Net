#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using FpML.V5r10.Reporting.Models.Assets;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Markets;
#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Deposit rates
    /// </summary>
    public class PriceableZeroRate : PriceableSimpleRateAsset
    {
        /// <summary>
        /// 
        /// </summary>
        public Boolean Adjust { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CompoundingFrequency CompoundingFrequency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DayCountFraction DayCountFraction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public new IModelAnalytic<IZeroRateAssetParameters, RateMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableZeroRate"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">The amount of cash.</param>
        /// <param name="term">The term.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableZeroRate(string id, DateTime baseDate, Decimal amount, Period term, ZeroRateNodeStruct nodeStruct,
                                 IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(id, baseDate, amount, nodeStruct.BusinessDayAdjustments, fixedRate)
        {
            ModelIdentifier = "ZeroCouponRate";
            Adjust = nodeStruct.AdjustDates;
            DayCountFraction = nodeStruct.DayCountFraction;
            RiskMaturityDate = !Adjust ? term.Add(baseDate) : GetEffectiveDate(baseDate, paymentCalendar, term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            CompoundingFrequency = nodeStruct.CompoundingFrequency;
            AdjustedStartDate = baseDate;
            YearFraction = GetYearFraction(DayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new ZeroCouponRateAssetAnalytic();
                //DependencyCreator.Resolve<IModelAnalytic<IZeroRateAssetParameters, RateMetrics>>(_modelIdentifier);
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            //
            var bEvalDiscountFactorAtMaturity = false;
            if (metrics.Contains(RateMetrics.DiscountFactorAtMaturity))
            {
                bEvalDiscountFactorAtMaturity = true;
                // Remove all metrics except DFAM
                //
                metrics.RemoveAll(metricItem => metricItem != RateMetrics.DiscountFactorAtMaturity);
            }
            IZeroRateAssetParameters analyticModelParameters = new ZeroRateAssetParameters { YearFraction = YearFraction };
            CalculationResults = new RateAssetResults();
            var metricsToEvaluate = metrics.ToArray();
            analyticModelParameters.PeriodAsTimesPerYear =
                CompoundingHelper.PeriodFractionFromCompoundingFrequency(BaseDate, CompoundingFrequency, DayCountFraction);
            var marketEnvironment = modelData.MarketEnvironment;
            IRateCurve curve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleRateMarketEnvironment))
            {
                curve = ((ISimpleRateMarketEnvironment)marketEnvironment).GetRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                curve = ((ISwapLegEnvironment)marketEnvironment).GetDiscountRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                curve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(CurveName);
            } 
            //
            analyticModelParameters.StartDiscountFactor = GetDiscountFactor(curve, BaseDate, modelData.ValuationDate);
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            if (bEvalDiscountFactorAtMaturity)
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                //
                CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, metricsToEvaluate);
                EndDiscountFactor = CalculationResults.DiscountFactorAtMaturity;
            }
            else
            {
                analyticModelParameters.NotionalAmount = Notional;
                //3. Get the end discount factor
                //
                analyticModelParameters.EndDiscountFactor = GetDiscountFactor(curve, GetRiskMaturityDate(), modelData.ValuationDate);
                //4. Set the anaytic input parameters and Calculate the respective metrics
                //
                CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, metricsToEvaluate);
            }

            return GetValue(CalculationResults);
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new ZeroCouponRateAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            IZeroRateAssetParameters analyticModelParameters = new ZeroRateAssetParameters
                                                                   {
                                                                       YearFraction = YearFraction,
                                                                       PeriodAsTimesPerYear =
                                                                           CompoundingHelper.
                                                                           PeriodFractionFromCompoundingFrequency(
                                                                           BaseDate, CompoundingFrequency,
                                                                           DayCountFraction),
                                                                       StartDiscountFactor =
                                                                           GetDiscountFactor(interpolatedSpace,
                                                                                             BaseDate, BaseDate)
                                                                   };
            //2. get start df = curve.getvalue(this._adjustedStartDate);
            //
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            CalculationResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.DiscountFactorAtMaturity });
            return CalculationResults.DiscountFactorAtMaturity;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new ZeroCouponRateAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            IZeroRateAssetParameters analyticModelParameters = new ZeroRateAssetParameters
                                                                   {
                                                                       YearFraction = YearFraction,
                                                                       PeriodAsTimesPerYear =
                                                                           CompoundingHelper.
                                                                           PeriodFractionFromCompoundingFrequency(
                                                                           BaseDate, CompoundingFrequency,
                                                                           DayCountFraction),
                                                                       StartDiscountFactor =
                                                                           GetDiscountFactor(interpolatedSpace,
                                                                                             BaseDate, BaseDate)
                                                                   };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            analyticModelParameters.NotionalAmount = Notional;
            //3. Get the end discount factor
            //
            analyticModelParameters.EndDiscountFactor = GetDiscountFactor(interpolatedSpace, GetRiskMaturityDate(), BaseDate);
            CalculationResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            CalculationResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return CalculationResults.ImpliedQuote;
        }
    }
}
