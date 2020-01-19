#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using FpML.V5r10.Reporting.Models.Commodities;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.DayCounters;
using Orion.CurveEngine.Markets;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public class PriceableCommodityForward : PriceableCommodityAssetController
    {
        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";
        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<ICommodityAssetParameters, CommodityMetrics> AnalyticsModel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ICommodityAssetResults AnalyticResults { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string ModelIdentifier { get; protected set; }
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
        public RelativeDateOffset SpotDateOffset { get; set; }

        /// <summary>
        /// Gets the notional amount.
        /// </summary>
        public decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the name of the fx curve.
        /// </summary>
        /// <value>The name of the fx curve.</value>
        public string CommodityCurveName { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public IBusinessCalendar FixingCalendar { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public Period Tenor { get; protected set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal IndexAtMaturity => AnalyticResults?.IndexAtMaturity ?? CommodityValue.value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpolatedSpace"></param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            return IndexAtMaturity;
        }

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return RiskMaturityDate;
        }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string Ccy1CurveName { get; set; }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string Ccy2CurveName { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public decimal PaymentDiscountFactorCcy1 { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public decimal PaymentDiscountFactorCcy12 { get; set; }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public BasicQuotation CommodityForward => CommodityValue;

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ICommodityAssetResults CalculationResults => AnalyticResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCommoditySpot"/> class.
        /// </summary>
        /// <param name="notionalAmount"></param>
        /// <param name="baseDate"></param>
        /// <param name="term">The tenor.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// <param name="commodityForward">The forward points.</param>
        public PriceableCommodityForward(DateTime baseDate, decimal notionalAmount, string term,  CommoditySpotNodeStruct nodeStruct,
                                         IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation commodityForward)
        {
            PaymentDiscountFactorCcy12 = 1.0m;
            PaymentDiscountFactorCcy1 = 1.0m;
            Ccy2CurveName = string.Empty;
            Ccy1CurveName = string.Empty;
            ModelIdentifier = "SimpleCommodityAsset";
            if (nodeStruct.Commodity != null)
            {
                Id = nodeStruct.Commodity.id;
            }
            Tenor = PeriodHelper.Parse(term);
            NotionalAmount = notionalAmount;
            CommodityAsset = nodeStruct.Commodity;
            BaseDate = baseDate;
            SpotDateOffset = nodeStruct.SpotDate;
            //FixingCalendar = BusinessCenterHelper.ToBusinessCalendar(SpotDateOffset.businessCenters);
            var spotDate = GetSpotDate(baseDate, fixingCalendar, SpotDateOffset);
            RiskMaturityDate = GetForwardDate(spotDate, paymentCalendar, Tenor, SpotDateOffset.businessDayConvention);
            SetRate(commodityForward);
        }

        /// <summary>
        /// Sets the rate.
        /// </summary>
        /// <param name="commodityValue">The fixed rate.</param>
        private void SetRate(BasicQuotation commodityValue)
        {
            if (String.Compare(commodityValue.measureType.Value, RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                commodityValue.measureType.Value = RateQuotationType;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }

            MarketQuote = commodityValue.measureType.Value == RateQuotationType ? commodityValue : CommodityValue;
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new CommodityAssetAnalytic();
                //DependencyCreator.Resolve<IModelAnalytic<ICommodityAssetParameters, CommodityMetrics>>(_modelIdentifier);
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalForwardAtMaturity = false;
            if (metrics.Contains(CommodityMetrics.IndexAtMaturity))
            {
                bEvalForwardAtMaturity = true;
                metrics.RemoveAll(
                    metricItem => metricItem != CommodityMetrics.IndexAtMaturity);
            }
            ICommodityAssetParameters analyticModelParameters = new CommodityAssetParameters();
            AnalyticResults = new CommodityAssetResults();
            var metricsToEvaluate = metrics.ToArray();
            var marketEnvironment = modelData.MarketEnvironment;
            //1. instantiate curve
            ICommodityCurve commodityCurve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                commodityCurve = (ICommodityCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                CommodityCurveName = commodityCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleCommodityMarketEnvironment))
            {
                commodityCurve = ((ISimpleCommodityMarketEnvironment)marketEnvironment).GetCommodityCurve();
                CommodityCurveName = commodityCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                commodityCurve = ((ISwapLegEnvironment)marketEnvironment).GetCommodityCurve();
                CommodityCurveName = commodityCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                commodityCurve = (ICommodityCurve)modelData.MarketEnvironment.GetPricingStructure(CommodityCurveName);
            } 
            //3. Get the Rate
            if (CommodityValue != null)
            {
                analyticModelParameters.CommodityForward = CommodityValue.value;
            }
            if (bEvalForwardAtMaturity)
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<ICommodityAssetResults, CommodityAssetResults>(analyticModelParameters,
                                                                                             metricsToEvaluate);
            }
            else
            {
                analyticModelParameters.NotionalAmount = NotionalAmount;
                IDayCounter dc = Actual365.Instance;
                analyticModelParameters.YearFraction = (decimal)dc.YearFraction(modelData.ValuationDate, RiskMaturityDate);
                //3. Get the end discount factor - Need to fix this.
                analyticModelParameters.CommodityCurveForward =
                    GetIndexAtMaturity(commodityCurve, GetRiskMaturityDate(), modelData.ValuationDate);
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<ICommodityAssetResults, CommodityAssetResults>(analyticModelParameters,
                                                                                             metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="dfCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal GetDiscountFactor(IRateCurve dfCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            var point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)dfCurve.Value(point);
            return discountFactor;
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="commodityCurve">The fx curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal GetIndexAtMaturity(ICommodityCurve commodityCurve, DateTime targetDate,
                                          DateTime valuationDate)
        {
            var point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)commodityCurve.Value(point);
            return discountFactor;
        }
    }
}