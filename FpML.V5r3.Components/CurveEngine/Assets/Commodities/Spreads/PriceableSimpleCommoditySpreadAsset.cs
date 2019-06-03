#region Using directives

using System;
using Orion.Models.Commodities;
using Orion.CurveEngine.Markets;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableSimpleCommoditySpreadAsset : PriceableCommoditySpreadAssetController
    {
        ///<summary>
        ///</summary>
        public const string SpreadQuotationType = "MarketQuote";
        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<ICommodityAssetParameters, CommoditySpreadMetrics> AnalyticsModel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ICommoditySpreadAssetResults AnalyticResults { get; set; }
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
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal IndexAtMaturity => AnalyticResults?.IndexAtMaturity ?? ValueAtMaturity;

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ICommoditySpreadAssetResults CalculationResults => AnalyticResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleCommoditySpreadAsset"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="spread">The fixed rate.</param>
        protected PriceableSimpleCommoditySpreadAsset(DateTime baseDate, BasicQuotation spread)
        {
            ModelIdentifier = "SimpleSpreadAsset";
            BaseDate = baseDate;
            SetRate(spread);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new CommoditySpreadAssetAnalytic();
            //DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, SpreadMetrics>>(_modelIdentifier);
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalDiscountFactorAtMaturity = false;
            if (metrics.Contains(CommoditySpreadMetrics.IndexAtMaturity))
            {
                bEvalDiscountFactorAtMaturity = true;
                metrics.RemoveAll(
                    metricItem => metricItem != CommoditySpreadMetrics.IndexAtMaturity);
            }
            var analyticModelParameters = new CommodityAssetParameters();
            AnalyticResults = new CommoditySpreadAssetResults();
            var metricsToEvaluate = metrics.ToArray();
            var marketEnvironment = modelData.MarketEnvironment;
            ICommodityCurve curve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                curve = (ICommodityCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleCommodityMarketEnvironment))
            {
                curve = ((ISimpleCommodityMarketEnvironment)marketEnvironment).GetCommodityCurve();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                curve = (ICommodityCurve)modelData.MarketEnvironment.GetPricingStructure(CurveName);
            }
            var endDF = GetIndex(curve, RiskMaturityDate, modelData.ValuationDate);
            Values = new[] { endDF };
            //3. Get the end discount factor
            analyticModelParameters.CommodityForward = endDF;
            if (Spread != null)
            {
                analyticModelParameters.Spread = MarketQuoteHelper.NormaliseGeneralPriceUnits(SpreadQuotationType, Spread, "Price").value;
            }
            if (bEvalDiscountFactorAtMaturity)
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<ICommoditySpreadAssetResults, CommoditySpreadAssetResults>(analyticModelParameters,
                                                                                      metricsToEvaluate);
                ValueAtMaturity = AnalyticResults.IndexAtMaturity;
            }
            else
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<ICommoditySpreadAssetResults, CommoditySpreadAssetResults>(analyticModelParameters,
                                                                                      metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
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
                AnalyticsModel = new CommoditySpreadAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            //3. Get the Rate
            //var startDF = GetDiscountFactor(interpolatedSpace, AdjustedEffectiveDate, BaseDate);
            var endDF = GetIndex(interpolatedSpace, RiskMaturityDate, BaseDate);
            var analyticModelParameters = new CommodityAssetParameters
            {
                CommodityForward = endDF
            };
            Values = new[] { endDF };
            if (Spread != null)
            {
                analyticModelParameters.Spread = MarketQuoteHelper.NormaliseGeneralPriceUnits(SpreadQuotationType, Spread, "MarketQuote").value;
            }
            AnalyticResults = new CommoditySpreadAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<ICommoditySpreadAssetResults, CommoditySpreadAssetResults>(analyticModelParameters, new[] { CommoditySpreadMetrics.IndexAtMaturity });
            return AnalyticResults.IndexAtMaturity;
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
                AnalyticsModel = new CommoditySpreadAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            //3. Get the Rate
            var endDF = GetIndex(interpolatedSpace, RiskMaturityDate, BaseDate);
            var analyticModelParameters = new CommodityAssetParameters
                                                                 {
                                                                     CommodityForward =
                                                                         endDF
                                                                 };
            Values = new[] { endDF };
            if (Spread != null)
            {
                analyticModelParameters.Spread = MarketQuoteHelper.NormaliseGeneralPriceUnits(SpreadQuotationType, Spread, "MarketQuote").value;
            }
            AnalyticResults = new CommoditySpreadAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<ICommoditySpreadAssetResults, CommoditySpreadAssetResults>(analyticModelParameters, new[] { CommoditySpreadMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote;
        }

        /// <summary>
        /// Sets the price.
        /// </summary>
        /// <param name="commodityPrice">The commodity price.</param>
        protected void SetRate(BasicQuotation commodityPrice)
        {
            if (String.Compare(commodityPrice.measureType.Value, "MarketQuote", StringComparison.OrdinalIgnoreCase) == 0)
            {
                MarketQuote = commodityPrice; 
            }
            else
            {
                throw new ArgumentException(string.Format("Quotation must be of type 'MarketQuote', not '{0}'", commodityPrice.measureType.Value));
            }
        }


    }
}