#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Interpolations.Points;
using Orion.Models.ForeignExchange;
using Orion.Analytics.DayCounters;
using Orion.CurveEngine.Markets;
using Orion.ModelFramework;
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
    public class PriceableFxForwardRate : PriceableFxAssetController//, IPriceableRateSpreadAssetController
    {
        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        /// <summary>
        /// 
        /// </summary>
        public DateTime AdjustedStartDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime AdjustedEffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public Decimal StartDiscountFactor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IModelAnalytic<IFxRateAssetParameters, FxMetrics> AnalyticsModel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IFxAssetResults AnalyticResults { get; protected set; }
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
        /// 
        /// </summary>
        public Period Tenor { get; protected set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal ForwardAtMaturity => CalculationResults?.ForwardAtMaturity ?? FxRate.value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpolatedSpace"></param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            return ForwardAtMaturity;
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
        /// Gets or sets the name of the fx curve.
        /// </summary>
        /// <value>The name of the fx curve.</value>
        public string FxCurveName { get; set; }

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
        public BasicQuotation FxForwardRate => FxRate;

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public IFxAssetResults CalculationResults { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxSpotRate"/> class.
        /// </summary>
        /// <param name="notionalAmount">The notional.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="tenor">The tenor.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fxRateAsset">The asset itself</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fxForward">The forward points.</param>
        public PriceableFxForwardRate(DateTime baseDate, string tenor, decimal notionalAmount, FxSpotNodeStruct nodeStruct,
                                      FxRateAsset fxRateAsset, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fxForward)
        {
            PaymentDiscountFactorCcy12 = 1.0m;
            PaymentDiscountFactorCcy1 = 1.0m;
            Ccy2CurveName = string.Empty;
            Ccy1CurveName = string.Empty;
            ModelIdentifier = "SimpleFxRateAsset";
            Id = fxRateAsset.id;
            Tenor = PeriodHelper.Parse(tenor);
            NotionalAmount = notionalAmount;
            FxRateAsset = fxRateAsset;
            BaseDate = baseDate;
            SpotDateOffset = nodeStruct.SpotDate;
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);//GetSpotDate();
            AdjustedEffectiveDate = AdjustedStartDate; 
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, Tenor, nodeStruct.SpotDate.businessDayConvention);
            SetRate(fxForward);
        }

        /// <summary>
        /// Sets the rate.
        /// </summary>
        /// <param name="fxRate">The fixed rate.</param>
        private void SetRate(BasicQuotation fxRate)
        {
            if (String.Compare(fxRate.measureType.Value, RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                fxRate.measureType.Value = RateQuotationType;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }

            MarketQuote = fxRate.measureType.Value == RateQuotationType ? fxRate : FxRate;
        }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public decimal GetYearFraction(string dayCountFraction, DateTime adjustedStartDate, DateTime maturityDate)
        {
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCountFraction);
            var yearFraction = (decimal)dayCounter.YearFraction(adjustedStartDate, maturityDate);
            if (yearFraction == 0)
            {
                throw new NotSupportedException("YearFraction cannot be zero");
            }
            return yearFraction;
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new FxRateAssetAnalytic();
                //DependencyCreator.Resolve<IModelAnalytic<IFxRateAssetParameters, FxMetrics>>(_modelIdentifier);

            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
            var bEvalForwardAtMaturity = false;
            if (metrics.Contains(FxMetrics.ForwardAtMaturity))
            {
                bEvalForwardAtMaturity = true;
                metrics.RemoveAll(
                    metricItem => metricItem != FxMetrics.ForwardAtMaturity);
            }
            IFxRateAssetParameters analyticModelParameters = new FxRateAssetParameters();
            CalculationResults = new FxAssetResults();
            var metricsToEvaluate = metrics.ToArray();
            var marketEnvironment = modelData.MarketEnvironment;
            //1. instantiate curve
            //var fxCurve = (IFxCurve)modelData.MarketEnvironment.GetPricingStructure(FxCurveName);
            IFxCurve fxCurve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                fxCurve = (IFxCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                FxCurveName = fxCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleFxMarketEnvironment))
            {
                fxCurve = ((ISimpleFxMarketEnvironment)marketEnvironment).GetFxCurve();
                FxCurveName = fxCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                fxCurve = ((ISwapLegEnvironment)marketEnvironment).GetReportingCurrencyFxCurve();
                FxCurveName = fxCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                fxCurve = (IFxCurve)modelData.MarketEnvironment.GetPricingStructure(FxCurveName);
            }
            //3. Get the Rate
            if (FxRate != null)
            {
                analyticModelParameters.FxRate = FxRate.value;
            }
            if (bEvalForwardAtMaturity)
            {
                //4. Set the anaytic input parameters and Calculate the respective metrics
                CalculationResults =
                    AnalyticsModel.Calculate<IFxAssetResults, FxAssetResults>(analyticModelParameters,
                                                                               metricsToEvaluate);
            }
            else
            {
                analyticModelParameters.NotionalAmount = NotionalAmount;
                IDayCounter dc = Actual365.Instance;
                analyticModelParameters.YearFraction = (decimal)dc.YearFraction(modelData.ValuationDate, RiskMaturityDate);
                //3. Get the end discount factor - Need to fix this.
                analyticModelParameters.FxCurveSpotRate =
                    GetForwardAtMaturity(fxCurve, GetRiskMaturityDate(), modelData.ValuationDate);
                //4. Set the anaytic input parameters and Calculate the respective metrics
                CalculationResults =
                    AnalyticsModel.Calculate<IFxAssetResults, FxAssetResults>(analyticModelParameters,
                                                                               metricsToEvaluate);
            }
            return GetValue(CalculationResults);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="fxCurve">The fx curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal GetForwardAtMaturity(IFxCurve fxCurve, DateTime targetDate,
                                            DateTime valuationDate)
        {
            var point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)fxCurve.Value(point);
            return discountFactor;
        }

        #region Implementation of IPriceableSpreadAssetController

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public IList<decimal> Values
        {
            get => new[] { StartDiscountFactor, EndDiscountFactor };
            set => Values = value;
        }

        /// <summary>
        /// Gets the value at maturity.
        /// </summary>
        /// <value>The values.</value>
        public  decimal ValueAtMaturity
        {
            get
            {
                var values = Values;
                return values[values.Count - 1];
            }
            set
            {
                var values = Values;
                values[values.Count] = value;
                Values = values;
            }
        }

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public IList<DateTime> GetRiskDates()
        {
            return new[] { AdjustedStartDate, RiskMaturityDate };
        }

        #endregion

        #region Implementation of IPriceableSpreadAssetController2

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public decimal CalculateImpliedQuoteWithSpread(IInterpolatedSpace interpolatedSpace)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the marketquote of the asset.</returns>
        public decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace)
        {
            return MarketQuote.value - CalculateImpliedQuote(interpolatedSpace);
        }

        #endregion
    }
}