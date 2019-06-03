#region Using directives

using System;
using FpML.V5r3.Codes;
using Orion.Models.Commodities;
using Orion.Analytics.DayCounters;
using Orion.CurveEngine.Markets;
using Orion.ModelFramework;
using Orion.Analytics.Interpolations.Points;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableCommodityFuturesAsset : PriceableCommodityAssetController, IPriceableFuturesAssetController
    {
        ///<summary>
        ///</summary>
        ///<typeparam name="R"></typeparam>
        public delegate R DelegateToCalculateMethod<R>();

        /// <summary>
        /// The characteristic date roll of this particular contract. TODO Move to a configuration setting
        /// </summary>
        public Offset RollOffset { get; protected set; }

        //The units are in barrels
        public PriceQuoteUnitsEnum PriceQuoteUnits{ get; protected set; }

        //The number of untis for this particular contract
        public int Amount { get; protected set; }

        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        /// <summary>
        /// 
        /// </summary>
        public String CommodityType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Period ContractMonthPeriod { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String ExchangeIdentifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<ICommodityFuturesAssetParameters, CommodityMetrics> AnalyticsModel { get; set; }

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
        public string SettlementBasis { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public string ContractSeries { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public PeriodEnum Margin { get; protected set; }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        public int Position { get; set; }

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
        public DateTime LastTradeDate { get; protected set; }

        /// <summary>
        /// Returns the futures expiry or the option expiry date.
        /// </summary>
        /// <returns></returns>
        public DateTime GetBootstrapDate() => LastTradeDate;

        /// <summary>
        /// 
        /// </summary>
        public Future Future { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        protected Decimal ForwardIndex = 0.9m;

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal IndexAtMaturity => AnalyticResults?.IndexAtMaturity ?? CommodityValue.value;

        /// <summary>
        /// Gets the day counter.
        /// </summary>
        /// <value>The day counter.</value>
        public IDayCounter DayCounter { get; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal TimeToExpiry { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets the time to expiry.
        /// </summary>
        /// <returns></returns>
        public decimal GetTimeToExpiry()
        {
            return TimeToExpiry;
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public BasicQuotation Index => CommodityValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCommodityForward"/> class.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="future"></param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="commodityValue">The fixed rate.</param>
        /// <param name="rollOffset">The commodity contract roll offset.</param>
        protected PriceableCommodityFuturesAsset(DateTime baseDate, Future future, BusinessDayAdjustments businessDayAdjustments,
            BasicQuotation commodityValue, Offset rollOffset)
        {
            ModelIdentifier = "CommoditiesFuturesAsset";
            Margin = PeriodEnum.D;
            RollOffset = rollOffset;
            if (future?.multiplier != null)
            {
                Amount = int.Parse(future.multiplier);
            }
            Position = 1;
            BaseDate = baseDate;
            Future = future;
            BusinessDayAdjustments = businessDayAdjustments;
            SetIndex(commodityValue);
            TimeToExpiry = 1.0m;
            CurveName = string.Empty;
            DayCounter = Actual365.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCommodityForward"/> class.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="future"></param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="position">The number of contracts held.</param>
        /// <param name="commodityValue">The fixed rate.</param>
        /// <param name="rollOffset">The commodity contract roll offset.</param>
        protected PriceableCommodityFuturesAsset(DateTime baseDate, Future future,
            BusinessDayAdjustments businessDayAdjustments, int position, BasicQuotation commodityValue, 
            Offset rollOffset)
        {
            ModelIdentifier = "CommoditiesFuturesAsset";
            Margin = PeriodEnum.D;
            RollOffset = rollOffset;
            if (future?.multiplier != null)
            {
                Amount = int.Parse(future.multiplier);
            }
            Position = position;
            BaseDate = baseDate;
            Future = future;
            BusinessDayAdjustments = businessDayAdjustments;
            SetIndex(commodityValue);
            TimeToExpiry = 1.0m;
            CurveName = string.Empty;
            DayCounter = Actual365.Instance;
        }

        /// <summary>
        /// Sets the rate.
        /// </summary>
        /// <param name="commodityValue">The fixed rate.</param>
        private void SetIndex(BasicQuotation commodityValue)
        {
            if (String.Compare(commodityValue.measureType.Value, RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                commodityValue.measureType.Value = RateQuotationType;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }
            MarketQuote = commodityValue.measureType.Value == RateQuotationType
                                  ? commodityValue
                                  : CommodityValue;
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new CommodityFuturesAssetAnalytic();
                //DependencyCreator.Resolve<IModelAnalytic<ICommodityFuturesAssetParameters, CommodityMetrics>>(_modelIdentifier);
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalIndexAtMaturity = false;
            if (metrics.Contains(CommodityMetrics.IndexAtMaturity))
            {
                bEvalIndexAtMaturity = true;
                //  remove all except DiscountFactorAtMaturity metric
                //
                metrics.RemoveAll(metricItem => metricItem != CommodityMetrics.IndexAtMaturity);
            }
            var metricsToEvaluate = metrics.ToArray();
            ICommodityFuturesAssetParameters analyticModelParameters = new CommodityFuturesAssetParameters
                                                                           {
                                                                               UnitAmount = Amount,
                                                                               TimeToExpiry = TimeToExpiry
                                                                           };
            var marketEnvironment = modelData.MarketEnvironment;
            //1. instantiate curve
            ICommodityCurve commodityCurve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                commodityCurve = (ICommodityCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                CurveName = commodityCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleCommodityMarketEnvironment))
            {
                commodityCurve = ((ISimpleCommodityMarketEnvironment)marketEnvironment).GetCommodityCurve();
                CurveName = commodityCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                commodityCurve = ((ISwapLegEnvironment)marketEnvironment).GetCommodityCurve();
                CurveName = commodityCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                commodityCurve = (ICommodityCurve)modelData.MarketEnvironment.GetPricingStructure(CurveName);
            } 
            //4. Get the Index
            analyticModelParameters.Index = CommodityValue.value;
            if (bEvalIndexAtMaturity)
            {
                //5. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<ICommodityAssetResults, CommodityAssetResults>(analyticModelParameters,
                                                                                             metricsToEvaluate);
                ForwardIndex = IndexAtMaturity;
            }
            else
            {
                //4. Get the end discount factor
                ForwardIndex = GetCommodityForward(commodityCurve, GetRiskMaturityDate(), modelData.ValuationDate);
                analyticModelParameters.Index = IndexAtMaturity;
                //5. Get the TimeToExpiry
                analyticModelParameters.TimeToExpiry = GetTimeToExpiry();
                //6. Get the position
                analyticModelParameters.Position = Position;
                //7. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<ICommodityAssetResults, CommodityAssetResults>(analyticModelParameters,
                                                                                             metricsToEvaluate);
            }
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="commodityCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal GetCommodityForward(ICommodityCurve commodityCurve, DateTime targetDate,
                                           DateTime valuationDate)
        {
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)commodityCurve.Value(point);
            return discountFactor;
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
        /// Gets the exchange initial margin.
        /// </summary>
        /// <value>The exchange initial margin.</value>
        public decimal InitialMargin { get; set; }

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        /// <value>The convexity adjustment.</value>
        public decimal ConvexityAdjustment { get; set; }

        /// <summary>
        /// Gets the last settled value.
        /// </summary>
        /// <value>The last settled value.</value>
        public decimal LastSettledValue { get; set; }
    }
}