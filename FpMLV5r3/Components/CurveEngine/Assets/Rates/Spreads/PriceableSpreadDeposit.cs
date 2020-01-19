/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using DayCounterHelper=Highlander.Reporting.Analytics.V5r3.DayCounters.DayCounterHelper;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Spreads
{
    /// <summary>
    /// Deposit rates
    /// </summary>
    public class PriceableSpreadDeposit : PriceableSimpleRateSpreadAsset
    {
        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset FixingDateOffset { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Deposit Deposit { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal TimeToExpiry { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSpreadDeposit"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="spread">The spread.</param>
        /// <param name="paymentCalendar">A paymentCalendar.</param>
        public PriceableSpreadDeposit(DateTime baseDate, DepositNodeStruct nodeStruct, IBusinessCalendar fixingCalendar, 
            IBusinessCalendar paymentCalendar, BasicQuotation spread)
            : base(baseDate, nodeStruct.BusinessDayAdjustments, spread)
        {
            Id = nodeStruct.Deposit.id;
            Deposit = nodeStruct.Deposit;
            FixingDateOffset = nodeStruct.SpotDate;
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            AdjustedEffectiveDate = AdjustedStartDate;
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.Deposit.term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
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
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return RiskMaturityDate;
        }

        ///// <summary>
        ///// Gets the values.
        ///// </summary>
        ///// <value>The values.</value>
        //public override IList<decimal> Values => new[] { StartDiscountFactor, EndDiscountFactor };

        /// <summary>
        /// Gets the value at maturity.
        /// </summary>
        /// <value>The values.</value>
        public override decimal ValueAtMaturity => EndDiscountFactor;

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
        /////<returns>The spread calculated from the curve provided and the market quote of the asset.</returns>
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
                        DayCounterHelper.Parse(Deposit.dayCountFraction.Value).YearFraction(
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
        //    // Determine if DFAM has been requested - if so that all we evaluate - every other metric is ignored
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
        //        analyticModelParameters.Rate = fixedRate + Spread.value;
        //    }
        //    if (bEvalDiscountFactorAtMaturity)
        //    {
        //        //4. Set the analytic input parameters and Calculate the respective metrics
        //        AnalyticResults =
        //            AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
        //                                                                               metricsToEvaluate);
        //        EndDiscountFactor = AnalyticResults.DiscountFactorAtMaturity;
        //    }
        //    else
        //    {
        //        //3. Get the end discount factor
        //        analyticModelParameters.EndDiscountFactor = EndDiscountFactor;
        //        //4. Set the analytic input parameters and Calculate the respective metrics
        //        AnalyticResults =
        //            AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
        //                                                                               metricsToEvaluate);
        //    }
        //    return GetValue(AnalyticResults);
        //}

        ///// <summary>
        ///// Calculates the specified metric for the fast bootstrapper.
        ///// </summary>
        ///// <param name="interpolatedSpace">The interpolated Space.</param>
        ///// <returns></returns>
        //public override decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace)
        //{
        //    AnalyticsModel = new RateSpreadAssetAnalytic();
        //        //DependencyCreator.Resolve<IModelAnalytic<ISimpleAssetParameters, SpreadMetrics>>(_modelIdentifier);
        //    ISimpleRateAssetParameters analyticModelParameters = new RateAssetParameters { YearFraction = YearFraction };
        //    AnalyticResults = new RateSpreadAssetResults();
        //    //1. instantiate curve
        //    //2. get start df = curve.getvalue(this._adjustedStartDate);
        //    StartDiscountFactor = GetDiscountFactor(interpolatedSpace, AdjustedEffectiveDate, BaseDate);
        //    var endDiscountFactor = GetDiscountFactor(interpolatedSpace, RiskMaturityDate, BaseDate);
        //    //           Values = new[] { startDF, endDF };
        //    analyticModelParameters.StartDiscountFactor = StartDiscountFactor;
        //    //3. Get the Rate
        //    var fixedRate = GetRate(StartDiscountFactor, endDiscountFactor, YearFraction);//must calc from
        //    if (Spread != null)
        //    {
        //        analyticModelParameters.Rate = fixedRate + MarketQuoteHelper.NormaliseGeneralPriceUnits(SpreadQuotationType, Spread, "DecimalRate").value;
        //    }
        //    //4. Set the analytic input parameters and Calculate the respective metrics
        //    AnalyticResults =
        //        AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
        //                                                                           new[] { RateSpreadMetrics.DiscountFactorAtMaturity });
        //    EndDiscountFactor = AnalyticResults.DiscountFactorAtMaturity;
        //    return AnalyticResults.DiscountFactorAtMaturity;
        //}

        ///// <summary>
        ///// Calculates the specified metric for the fast bootstrapper.
        ///// </summary>
        ///// <param name="interpolatedSpace">The interpolated Space.</param>
        ///// <returns></returns>
        //public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        //{
        //    AnalyticsModel = new RateSpreadAssetAnalytic();
        //    AnalyticResults = new RateSpreadAssetResults();
        //    // get start df = curve.getvalue(this._adjustedStartDate);
        //    StartDiscountFactor = GetDiscountFactor(interpolatedSpace, AdjustedEffectiveDate, BaseDate);
        //    EndDiscountFactor = GetDiscountFactor(interpolatedSpace, RiskMaturityDate, BaseDate);
        //    ISimpleRateAssetParameters analyticModelParameters
        //        = new RateAssetParameters
        //              {
        //                  YearFraction = YearFraction,
        //                  StartDiscountFactor = StartDiscountFactor,
        //                  EndDiscountFactor = EndDiscountFactor
        //              };

        //    // Get the Rate
        //    var fixedRate = GetRate(StartDiscountFactor, EndDiscountFactor, YearFraction);//must calc from
        //    if (Spread != null)
        //    {
        //        analyticModelParameters.Rate = fixedRate + Spread.value;
        //    }
        //    // Set the analytic input parameters and Calculate the respective metrics
        //    AnalyticResults =
        //        AnalyticsModel.Calculate<IRateSpreadAssetResults, RateSpreadAssetResults>(analyticModelParameters,
        //                                                                           new[] { RateSpreadMetrics.ImpliedQuote });
        //    return AnalyticResults.ImpliedQuote;
        //}
    }
}