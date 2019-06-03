#region Using directives

using System;
using System.Collections.Generic;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using DayCounterHelper=Orion.Analytics.DayCounters.DayCounterHelper;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Deposit rates
    /// </summary>
    public class PriceableSpreadFra : PriceableSimpleRateSpreadAsset
    {
        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset FixingDateOffset { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public SimpleFra SimpleFra { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public RateIndex UnderlyingRateIndex { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal TimeToExpiry { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSpreadFra"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="underlyingRateIndex">The underlying rate index.</param>
        /// <param name="spread">The spread.</param>
        /// <param name="paymentCalendar">A paymentCalendar.</param>
        public PriceableSpreadFra(DateTime baseDate, SimpleFraNodeStruct nodeStruct, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, 
                                  RateIndex underlyingRateIndex, BasicQuotation spread)
            : base(baseDate, nodeStruct.BusinessDayAdjustments, spread)
        {
            Id = nodeStruct.SimpleFra.id;
            SimpleFra = nodeStruct.SimpleFra;
            FixingDateOffset = nodeStruct.SpotDate;
            UnderlyingRateIndex = underlyingRateIndex;
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            AdjustedEffectiveDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.SimpleFra.startTerm, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.SimpleFra.endTerm, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFractions()[0];
            TimeToExpiry = GetTimeToMaturity(baseDate, RiskMaturityDate);
            SetSpread(spread);
        }

        /// <summary>
        /// Gets the forward spot date.
        /// </summary>
        /// <returns></returns>
        protected DateTime GetSpotDate()
        {
            return AdjustedStartDate;
        }

        /// <summary>
        /// Gets the effective date.
        /// </summary>
        /// <returns></returns>
        protected DateTime GetEffectiveDate()
        {
            return AdjustedEffectiveDate;
        }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return RiskMaturityDate;
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public override IList<decimal> Values => new[] { StartDiscountFactor, DiscountFactorAtMaturity };

        /// <summary>
        /// Gets the value at maturity.
        /// </summary>
        /// <value>The values.</value>
        public override decimal ValueAtMaturity => DiscountFactorAtMaturity;

        ///// <summary>
        ///// Gets the risk maturity date.
        ///// </summary>
        ///// <returns></returns>
        //public override IList<DateTime> GetRiskDates()
        //{
        //    return new[] { AdjustedEffectiveDate, RiskMaturityDate };
        //}

        /////<summary>
        /////</summary>
        /////<param name="interpolatedSpace"></param>
        /////<returns>The spread calculated from the curve provided and the marketquote of the asset.</returns>
        //public override decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace)
        //{
        //    return Spread.value;
        //}

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public decimal[] GetYearFractions()
        {
            return
                new[]
                    {
                        (decimal)
                        DayCounterHelper.Parse(SimpleFra.dayCountFraction.Value).YearFraction(
                            AdjustedEffectiveDate, GetRiskMaturityDate())
                    };
        }

        ///// <summary>
        ///// Gets the year fraction to maturity.
        ///// </summary>
        ///// <returns></returns>
        //public decimal GetTimeToMaturity(DateTime baseDate, DateTime maturityDate)
        //{
        //    return (decimal)DayCounterHelper.ToDayCounter(DayCountFractionEnum.ACT_365_FIXED).YearFraction(baseDate, maturityDate);
        //}

        ///// <summary>
        ///// Calculates the specified model data.
        ///// </summary>
        ///// <param name="modelData">The model data.</param>
        ///// <returns></returns>
        //public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        //{
        //    ModelData = modelData;
        //    AnalyticsModel = new RateSpreadAssetAnalytic();
        //    var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
        //    // Determine if DFAM has been requested - if so thats all we evaluate - every other metric is ignored
        //    var bEvalDiscountFactorAtMaturity = false;
        //    if (metrics.Contains(RateSpreadMetrics.DiscountFactorAtMaturity))
        //    {
        //        bEvalDiscountFactorAtMaturity = true;
        //        metrics.RemoveAll(
        //            metricItem => metricItem != RateSpreadMetrics.DiscountFactorAtMaturity);
        //    }
        //    AnalyticResults = new RateSpreadAssetResults();
        //    var metricsToEvaluate = metrics.ToArray();
        //    var marketEnvironment = modelData.MarketEnvironment;
        //    IRateCurve curve = null;
        //    //1. instantiate curve
        //    if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
        //    {
        //        curve = (IRateCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
        //        CurveName = curve.GetPricingStructureId().UniqueIdentifier;
        //    }
        //    if (marketEnvironment.GetType() == typeof(SimpleRateMarketEnvironment))
        //    {
        //        curve = ((ISimpleRateMarketEnvironment)marketEnvironment).GetRateCurve();
        //        CurveName = curve.GetPricingStructureId().UniqueIdentifier;
        //    }
        //    if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
        //    {
        //        curve = ((ISwapLegEnvironment)marketEnvironment).GetDiscountRateCurve();
        //        CurveName = curve.GetPricingStructureId().UniqueIdentifier;
        //    }
        //    if (marketEnvironment.GetType() == typeof(MarketEnvironment))
        //    {
        //        curve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(CurveName);
        //    } 
        //    StartDiscountFactor = GetDiscountFactor(curve, AdjustedEffectiveDate, modelData.ValuationDate);
        //    EndDiscountFactor = GetDiscountFactor(curve, RiskMaturityDate, modelData.ValuationDate);
        //    ISimpleRateAssetParameters analyticModelParameters
        //        = new RateAssetParameters
        //              {
        //                  YearFraction = YearFraction,
        //                  StartDiscountFactor = StartDiscountFactor,
        //                  EndDiscountFactor = EndDiscountFactor
        //              };
        //    //3. Get the Rate
        //    var fixedRate = GetRate(StartDiscountFactor, EndDiscountFactor, YearFraction);//must calc from
        //    if (Spread != null)
        //    {
        //        analyticModelParameters.Rate = fixedRate + MarketQuoteHelper.NormaliseGeneralPriceUnits(SpreadQuotationType, Spread, "DecimalRate").value;
        //    }
        //    if (bEvalDiscountFactorAtMaturity)
        //    {
        //        //4. Set the anaytic input parameters and Calculate the respective metrics
        //        AnalyticResults =
        //            AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
        //                                                                               metricsToEvaluate);
        //        EndDiscountFactor = AnalyticResults.DiscountFactorAtMaturity;
        //    }
        //    else
        //    {
        //        //3. Get the end discount factor
        //        analyticModelParameters.EndDiscountFactor = EndDiscountFactor;
        //        //4. Set the anaytic input parameters and Calculate the respective metrics
        //        AnalyticResults =
        //            AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
        //                                                                               metricsToEvaluate);
        //    }
        //    return GetValue(AnalyticResults);
        //}

        ///// <summary>
        ///// Calculates the specified metric for the fast bootstrapper.
        ///// </summary>
        ///// <param name="interpolatedSpace">The intepolated Space.</param>
        ///// <returns></returns>
        //public override decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace)
        //{
        //    AnalyticsModel = new RateSpreadAssetAnalytic();
        //    AnalyticResults = new RateSpreadAssetResults();
        //    StartDiscountFactor = GetDiscountFactor(interpolatedSpace, AdjustedEffectiveDate, BaseDate);
        //    EndDiscountFactor = GetDiscountFactor(interpolatedSpace, RiskMaturityDate, BaseDate);
        //    ISimpleRateAssetParameters analyticModelParameters 
        //        = new RateAssetParameters
        //              {
        //                  YearFraction = YearFraction,
        //                  StartDiscountFactor = StartDiscountFactor,
        //                  EndDiscountFactor = EndDiscountFactor
        //              };
        //    //3. Get the Rate
        //    var fixedRate = GetRate(StartDiscountFactor, EndDiscountFactor, YearFraction);//must calc from
        //    if (Spread != null)
        //    {
        //        analyticModelParameters.Rate = fixedRate + 
        //                MarketQuoteHelper.NormaliseGeneralPriceUnits(SpreadQuotationType, Spread, "DecimalRate").value;
        //    }
        //    //4. Set the anaytic input parameters and Calculate the respective metrics
        //    AnalyticResults =
        //        AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
        //                                                                           new[] { RateSpreadMetrics.DiscountFactorAtMaturity });
        //    EndDiscountFactor = AnalyticResults.DiscountFactorAtMaturity;
        //    return AnalyticResults.DiscountFactorAtMaturity;
        //}

        ///// <summary>
        ///// Calculates the specified metric for the fast bootstrapper.
        ///// </summary>
        ///// <param name="interpolatedSpace">The intepolated Space.</param>
        ///// <returns></returns>
        //public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        //{
        //    AnalyticsModel = new RateSpreadAssetAnalytic();
        //    AnalyticResults = new RateSpreadAssetResults();
        //    StartDiscountFactor = GetDiscountFactor(interpolatedSpace, AdjustedEffectiveDate, BaseDate);
        //    EndDiscountFactor = GetDiscountFactor(interpolatedSpace, RiskMaturityDate, BaseDate);
        //    ISimpleRateAssetParameters analyticModelParameters 
        //        = new RateAssetParameters
        //              {
        //                  YearFraction = YearFraction,
        //                  StartDiscountFactor = StartDiscountFactor,
        //                  EndDiscountFactor = EndDiscountFactor
        //              };
        //    var fixedRate = GetRate(StartDiscountFactor, EndDiscountFactor, YearFraction);//must calc from
        //    if (Spread != null)
        //    {
        //        analyticModelParameters.Rate = fixedRate + MarketQuoteHelper.NormaliseGeneralPriceUnits(SpreadQuotationType, Spread, "DecimalRate").value;
        //    }
        //    AnalyticResults =
        //        AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
        //                                                                           new[] { RateSpreadMetrics.ImpliedQuote });
        //    return AnalyticResults.ImpliedQuote;
        //}
    }
}