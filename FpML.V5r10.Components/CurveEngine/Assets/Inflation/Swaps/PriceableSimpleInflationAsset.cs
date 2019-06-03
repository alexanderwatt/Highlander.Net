#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using FpML.V5r10.Reporting.Models.Assets;
using Orion.Analytics.DayCounters;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Markets;

#endregion

namespace Orion.CurveEngine.Assets.Inflation.Swaps
{
    ///<summary>
    ///</summary>
    public abstract class PriceableSimpleInflationAsset : PriceableInflationAssetController
    {
        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        /// <summary>
        /// 
        /// </summary>
        protected IModelAnalytic<ISimpleDualAssetParameters, RateMetrics> AnalyticsModel { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        protected IRateAssetResults AnalyticResults { get; set; }

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
        public string IndexCurveName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal PaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal DiscountFactorAtMaturity => AnalyticResults?.DiscountFactorAtMaturity ?? EndDiscountFactor;

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
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public decimal Notional { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public Decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public abstract decimal[] GetYearFractions();

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableDeposit"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        protected PriceableSimpleInflationAsset(DateTime baseDate, Decimal amount,
                                                BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
        {
            Notional = amount;
            BaseDate = baseDate;
            BusinessDayAdjustments = businessDayAdjustments;
            SetRate(fixedRate);
        }

        /// <summary>
        /// Sets the rate.
        /// </summary>
        /// <param name="fixedRate">The fixed rate.</param>
        private void SetRate(BasicQuotation fixedRate)
        {
            if (String.Compare(fixedRate.measureType.Value, RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                fixedRate.measureType.Value = RateQuotationType;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }
            MarketQuote = fixedRate.measureType.Value == RateQuotationType ? fixedRate : FixedRate;
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new InflationAssetAnalytic();
            //DependencyCreator.Resolve<IModelAnalytic<ISimpleDualAssetParameters, RateMetrics>>("InflationAsset");
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalDiscountFactorAtMaturity = false;
            if (metrics.Contains(RateMetrics.DiscountFactorAtMaturity))
            {
                bEvalDiscountFactorAtMaturity = true;
                metrics.RemoveAll(
                    metricItem => metricItem != RateMetrics.DiscountFactorAtMaturity);
            }

            ISimpleDualAssetParameters analyticModelParameters = new DualRateAssetParameters
                                                                     {YearFraction = YearFraction};
            AnalyticResults = new RateAssetResults();
            var metricsToEvaluate = metrics.ToArray();
            var marketEnvironment = modelData.MarketEnvironment;
            IRateCurve curve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                curve = (IRateCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                CurveName = curve.GetPricingStructureId().UniqueIdentifier;
            }
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
            //2. get start df = curve.getvalue(this._adjustedStartDate);
            analyticModelParameters.StartDiscountFactor =
                GetDiscountFactor(curve, AdjustedStartDate, modelData.ValuationDate);
            //3. Get the Rate
            analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            if (bEvalDiscountFactorAtMaturity)
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
                                                                                  metricsToEvaluate);
                EndDiscountFactor = DiscountFactorAtMaturity;
            }
            else
            {
                analyticModelParameters.NotionalAmount = Notional;
                //3. Get the end index discount factor
                analyticModelParameters.EndDiscountFactor =
                    GetDiscountFactor(curve, GetRiskMaturityDate(), modelData.ValuationDate);
                //4. Get the payment discount factor
                analyticModelParameters.PaymentDiscountFactor =
                    GetDiscountFactor(curve, GetRiskMaturityDate(), modelData.ValuationDate);
                //5. Set the anaytic input parameters and Calculate the respective metrics
                AnalyticResults =
                    AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters,
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
                AnalyticsModel = new InflationAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            ISimpleDualAssetParameters analyticModelParameters = new DualRateAssetParameters
                                                                     {
                                                                         YearFraction = YearFraction,
                                                                         StartDiscountFactor =
                                                                             GetDiscountFactor(interpolatedSpace,
                                                                                               AdjustedStartDate, BaseDate)
                                                                     };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            AnalyticResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.DiscountFactorAtMaturity });
            return AnalyticResults.DiscountFactorAtMaturity;
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
                AnalyticsModel = new InflationAssetAnalytic();
                // DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, RateMetrics>>(_modelIdentifier);
            }
            ISimpleDualAssetParameters analyticModelParameters = new DualRateAssetParameters
                                                                     {
                                                                         YearFraction = YearFraction,
                                                                         StartDiscountFactor =
                                                                             GetDiscountFactor(interpolatedSpace,
                                                                                               AdjustedStartDate, BaseDate),
                                                                         EndDiscountFactor =
                                                                             GetDiscountFactor(interpolatedSpace,
                                                                                               GetRiskMaturityDate(), BaseDate)
                                                                     };
            //3. Get the Rate
            //
            if (FixedRate != null)
            {
                analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            }
            AnalyticResults = new RateAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<IRateAssetResults, RateAssetResults>(analyticModelParameters, new[] { RateMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote;
        }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public decimal GetYearFraction(string dayCountFraction, DateTime adjustedStartDate, DateTime maturityDate)
        {
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCountFraction);
            decimal yearFraction = (decimal)dayCounter.YearFraction(adjustedStartDate, maturityDate);
            if (yearFraction == 0)
            {
                throw new NotSupportedException("YearFraction cannot be zero");
            }
            return yearFraction;
        }
    }
}