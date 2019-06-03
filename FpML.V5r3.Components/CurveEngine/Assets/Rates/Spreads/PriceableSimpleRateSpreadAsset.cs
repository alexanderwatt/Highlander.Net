#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Codes;
using Orion.CurveEngine.Markets;
using FpML.V5r3.Reporting;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Rates;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.CurveEngine.Helpers;
using Orion.Models.Assets;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableSimpleRateSpreadAsset : PriceableRateSpreadAssetController
    {
        ///<summary>
        ///</summary>
        public const string SpreadQuotationType = "MarketQuote";

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<ISimpleRateAssetParameters, RateSpreadMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IRateSpreadAssetResults AnalyticResults { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ModelIdentifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime RiskMaturityDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime AdjustedEffectiveDate { get; set; }


        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal DiscountFactorAtMaturity => AnalyticResults?.DiscountFactorAtMaturity ??
                                                            EndDiscountFactor;

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal DiscountFactorAtStart => AnalyticResults?.DiscountFactorAtStart ?? StartDiscountFactor;

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public Decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public Decimal YearFraction { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public Decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IRateSpreadAssetResults CalculationResults => AnalyticResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleRateSpreadAsset"/> class.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="spread">The fixed rate.</param>
        protected PriceableSimpleRateSpreadAsset(DateTime baseDate,
            BusinessDayAdjustments businessDayAdjustments, BasicQuotation spread)
        {
            ModelIdentifier = "SimpleSpreadAsset";
            BaseDate = baseDate;
            BusinessDayAdjustments = businessDayAdjustments;
            SetSpread(spread);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleRateAsset"/> class.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="riskMaturityDate">The risk maturity date.</param>
        /// <param name="spread">The spread to apply.</param>
        protected PriceableSimpleRateSpreadAsset(DateTime effectiveDate,
            DateTime riskMaturityDate, BasicQuotation spread)
        {
            ModelIdentifier = "SimpleSpreadAsset";
            AdjustedStartDate = effectiveDate;
            RiskMaturityDate = riskMaturityDate;
            SetSpread(spread);
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public override IList<decimal> Values => new[] { StartDiscountFactor, EndDiscountFactor };

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the marketquote of the asset.</returns>
        public override decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace)
        {
            return Spread.value;
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new RateSpreadAssetAnalytic();
            //DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, SpreadMetrics>>(_modelIdentifier);
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalDiscountFactorAtMaturity = false;
            var bEvalDiscountFactorAtStart = false;
            if (metrics.Contains(RateSpreadMetrics.DiscountFactorAtMaturity))
            {
                bEvalDiscountFactorAtMaturity = true;
                metrics.RemoveAll(
                    metricItem => metricItem != RateSpreadMetrics.DiscountFactorAtMaturity);
            }
            if (metrics.Contains(RateSpreadMetrics.DiscountFactorAtStart))
            {
                bEvalDiscountFactorAtStart = true;
                metrics.RemoveAll(
                    metricItem => metricItem != RateSpreadMetrics.DiscountFactorAtStart);
            }
            ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters {YearFraction = YearFraction};
            AnalyticResults = new RateSpreadAssetResults();
            var metricsToEvaluate = metrics.ToArray();
            var marketEnvironment = modelData.MarketEnvironment;
            IRateCurve curve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                curve = (IRateCurve) ((ISimpleMarketEnvironment) marketEnvironment).GetPricingStructure();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleRateMarketEnvironment))
            {
                curve = ((ISimpleRateMarketEnvironment) marketEnvironment).GetRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                curve = ((ISwapLegEnvironment) marketEnvironment).GetDiscountRateCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                curve = (IRateCurve) modelData.MarketEnvironment.GetPricingStructure(CurveName);
            }
            //3. Get the Rate
            StartDiscountFactor = GetDiscountFactor(curve, AdjustedStartDate, modelData.ValuationDate);
            //2. get start df = curve.getvalue(this._adjustedStartDate);
            analyticModelParameters.StartDiscountFactor = StartDiscountFactor;
            EndDiscountFactor = GetDiscountFactor(curve, RiskMaturityDate, modelData.ValuationDate);
            var fixedRate = GetRate(StartDiscountFactor, EndDiscountFactor, YearFraction);
            if (Spread != null)
            {
                analyticModelParameters.Rate = fixedRate + MarketQuoteHelper
                                                   .NormaliseGeneralPriceUnits(SpreadQuotationType, Spread,
                                                       "DecimalRate").value;
            }
            if (bEvalDiscountFactorAtMaturity)
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
                        metricsToEvaluate);
            }
            if (bEvalDiscountFactorAtStart)
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
                        metricsToEvaluate);
            }
            else
            {
                //3. Get the end discount factor
                EndDiscountFactor = GetDiscountFactor(curve, GetRiskMaturityDate(), modelData.ValuationDate);
                analyticModelParameters.EndDiscountFactor = EndDiscountFactor;
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
                        metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
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
                AnalyticsModel = new RateSpreadAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            //3. Get the Rate
            StartDiscountFactor = GetDiscountFactor(interpolatedSpace, AdjustedStartDate, BaseDate);
            ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters
            {
                YearFraction = YearFraction,
                StartDiscountFactor =
                    StartDiscountFactor
            };
            EndDiscountFactor = GetDiscountFactor(interpolatedSpace, RiskMaturityDate, BaseDate);
            var fixedRate = GetRate(StartDiscountFactor, EndDiscountFactor, YearFraction);
            if (Spread != null)
            {
                analyticModelParameters.Rate = fixedRate + MarketQuoteHelper
                                                   .NormaliseGeneralPriceUnits(SpreadQuotationType, Spread,
                                                       "DecimalRate").value;
            }
            AnalyticResults = new RateSpreadAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults =
                AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
                    new[] {RateSpreadMetrics.DiscountFactorAtMaturity});
            EndDiscountFactor = AnalyticResults.DiscountFactorAtMaturity;
            return AnalyticResults.DiscountFactorAtMaturity;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace)
        {
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new RateSpreadAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            //3. Get the Rate
            StartDiscountFactor = GetDiscountFactor(interpolatedSpace, AdjustedEffectiveDate, BaseDate);
            EndDiscountFactor = GetDiscountFactor(interpolatedSpace, RiskMaturityDate, BaseDate);
            ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters
            {
                YearFraction = YearFraction,
                StartDiscountFactor =
                    StartDiscountFactor,
                EndDiscountFactor =
                    EndDiscountFactor
            };
            //Values = new[] { startDF, endDF };
            var fixedRate = GetRate(StartDiscountFactor, EndDiscountFactor, YearFraction);
            if (Spread != null)
            {
                analyticModelParameters.Rate = fixedRate + MarketQuoteHelper
                                                   .NormaliseGeneralPriceUnits(SpreadQuotationType, Spread,
                                                       "DecimalRate").value;
            }
            AnalyticResults = new RateSpreadAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults =
                AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
                    new[] {RateSpreadMetrics.ImpliedQuote});
            return AnalyticResults.ImpliedQuote;
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
                AnalyticsModel = new RateSpreadAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            //3. Get the Rate
            StartDiscountFactor = GetDiscountFactor(interpolatedSpace, AdjustedStartDate, BaseDate);
            EndDiscountFactor = GetDiscountFactor(interpolatedSpace, RiskMaturityDate, BaseDate);
            ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters
            {
                YearFraction = YearFraction,
                StartDiscountFactor =
                    StartDiscountFactor,
                EndDiscountFactor =
                    EndDiscountFactor
            };
            var fixedRate = GetRate(StartDiscountFactor, EndDiscountFactor, YearFraction);
            if (Spread != null)
            {
                analyticModelParameters.Rate = fixedRate + MarketQuoteHelper
                                                   .NormaliseGeneralPriceUnits(SpreadQuotationType, Spread,
                                                       "DecimalRate").value;
            }
            AnalyticResults = new RateSpreadAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults =
                AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
                    new[] {RateSpreadMetrics.ImpliedQuote});
            return AnalyticResults.ImpliedQuote;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDF"></param>
        /// <param name="endDF"></param>
        /// <param name="yearFraction"></param>
        /// <returns></returns>
        protected static Decimal GetRate(decimal startDF, decimal endDF, decimal yearFraction)
        {
            return RateAnalytics.GetRate(startDF, endDF, yearFraction);
        }

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public override IList<DateTime> GetRiskDates()
        {
            return new[] { AdjustedEffectiveDate, RiskMaturityDate };
        }

        /// <summary>
        /// Sets the spread.
        /// </summary>
        /// <param name="spread">The spread.</param>
        protected void SetSpread(BasicQuotation spread)
        {
            if (String.Compare(spread.measureType.Value, "MarketQuote", StringComparison.OrdinalIgnoreCase) == 0)
            {
                MarketQuote = spread;
            }
            else
            {
                throw new ArgumentException(
                    $"Quotation must be of type 'MarketQuote', not '{spread.measureType.Value}'");
            }
        }

        /// <summary>
        /// Gets the year fraction to maturity.
        /// </summary>
        /// <returns></returns>
        public decimal GetTimeToMaturity(DateTime baseDate, DateTime maturityDate)
        {
            return (decimal) DayCounterHelper.ToDayCounter(DayCountFractionEnum.ACT_365_FIXED)
                .YearFraction(baseDate, maturityDate);
        }
    }
}